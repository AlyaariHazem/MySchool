# IdentityService — Vertical Slice Architecture

## Layout

```
MySchool.IdentityService/
  Protos/
    identity.proto          # gRPC contract
  Grpc/
    IdentityGrpcService.cs    # Thin adapter (namespace: GrpcServices)
  Features/
    Auth/
      Login/
        LoginCommand.cs
        LoginResponse.cs
        LoginHandler.cs
      Register/
        RegisterCommand.cs
        RegisterResponse.cs
        RegisterHandler.cs
      RefreshToken/
        RefreshTokenCommand.cs
        RefreshTokenResponse.cs
        RefreshTokenHandler.cs
      GetCurrentUser/
        GetCurrentUserQuery.cs
        GetCurrentUserResponse.cs
        GetCurrentUserHandler.cs
    Users/
      GetUsers/
        GetUsersQuery.cs
        GetUsersResponse.cs
        GetUsersHandler.cs
    Roles/
      GetRoles/
        GetRolesQuery.cs
        GetRolesResponse.cs
        GetRolesHandler.cs
  Services/                   # Shared infrastructure (not feature logic)
    JwtTokenFactory.cs
    RefreshTokenService.cs
    UserClaimsBuilder.cs
    PermissionClaimService.cs
    MonolithIntegrationClient.cs
  Controllers/                # Internal HTTP for Backend integration
  Data/
  Entities/
```

## Principles

1. **Business logic lives in feature handlers** — not in controllers or `IdentityGrpcService`.
2. **gRPC service is an adapter** — maps proto messages to commands/queries, invokes handler, maps response.
3. **HTTP controllers** remain for Backend service-to-service calls (user CRUD, role permissions, tenant helpers). Shared read paths delegate to the same handlers where applicable (`GetUsers`, `GetRoles`, login/register/refresh).
4. **One feature per folder** — command/query, response, handler; validators can be added per feature when rules grow.

## Handler registration

`FeatureServiceCollectionExtensions.AddIdentityFeatures()` registers all handlers and shared token/claims services in `Program.cs`.

## Adding a new identity capability

1. Add RPC to `Protos/identity.proto`.
2. Create `Features/<Area>/<Name>/` with command, response, handler.
3. Register handler in `FeatureServiceCollectionExtensions`.
4. Add thin mapping in `Grpc/IdentityGrpcService.cs`.
5. Add BFF HTTP endpoint that calls the gRPC client (if exposed to Angular).

## gRPC server

Configured in `Program.cs`:

```csharp
builder.Services.AddGrpc();
builder.Services.AddIdentityFeatures();
// ...
app.MapGrpcService<IdentityGrpcService>();
```

Kestrel uses `Http1AndHttp2` so gRPC and internal HTTP share the same port.
