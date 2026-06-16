# Identity Service

ASP.NET Core 8 microservice owning authentication, users, roles, permissions, and JWT issuance.

## Responsibilities

| Area | Endpoints |
|------|-----------|
| Authentication | `POST /api/auth/login`, `register`, `refresh`, `logout` |
| Tenant selection | `GET /api/auth/my-tenants`, `POST /api/auth/select-tenant` |
| Public registration | `GET /api/auth/PublicSchools`, `POST RequestRegistration`, `PendingRequests`, `ApproveRequest`, `RejectRequest` (proxied to monolith internal API) |
| Users | `GET/POST/PUT/DELETE /api/users/*` (writes require internal API key) |
| Roles | `GET /api/roles` |
| Permissions | `GET/PUT /api/rolepermissions/matrix` |

## Database

`IdentityDbContext` maps to the shared `MySchool` SQL Server database (same server as monolith master DB):

- `AspNetUsers`, `AspNetRoles`, and related Identity tables
- `RefreshTokens`
- `Permissions`, `RolePermissions`

The monolith `DatabaseContext` no longer maps Identity tables.

## Monolith integration

Identity calls the monolith over HTTP (`MonolithService:BaseUrl`) with header `X-Internal-Service-ApiKey`:

| Monolith endpoint | Purpose |
|-------------------|---------|
| `POST /api/internal/auth/login-enrichment` | School/tenant data for login JWT |
| `GET /api/internal/auth/tenant-summaries/{userId}` | Multi-school picker |
| `GET /api/internal/auth/school-role/{userId}/{tenantId}` | Permission claim resolution |
| `api/internal/registration/*` | Registration workflow |

The monolith calls Identity via `IdentityUserApiClient` for user CRUD during student enrollment and registration approval.

## JWT

Both services validate tokens using **identical** configuration:

```json
"JWT": {
  "SecretKey": "...",
  "IssuerIP": "http://localhost:7258/",
  "AudienceIP": "http://localhost:4200/"
}
```

In Docker Compose, override with `JWT_SECRET_KEY` environment variable on `identityservice` and `monolithservice`.

## Local development

```bash
# Terminal 1 — SQL Server (or use existing instance on 1433)
# Terminal 2 — Monolith
cd Backend && dotnet run

# Terminal 3 — Identity (port 8082)
cd MySchool-Microservices/services/IdentityService/MySchool.IdentityService && dotnet run

# Terminal 4 — Gateway (port 5001)
cd MySchool-Microservices/gateway/MySchool.Gateway && dotnet run

# Terminal 5 — Frontend
cd MySchool-Microservices/frontend && npx ng serve
```

Set `MonolithService:BaseUrl` to `http://localhost:5000` in Identity `appsettings.Development.json`.

## Docker

From repository root:

```bash
docker compose up -d --build
```

Gateway: `http://localhost:8081/api/auth/login`

## Shared contracts

DTOs and authorization constants live in `MySchool-Microservices/shared/MySchool.Contracts/`.  
JWT wiring extension: `MySchool.BuildingBlocks/JwtAuthenticationExtensions.cs`.
