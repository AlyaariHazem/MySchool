export const environment = {
    production: false,
    baseUrl: 'https://localhost:7258/api',
    // When running with Docker Compose, backend is on port 8080
    // baseUrl: 'http://localhost:8080/api'

    /**
     * Sent as `X-Tenant-Id` on API calls when the user is not logged in (public job board).
     * Must match a row in the admin `Tenants` table. Also set `PublicRecruitment:DefaultTenantId` in Backend appsettings as a fallback.
     */
    publicTenantId: 1,
};
