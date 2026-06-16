# Frontend (MySchool Angular)

The Angular SPA was relocated from the repository root `MySchool/` folder to `MySchool-Microservices/frontend/` as part of the microservices foundation.

## API base URL

The app calls the **Web BFF** (`MySchool.WebBff`), not the monolith or IdentityService directly:

| Profile | `baseUrl` | `bffUrl` |
|---------|-----------|----------|
| Development (`ng serve`) | `http://localhost:5001/api` | `http://localhost:5001/bff` |
| Docker (`ng serve --configuration docker`) | `http://localhost:8081/api` | `http://localhost:8081/bff` |
| Production | Set in `environment.ts` | Set in `environment.ts` |

## Commands

```bash
npm install
npx ng serve                    # development → Web BFF :5001
npx ng serve --configuration docker   # Docker Compose → Web BFF :8081
```
