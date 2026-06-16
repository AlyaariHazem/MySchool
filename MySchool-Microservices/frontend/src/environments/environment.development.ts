export const environment = {
    production: false,
    /** API Gateway (YARP) — forwards to the monolith. Run: dotnet run --project MySchool-Microservices/gateway/MySchool.Gateway */
    baseUrl: 'http://localhost:5001/api',
    /**
     * Dev: browser calls same origin `/school-ai-support/...` → `proxy.conf.json` forwards to minimal-agent at http://localhost:5043.
     */
    schoolAiSupportUrl: '/school-ai-support',
  /** Direct monolith (bypass gateway): `https://localhost:7258/api` or Docker monolith: `http://localhost:8080/api` */
  /** Docker Compose gateway: `http://localhost:8081/api` (use `ng serve --configuration docker`) */

    /**
     * Sent as `X-Tenant-Id` on API calls when the user is not logged in (public job board).
     */
    publicTenantId: 1,
};
