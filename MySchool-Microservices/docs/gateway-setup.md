# API Gateway Setup

The **MySchool.Gateway** project is a YARP reverse proxy that sits in front of the existing monolith. It forwards requests without changing business logic or API contracts.

## Architecture

```
Browser (Angular :4200)
        │
        ▼
 MySchool.Gateway (:5001 local / :8081 Docker)
        │
        ▼
 MonolithService (Backend :7258 local / :8080 Docker internal)
        │
        ▼
 SQL Server
```

## YARP routes

Routes are defined in `gateway/MySchool.Gateway/appsettings.json`. More specific routes are evaluated first (`Order`).

| Route ID | Path pattern | Destination |
|----------|--------------|-------------|
| `auth-route` | `/api/auth/{**catch-all}` | Monolith |
| `users-route` | `/api/users/{**catch-all}` | Monolith |
| `roles-route` | `/api/roles/{**catch-all}` | Monolith |
| `api-route` | `/api/{**catch-all}` | Monolith |
| `uploads-route` | `/uploads/{**catch-all}` | Monolith (static files) |
| `swagger-route` | `/swagger/{**catch-all}` | Monolith (Swagger UI) |

All routes target the `monolith` cluster.

## Configuration

### Local development

| File | Monolith address |
|------|------------------|
| `appsettings.json` | `http://monolithservice:8080/` (Docker default) |
| `appsettings.Development.json` | `http://localhost:5000/` (local monolith HTTP profile) |

Gateway listens on **http://localhost:5001** (`Properties/launchSettings.json`).

### Docker Compose

Environment variables override the cluster address:

```yaml
ReverseProxy__Clusters__monolith__Destinations__monolithservice__Address: "http://monolithservice:8080/"
```

The monolith is **not** published to the host; only the gateway is exposed on port **8081**.

### CORS

The gateway allows `http://localhost:4200` so the Angular dev server can call the API through the gateway. Override with:

```json
"Cors": {
  "AllowedOrigins": [ "http://localhost:4200" ]
}
```

Or environment variable `Cors__AllowedOrigins__0`.

## Running locally

### Terminal 1 — Monolith

```bash
cd Backend
dotnet run
```

### Terminal 2 — Gateway

```bash
cd MySchool-Microservices/gateway/MySchool.Gateway
dotnet run
```

Verify: `curl http://localhost:5001/health`

### Terminal 3 — Frontend

```bash
cd MySchool-Microservices/frontend
npx ng serve
```

`environment.development.ts` uses `baseUrl: 'http://localhost:5001/api'`.

## Running with Docker

From the repository root:

```bash
docker compose up -d --build
```

Or from `MySchool-Microservices/`:

```bash
docker compose up -d --build
```

| Endpoint | URL |
|----------|-----|
| Gateway API | http://localhost:8081/api |
| Gateway health | http://localhost:8081/health |
| Swagger | http://localhost:8081/swagger |
| Angular | http://localhost:4200 |

Frontend uses the `docker` Angular configuration (`environment.docker.ts` → `http://localhost:8081/api`).

## Health check

```
GET /health
```

Returns `{ "status": "healthy", "service": "MySchool.Gateway" }`.

## Adding future services

When Identity or other services are extracted, add routes **before** the catch-all `api-route` with a lower `Order` value, for example:

```json
"identity-auth-route": {
  "ClusterId": "identity",
  "Order": 1,
  "Match": { "Path": "/api/auth/login" }
}
```

Keep the monolith catch-all until each controller group is migrated.

## Troubleshooting

| Issue | Check |
|-------|--------|
| 502 from gateway | Monolith running? Development: `https://localhost:7258`. Docker: `docker compose logs monolithservice` |
| CORS errors | Frontend must use gateway URL, not monolith directly |
| SSL errors in dev | Gateway uses HTTP `:5001`; monolith can stay HTTPS — YARP forwards to `https://localhost:7258` |
| Cookies / auth | Gateway forwards `Authorization` header; ensure JWT is sent to gateway base URL |

## Related files

| File | Purpose |
|------|---------|
| `gateway/MySchool.Gateway/Program.cs` | YARP + CORS registration |
| `gateway/MySchool.Gateway/appsettings.json` | Routes and Docker cluster |
| `gateway/MySchool.Gateway/Dockerfile` | Gateway container image |
| `docker-compose.yml` (repo root) | Full stack with gateway |
| `MySchool-Microservices/docker-compose.yml` | Same stack, paths relative to microservices folder |
