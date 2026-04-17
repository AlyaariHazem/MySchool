import { Router } from '@angular/router';

/**
 * Public applicant / job-board URLs under /school/recruitment (no login).
 * Excludes HR-only paths such as job-postings/create, job-postings/:id/edit, job-applications (list), workflow detail.
 */
export function isPublicSchoolRecruitmentPath(url: string): boolean {
  let path = url.split('?')[0].trim();
  if (!path.startsWith('/')) path = `/${path}`;
  path = path.replace(/\/$/, '') || '/';

  if (path.startsWith('/school/recruitment/job-applications/create')) return true;

  if (path === '/school/recruitment/job-postings') return true;

  if (path.startsWith('/school/recruitment/job-postings/')) {
    if (path.includes('/edit') || path.endsWith('/create')) return false;
    return /^\/school\/recruitment\/job-postings\/\d+$/.test(path);
  }

  return false;
}

/**
 * Resolves public recruitment access during navigation when a single URL snapshot may be stale
 * (e.g. parent <c>canMatch</c> before <c>router.url</c> updates).
 */
export function isPublicRecruitmentRouteUrl(router: Router): boolean {
  const candidates: string[] = [];
  const nav = router.getCurrentNavigation();
  if (nav?.extractedUrl != null) {
    candidates.push(router.serializeUrl(nav.extractedUrl).split('?')[0]);
  }
  candidates.push(router.url.split('?')[0]);
  if (typeof window !== 'undefined') {
    candidates.push(window.location.pathname);
  }
  return candidates.some((p) => p && isPublicSchoolRecruitmentPath(p));
}
