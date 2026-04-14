# Project & Issue Management API

A lightweight, production-style .NET Web API for managing Projects, Issues/Tasks, Comments, and Attachments with JWT authentication, filtering, and seeding.

- Target Framework: .NET 10 (net10.0)
- API Style: REST + OpenAPI (Scalar UI)/ Postman
- Data: SQL Server with EF Core migrations and automatic seeding
- Auth: JWT Bearer with seeded demo accounts and roles (Admin/Member)

## Contents

- Overview
- Features
- Tech Stack
- Solution Structure
- Quick Start
- Configuration
- Database & Seeding
- Authentication
- API Reference
  - Auth
  - Projects
  - Issues (per project + global)
  - Comments
  - Attachments
  - Filtering examples
- Testing
- Assumptions & Trade-offs
- Prompts Used (AI Assist)
- Roadmap / Next Steps
- GitHub Push (quick reference)

---

## Overview

Delivery teams often lose track of what’s active, who owns what, and current status when work is managed in chats/spreadsheets. WorkTrack Lite provides a clean backend to:

- Create and manage projects and their lifecycle
- Create, update, assign, and track issues with status/priority/due dates
- Collaborate via comments and file attachments
- Filter issues by project, status, priority, assignee, text, and dates
- Secure API access with JWT and role-based authorization
- Explore and test via OpenAPI (Scalar UI)/Postman client.


---

## Features

- Project management (CRUD; Admin-restricted for writes)
- Issue management per project (CRUD; Admin delete)
- Cross-project issue listing with filters
- Comments (add/list/delete with author-or-admin rule)
- File attachments (upload/list/delete with uploader-or-admin rule)
- JWT authentication with seeded Admin/Member users
- Global exception handling middleware and validation
- EF Core migrations + automatic database creation/seed on startup
- OpenAPI generation and Scalar API Reference in Development
- Unit/integration tests (xUnit, FluentAssertions, WebApplicationFactory)

---

## Tech Stack

- ASP.NET Core Web API (net10.0)
- Entity Framework Core (SqlServer, Sqlite for tests, InMemory in tests)
- JWT Bearer Authentication
- BCrypt for password hashing
- Scalar.AspNetCore (OpenAPI UI)
- xUnit, FluentAssertions, Moq, Microsoft.AspNetCore.Mvc.Testing
- Coverlet collector (code coverage)

Key packages (see ServiceApi.API.csproj):
- Microsoft.AspNetCore.Authentication.JwtBearer
- Microsoft.EntityFrameworkCore (Design, SqlServer, Sqlite, InMemory)
- BCrypt.Net-Next
- Dapper
- Scalar.AspNetCore

---
## Screenshots

<img width="221" height="419" alt="image" src="https://github.com/user-attachments/assets/effbc3a0-5090-45c3-ade8-223deedf9349" />
<img width="706" height="463" alt="image" src="https://github.com/user-attachments/assets/c5afbbbb-ba32-4ca6-a2e0-457c233ae104" />
<img width="448" height="465" alt="image" src="https://github.com/user-attachments/assets/fa396ac1-9283-4686-8f4a-337c0d92c40a" />
<img width="717" height="436" alt="image" src="https://github.com/user-attachments/assets/73db12f8-ff90-4f67-ae91-5232f9d357b1" />
<img width="701" height="281" alt="image" src="https://github.com/user-attachments/assets/826d0e42-37d7-4fc3-b110-f9e61324bcd9" />
<img width="719" height="326" alt="image" src="https://github.com/user-attachments/assets/9b27f104-60f0-4ab0-860d-55061854b5ba" />


## Solution Structure

```
ServiceApi.API/
├─ Controllers/
│  ├─ AuthController.cs
│  ├─ ProjectsController.cs
│  ├─ IssuesController.cs            
│  ├─ CommentsController.cs
│  └─ AttachmentsController.cs
├─ Data/
│  ├─ AppDbContext.cs
│  └─ SeedData.cs                    
├─ DTOs/
│  └─ Dtos.cs                        
├─ Middlewares/
│  └─ ExceptionMiddleware.cs
├─ Migrations/                       
├─ Models/
├─ Repositories/
├─ Services/
├─ wwwroot/uploads/                  
├─ Program.cs                     
├─ appsettings.json                
└─ ServiceApi.API.csproj

ServiceApi.Tests/
├─ Controllers/ProjectsControllerTests.cs
├─ Infrastructure/TestAuthHandler.cs
├─ Services/ProjectServiceTests.cs
└─ ServiceApi.Tests.csproj
```

---

## Quick Start

Prerequisites:
- .NET SDK 10 (net10.0). If not available on your machine, you can retarget to `net8.0` quickly in both .csproj files.
- SQL Server running locally (e.g., Developer/Express). The default connection string uses `Server=localhost`.

Run:
```
# from repo root
dotnet restore
dotnet build

# run the API
dotnet run --project ServiceApi.API
```

Default launch URLs (Development):
- HTTP: http://localhost:5174
- HTTPS: https://localhost:7165

OpenAPI:
- JSON: /openapi/v1.json (Development)
- Scalar UI: /scalar/v1 (Development)

Example:
- https://localhost:7165/scalar/v1

---

## Configuration

Edit `ServiceApi.API/appsettings.json`:

```
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=WorkTrackDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "WorkTrackLite-SuperSecret-Key-32chars!!",
    "Issuer": "WorkTrackLite",
    "Audience": "WorkTrackLiteUsers"
  },
  "AllowedHosts": "*"
}
```

---

## Database & Seeding

- On startup, the app runs `db.Database.MigrateAsync()` and `DbSeeder.SeedAsync(db)`.
- The initial migration exists, so the DB schema will be created/updated automatically.
- Seeding runs only if there are no users; it inserts:
  - Admin: username `admin`, password `Admin@123`, role `Admin`
  - Member: username `member1`, password `Member@123`, role `Member`
- A sample project “WorkTrack Demo” and a couple of issues/comments are created.

No manual migration command is required for local run. If needed:
```
# optional dev workflow
dotnet tool install --global dotnet-ef
dotnet ef database update --project ServiceApi.API
```

---

## Authentication

Login to receive a JWT, then include it in `Authorization: Bearer {token}` for secured endpoints.

Sample credentials:
- Admin: admin / Admin@123
- Member: member1 / Member@123

Login:
```
POST /api/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "Admin@123"
}
```

Response:
```
200 OK
{
  "token": "<jwt-here>",
  "username": "admin",
  "role": "Admin"
}
```

Register (open for demo):
```
POST /api/auth/register
{
  "username": "alice",
  "email": "alice@example.com",
  "password": "Pass@12345",
  "role": "Member"
}
```

---

## API Reference

Base path:
- https://localhost:7165 (HTTPS)
- http://localhost:5174 (HTTP)

OpenAPI UI: /scalar/v1

### Projects (secured; Admin required for writes)
- GET /api/projects — list all projects
- GET /api/projects/{id} — project details
- POST /api/projects — create (Admin)
  ```
  {
    "name": "My Project",
    "description": "Optional"
  }
  ```
- PUT /api/projects/{id} — update (Admin)
  ```
  {
    "name": "Renamed",
    "description": "Updated",
    "status": "Active"  // Active/Inactive (enum as string)
  }
  ```
- DELETE /api/projects/{id} — delete (Admin)

cURL example:
```
curl -H "Authorization: Bearer %TOKEN%" https://localhost:7165/api/projects
```

### Issues (per project; secured)
Route prefix: /api/projects/{projectId}/issues

- GET /api/projects/{projectId}/issues — list with filters
  - Query: `status`, `priority`, `assigneeId`, `search`, `dueBefore`
- GET /api/projects/{projectId}/issues/{id} — details
- POST /api/projects/{projectId}/issues
  ```
  {
    "title": "Write unit tests",
    "description": "Focus on services",
    "priority": "High",        // Low/Medium/High
    "assigneeId": 2,           // optional
    "dueDate": "2026-05-01"    // optional
  }
  ```
- PUT /api/projects/{projectId}/issues/{id}
  ```
  {
    "title": "Updated title",
    "description": "Updated description",
    "status": "InProgress",    // Open/InProgress/Resolved/Closed (by design)
    "priority": "Medium",
    "assigneeId": 2,
    "dueDate": "2026-05-10"
  }
  ```
- DELETE /api/projects/{projectId}/issues/{id} — Admin only

### Global Issues (secured)
- GET /api/issues — list all issues across projects with the same filters

Examples:
```
# All Open, High priority issues
GET /api/issues?status=Open&priority=High

# Issues assigned to user 2 containing 'unit'
GET /api/issues?assigneeId=2&search=unit
```

### Comments (secured)
Route prefix: /api/issues/{issueId}/comments

- GET /api/issues/{issueId}/comments — list comments
- POST /api/issues/{issueId}/comments
  ```
  { "content": "Great progress!" }
  ```
- DELETE /api/issues/{issueId}/comments/{id} — author or Admin

### Attachments (secured; multipart/form-data)
Route prefix: /api/issues/{issueId}/attachments

- GET /api/issues/{issueId}/attachments — list attachments
- POST /api/issues/{issueId}/attachments/upload — upload file (multipart/form-data)
- DELETE /api/issues/{issueId}/attachments/{id} — uploader or Admin

cURL upload example (PowerShell escaping may vary):
```
curl -X POST "https://localhost:7165/api/issues/1/attachments/upload" ^
  -H "Authorization: Bearer %TOKEN%" ^
  -H "Content-Type: multipart/form-data" ^
  -F "file=@.\sample.pdf"
```

Files are stored under `wwwroot/uploads/` and served via static files. The API returns metadata including `FileName` and `FilePath`.

---

## Filtering Examples

Issue filter DTO supports:
- projectId (only in global list)
- status
- priority
- assigneeId
- search (in title)
- dueBefore

Examples:
```
GET /api/projects/1/issues?status=InProgress&dueBefore=2026-06-01
GET /api/issues?assigneeId=2&priority=High
GET /api/issues?search=pipeline
```

---

## Testing

Run tests:
```
dotnet test
```

Included:
- Controller tests: ProjectsController basic behaviors
- Service tests: ProjectService core behaviors
- Test infrastructure: TestAuthHandler for authenticated scenarios
- Coverage: `coverlet.collector` is enabled; to collect coverage in TRX:
  ```
  dotnet test --collect:"XPlat Code Coverage"
  # Results in TestResults/*.trx and coverage file(s)
  ```

---

## Assumptions & Trade-offs

- Admin-only writes for Projects and destructive operations (delete Issue/Attachment/Comment as noted).
- Registration endpoint is left open for demo convenience; would be Admin-only or invited in production.
- JWT uses symmetric key from `appsettings.json` for local dev; use secure secrets and rotation strategies in real deployments.
- Attachments are saved to local `wwwroot/uploads`. Production would use durable object storage (e.g., Azure Blob/S3) and virus scanning.
- Basic validation is implemented via DataAnnotations on DTOs; more advanced rules can be added in services.
- Status/Priority treated as enums serialized to strings for readability.
- Targeting net10.0 as per project file; if not available locally, switch to net8.0 for compatibility.

---

## Prompts Used (AI Assist)

Representative prompts that guided the implementation within the timebox:
- “Design a minimal clean structure for a .NET Web API for projects/issues/comments/attachments with JWT and EF Core. Propose folders, DTOs, and services.”
- “Create EF Core entities and relationships for User, Project, Issue, Comment, Attachment with enums for Status/Priority.”
- “Add JWT authentication with seeded users and roles Admin/Member; expose login/register endpoints and secure controllers appropriately.”
- “Implement IssuesController with nested routing under projects, including filters, assignment, and validation rules.”
- “Add file upload endpoint that saves to wwwroot/uploads and returns metadata, plus static file hosting.”
- “Write xUnit tests for ProjectService and ProjectsController using WebApplicationFactory.”
- “Generate OpenAPI/Scalar setup for Development and show how to pass Bearer token in requests.”
- "Implement Pagination for the Get endpoint when results are filtered as query params."

Note: These summarize the kinds of prompts used to accelerate development and documentation.

---

## Roadmap / Next Steps

- Rich search across title/description/comments
- Refresh tokens & role management endpoints
- Soft delete and audit logs
- CI pipeline (GitHub Actions) and deployment scripts
- More comprehensive tests and coverage thresholds

---

## License

Assessment submission — no explicit license declared. Add one if publishing publicly.
