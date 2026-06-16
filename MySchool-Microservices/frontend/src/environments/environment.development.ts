export const environment = {
    production: false,
    /** Web BFF — school APIs via YARP to Backend */
    baseUrl: 'http://localhost:5001/api',
    /** Web BFF — identity via gRPC-backed HTTP endpoints */
    bffUrl: 'http://localhost:5001/bff',
    /**
     * Dev: browser calls same origin `/school-ai-support/...` → `proxy.conf.json` forwards to minimal-agent at http://localhost:5043.
     */
    schoolAiSupportUrl: '/school-ai-support',
  /** Direct monolith (bypass BFF): `https://localhost:7258/api` or Docker backend: `http://localhost:8080/api` */
  /** Docker Compose Web BFF: `http://localhost:8081/api` and `http://localhost:8081/bff` (use `ng serve --configuration docker`) */

    /**
     * Sent as `X-Tenant-Id` on API calls when the user is not logged in (public job board).
     */
    publicTenantId: 1,
};
