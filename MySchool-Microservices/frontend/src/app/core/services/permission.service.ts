import { Injectable } from '@angular/core';

const STORAGE_PERMISSIONS = 'permissions';
const STORAGE_SCHOOL_ROLE = 'schoolRole';

/** CRUD keys for a module (matches <c>PagePermissionNames</c> on the server). */
function crud(page: string) {
  return {
    View: `${page}.View`,
    Create: `${page}.Create`,
    Update: `${page}.Update`,
    Delete: `${page}.Delete`,
  } as const;
}

/** Mirrors backend <c>PagePermissionNames</c> for templates and route <c>data</c>. */
export const PagePermission = {
  Dashboard: { View: 'Dashboard.View' },
  Settings: crud('Settings'),
  Employees: crud('Employees'),
  Recruitment: crud('Recruitment'),
  Teachers: crud('Teachers'),
  Students: crud('Students'),
  Guardians: crud('Guardians'),
  Accounts: crud('Accounts'),
  Grades: crud('Grades'),
  Reports: crud('Reports'),
  ReportsFinancial: crud('ReportsFinancial'),
  ReportsTerm: crud('ReportsTerm'),
  ReportsMonthly: crud('ReportsMonthly'),
  ReportsRegistration: crud('ReportsRegistration'),
  ReportsAllotment: crud('ReportsAllotment'),
  AiChat: crud('AiChat'),
  Calendar: crud('Calendar'),
  Schedule: crud('Schedule'),
  Exams: crud('Exams'),
  Homework: crud('Homework'),
  Attendance: crud('Attendance'),
  Notifications: crud('Notifications'),
  Tests: crud('Tests'),
  Holidays: crud('Holidays'),
  Events: crud('Events'),
  Fees: crud('Fees'),
  Courses: crud('Courses'),
  Payroll: crud('Payroll'),
  Blogs: crud('Blogs'),
  Management: crud('Management'),
  Evaluations: crud('Evaluations'),
  Plans: crud('Plans'),
  Activities: crud('Activities'),
  Complaints: crud('Complaints'),
  Meetings: crud('Meetings'),
  Requests: crud('Requests'),
} as const;

@Injectable({ providedIn: 'root' })
export class PermissionService {
  /** Lowercase key → canonical permission string */
  private granted = new Map<string, string>();

  constructor() {
    this.hydrateFromStorage();
  }

  /** Call after login and on app load when a token exists. */
  hydrateFromStorage(): void {
    if (typeof localStorage === 'undefined') return;
    const raw = localStorage.getItem(STORAGE_PERMISSIONS);
    if (raw) {
      try {
        const arr = JSON.parse(raw) as unknown;
        if (Array.isArray(arr)) {
          this.granted.clear();
          for (const x of arr) this.addGrant(String(x));
        }
      } catch {
        /* ignore */
      }
    }
    const token = localStorage.getItem('token');
    if (token) this.applyJwtPermissions(token, false);
  }

  setFromLoginResponse(response: { permissions?: string[]; schoolRole?: string | null; token?: string }): void {
    if (response.permissions && response.permissions.length > 0) {
      this.granted.clear();
      for (const p of response.permissions) this.addGrant(p);
      this.persistPermissions();
    } else if (response.token) {
      this.applyJwtPermissions(response.token, true);
    }
    if (response.schoolRole != null && typeof localStorage !== 'undefined') {
      localStorage.setItem(STORAGE_SCHOOL_ROLE, String(response.schoolRole));
    }
  }

  /** Union JWT permission claims into the current set (e.g. after token refresh). */
  mergeFromJwt(token: string): void {
    this.applyJwtPermissions(token, false);
  }

  private applyJwtPermissions(token: string, replace: boolean): void {
    const fromJwt = this.parsePermissionsFromJwt(token);
    if (replace) this.granted.clear();
    for (const p of fromJwt) this.addGrant(p);
    this.persistPermissions();
  }

  hasPermission(permission: string): boolean {
    return this.granted.has(permission.toLowerCase());
  }

  /** True if any of the listed permissions is granted. */
  hasAny(permissions: readonly string[]): boolean {
    return permissions.some((p) => this.hasPermission(p));
  }

  getSchoolRole(): string | null {
    if (typeof localStorage === 'undefined') return null;
    return localStorage.getItem(STORAGE_SCHOOL_ROLE);
  }

  clear(): void {
    this.granted.clear();
    if (typeof localStorage === 'undefined') return;
    localStorage.removeItem(STORAGE_PERMISSIONS);
    localStorage.removeItem(STORAGE_SCHOOL_ROLE);
  }

  private addGrant(permission: string): void {
    const k = permission.toLowerCase();
    if (!this.granted.has(k)) this.granted.set(k, permission);
  }

  private persistPermissions(): void {
    if (typeof localStorage === 'undefined') return;
    localStorage.setItem(STORAGE_PERMISSIONS, JSON.stringify([...this.granted.values()]));
  }

  private parsePermissionsFromJwt(token: string): string[] {
    const payload = this.readJwtPayload(token);
    if (!payload) return [];
    return extractPermissionStrings(payload);
  }

  private readJwtPayload(token: string): Record<string, unknown> | null {
    try {
      const parts = token.split('.');
      if (parts.length < 2) return null;
      let base64 = parts[1].replace(/-/g, '+').replace(/_/g, '/');
      const pad = base64.length % 4;
      if (pad) base64 += '='.repeat(4 - pad);
      return JSON.parse(atob(base64)) as Record<string, unknown>;
    } catch {
      return null;
    }
  }
}

function extractPermissionStrings(payload: Record<string, unknown>): string[] {
  const out: string[] = [];
  for (const key of Object.keys(payload)) {
    if (key === 'permission' || key.endsWith('/permission')) {
      pushRaw(out, payload[key]);
    }
  }
  return [...new Set(out.map((s) => s.trim()).filter((s) => s.length > 0))];
}

function pushRaw(out: string[], raw: unknown): void {
  if (raw == null) return;
  if (Array.isArray(raw)) {
    for (const x of raw) {
      if (typeof x === 'string') out.push(x);
    }
    return;
  }
  if (typeof raw === 'string') out.push(raw);
}
