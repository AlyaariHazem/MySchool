export const environment = {
    production: false,
    baseUrl: 'https://localhost:7258/api',
    /**
     * Dev: browser calls same origin `/school-ai-support/...` → `proxy.conf.json` forwards to minimal-agent at http://localhost:5043.
     * Avoids TLS errors on https://localhost:7127 (untrusted dev cert) and CORS.
     * Ensure minimal-agent is running (HTTP on 5043). To call it directly instead, use e.g. `http://localhost:5043`.
     */
    schoolAiSupportUrl: '/school-ai-support',
    // When running with Docker Compose, backend is on port 8080
    // baseUrl: 'http://localhost:8080/api'

    /**
     * Sent as `X-Tenant-Id` on API calls when the user is not logged in (public job board).
     * Must match a row in the admin `Tenants` table. Also set `PublicRecruitment:DefaultTenantId` in Backend appsettings as a fallback.
     */
    publicTenantId: 1,
};
