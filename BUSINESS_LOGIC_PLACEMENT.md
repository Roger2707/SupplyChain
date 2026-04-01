# Business Logic Placement: Service vs Repository

## ✅ Short Answer: **YES, it makes perfect sense!**

**Business logic belongs in the Service layer**, not in the Repository layer. This is the correct architectural pattern.

---

## 🎯 Layer Responsibilities

### Repository Layer (Data Access)
**Purpose:** Handle data access operations

**Should contain:**
- ✅ CRUD operations
- ✅ Database queries
- ✅ Data retrieval patterns
- ✅ Query optimization (Include, Select, etc.)

**Should NOT contain:**
- ❌ Business rules
- ❌ Validation logic
- ❌ Domain rules
- ❌ Transaction orchestration

### Service Layer (Business Logic)
**Purpose:** Handle business logic and orchestration

**Should contain:**
- ✅ Business rules and validation
- ✅ Domain logic
- ✅ Transaction management
- ✅ Orchestration of multiple repositories
- ✅ DTO mapping
- ✅ Error handling

**Should NOT contain:**
- ❌ Direct database access
- ❌ SQL queries
- ❌ Entity Framework specifics

---

## 📊 Analysis of Your Current Code

Looking at your `WarehouseService`, here's what you have:

### ✅ Business Logic in Service (CORRECT!)

#### 1. **Business Rule: Warehouse Code Must Be Unique**
```csharp
// Line 43-51 in CreateAsync
var existingWarehouse = await _repository.FirstOrDefaultAsync(
    w => w.WarehouseCode == createDto.WarehouseCode, 
    cancellationToken);

if (existingWarehouse != null)
{
    return Result<WarehouseDto>.Failure($"Warehouse with code '{createDto.WarehouseCode}' already exists.");
}
```
**✅ CORRECT:** This is a **business rule** - "warehouse codes must be unique". This belongs in the service.

#### 2. **Business Rule: Can't Change Code to Existing One**
```csharp
// Line 70-81 in UpdateAsync
if (warehouse.WarehouseCode != updateDto.WarehouseCode)
{
    var existingWarehouse = await _repository.FirstOrDefaultAsync(
        w => w.WarehouseCode == updateDto.WarehouseCode && w.Id != id, 
        cancellationToken);

    if (existingWarehouse != null)
    {
        return Result<WarehouseDto>.Failure($"Warehouse with code '{updateDto.WarehouseCode}' already exists.");
    }
}
```
**✅ CORRECT:** This is a **business validation rule**. Service is the right place.

#### 3. **Business Decision: Soft Delete**
```csharp
// Line 100-101 in DeleteAsync
// Soft delete
warehouse.IsDeleted = true;
await _repository.UpdateAsync(warehouse, cancellationToken);
```
**✅ CORRECT:** This is a **business decision** - "we don't hard delete, we soft delete". Service handles this.

#### 4. **Transaction Management**
```csharp
await _repository.AddAsync(warehouse, cancellationToken);
await _unitOfWork.SaveChangesAsync(cancellationToken);
```
**✅ CORRECT:** Service orchestrates the transaction.

---

## 🔍 What Could Be Improved (Optional)

### Data Access Patterns (Could Move to Repository)

The query pattern "get by warehouse code" is repeated:

```csharp
// In CreateAsync (line 44-46)
var existingWarehouse = await _repository.FirstOrDefaultAsync(
    w => w.WarehouseCode == createDto.WarehouseCode, 
    cancellationToken);

// In UpdateAsync (line 73-75)
var existingWarehouse = await _repository.FirstOrDefaultAsync(
    w => w.WarehouseCode == updateDto.WarehouseCode && w.Id != id, 
    cancellationToken);
```

**This is a data access pattern**, not business logic. You could move it to a specific repository:

```csharp
// In IWarehouseRepository
Task<Warehouse?> GetByCodeAsync(string warehouseCode, CancellationToken cancellationToken = default);
Task<bool> ExistsByCodeAsync(string warehouseCode, CancellationToken cancellationToken = default);
```

**But the business rule stays in the service:**
```csharp
// In WarehouseService
public async Task<Result<WarehouseDto>> CreateAsync(...)
{
    // Business rule: Check uniqueness
    if (await _repository.ExistsByCodeAsync(createDto.WarehouseCode, cancellationToken))
    {
        return Result<WarehouseDto>.Failure($"Warehouse with code '{createDto.WarehouseCode}' already exists.");
    }
    
    // Business logic: Create warehouse
    var warehouse = MapToEntity(createDto);
    await _repository.AddAsync(warehouse, cancellationToken);
    await _unitOfWork.SaveChangesAsync(cancellationToken);
    
    return Result<WarehouseDto>.Success(MapToDto(warehouse));
}
```

---

## 📋 Decision Matrix: Where Should This Go?

| Type of Logic | Repository | Service | Example |
|--------------|------------|---------|---------|
| **Data Query Pattern** | ✅ | ❌ | `GetByCodeAsync()`, `GetActiveWarehousesAsync()` |
| **Business Rule** | ❌ | ✅ | "Code must be unique", "Can't delete if has inventory" |
| **Validation** | ❌ | ✅ | "Warehouse name required", "Area must be positive" |
| **Transaction** | ❌ | ✅ | Save multiple entities atomically |
| **Orchestration** | ❌ | ✅ | Create warehouse + create default location |
| **DTO Mapping** | ❌ | ✅ | Entity ↔ DTO conversion |
| **Domain Logic** | ❌ | ✅ | "Calculate total capacity", "Check if warehouse is full" |

---

## 💡 Examples: What Goes Where

### ✅ Repository: Data Access Patterns

```csharp
public interface IWarehouseRepository : IRepository<Warehouse>
{
    // Data access patterns - reusable queries
    Task<Warehouse?> GetByCodeAsync(string warehouseCode, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCodeAsync(string warehouseCode, CancellationToken cancellationToken = default);
    Task<IEnumerable<Warehouse>> GetActiveWarehousesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Warehouse>> GetWarehousesByManagerAsync(int managerId, CancellationToken cancellationToken = default);
}
```

**Why Repository?**
- These are **data access patterns**
- Reusable across different services
- Can be optimized (Include, Select, etc.)
- No business logic involved

---

### ✅ Service: Business Logic

```csharp
public class WarehouseService : IWarehouseService
{
    private readonly IWarehouseRepository _repository;
    private readonly IInventoryRepository _inventoryRepository; // Example
    private readonly IUnitOfWork _unitOfWork;

    public async Task<Result<WarehouseDto>> CreateAsync(CreateWarehouseDto createDto, CancellationToken cancellationToken = default)
    {
        // ✅ Business Rule 1: Code must be unique
        if (await _repository.ExistsByCodeAsync(createDto.WarehouseCode, cancellationToken))
        {
            return Result<WarehouseDto>.Failure($"Warehouse with code '{createDto.WarehouseCode}' already exists.");
        }

        // ✅ Business Rule 2: Area must be positive
        if (createDto.Area.HasValue && createDto.Area <= 0)
        {
            return Result<WarehouseDto>.Failure("Warehouse area must be greater than zero.");
        }

        // ✅ Business Rule 3: Manager must exist (if provided)
        if (createDto.ManagerId.HasValue)
        {
            // Would check manager exists in another service/repository
        }

        // ✅ Business Logic: Create warehouse
        var warehouse = MapToEntity(createDto);
        await _repository.AddAsync(warehouse, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<WarehouseDto>.Success(MapToDto(warehouse));
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var warehouse = await _repository.GetByIdAsync(id, cancellationToken);
        
        if (warehouse == null)
        {
            return Result.Failure($"Warehouse with ID {id} not found.");
        }

        // ✅ Business Rule: Can't delete warehouse with active inventory
        var hasInventory = await _inventoryRepository.ExistsAsync(
            i => i.WarehouseId == id && i.Quantity > 0, 
            cancellationToken);
        
        if (hasInventory)
        {
            return Result.Failure("Cannot delete warehouse with active inventory. Please transfer or remove inventory first.");
        }

        // ✅ Business Decision: Soft delete
        warehouse.IsDeleted = true;
        await _repository.UpdateAsync(warehouse, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<WarehouseDto>> TransferInventoryAsync(
        int fromWarehouseId, 
        int toWarehouseId, 
        int productId, 
        int quantity,
        CancellationToken cancellationToken = default)
    {
        // ✅ Business Logic: Complex orchestration
        var fromWarehouse = await _repository.GetByIdAsync(fromWarehouseId, cancellationToken);
        var toWarehouse = await _repository.GetByIdAsync(toWarehouseId, cancellationToken);

        // Business rules...
        if (fromWarehouse == null || toWarehouse == null)
            return Result<WarehouseDto>.Failure("One or both warehouses not found.");

        if (!fromWarehouse.IsActive || !toWarehouse.IsActive)
            return Result<WarehouseDto>.Failure("Both warehouses must be active.");

        // Business logic: Transfer inventory
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // Complex business operation...
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }

        return Result<WarehouseDto>.Success(MapToDto(toWarehouse));
    }
}
```

**Why Service?**
- Contains **business rules** and **validation**
- Orchestrates **multiple repositories**
- Manages **transactions**
- Handles **domain logic**

---

## 🎯 Best Practices

### ✅ DO: Put in Service
- Business rules ("code must be unique", "can't delete if has inventory")
- Validation logic
- Transaction orchestration
- Cross-entity operations
- Domain calculations
- Business decisions

### ✅ DO: Put in Repository
- Data access patterns
- Reusable queries
- Query optimization
- Database-specific operations

### ❌ DON'T: Put in Repository
- Business rules
- Validation
- Transaction management
- Business logic

### ❌ DON'T: Put in Service
- Direct SQL queries
- Entity Framework specifics
- Database connection management

---

## 📝 Summary

### Your Current Code: ✅ **CORRECT!**

You're doing it right! Your business logic is in the service layer where it belongs:

1. ✅ **Business rule validation** (unique code check) → Service
2. ✅ **Business decision** (soft delete) → Service
3. ✅ **Transaction management** → Service
4. ✅ **DTO mapping** → Service

### Optional Improvement:

You could extract **data access patterns** to a specific repository for reusability, but the **business logic stays in the service**.

**Example:**
- **Repository:** `GetByCodeAsync()` - data access pattern
- **Service:** "Code must be unique" - business rule (uses repository method)

---

## 🏆 Conclusion

**YES, placing business logic in `WarehouseService` is absolutely correct!**

The Service layer is the right place for:
- Business rules
- Validation
- Transaction orchestration
- Domain logic

The Repository layer should only handle:
- Data access
- Query patterns
- Database operations

Your architecture is sound! 🎉

