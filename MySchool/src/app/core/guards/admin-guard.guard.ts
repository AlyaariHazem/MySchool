import { inject } from '@angular/core';
import { CanMatchFn, Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';

import { AuthAPIService } from '../../auth/authAPI.service';
import { isPublicRecruitmentRouteUrl } from '../utils/public-recruitment-route.util';

export const adminGuardGuard: CanMatchFn = () => {
  const router = inject(Router);
  const authService = inject(AuthAPIService);
  const toastr = inject(ToastrService);

  if (isPublicRecruitmentRouteUrl(router)) {
    return true;
  }

  // Check if we are in a browser environment before accessing localStorage
  const token = typeof window !== 'undefined' ? localStorage.getItem('token') : null;

  if (token) {
    return true; // Allow access if the token exists
  } else {
    // Redirect to the login page if there's no token
    toastr.error('Username or password is wrong.');
    authService.router.navigateByUrl('/login'); // Adjust the route as necessary
    return false; // Deny access
  }
};