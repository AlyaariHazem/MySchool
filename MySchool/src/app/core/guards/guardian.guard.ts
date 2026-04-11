import { inject } from '@angular/core';
import { CanMatchFn, Router } from '@angular/router';

/** Restricts lazy-loaded `guardian` routes to accounts with userType GUARDIAN. */
export const guardianGuard: CanMatchFn = () => {
  const router = inject(Router);
  const ut =
    typeof localStorage !== 'undefined' ? localStorage.getItem('userType') : null;
  if (ut === 'GUARDIAN') {
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
  if (ut === 'STUDENT') {
    void router.navigateByUrl('/students/home');
    return false;
  }
  void router.navigateByUrl('/school/dashboard');
  return false;
};
