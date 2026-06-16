# Monolith Service

The existing MySchool ASP.NET Core monolith lives at the repository root:

```
../../Backend/
```

This folder is the **logical service boundary** for the monolith during the microservices migration. No business code has been moved yet.

## Local development

Run the monolith directly (unchanged):

```bash
cd ../../Backend
dotnet run
```

Default URLs: `https://localhost:7258` / `http://localhost:5000`

## Docker

`docker-compose.yml` builds the monolith from `../../Backend/Dockerfile`. The service name in Compose is `monolithservice`.

## API Gateway

The Angular frontend and external clients should call **MySchool.Gateway** instead of the monolith directly. The gateway forwards all `/api/*` traffic to this service.
