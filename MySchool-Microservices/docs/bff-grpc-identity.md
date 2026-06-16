# BFF, gRPC, and Identity Architecture

## Overview

Angular talks only to **MySchool.WebBff** over HTTP. The BFF does not access the identity database.

| Layer | Protocol | Responsibility |
|-------|----------|----------------|
| Angular | HTTP → BFF | UI, JWT in `localStorage`, cookies for refresh |
| Web BFF | gRPC → IdentityService | Auth, users, roles; JSON shaping for frontend |
| Web BFF | HTTP (YARP) → Backend | All other `/api/*` school APIs |
| IdentityService | gRPC + internal HTTP | Users, roles, JWT, refresh tokens, identity DB |

## BFF folder structure (RESTful + gRPC)

```
bff/MySchool.WebBff/
  Abstracts/
    RestfulBffControllerBase.cs
    RestfulBffControllerBase.GetAll.cs
    RestfulBffControllerBase.GetById.cs
    RestfulBffControllerBase.GetPage.cs
    RestfulBffControllerBase.Command.Add.cs
    RestfulBffControllerBase.Command.Update.cs
    RestfulBffControllerBase.Command.Delete.cs
  Controllers/
    Identity/
      AuthController.cs       # /bff/auth/*
      UsersController.cs      # /bff/users (GetAll via base)
      RolesController.cs      # /bff/roles (GetAll via base)
  GrpcServices/
    IIdentityGrpcGateway.cs
    IdentityGrpcGateway.cs
  GrpcJsonConverters/
    ProtobufTimestampExtensions.cs
  Infrastructure/
    Auth/                     ClaimsPrincipalExtensions.cs
    Cookies/                  RefreshTokenCookieWriter.cs
  Protos/
    identity/v1/identity.proto
  Common/
    DTOs/
      CommonPageDtos.cs
      Identity/               Auth, Me, User, Role response DTOs + mappers
    Results/
      BffResults.cs
    Extensions/
      WebBffServiceCollectionExtensions.cs
      OpenApiProxy*.cs
  Program.cs
```

**Controllers** are thin: they call `IIdentityGrpcGateway` only. JSON shaping lives in `Common/DTOs`.  
**RestfulBffControllerBase** provides reusable `GetAll`, `GetById`, `GetPage`, `Add`, `Update`, `Delete` partials; derived controllers enable routes by overriding with HTTP verb attributes.

## HTTP endpoints (BFF)

| Method | Path | Auth | gRPC RPC |
|--------|------|------|----------|
| POST | `/bff/auth/login` | Anonymous | `Login` |
| POST | `/bff/auth/register` | Anonymous | `Register` |
| POST | `/bff/auth/refresh-token` | Cookie `refreshToken` | `RefreshToken` |
| GET | `/bff/auth/me` | Bearer JWT | `GetCurrentUser` |
| GET | `/bff/users` | Bearer JWT | `GetUsers` |
| GET | `/bff/roles` | Bearer JWT (ADMIN/MANAGER) | `GetRoles` |

## YARP (temporary)

Non-identity traffic is proxied to Backend:

- `/api/{**catch-all}` → `backend:8080`
- `/uploads/{**catch-all}` → `backend:8080`

Identity routes are **not** proxied:

- `/api/auth/*`
- `/api/users/*`
- `/api/roles/*`

## JWT flow

1. **Login / register** — BFF calls IdentityService gRPC; IdentityService creates JWT and refresh token.
2. **BFF** sets `refreshToken` HttpOnly cookie and returns access token JSON to Angular.
3. **Protected BFF routes** — BFF validates JWT using shared `JWT:*` settings (same secret as IdentityService and Backend).
4. **Proxied Backend routes** — Browser sends `Authorization: Bearer <token>`; YARP forwards headers to Backend.

## gRPC contract

Proto (BFF): `bff/MySchool.WebBff/Protos/identity/v1/identity.proto`  
Canonical (IdentityService): `services/IdentityService/MySchool.IdentityService/Protos/identity.proto`

Both projects must stay in sync when the contract changes.

Development gRPC URL: `http://localhost:8083` (dedicated HTTP/2 port; REST stays on `8082`).

**Why two ports?** Without TLS, Kestrel does not enable HTTP/2 on `Http1AndHttp2` cleartext endpoints. gRPC requires HTTP/2, so IdentityService listens on:
- `8082` — HTTP/1.1 (Swagger, internal REST)
- `8083` — HTTP/2 only (gRPC)

Docker: HTTP `8080`, gRPC `8081`.

## Docker Compose services

| Service | Host port | Notes |
|---------|-----------|-------|
| `myschool-webbff` | 8081 | Public entry point |
| `myschool-identityservice` | internal | gRPC + internal HTTP only |
| `backend` | internal | School monolith |
| `sqlserver` | 1433 | Shared SQL Server |

## Local development (without Docker)

```bash
# Terminal 1 — SQL Server (or Docker sqlserver only)
# Terminal 2 — IdentityService
dotnet run --project MySchool-Microservices/services/IdentityService/MySchool.IdentityService

# Terminal 3 — Backend
dotnet run --project Backend

# Terminal 4 — Web BFF
dotnet run --project MySchool-Microservices/bff/MySchool.WebBff

# Terminal 5 — Angular
cd MySchool-Microservices/frontend && ng serve
```

Ensure `appsettings.Development.json` URLs match your local ports (BFF → Identity gRPC on 8082, YARP → Backend on 5000).

## Test login through BFF

```bash
curl -X POST http://localhost:5001/bff/auth/login \
  -H "Content-Type: application/json" \
  -d "{\"userName\":\"admin\",\"password\":\"your-password\",\"userType\":\"ADMIN\"}"
```

Or via Angular after `ng serve` with `environment.bffUrl` pointing at the BFF.

## Remaining manual checks

- **Tenant selection** (`my-tenants`, `select-tenant`) — not yet exposed on BFF; multi-tenant login may need follow-up gRPC RPCs.
- **Logout** — not implemented on BFF yet.
- **User/role CRUD** — `RestfulBffControllerBase` supports Add/Update/Delete/GetById/GetPage; enable when IdentityService gRPC RPCs exist.
- **Role permissions admin** — IdentityService internal HTTP only (`/api/rolepermissions`).
- **Backend → Identity HTTP** — user CRUD and internal APIs remain on IdentityService HTTP for service-to-service use.
- **Proto sync** — keep `Protos/identity/v1/identity.proto` aligned with IdentityService proto when extending the contract.
