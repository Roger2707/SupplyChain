## Kế hoạch xử lý concurrency cho SalesOrder

### Mục tiêu
- **Đảm bảo an toàn khi nhiều user cùng thao tác** trên cùng một `SalesOrder` (đặc biệt khi status = Draft, đang bị sửa qty, thêm bớt line).
- **Đảm bảo chính xác tồn khả dụng (available)** khi nhiều user cùng confirm/post `SalesOrder` sử dụng chung các `InventoryCostLayer` (FIFO + Reservation).
- **Giữ nguyên kiến trúc hiện tại**: sử dụng kết hợp EF Core (UnitOfWork/Repositories) và Dapper (nếu có) cho query phụ trợ, nhưng ưu tiên dùng transaction + concurrency của EF Core.

### Hiện trạng (tóm tắt)
- `SalesOrder`:
  - Tạo ở trạng thái **Draft**, update thoải mái (`AllowUpdate()` chỉ check Status = Draft).
  - Khi confirm (`SalesOrderService.ConfirmAsync`):
    - Gọi `so.Confirm()` -> đổi status sang Confirmed.
    - Chạy FIFO: lấy `InventoryCostLayer` theo `GetFIFOProductsById(productId)` (điều kiện `RemainingQty - ReservedQty > 0`).
    - Tạo `InventoryReservation` theo từng layer, cập nhật `layer.ReservedQty += takeQty`.
    - Replace lại `so.Lines` bằng danh sách line đã split theo cost layer.
    - Tất cả bọc trong một transaction EF (`BeginTransactionAsync` + `SaveChangesAsync` + `CommitTransactionAsync`).
- `BaseEntity` đã có trường `RowVersion` (byte[]?) với mục tiêu dùng cho **optimistic concurrency**, nhưng **chưa cấu hình trong EF configuration** và chưa được dùng trong logic.

### Vấn đề concurrency cụ thể
- **1. Cùng lúc nhiều người sửa SalesOrder Draft**
  - Ví dụ: User A và B cùng mở SalesOrder #10 (Draft), A đổi qty line 1, B đổi qty line 2, hoặc cả 2 cùng sửa qty line 1.
  - Vì không có check version, `SaveChangesAsync` cuối cùng sẽ overwrite thay đổi của người còn lại (lost update).

- **2. Cùng lúc nhiều người confirm/post các SalesOrder dùng chung InventoryCostLayer**
  - `GetFIFOProductsById` chỉ lọc `RemainingQty - ReservedQty > 0`, sau đó tính available = `RemainingQty - ReservedQty` trong code C#.
  - Nếu hai transaction cùng đọc layer trước khi transaction kia commit, cả hai đều thấy available > 0, rồi cùng tăng `ReservedQty` -> có thể **over-reserve** so với `RemainingQty`.
  - Khi đó tồn khả dụng thực tế (`RemainingQty - ReservedQty`) và stock business logic có thể sai.

### Chiến lược tổng quát
- **Sử dụng optimistic concurrency với RowVersion**:
  - Cấu hình `RowVersion` là `IsRowVersion()` trong các entity quan trọng: `SalesOrder`, `InventoryCostLayer`, có thể cả `InventoryReservation` nếu cần.
  - Đảm bảo mỗi lần `SaveChangesAsync` sẽ tự động check `RowVersion`, nếu mismatch sẽ ném `DbUpdateConcurrencyException`.
  - Trong `SalesOrderService.ConfirmAsync` và `UpdateAsync`:
    - **Đọc RowVersion** từ DB khi lấy entity (EF sẽ track).
    - Khi `SaveChangesAsync` gặp concurrency error, bắt exception, rollback transaction, trả về message thân thiện cho client (VD: "Dữ liệu đã thay đổi bởi user khác, vui lòng reload").

- **Giữ transaction ngắn và rõ ràng**:
  - Đã dùng `BeginTransactionAsync`/`CommitTransactionAsync` trong `ConfirmAsync`, tiếp tục sử dụng.
  - Tất cả thao tác:
    - Đọc `SalesOrder` + Lines.
    - Đọc `InventoryCostLayer` liên quan.
    - Tính toán FIFO + Reservation.
    - Cập nhật `layer.ReservedQty`, tạo `InventoryReservation`, cập nhật `so.Lines` và `Status`.
    - `SaveChangesAsync`.
  - Được bao trong **một transaction** + optimistic concurrency trên `SalesOrder` và `InventoryCostLayer`.

### Thiết kế chi tiết thay đổi

#### 1. Cấu hình RowVersion trong EF Core
- **Mục tiêu**: kích hoạt cơ chế optimistic concurrency built-in.
- Việc cần làm:
  - Trong các file configuration liên quan (VD: `SalesOrderConfiguration`, `InventoryCostLayerConfiguration`, có thể `BaseEntityConfiguration` nếu đang dùng):
    - Đánh dấu `RowVersion` là concurrency token:
      - `builder.Property(e => e.RowVersion).IsRowVersion();`
  - Đảm bảo `RowVersion` được map đúng kiểu `rowversion`/`timestamp` ở DB (migrations đã có vì field nằm trong `BaseEntity`, nếu chưa thì tạo migration mới).

#### 2. Bảo vệ **Update SalesOrder Draft** bằng optimistic concurrency
- Ở `SalesOrderService.UpdateAsync`:
  - Khi lấy `salesOrderExist = GetWithLinesAsync(id, ...)`, EF sẽ track đối tượng kèm `RowVersion`.
  - Client cần gửi kèm `RowVersion` hiện tại khi update:
    - Thêm field `byte[] RowVersion` (hoặc string Base64) vào `UpdateSalesOrderDto`.
    - Map `salesOrderExist.RowVersion = updateSalesOrderDto.RowVersion;` để EF biết phiên bản mà client đang sửa.
  - Khi `SaveChangesAsync`:
    - Nếu `RowVersion` ở DB khác với `RowVersion` client gửi => EF ném `DbUpdateConcurrencyException`.
  - Xử lý:
    - Bọc `SaveChangesAsync` trong `try/catch(DbUpdateConcurrencyException)`:
      - Rollback transaction.
      - Trả về `Result<SalesOrderDto>.Failure("SalesOrder đã được cập nhật bởi user khác. Vui lòng tải lại trước khi sửa tiếp.")`.

#### 3. Bảo vệ **Confirm/Post SalesOrder** + xử lý FIFO/Reservation
- Ở `SalesOrderService.ConfirmAsync`:
  - Vẫn **bọc trong transaction** như hiện tại.
  - Thêm concurrency:
    - Đảm bảo `SalesOrder` dùng `RowVersion` như trên.
    - Đảm bảo `InventoryCostLayer` cũng có `RowVersion` cấu hình.
  - Khi đọc layer:
    - Có thể giữ logic `GetFIFOProductsById` (đã filter theo `RemainingQty - ReservedQty > 0`).
    - Sau khi tính toán `available` và `takeQty`:
      - Cập nhật `layer.ReservedQty += takeQty;`
      - Không cần thay đổi cách tính, chỉ dựa vào optimistic concurrency để tránh **2 transaction cùng ghi chồng**.
  - `SaveChangesAsync`:
    - Nếu có transaction khác đã update `ReservedQty` hoặc `RemainingQty` của layer:
      - `RowVersion` khác -> EF ném `DbUpdateConcurrencyException`.
    - Bắt exception:
      - Rollback transaction.
      - Trả về `Result.Failure("Tồn kho đã thay đổi trong lúc xử lý. Vui lòng thử confirm lại SalesOrder.")`.
  - Nếu cần **mức chặt hơn**:
    - Có thể thêm một bước re-check available trước khi commit:
      - Sau khi tính xong reservation nhưng trước `SaveChangesAsync`, thực hiện query Dapper hoặc EF:
        - `SELECT RemainingQty, ReservedQty, RowVersion FROM InventoryCostLayers WHERE Id IN (...) FOR UPDATE` (nếu DB hỗ trợ) hoặc re-load bằng EF.
        - Nếu bất kỳ layer nào không còn `RemainingQty - ReservedQty >= tổng ReservedQty mới` thì abort với message "Not enough stock".

#### 4. Đồng bộ EF Core và Dapper (nếu có)
- Nếu các chỗ khác dùng Dapper để:
  - Tính Available.
  - Xem tồn kho nhanh.
- Cần **tuân theo cùng một định nghĩa**:
  - Available = `RemainingQty - ReservedQty`.
  - Không được trừ trực tiếp `RemainingQty` khi chỉ reservation.
  - Khi **Delivery/Invoice** thực tế xuất kho:
    - Lúc đó mới giảm `RemainingQty` và giảm `ReservedQty` tương ứng (nếu reservation đã tồn tại).
  - Dapper chỉ dùng để đọc / report, **không được cập nhật cùng bảng** trong khi EF đang quản lý transaction, tránh phá vỡ optimistic concurrency.

### Bước triển khai cụ thể
1. **EF Config**:
   - Cập nhật `BaseEntityConfiguration` hoặc từng configuration riêng (SalesOrder, InventoryCostLayer) để đánh dấu `RowVersion` là concurrency token.
   - Chạy migration nếu cần.

2. **DTO + API**:
   - Thêm `RowVersion` vào các DTO:
     - `SalesOrderDto` (để client nhận giá trị hiện tại).
     - `UpdateSalesOrderDto` (để client gửi lại khi update).
   - Đảm bảo `SalesOrderController` map/nhận `RowVersion` chính xác (byte[] hoặc Base64 string).

3. **Service layer**:
   - `SalesOrderService.UpdateAsync`:
     - Set lại `RowVersion` từ DTO.
     - Bọc `SaveChangesAsync` với `DbUpdateConcurrencyException`.
   - `SalesOrderService.ConfirmAsync`:
     - Đảm bảo transaction như hiện tại.
     - Thêm catch `DbUpdateConcurrencyException`.
     - Message rõ ràng cho user.

4. **(Tuỳ chọn) Kiểm tra available lần cuối**:
   - Trước khi commit Confirm, re-load layer hoặc query Dapper để chắc chắn available vẫn đủ.
   - Nếu không đủ -> rollback, báo lỗi rõ.

5. **Test case**:
   - 2 user cùng mở SalesOrder Draft:
     - A update trước -> OK.
     - B update sau với RowVersion cũ -> nhận lỗi concurrency, phải reload.
   - 2 user cùng confirm 2 SO sử dụng chung `InventoryCostLayer`:
     - Nếu stock đủ cho 1 SO nhưng không đủ cho 2:
       - Một transaction commit thành công.
       - Transaction còn lại gặp concurrency hoặc thiếu tồn kho -> rollback + báo "Not enough stock" / "Inventory changed".

### Kết luận
- Giải pháp chính: **Optimistic concurrency với RowVersion + transaction EF** cho cả `SalesOrder` và `InventoryCostLayer`.
- Không thay đổi nhiều kiến trúc, chỉ:
  - Bật đúng tính năng concurrency của EF Core.
  - Thêm `RowVersion` vào DTO/Service.
  - Bắt và xử lý `DbUpdateConcurrencyException` để tránh lost update và over-reservation.
