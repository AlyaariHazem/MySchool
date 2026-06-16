/** Sidebar / reports use this when API has no logo yet. */
export const SCHOOL_LOGO_FALLBACK = 'assets/img/logo1.png';

/**
 * Some tenants store a folder path like `/uploads/School/School_1` with no filename → 404 and noisy console.
 * Returns a safe static asset URL when the value is empty or looks like that placeholder.
 */
export function resolveSchoolLogoSrc(raw: string | null | undefined): string {
  const s = raw?.trim();
  if (!s) {
    return SCHOOL_LOGO_FALLBACK;
  }
  // Full URL ending at School_<id> with no file extension
  if (/^https?:\/\/.+\/uploads\/School\/School_\d+$/i.test(s)) {
    return SCHOOL_LOGO_FALLBACK;
  }
  // Same, relative to origin
  if (/^\/uploads\/School\/School_\d+$/i.test(s)) {
    return SCHOOL_LOGO_FALLBACK;
  }
  return s;
}
