PulseDesk API

A Help Desk Ticket Management System built with ASP.NET Core, Entity Framework Core, and MySQL.

---

## Tech Stack

| Layer            | Technology              |
| ---------------- | ----------------------- |
| Framework        | ASP.NET Core 10         |
| ORM              | Entity Framework Core 9 |
| Database         | MySQL 9 (Docker)        |
| Authentication   | JWT Bearer Tokens       |
| Password Hashing | BCrypt.Net              |
| API Docs         | Swagger / Swashbuckle   |

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/products/docker-desktop)
- [MySQL Workbench](https://www.mysql.com/products/workbench/) (optional)

### 1. Clone the repository

```bash
git clone https://github.com/FayaadAbrahams/PulseDesk.git
cd PulseDesk/server/PulseDesk
```

### 2. Start MySQL with Docker

```bash
docker run --name pulsedesk-mysql \
  -e MYSQL_ROOT_PASSWORD=yourpassword \
  -e MYSQL_DATABASE=pulsedesk_schema \
  -p 3306:3306 \
  -d mysql:8
```

### 3. Configure the app

Create `appsettings.json` in the project root:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=127.0.0.1;Port=3306;Database=pulsedesk_schema;User=root;Password=yourpassword"
  },
  "JwtSettings": {
    "Secret": "YourSuperSecretKeyMinimum32CharactersLong",
    "ExpiryDays": 7
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

> DO NOT commit `appsettings.json` to source control. Add it to `.gitignore`.

### 4. Run the API

```bash
dotnet restore
dotnet run
```

Visit `http://localhost:5286/swagger` to explore the API.

---

## Project Structure

```
PulseDesk/
├── Controllers/
│   ├── BaseController.cs        # Shared claims helpers
│   ├── AuthController.cs        # Register, Login
│   ├── TicketsController.cs     # Ticket CRUD
│   ├── CommentsController.cs    # Ticket comments
│   └── DashboardController.cs   # Admin/Agent analytics
├── Data/
│   └── AppDbContext.cs          # EF Core database context
├── DTOs/
│   ├── Auth/                    # RegisterRequest, LoginRequest, AuthResponse
│   ├── Tickets/                 # CreateTicketRequest, UpdateTicketRequest, TicketResponse
│   └── Comments/                # CreateCommentRequest, CommentResponse
├── Models/
│   ├── User.cs
│   ├── Ticket.cs
│   ├── Comment.cs
│   ├── AuditLog.cs
│   └── Enums/
│       ├── UserRole.cs          # Customer, Agent, Admin
│       ├── StatusType.cs        # Open, InProgress, Resolved, Closed
│       └── PriorityType.cs      # Low, Medium, High
└── Program.cs                   # App entry point and middleware
```

---

## Database Schema


---

## API Endpoints

### Auth

|Method|Endpoint|Description|Auth|
|---|---|---|---|
|POST|`/api/auth/register`|Register a new user|Public|
|POST|`/api/auth/login`|Login and receive JWT token|Public|

### Tickets

|Method|Endpoint|Description|Auth|
|---|---|---|---|
|GET|`/api/tickets`|Get all tickets (customers see own only)|All roles|
|GET|`/api/tickets/{id}`|Get ticket by ID|All roles|
|POST|`/api/tickets`|Create a new ticket|Customer|
|PUT|`/api/tickets/{id}`|Update ticket status/priority/agent|Agent, Admin|
|DELETE|`/api/tickets/{id}`|Delete a ticket|Admin|

### Comments

|Method|Endpoint|Description|Auth|
|---|---|---|---|
|GET|`/api/tickets/{ticketId}/comments`|Get all comments for a ticket|All roles|
|POST|`/api/tickets/{ticketId}/comments`|Add a comment to a ticket|All roles|
|DELETE|`/api/tickets/{ticketId}/comments/{id}`|Delete a comment|Admin|

### Dashboard

|Method|Endpoint|Description|Auth|
|---|---|---|---|
|GET|`/api/dashboard/stats`|Ticket counts by status and priority|Agent, Admin|
|GET|`/api/dashboard/agent-workload`|Per agent ticket breakdown|Agent, Admin|
|GET|`/api/dashboard/recent-activity`|Recent tickets and audit trail|Agent, Admin|
|GET|`/api/dashboard/users`|All users with activity stats|Admin|

---

## Authentication

PulseDesk uses JWT Bearer tokens. After logging in, include the token in all requests:

```
Authorization: Bearer dhjeauihauihdAWIYUDgaWYeyuawfaAd018923dhabwdaWDawd...
```

### Roles

|Role|Permissions|
|---|---|
|Customer|Create tickets, view own tickets, add comments|
|Agent|View all tickets, update status, assign self, view dashboard|
|Admin|Full access, manage users, delete tickets/comments, view all dashboard data|

---

## Ticket Status Flow

```
Open → InProgress → Resolved → Closed
```

Tickets are created as `Open` by default. Only Agents and Admins can update the status.

---

## Security

- Passwords are hashed with **BCrypt** — plain text passwords are never stored
- JWT tokens are signed with **HmacSha256**
- Role-based access control is enforced at the endpoint level with `[Authorize(Roles)]`
- Soft deletes on users — accounts are deactivated not deleted, preserving ticket history
- Least privilege database user recommended for production

---

## Design Decisions

**Why EF Core?** Removes boilerplate database code, keeps schema version controlled, and protects against SQL injection by default.

**Why JWT?** Server doesn't need to store session data. User identity and role are baked into the token, avoiding database calls on every request.

**Why soft deletes?** Deleting a user would cut all their tickets and comments. Setting `IsActive = false` keeps the full history and prevents issues if the decision was to lose all tickets, as a business owner it makes more sense to cover your own .

**Why AuditLogs? What is the Audit Log?** 
Every status, priority and agent change is recorded with before/after values. This gives us a logged experience of everything that happened to a ticket. 

---

## Environment Variables

For production, use environment variables instead of `appsettings.json`:

```bash
ConnectionStrings__DefaultConnection="Server=...;Database=...;User=...;Password=..."
JwtSettings__Secret="YourProductionSecret"
JwtSettings__ExpiryDays="7"
```

---

## Packages

```xml
    <PackageReference Include="BCrypt.Net-Next" Version="4.1.0" />
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.0.0" />
    <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="9.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.0">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="Microsoft.EntityFrameworkCore.Relational" Version="9.0.0" />
    <PackageReference Include="Microsoft.IdentityModel.Tokens" Version="8.0.1" />
    <PackageReference Include="Microsoft.OpenApi" Version="1.6.22" />
    <PackageReference Include="Pomelo.EntityFrameworkCore.MySql" Version="9.0.0" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
```

---

## License

MIT
