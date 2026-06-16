import { inject } from '@angular/core';
import { CanMatchFn, Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';

import { PermissionService } from '../services/permission.service';

/**
 * Route <c>data</c>:
 * - <c>permission: 'Employees.View'</c> — single permission
 * - <c>permissions: ['A.View','B.View']</c> — any one grants access
 */
export const permissionGuard: CanMatchFn = (route) => {
  const perm = inject(PermissionService);
  const router = inject(Router);
  const toastr = inject(ToastrService);

  const single = route.data?.['permission'] as string | undefined;
  const many = route.data?.['permissions'] as string[] | undefined;
  const required = single ? [single] : many ?? [];
  if (required.length === 0) return true;

  const ok = required.some((p) => perm.hasPermission(p));
  if (ok) return true;

  toastr.error('ليس لديك صلاحية الوصول لهذه الصفحة.');
  const ut = typeof localStorage !== 'undefined' ? localStorage.getItem('userType') : null;
  const home = ut === 'ADMIN' ? '/admin/dashboard' : '/school/dashboard';
  router.navigateByUrl(home).catch(() => undefined);
  return false;
};
