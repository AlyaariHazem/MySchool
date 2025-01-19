import { inject } from '@angular/core';
import { CanMatchFn } from '@angular/router';
import { ToastrService } from 'ngx-toastr';

import { AuthAPIService } from '../../auth/authAPI.service';

export const adminGuardGuard: CanMatchFn = (route, state) => {
  
  const authService = inject(AuthAPIService);
  const toastr = inject(ToastrService); 

  // Check if we are in a browser environment before accessing localStorage
  const token = typeof window !== 'undefined' ? localStorage.getItem('token') : null;

  if (token) {
    return true; // Allow access if the token exists
  } else {
    // Redirect to the login page if there's no token
    toastr.error('userName or password is wroing');
    authService.router.navigateByUrl('/login'); // Adjust the route as necessary
    return false; // Deny access
  }
};