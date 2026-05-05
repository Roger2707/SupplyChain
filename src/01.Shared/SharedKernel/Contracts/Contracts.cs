namespace SharedKernel.Contracts
{
    // Event : Order is created successfully at Ecommercial
    public record OrderCreated(int OrderId, List<ReserveItemDto> Items);
    public record ReserveItemDto(int ProductId, string ProductName, decimal NeccessaryQty);

    // Event : Inventory is reserved successfully
    public record InventoryReserved(
        int OrderId,
        List<ReservedItemDetail> Details // Gửi kèm thông tin đã split ở đây
    );

    public record ReservedItemDetail(
        int ProductId,
        string ProductName,
        decimal ReservedQty,
        decimal UnitCost
    );

    // Event : Inventory is reserved failed (out of stock, error DB, race condition...)
    public record InventoryReservationFailed(int OrderId, string Reason);

    // Command: check payment after time define
    public record OrderPaymentTimeoutCheck(int OrderId);
}
