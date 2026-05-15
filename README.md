Task Management API (ASP.NET Core .NET 8)



A RESTful, multi-tenant Task Management API built with ASP.NET Core (.NET 8), Entity Framework Core, and Swagger. The system enforces strict business rules, supports soft deletion, and isolates data per tenant using request headers.


Overview

This API provides CRUD operations for managing TaskItem entities in a multi-tenant environment. Each request must include a tenant identifier, and all business logic is enforced at the service layer to ensure consistency and rule compliance.


Key features:

Multi-tenant isolation via X-Tenant-Id header
Soft delete support
Strong domain validation rules
Status transition enforcement
Swagger documentation
Clean layered architecture

Tech Stack
ASP.NET Core Web API (.NET 8 LTS)
Entity Framework Core
SQLite (configurable)
Swagger


Project Structure

A typical clean architecture-style layout:

/Controllers        -> API endpoints
/Services           -> Business logic & rules
/Repositories       -> Data access layer (EF Core abstraction)
/Data               -> DbContext & configurations
/Models             -> Domain entities
/DTOs               -> Request/Response contracts
/Middleware        -> Tenant resolution, validation, error handling


Entity: TaskItem
Id (Guid)
Title (string)
Description (string)
Status (Pending | InProgress | Completed | Cancelled)
Priority (Low | Medium | High)
DueDate (DateTime?)
AssignedTo (string)
CreatedAt (DateTime)
UpdatedAt (DateTime)
IsDeleted (bool)
TenantId (string)
Multi-Tenancy Design
Approach: Header-based Tenant Isolation

Every request must include:

X-Tenant-Id: <tenant-id>
Enforcement Strategy:
Middleware extracts X-Tenant-Id

Why this approach:
Simple and stateless
No authentication dependency required
Easy to extend to JWT-based tenant claims later
Business Rules Implementation

All rules are enforced in the service layer, not controllers or database constraints.

1. Tenant Header Required
Requests without X-Tenant-Id are rejected with 400 Bad Request
2. Soft Delete Rules
IsDeleted = true instead of physical deletion
Deleted tasks are excluded via global query filter
3. Completed Task Restriction
Completed tasks cannot be deleted
Attempt returns 409 Conflict
4. Status Transition Rules
Completed → InProgress is not allowed
Enforced during update operations
5. Priority Rule
If Priority = High, then DueDate is required
Validation occurs before persistence
6. Cancelled Task Restriction
Cancelled tasks are read-only
No updates allowed except read operations
Data Access Strategy


Typical endpoints:

GET    /api/tasks
GET    /api/tasks/{id}
POST   /api/tasks
PUT    /api/tasks/{id}
DELETE /api/tasks/{id}
Notes:
All endpoints require X-Tenant-Id
Responses are tenant-scoped automatically
Swagger UI available at /swagger
Error Handling Strategy

Centralized exception handling middleware:

Validation errors → 400 Bad Request
Business rule violations → 409 Conflict
Missing tenant → 400 Bad Request
Not found → 404 Not Found

Standard response format:

{
  "error": "Message",
  "details": "Optional details"
}
Design Decisions
1. Service Layer for Business Rules

Business rules are intentionally kept out of controllers to:

Improve testability
Enforce separation of concerns
Allow reuse across endpoints or background jobs
2. Soft Delete Instead of Hard Delete

Chosen to:

Preserve audit history
Prevent accidental data loss
Support future restore functionality
3. Middleware-based Tenant Resolution

Instead of passing tenant manually:

Ensures consistent enforcement
Reduces developer error risk
Keeps controllers clean
4. EF Core Global Filters

Used for:

Automatic tenant isolation
Preventing accidental cross-tenant leaks
Reducing repetitive query conditions
Validation Strategy

Two-layer validation:

DTO validation (FluentValidation or DataAnnotations)
Required fields
Format constraints
Domain validation (Service layer)
Business rules
State transitions
Cross-field constraints
Running the Project

1. Configure Database

Update appsettings.json:

"ConnectionStrings": {
  "DefaultConnection": "Data Source=taskmanagement.db"
}
2. Apply Migrations
dotnet ef migrations add InitialCreate --project src/TaskManagement.Infrastructure --startup-project src/TaskManagement.Api
dotnet ef database update --project src/TaskManagement.Infrastructure --startup-project src/TaskManagement.Api
3. Run Application
dotnet run
4. Open Swagger
https://localhost:<port>/swagger
Testing Approach (Suggested)
Unit tests for service layer (business rules)
Integration tests for API endpoints
In-memory database for fast test execution
Future Improvements
JWT-based authentication with tenant claims
Role-based access control (RBAC)
Audit logging per tenant
Background job processing for overdue tasks
Event-driven architecture (task state changes)
