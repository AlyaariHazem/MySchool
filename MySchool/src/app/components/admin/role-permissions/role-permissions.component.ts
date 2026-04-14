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

const PAGE_LABELS_AR: Record<string, string> = {
  Dashboard: 'لوحة التحكم',
  Employees: 'الموظفون',
  Teachers: 'المعلمون',
  Students: 'الطلاب',
  Evaluations: 'التقييمات',
  Reports: 'التقارير',
  Plans: 'الخطط',
  Activities: 'الأنشطة',
  Complaints: 'الشكاوى',
  Meetings: 'الاجتماعات',
  Requests: 'الطلبات',
  Settings: 'الإعدادات',
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
  /** Local editable copy of matrix keys → allowed */
  localMatrix: Record<string, boolean> = {};
  dirty = false;

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
          this.dirty = false;
          this.loading = false;
        },
        error: (e) => {
          this.error = e?.error?.message ?? 'تعذر تحميل مصفوفة الصلاحيات.';
          this.loading = false;
        },
      });
  }

  cellKey(role: string, permissionName: string): string {
    return `${role}|${permissionName}`;
  }

  isAllowed(role: string, permissionName: string): boolean {
    const k = this.matrixKey(role, permissionName);
    return !!this.localMatrix[k];
  }

  toggle(role: string, permissionName: string, checked: boolean): void {
    const k = this.matrixKey(role, permissionName);
    this.localMatrix[k] = checked;
    this.dirty = true;
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

  save(): void {
    const cells: { roleName: string; permissionName: string; isAllowed: boolean }[] = [];
    for (const role of this.roles) {
      for (const p of this.rows) {
        const k = this.matrixKey(role, p.name);
        cells.push({
          roleName: role,
          permissionName: p.name,
          isAllowed: !!this.localMatrix[k],
        });
      }
    }
    this.saving = true;
    this.error = null;
    this.http.put(`${this.api.baseUrl}/RolePermissions/matrix`, { cells }).subscribe({
      next: () => {
        this.saving = false;
        this.dirty = false;
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
