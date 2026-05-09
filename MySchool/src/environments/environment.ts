export const environment = {
    production: true,
    baseUrl: 'https://production-url.com/api',
    /** Base URL of minimal-agent WebApi (no trailing slash). Set in deployment. */
    schoolAiSupportUrl: '',
    /** Optional: tenant id for anonymous job-board API calls (see Backend PublicRecruitment). */
    publicTenantId: undefined as number | undefined,
};
