# WorkTrack Lite – Architecture Diagrams (Mermaid)

These Mermaid blocks are simplified to be GitHub-safe (no inline comments in ERD fields, no special characters in flow labels). Paste them directly in GitHub Markdown or any Mermaid renderer.

## 1) Entity Relationship Diagram (ERD)

```mermaid
erDiagram
    USER {
      int Id PK
      string Username
      string Email
      string PasswordHash
      string Role
      datetime CreatedOn
    }

    PROJECT {
      int Id PK
      string Name
      string Description
      string Status
      datetime CreatedOn
      datetime UpdatedOn
    }

    ISSUE {
      int Id PK
      int ProjectId FK
      string Title
      string Description
      string Status
      string Priority
      int ReporterId FK
      int AssigneeId FK
      datetime DueDate
      datetime CreatedOn
      datetime UpdatedOn
    }

    COMMENT {
      int Id PK
      int IssueId FK
      int AuthorId FK
      string Content
      datetime CreatedOn
    }

    ATTACHMENT {
      int Id PK
      int IssueId FK
      string FileName
      string FilePath
      string ContentType
      int UploadedById FK
      datetime UploadedOn
    }

    PROJECT ||--o{ ISSUE : contains
    USER ||--o{ ISSUE : reports
    USER ||--o{ ISSUE : assigned_to
    ISSUE ||--o{ COMMENT : has
    USER ||--o{ COMMENT : writes
    ISSUE ||--o{ ATTACHMENT : has
    USER ||--o{ ATTACHMENT : uploads
```

Notes:
- Status and Priority are enums represented as strings in API responses.
- AssigneeId and DueDate may be null in the database model, represented here without nullability to keep Mermaid syntax simple.

---

## 2) Request Lifecycle (High Level)

```mermaid
flowchart LR
    Client --> API
    API --> Auth
    Auth --> Controller
    Controller --> Service
    Service --> Repository
    Repository --> Database
    Repository --> Uploads

    Uploads[wwwroot/uploads]
    API[API Endpoint]
    Auth[JWT Auth]
    Controller[Controllers]
    Service[Services]
    Repository[Repositories]
    Database[(SQL Server)]
```

---

## 3) Auth: Login Flow (Sequence)

```mermaid
sequenceDiagram
    participant Client
    participant AuthController
    participant AuthService
    participant UserRepo

    Client->>AuthController: POST /api/auth/login
    AuthController->>AuthService: LoginAsync
    AuthService->>UserRepo: Find by username
    UserRepo-->>AuthService: User + PasswordHash
    AuthService-->>AuthController: LoginResponse (JWT, username, role)
    AuthController-->>Client: 200 OK
```

---

## 4) Projects: List with Sorting and Paging

```mermaid
flowchart TD
    Client --> ProjectsController
    ProjectsController --> ProjectService
    ProjectService --> ProjectRepository
    ProjectRepository --> Database
    ProjectsController --> SortAndPage

    SortAndPage[Apply sorting and paging]
    Database[(SQL Server)]
```

- SortBy: createdOn, name, status, issueCount
- SortDir: asc, desc
- PageNumber default 1, PageSize default 20 (min 1, max 100)

---

## 5) Issues: Create

```mermaid
flowchart TD
    Client --> IssuesController_Create[IssuesController Create]
    IssuesController_Create --> ValidateProject[Validate project exists]
    IssuesController_Create --> ValidateAssignee[Validate assignee optional]
    IssuesController_Create --> IssueService_Create[IssueService CreateAsync]
    IssueService_Create --> IssueRepository_Add[Add + SaveChanges]
    IssueRepository_Add --> Database

    Database[(SQL Server)]
```

---

## 6) Issues: Filter, Sort, Paginate (Project Scoped)

```mermaid
flowchart TD
    Client --> IssuesController_List[IssuesController GetAll]
    IssuesController_List --> IssueService_List[IssueService GetAllByProjectAsync]
    IssueService_List --> IssueRepository_List[IssueRepository GetAllByProjectAsync]
    IssueRepository_List --> ApplyFilters[Apply filters]
    ApplyFilters --> ApplySorting[Apply sorting]
    ApplySorting --> ApplyPaging[Apply paging]
    ApplyPaging --> Database

    Database[(SQL Server)]
```

- Filters: status, priority, assigneeId, search, dueBefore
- SortBy: createdOn, updatedOn, dueDate, priority, status, title, assigneeName
- Pagination: pageNumber, pageSize

---

## 7) Issues: Global Listing

```mermaid
flowchart TD
    Client --> AllIssuesController[All issues controller]
    AllIssuesController --> IssueService_Global[IssueService GetAllGlobalAsync]
    IssueService_Global --> IssueRepository_Global[IssueRepository GetAllGlobalAsync]
    IssueRepository_Global --> ApplyFiltersGlobal[Apply filters]
    ApplyFiltersGlobal --> ApplySortingGlobal[Apply sorting]
    ApplySortingGlobal --> ApplyPagingGlobal[Apply paging]
    ApplyPagingGlobal --> Database

    Database[(SQL Server)]
```

---

## 8) Comments: Add and Delete

```mermaid
flowchart LR
    Client --> Comments_Create[CommentsController Create]
    Comments_Create --> CommentService_Create[CommentService CreateAsync]
    CommentService_Create --> CommentRepository_Save[SaveChanges]
    CommentRepository_Save --> Database

    Client --> Comments_Delete[CommentsController Delete]
    Comments_Delete --> CommentService_Delete[CommentService DeleteAsync]
    CommentService_Delete --> CheckAuthorAdmin[Check author or admin]
    CheckAuthorAdmin --> CommentRepository_Remove[Remove + SaveChanges]
    CommentRepository_Remove --> Database

    Database[(SQL Server)]
```

---

## 9) Attachments: Upload and Delete

```mermaid
flowchart TD
    Client --> Attachments_Upload[AttachmentsController Upload]
    Attachments_Upload --> AttachmentService_Upload[AttachmentService UploadAsync]
    AttachmentService_Upload --> ValidateIssue[Validate issue exists]
    AttachmentService_Upload --> SaveFile[Save file to wwwroot/uploads]
    AttachmentService_Upload --> AttachmentRepository_Save[Persist metadata]
    AttachmentRepository_Save --> Database

    Client --> Attachments_Delete[AttachmentsController Delete]
    Attachments_Delete --> AttachmentService_Delete[AttachmentService DeleteAsync]
    AttachmentService_Delete --> CheckUploaderAdmin[Check uploader or admin]
    AttachmentService_Delete --> AttachmentRepository_Remove[Remove + SaveChanges]
    AttachmentRepository_Remove --> Database

    Database[(SQL Server)]
```

---

## 10) Error Handling and Auth Pipeline

```mermaid
flowchart LR
    Request --> ExceptionMiddleware
    ExceptionMiddleware --> Authentication
    Authentication --> Authorization
    Authorization --> MapControllers
    MapControllers --> Response

    Request[Request]
    Authentication[JWT Authentication]
    Authorization[Authorization]
    MapControllers[Route to controllers]
    Response[Response]
