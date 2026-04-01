# Identity Flow Explanation

## Overview

This project implements a custom authentication and authorization system using JWT tokens, following Clean Architecture principles. The system uses **ASP.NET Core Authorization** but implements **custom authentication** (not ASP.NET Identity).

## Architecture Layers

### 1. Domain Layer (`InventorySystem.Domain`)
Contains the Identity entities:
- **User**: User accounts with username, password hash, email, etc.
- **Role**: User roles (e.g., Admin, Manager, Employee)
- **Permission**: Permissions that can be assigned to roles (e.g., "Warehouse.Create", "Product.Delete")
- **UserRole**: Many-to-many relationship between Users and Roles
- **RolePermission**: Many-to-many relationship between Roles and Permissions

### 2. Application Layer (`InventorySystem.Application`)
Contains business logic interfaces and services:
- **Interfaces**: `IAuthenticationService`, `IAuthorizationService`, `IJwtService`, `IPasswordHasher`, `IUserRepository`, `IRoleRepository`, `IPermissionRepository`
- **Services**: `AuthenticationService`, `AuthorizationService`, `UserService`, `RoleService`
- **DTOs**: Data transfer objects for API communication

### 3. Infrastructure Layer (`InventorySystem.Infrastructure`)
Contains data access and external services:
- **Repositories**: `UserRepository`, `RoleRepository`, `PermissionRepository`
- **Entity Configurations**: EF Core configurations for all Identity entities
- **Services**: `JwtService`, `PasswordHasher`
- **DbContext**: `ApplicationDbContext` with Identity DbSets

### 4. WebApi Layer (`InventorySystem.WebApi`)
Contains API controllers and middleware:
- **Controllers**: `AuthController`, `UsersController`, `RolesController`
- **Middleware**: `JwtAuthenticationHandler` (custom authentication handler)
- **Handlers**: `PermissionAuthorizationHandler`, `PermissionPolicyProvider`
- **Attributes**: `RequirePermissionAttribute` for permission-based authorization

## Authentication Flow

### 1. User Registration/Login

```
Client → POST /api/auth/register or /api/auth/login
    ↓
AuthController → AuthenticationService
    ↓
PasswordHasher (hash/verify password)
    ↓
UserRepository (save/retrieve user)
    ↓
JwtService (generate JWT token with user claims)
    ↓
Return AuthResponseDto with Token and User info
```

### 2. JWT Token Structure

The JWT token contains:
- **Claims**: User ID, Username, Email, FullName
- **Roles**: All roles assigned to the user
- **Permissions**: All permissions from all roles assigned to the user

### 3. Request Authentication

```
Client Request with Header: Authorization: Bearer <token>
    ↓
JwtAuthenticationHandler (middleware)
    ↓
JwtService.ValidateToken()
    ↓
Extract ClaimsPrincipal from token
    ↓
Set HttpContext.User with ClaimsPrincipal
    ↓
Continue to Controller
```

## Authorization Flow

### 1. Permission-Based Authorization

The system uses **ASP.NET Core Authorization** with custom permission checking:

```
Controller Action with [RequirePermission("Warehouse.Create")]
    ↓
PermissionPolicyProvider (creates policy dynamically)
    ↓
PermissionAuthorizationHandler
    ↓
Check if User has "Permission" claim with value "Warehouse.Create"
    ↓
Allow or Deny request
```

### 2. Using Authorization in Controllers

```csharp
// Require authentication only
[Authorize]
public IActionResult GetUsers() { ... }

// Require specific permission
[RequirePermission("Warehouse.Create")]
public IActionResult CreateWarehouse() { ... }

// Allow anonymous access
[AllowAnonymous]
public IActionResult Login() { ... }
```

## Key Components

### 1. JWT Service (`JwtService`)
- **GenerateToken**: Creates JWT token with user claims (roles, permissions)
- **ValidateToken**: Validates JWT token and extracts ClaimsPrincipal
- **Configuration**: Token key, issuer, audience, expiration from `appsettings.json`

### 2. Password Hasher (`PasswordHasher`)
- Uses SHA256 for password hashing
- **HashPassword**: Hashes plain text password
- **VerifyPassword**: Verifies password against hash

### 3. Authentication Service (`AuthenticationService`)
- **LoginAsync**: Authenticates user and returns JWT token
- **RegisterAsync**: Creates new user and returns JWT token
- **ChangePasswordAsync**: Changes user password

### 4. Authorization Service (`AuthorizationService`)
- **HasPermissionAsync**: Checks if user has specific permission
- **GetUserPermissionsAsync**: Gets all permissions for a user
- **GetUserRolesAsync**: Gets all roles for a user

## Configuration

### appsettings.json

```json
{
  "JWTSettings": {
    "TokenKey": "B8Kp392sRQma0q5LgN9Jwngjc4HEa4fgQW9V40L9dP77LzbzLMZyoR7zgw4jK2AG",
    "Issuer": "InventorySystem",
    "Audience": "InventorySystem",
    "ExpirationMinutes": "1440"
  }
}
```

### Program.cs Configuration

1. **Authentication**: Custom JWT handler registered
2. **Authorization**: Permission-based policies configured
3. **Services**: All services registered with dependency injection

## Database Schema

### Tables
- **Users**: User accounts
- **Roles**: User roles
- **Permissions**: Available permissions
- **UserRoles**: User-Role assignments
- **RolePermissions**: Role-Permission assignments

### Relationships
- User ↔ UserRole ↔ Role (Many-to-Many)
- Role ↔ RolePermission ↔ Permission (Many-to-Many)

## API Endpoints

### Authentication
- `POST /api/auth/login` - Login with username/password
- `POST /api/auth/register` - Register new user
- `POST /api/auth/change-password` - Change password (requires auth)

### Users
- `GET /api/users` - Get all users (requires auth)
- `GET /api/users/{id}` - Get user by ID (requires auth)
- `POST /api/users` - Create user (requires auth)
- `PUT /api/users/{id}` - Update user (requires auth)
- `DELETE /api/users/{id}` - Delete user (requires auth)
- `POST /api/users/{id}/roles` - Assign roles to user (requires auth)

### Roles
- `GET /api/roles` - Get all roles (requires auth)
- `GET /api/roles/{id}` - Get role by ID (requires auth)
- `POST /api/roles` - Create role (requires auth)
- `PUT /api/roles/{id}` - Update role (requires auth)
- `DELETE /api/roles/{id}` - Delete role (requires auth)
- `POST /api/roles/{id}/permissions` - Assign permissions to role (requires auth)

## Usage Example

### 1. Register a User
```http
POST /api/auth/register
Content-Type: application/json

{
  "username": "john.doe",
  "password": "SecurePassword123!",
  "fullName": "John Doe",
  "email": "john.doe@example.com"
}
```

### 2. Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "john.doe",
  "password": "SecurePassword123!"
}
```

Response:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2024-01-02T12:00:00Z",
  "user": {
    "id": 1,
    "username": "john.doe",
    "roles": ["Admin"],
    "permissions": ["Warehouse.Create", "Warehouse.Update", ...]
  }
}
```

### 3. Use Token in Requests
```http
GET /api/users
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### 4. Protect Endpoints with Permissions
```csharp
[RequirePermission("Warehouse.Create")]
[HttpPost]
public IActionResult CreateWarehouse([FromBody] CreateWarehouseDto dto)
{
    // Only users with "Warehouse.Create" permission can access this
}
```

## Security Features

1. **Password Hashing**: SHA256 hashing (consider upgrading to bcrypt/Argon2 for production)
2. **JWT Tokens**: Secure token-based authentication
3. **Permission-Based Authorization**: Fine-grained access control
4. **Soft Delete**: Users and roles are soft-deleted (IsDeleted flag)
5. **Token Expiration**: Configurable token expiration time

## Clean Architecture Benefits

1. **Separation of Concerns**: Each layer has a specific responsibility
2. **Testability**: Services can be easily unit tested
3. **Maintainability**: Changes in one layer don't affect others
4. **Repository Pattern**: Data access is abstracted
5. **Dependency Injection**: Loose coupling between components

## Next Steps

1. **Add Refresh Tokens**: Implement token refresh mechanism
2. **Password Policy**: Add password strength requirements
3. **Two-Factor Authentication**: Add 2FA support
4. **Audit Logging**: Log authentication and authorization events
5. **Rate Limiting**: Prevent brute force attacks
6. **Password Reset**: Implement password reset functionality

