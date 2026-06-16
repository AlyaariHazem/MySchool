# MySchool Microservices

Foundation layout for migrating the MySchool monolith to microservices.

## Structure

```
MySchool-Microservices/
├── gateway/MySchool.Gateway/     YARP API Gateway
├── services/MonolithService/     Logical boundary → ../../Backend (unchanged)
├── shared/
│   ├── MySchool.Contracts/       Shared DTOs and contracts (placeholder)
│   └── MySchool.BuildingBlocks/  Cross-cutting helpers (placeholder)
├── frontend/                     Angular SPA (moved from repo root MySchool/)
├── docs/                         Architecture and gateway documentation
└── docker-compose.yml            sqlserver + monolithservice + gateway + frontend
```

## Quick start (local)

1. Start SQL Server (or use Docker `sqlserver` service).
2. Start the monolith:

   ```bash
   cd ../../Backend
   dotnet run
   ```

3. Start the identity service:

   ```bash
   cd services/IdentityService/MySchool.IdentityService
   dotnet run
   ```

4. Start the gateway:

   ```bash
   cd gateway/MySchool.Gateway
   dotnet run
   ```

5. Start the frontend (calls gateway at `http://localhost:5001/api`):

   ```bash
   cd frontend
   npm install
   npx ng serve
   ```

## Quick start (Docker)

From this folder or the repository root:

```bash
docker compose up -d --build
```

| Service | Host URL |
|---------|----------|
| Gateway | http://localhost:8081 |
| Identity (direct, debug) | http://localhost:8082 |
| Frontend | http://localhost:4200 |
| Swagger (via gateway → monolith) | http://localhost:8081/swagger |

Set `MSSQL_SA_PASSWORD` in `.env` at the repository root.

See [docs/gateway-setup.md](./docs/gateway-setup.md) for details.
