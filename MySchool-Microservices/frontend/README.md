# Frontend (MySchool Angular)

The Angular SPA was relocated from the repository root `MySchool/` folder to `MySchool-Microservices/frontend/` as part of the microservices foundation.

## API base URL

The app calls the **API Gateway**, not the monolith directly:

| Profile | `baseUrl` |
|---------|-----------|
| Development (`ng serve`) | `http://localhost:5001/api` |
| Docker (`ng serve --configuration docker`) | `http://localhost:8081/api` |
| Production | Set in `environment.ts` |

## Commands

```bash
npm install
npx ng serve                    # development → gateway :5001
npx ng serve --configuration docker   # Docker Compose → gateway :8081
```

See [../docs/gateway-setup.md](../docs/gateway-setup.md) for full gateway documentation.
