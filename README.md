# Inventory System Backend (Clean Architecture)

Backend ERP Inventory + ECommerce trên ASP.NET Core Web API theo Clean Architecture.

## Cấu trúc sau khi refactor

```text
InventorySystem/
├─ src/
│  ├─ SharedKernel/                   # BaseEntity, Result<T>, Result
│  ├─ InventorySystem.Domain/
│  ├─ InventorySystem.Application/
│  ├─ InventorySystem.Infrastructure/
│  ├─ InventorySystem.WebApi/
│  ├─ ECommerce.Domain/
│  ├─ ECommerce.Application/
│  ├─ ECommerce.Infrastructure/
│  └─ ECommerce.WebApi/
├─ InventorySystem.sln
└─ README.md
```

## Bounded contexts

### Inventory (ERP)
- Giữ nguyên kiến trúc cũ:
  - `InventorySystem.Domain`
  - `InventorySystem.Application`
  - `InventorySystem.Infrastructure`
  - `InventorySystem.WebApi`
- Không chứa Basket nữa.

### ECommerce
- Tách project độc lập theo cùng clean architecture:
  - `ECommerce.Domain`
  - `ECommerce.Application`
  - `ECommerce.Infrastructure`
  - `ECommerce.WebApi`
- Module `Basket` nằm hoàn toàn trong ECommerce.

## Vai trò từng layer

### `src/InventorySystem.Domain`
- Core business: entities, value objects, rules nghiệp vụ.
- Không phụ thuộc project nào khác.

### `src/InventorySystem.Application`
- Use cases, service contracts, DTOs, orchestration.
- Chỉ phụ thuộc `Domain`.

### `src/InventorySystem.Infrastructure`
- Triển khai persistence và external concerns.
- Chứa EF Core repositories, UnitOfWork, Dapper queries, JWT/Redis services.
- Phụ thuộc `Application` và `Domain`.

### `src/InventorySystem.WebApi`
- Presentation + composition root (DI, middleware, controllers).
- Phụ thuộc `Application` và `Infrastructure`.

## Dependency rule

```text
WebApi -> Application
WebApi -> Infrastructure
Infrastructure -> Application -> Domain
```

## Data access strategy

- `EF Core`: create/update/delete, transaction boundaries, migrations.
- `Dapper`: query tối ưu cho dashboard/report/list phức tạp.
- Khi cần nhất quán dữ liệu trong transaction, Dapper dùng cùng connection/transaction với EF.

## Chạy dự án

```bash
dotnet restore
dotnet build InventorySystem.sln
dotnet run --project src/InventorySystem.WebApi
dotnet run --project src/ECommerce.WebApi
```

Swagger UI: `/swagger`


