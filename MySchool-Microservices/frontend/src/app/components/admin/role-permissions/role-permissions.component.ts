import { Component, OnInit, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map } from 'rxjs';

import { BackendAspService } from '../../../ASP.NET/backend-asp.service';

/** Display labels (API still uses English keys for roles / matrix). */
const ROLE_LABELS_AR: Record<string, string> = {
  Teacher: 'معلم',
  Manager: 'مدير',
  SystemAdmin: 'مدير نظام',
  EducationalSupervisor: 'مشرف تربوي',
  AdministrativeSupervisor: 'مشرف إداري',
  AdministrativeEmployee: 'موظف إداري',
  Student: 'طالب',
  Guardian: 'ولي أمر',
};

/** Module/page keys from API → Arabic titles (permission matrix display). */
const PAGE_LABELS_AR: Record<string, string> = {
  Accounts: 'الحسابات',
  Activities: 'الأنشطة',
  AiChat: 'محادثة الذكاء الاصطناعي',
  Attendance: 'الحضور والغياب',
  Blogs: 'المدونات',
  Calendar: 'التقويم',
  Complaints: 'الشكاوى',
  Courses: 'المقررات',
  Dashboard: 'لوحة التحكم',
  Employees: 'الموظفون',
  Evaluations: 'التقييمات',
  Events: 'الفعاليات',
  Exams: 'الامتحانات',
  Fees: 'الرسوم',
  Grades: 'الدرجات',
  Guardians: 'أولياء الأمور',
  Holidays: 'الإجازات',
  Homework: 'الواجبات',
  Management: 'الإدارة',
  Meetings: 'الاجتماعات',
  Notifications: 'الإشعارات',
  Payroll: 'الرواتب',
  Plans: 'الخطط',
  Reports: 'التقارير',
  ReportsAllotment: 'تقارير التوزيع',
  ReportsFinancial: 'التقارير المالية',
  ReportsMonthly: 'التقارير الشهرية',
  ReportsRegistration: 'تقارير التسجيل',
  ReportsTerm: 'تقارير الفصل الدراسي',
  Requests: 'الطلبات',
  Schedule: 'الجدول الدراسي',
  Settings: 'الإعدادات',
  Students: 'الطلاب',
  Teachers: 'المعلمون',
  Tests: 'الاختبارات',
};

const ACTION_LABELS_AR: Record<string, string> = {
  View: 'عرض',
  Create: 'إنشاء',
  Update: 'تعديل',
  Delete: 'حذف',
};

function labelAr(map: Record<string, string>, key: string): string {
  if (map[key]) return map[key];
  const hit = Object.keys(map).find((k) => k.toLowerCase() === key.toLowerCase());
  return hit ? map[hit] : key;
}

export interface PermissionItemDto {
  name: string;
  page: string;
  action: string;
}

export interface RolePermissionMatrixDto {
  roles: string[];
  permissions: PermissionItemDto[];
  matrix: Record<string, boolean>;
}

/** Permissions for one page/module, used for card UI. */
export interface PagePermissionGroup {
  pageKey: string;
  items: PermissionItemDto[];
}

const ACTION_SORT_ORDER = ['View', 'Create', 'Update', 'Delete'];

@Component({
  selector: 'app-role-permissions',
  templateUrl: './role-permissions.component.html',
  styleUrl: './role-permissions.component.scss',
})
export class RolePermissionsComponent implements OnInit {
  private readonly http = inject(HttpClient);
  private readonly api = inject(BackendAspService);

  loading = true;
  saving = false;
  error: string | null = null;

  roles: string[] = [];
  rows: PermissionItemDto[] = [];
  /** Grouped by `page` for card grid (lazy-rendered). */
  pageGroups: PagePermissionGroup[] = [];
  /** Currently edited role (matrix columns collapsed into one). */
  selectedRole: string | null = null;
  /** Local editable copy of matrix keys → allowed */
  localMatrix: Record<string, boolean> = {};
  /** Roles with unsaved edits (partial PUT sends one role at a time). */
  private readonly dirtyRoles = new Set<string>();

  /** True when the role in the dropdown has pending changes. */
  get hasDirtyChangesForSelection(): boolean {
    return !!this.selectedRole && this.dirtyRoles.has(this.selectedRole);
  }

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.error = null;
    this.http
      .get<unknown>(`${this.api.baseUrl}/RolePermissions/matrix`)
      .pipe(map((raw) => this.normalizeMatrix(raw)))
      .subscribe({
        next: (dto) => {
          this.roles = dto.roles;
          this.rows = dto.permissions;
          this.localMatrix = { ...dto.matrix };
          this.dirtyRoles.clear();
          this.selectedRole = this.roles[0] ?? null;
          this.rebuildPageGroups();
          this.loading = false;
        },
        error: (e) => {
          this.error = e?.error?.message ?? 'تعذر تحميل مصفوفة الصلاحيات.';
          this.loading = false;
        },
      });
  }

  isAllowed(role: string, permissionName: string): boolean {
    const k = this.matrixKey(role, permissionName);
    return !!this.localMatrix[k];
  }

  toggle(role: string, permissionName: string, checked: boolean): void {
    const k = this.matrixKey(role, permissionName);
    this.localMatrix[k] = checked;
    this.dirtyRoles.add(role);
  }

  /** Arabic label for role column (canonical key unchanged in API). */
  translateRole(roleKey: string): string {
    return labelAr(ROLE_LABELS_AR, roleKey);
  }

  translatePage(pageKey: string): string {
    return labelAr(PAGE_LABELS_AR, pageKey);
  }

  translateAction(actionKey: string): string {
    return labelAr(ACTION_LABELS_AR, actionKey);
  }

  /** All permissions on this page allowed for `selectedRole`. */
  pageAllAllowed(group: PagePermissionGroup): boolean {
    const role = this.selectedRole;
    if (!role || !group.items.length) return false;
    return group.items.every((p) => this.isAllowed(role, p.name));
  }

  /** Some but not all (for indeterminate). */
  pageSomeAllowed(group: PagePermissionGroup): boolean {
    const role = this.selectedRole;
    if (!role || !group.items.length) return false;
    const any = group.items.some((p) => this.isAllowed(role, p.name));
    const all = group.items.every((p) => this.isAllowed(role, p.name));
    return any && !all;
  }

  togglePageForRole(pageKey: string, checked: boolean): void {
    const role = this.selectedRole;
    if (!role) return;
    for (const p of this.rows) {
      if (p.page === pageKey) {
        this.toggle(role, p.name, checked);
      }
    }
  }

  private rebuildPageGroups(): void {
    const map = new Map<string, PermissionItemDto[]>();
    for (const p of this.rows) {
      const list = map.get(p.page) ?? [];
      list.push(p);
      map.set(p.page, list);
    }
    const actionRank = (a: string): number => {
      const i = ACTION_SORT_ORDER.indexOf(a);
      return i >= 0 ? i : 100;
    };
    this.pageGroups = [...map.keys()]
      .sort((a, b) => a.localeCompare(b, undefined, { sensitivity: 'base' }))
      .map((pageKey) => {
        const items = (map.get(pageKey) ?? [])
          .slice()
          .sort((x, y) => actionRank(x.action) - actionRank(y.action) || x.name.localeCompare(y.name));
        return { pageKey, items };
      });
  }

  save(): void {
    const role = this.selectedRole;
    if (!role || !this.dirtyRoles.has(role)) {
      return;
    }
    const cells = this.rows.map((p) => ({
      permissionName: p.name,
      isAllowed: !!this.localMatrix[this.matrixKey(role, p.name)],
    }));
    const body = {
      scopeToRoleName: role,
      cells,
    };
    this.saving = true;
    this.error = null;
    this.http.put(`${this.api.baseUrl}/RolePermissions/matrix`, body).subscribe({
      next: () => {
        this.saving = false;
        this.dirtyRoles.delete(role);
        this.load();
      },
      error: (e) => {
        this.error = e?.error?.message ?? 'فشل حفظ التغييرات.';
        this.saving = false;
      },
    });
  }

  private matrixKey(role: string, permissionName: string): string {
    const direct = `${role}|${permissionName}`;
    if (direct in this.localMatrix) return direct;
    const lower = `${role}|${permissionName}`.toLowerCase();
    for (const k of Object.keys(this.localMatrix)) {
      if (k.toLowerCase() === lower) return k;
    }
    return direct;
  }

  private normalizeMatrix(raw: unknown): RolePermissionMatrixDto {
    const o = raw as Record<string, unknown>;
    const roles = (o['roles'] ?? o['Roles']) as string[] | undefined;
    const permissions = (o['permissions'] ?? o['Permissions']) as PermissionItemDto[] | undefined;
    const matrixRaw = (o['matrix'] ?? o['Matrix']) as Record<string, boolean> | undefined;
    const matrix: Record<string, boolean> = {};
    if (matrixRaw) {
      for (const [k, v] of Object.entries(matrixRaw)) {
        matrix[k] = !!v;
      }
    }
    return {
      roles: Array.isArray(roles) ? roles : [],
      permissions: Array.isArray(permissions) ? permissions : [],
      matrix,
    };
  }
}
