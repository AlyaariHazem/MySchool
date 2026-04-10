import { inject } from '@angular/core';
import { CanMatchFn, Router } from '@angular/router';

/** Restricts lazy-loaded `students` routes to accounts with userType STUDENT. */
export const studentGuard: CanMatchFn = () => {
  const router = inject(Router);
  const ut =
    typeof localStorage !== 'undefined' ? localStorage.getItem('userType') : null;
  if (ut === 'STUDENT') {
    return true;
  }
  if (ut === 'ADMIN') {
    void router.navigateByUrl('/admin');
    return false;
  }
  if (ut === 'TEACHER') {
    void router.navigateByUrl('/teacher');
    return false;
  }
  void router.navigateByUrl('/school/dashboard');
  return false;
};
