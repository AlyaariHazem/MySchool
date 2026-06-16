# MySchool Microservices

Foundation layout for migrating the MySchool monolith to microservices.

## Structure

```
MySchool-Microservices/
├── bff/MySchool.WebBff/          Public HTTP entry (BFF + YARP)
├── services/IdentityService/     Identity, auth, users, roles (gRPC + internal HTTP)
├── services/MonolithService/     Logical boundary → ../../Backend (unchanged)
├── shared/
│   ├── MySchool.Contracts/       Shared DTOs and contracts
│   └── MySchool.BuildingBlocks/  Cross-cutting helpers
├── frontend/                     Angular SPA
├── docs/                         Architecture documentation
└── docker-compose.yml            sqlserver + identity + backend + webbff + frontend
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

4. Start the Web BFF:

   ```bash
   cd bff/MySchool.WebBff
   dotnet run
   ```

5. Start the frontend (calls BFF at `http://localhost:5001`):

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
| Web BFF | http://localhost:8081 |
| Identity (internal gRPC, debug HTTP) | http://localhost:8082 |
| Frontend | http://localhost:4200 |
| Swagger (BFF) | http://localhost:8081/swagger |

Set `MSSQL_SA_PASSWORD` in `.env` at the repository root.

See [docs/bff-grpc-identity.md](./docs/bff-grpc-identity.md) for BFF routing and gRPC details.
