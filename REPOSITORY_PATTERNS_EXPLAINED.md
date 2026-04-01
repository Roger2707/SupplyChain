# Repository Patterns: DbContext Sharing & Specific vs Generic Repositories

## 🔍 Question 1: Do UnitOfWork and Repository Use the Same DbContext?

### Current Situation

**Short Answer:** They **SHOULD** share the same DbContext (because both are registered as `Scoped`), but the **architecture doesn't guarantee it**.

### How It Works Currently

```csharp
// In Program.cs
builder.Services.AddDbContext<ApplicationDbContext>(options => ...);  // Scoped
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));  // Scoped
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();  // Scoped
```

**What happens:**
1. Both `Repository<Warehouse>` and `UnitOfWork` are registered as **Scoped**
2. In ASP.NET Core, **Scoped** services share the same instance within a single HTTP request
3. Both receive `ApplicationDbContext` via constructor injection
4. **In practice**, they usually get the same DbContext instance

### The Problem

**However**, there's no **architectural guarantee**:

```csharp
// In WarehouseService
public WarehouseService(
    IRepository<Warehouse> repository,  // Gets DbContext #1 (maybe)
    IUnitOfWork unitOfWork)             // Gets DbContext #2 (maybe)
```

**Issues:**
- ❌ No explicit relationship between Repository and UnitOfWork
- ❌ If they get different DbContext instances, transactions won't work
- ❌ Changes in Repository might not be visible to UnitOfWork
- ❌ Can't guarantee atomic operations across multiple repositories

### The Solution (Proper Unit of Work Pattern)

When UnitOfWork exposes repositories, they **guaranteed** to share the same DbContext:

```csharp
// Proper implementation
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;  // ONE DbContext
    
    public IRepository<T> GetRepository<T>()
    {
        return new Repository<T>(_context);  // Same DbContext!
    }
}
```

**Benefits:**
- ✅ **Guaranteed** same DbContext instance
- ✅ All repositories share the same transaction
- ✅ Changes are immediately visible across repositories
- ✅ Proper transaction support

---

## 🔍 Question 2: IWarehouseRepository vs Repository<Warehouse>

### Current Code (What You Have)

You're currently using:
```csharp
IRepository<Warehouse> _repository;  // Generic repository
```

### What is `IWarehouseRepository`?

`IWarehouseRepository` would be a **specific repository interface** that extends the generic repository:

```csharp
// Specific Repository Interface
public interface IWarehouseRepository : IRepository<Warehouse>
{
    // Domain-specific methods for Warehouse
    Task<Warehouse?> GetByCodeAsync(string warehouseCode, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCodeAsync(string warehouseCode, CancellationToken cancellationToken = default);
    Task<IEnumerable<Warehouse>> GetActiveWarehousesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Warehouse>> GetWarehousesByManagerAsync(int managerId, CancellationToken cancellationToken = default);
}
```

### Comparison Table

| Aspect | `IRepository<Warehouse>` | `IWarehouseRepository` |
|--------|-------------------------|------------------------|
| **Type** | Generic interface | Specific interface |
| **Methods** | Generic CRUD operations | Generic CRUD + Domain-specific methods |
| **Flexibility** | Works for any entity | Only for Warehouse |
| **Domain Logic** | None (pure data access) | Can include warehouse-specific queries |
| **Usage** | `IRepository<Warehouse>` | `IWarehouseRepository` |
| **Implementation** | `Repository<Warehouse>` | `WarehouseRepository : Repository<Warehouse>, IWarehouseRepository` |

---

## 📊 Detailed Comparison

### 1. Generic Repository: `IRepository<Warehouse>`

**Interface:**
```csharp
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(object id, ...);
    Task<IEnumerable<T>> GetAllAsync(...);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, ...);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, ...);
    // ... generic CRUD methods
}
```

**Usage:**
```csharp
// In WarehouseService
var warehouse = await _repository.FirstOrDefaultAsync(
    w => w.WarehouseCode == "WH001", 
    cancellationToken);
```

**Pros:**
- ✅ Simple and straightforward
- ✅ Works for all entities
- ✅ Less code to maintain
- ✅ Good for simple CRUD operations

**Cons:**
- ❌ No domain-specific methods
- ❌ Business logic queries scattered in services
- ❌ Less discoverable (need to know what predicates to write)

---

### 2. Specific Repository: `IWarehouseRepository`

**Interface:**
```csharp
public interface IWarehouseRepository : IRepository<Warehouse>
{
    // Inherits all generic methods from IRepository<Warehouse>
    // PLUS domain-specific methods:
    
    Task<Warehouse?> GetByCodeAsync(string warehouseCode, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCodeAsync(string warehouseCode, CancellationToken cancellationToken = default);
    Task<IEnumerable<Warehouse>> GetActiveWarehousesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Warehouse>> GetWarehousesByManagerAsync(int managerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Warehouse>> GetWarehousesByAreaRangeAsync(decimal minArea, decimal maxArea, CancellationToken cancellationToken = default);
}
```

**Implementation:**
```csharp
public class WarehouseRepository : Repository<Warehouse>, IWarehouseRepository
{
    public WarehouseRepository(ApplicationDbContext context) 
        : base(context)
    {
    }

    public async Task<Warehouse?> GetByCodeAsync(string warehouseCode, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(w => w.WarehouseCode == warehouseCode, cancellationToken);
    }

    public async Task<bool> ExistsByCodeAsync(string warehouseCode, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(w => w.WarehouseCode == warehouseCode, cancellationToken);
    }

    public async Task<IEnumerable<Warehouse>> GetActiveWarehousesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(w => w.IsActive == true)
            .OrderBy(w => w.WarehouseName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Warehouse>> GetWarehousesByManagerAsync(int managerId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(w => w.ManagerId == managerId && w.IsActive == true)
            .ToListAsync(cancellationToken);
    }
}
```

**Usage:**
```csharp
// In WarehouseService
public WarehouseService(IWarehouseRepository repository, IUnitOfWork unitOfWork)
{
    _repository = repository;
    _unitOfWork = unitOfWork;
}

// Much cleaner!
var warehouse = await _repository.GetByCodeAsync("WH001", cancellationToken);
var exists = await _repository.ExistsByCodeAsync("WH001", cancellationToken);
var activeWarehouses = await _repository.GetActiveWarehousesAsync(cancellationToken);
```

**Pros:**
- ✅ **Domain-specific methods** - clearer intent
- ✅ **Reusable queries** - common queries in one place
- ✅ **Better encapsulation** - repository knows about Warehouse domain
- ✅ **More discoverable** - IntelliSense shows warehouse-specific methods
- ✅ **Can optimize queries** - add `.Include()`, `.Select()`, etc.
- ✅ **Easier to test** - mock specific methods

**Cons:**
- ❌ More code to write and maintain
- ❌ Need one repository per entity (if you want specific methods)
- ❌ Can lead to repository bloat if overused

---

## 🎯 When to Use Which?

### Use `IRepository<Warehouse>` (Generic) When:
- ✅ Simple CRUD operations are enough
- ✅ You have many entities and want consistency
- ✅ Queries are simple and don't repeat
- ✅ You want minimal code

### Use `IWarehouseRepository` (Specific) When:
- ✅ You have **frequently used domain queries**
- ✅ You need **optimized queries** (Include, Select, etc.)
- ✅ You want **better encapsulation** of warehouse logic
- ✅ You have **complex queries** that are reused
- ✅ You want **better testability**

---

## 💡 Real-World Example from Your Code

### Current (Generic Repository):
```csharp
// In WarehouseService.CreateAsync
var existingWarehouse = await _repository.FirstOrDefaultAsync(
    w => w.WarehouseCode == createDto.WarehouseCode, 
    cancellationToken);

// In WarehouseService.UpdateAsync
var existingWarehouse = await _repository.FirstOrDefaultAsync(
    w => w.WarehouseCode == updateDto.WarehouseCode && w.Id != id, 
    cancellationToken);
```

**Problem:** The same query pattern (`GetByCode`) is repeated in multiple places.

### With Specific Repository:
```csharp
// In WarehouseService.CreateAsync
var existingWarehouse = await _repository.GetByCodeAsync(
    createDto.WarehouseCode, 
    cancellationToken);

// In WarehouseService.UpdateAsync
var existingWarehouse = await _repository.GetByCodeAsync(
    updateDto.WarehouseCode, 
    cancellationToken);
```

**Benefits:**
- ✅ More readable
- ✅ Reusable
- ✅ Can add caching/optimization in one place
- ✅ Easier to change implementation later

---

## 🔧 Hybrid Approach (Recommended)

You can use **both**:

```csharp
public interface IWarehouseRepository : IRepository<Warehouse>
{
    // Only add methods that are:
    // 1. Frequently used
    // 2. Domain-specific
    // 3. Need optimization
    
    Task<Warehouse?> GetByCodeAsync(string warehouseCode, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCodeAsync(string warehouseCode, CancellationToken cancellationToken = default);
    
    // For simple queries, still use generic methods:
    // _repository.FirstOrDefaultAsync(w => w.IsActive == true)
}
```

**Best Practice:**
- Use **generic repository** for simple, one-off queries
- Use **specific repository** for frequently used, domain-specific queries

---

## 📝 Summary

### DbContext Sharing:
- **Current:** Should work (both Scoped), but not guaranteed
- **Proper:** UnitOfWork should expose repositories to guarantee same DbContext

### Repository Types:
- **`IRepository<Warehouse>`**: Generic, simple, works for all entities
- **`IWarehouseRepository`**: Specific, domain-focused, better for complex scenarios

**Recommendation:** Start with generic, add specific repository when you have repeated domain queries.

