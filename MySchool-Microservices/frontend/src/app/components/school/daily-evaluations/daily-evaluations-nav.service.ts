import { Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';

/**
 * Resolves the daily evaluation area base URL for both school admin (`/school/daily-evaluations`)
 * and teacher portal (`/teacher/daily-evaluations`) so shared components navigate correctly.
 */
@Injectable({ providedIn: 'root' })
export class DailyEvaluationsNavService {
  private readonly router = inject(Router);

  basePath(): string {
    const path = this.router.url.split('?')[0];
    if (path.startsWith('/teacher/daily-evaluations')) return '/teacher/daily-evaluations';
    if (path.startsWith('/students/daily-evaluations')) return '/students/daily-evaluations';
    return '/school/daily-evaluations';
  }

  /** Teacher portal reuses the same components; hide HR-only filters (e.g. employee list) in this context. */
  isTeacherDailyEvaluationsRoute(): boolean {
    const path = this.router.url.split('?')[0];
    return path.startsWith('/teacher/daily-evaluations');
  }

  /** Student portal: same shared components as teacher/school. */
  isStudentDailyEvaluationsRoute(): boolean {
    const path = this.router.url.split('?')[0];
    return path.startsWith('/students/daily-evaluations');
  }

  /**
   * Teachers must not call GET /api/School. Current school comes from the same session keys as the rest of the teacher UI.
   */
  teacherSessionSchoolOption(): { label: string; value: number } | null {
    if (typeof localStorage === 'undefined') return null;
    const raw = localStorage.getItem('schoolId');
    const sid = raw ? Number(raw) : NaN;
    if (!Number.isFinite(sid) || sid <= 0) return null;
    const label = localStorage.getItem('schoolName')?.trim() || String(sid);
    return { label, value: sid };
  }

  teacherSessionYearId(): number | undefined {
    if (typeof localStorage === 'undefined') return undefined;
    const yRaw = localStorage.getItem('yearID') ?? localStorage.getItem('yearId');
    const yid = yRaw ? Number(yRaw) : NaN;
    if (Number.isFinite(yid) && yid > 0) return yid;
    return undefined;
  }

  /** Same session keys as teacher — login stores school/year for school users. */
  studentSessionSchoolOption(): { label: string; value: number } | null {
    return this.teacherSessionSchoolOption();
  }

  studentSessionYearId(): number | undefined {
    return this.teacherSessionYearId();
  }
}
