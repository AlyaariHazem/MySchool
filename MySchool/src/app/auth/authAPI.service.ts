import { Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';

import { User } from '../core/models/user.model';
import { BackendAspService } from '../environments/ASP.NET/backend-asp.service';


@Injectable({
  providedIn: 'root'
})
export class AuthAPIService {
  private API = inject(BackendAspService);

  constructor(public router: Router) { }

  login(user: User): Observable<any> {
    return this.API.http.post(`${this.API.baseUrl}/account/login`, user).pipe(
      tap((response: any) => {
        if (response && response.token) {
          localStorage.setItem('token', response.token);
        }
        if (response.managerName) {
          localStorage.setItem('managerName', response.managerName);
        }
        if (response.managerName===" ") {
          localStorage.setItem('managerName',"Admin");
        }
        if (response.schoolName) {
          localStorage.setItem('schoolName', response.schoolName);
        }
        if (response.userName) {
          localStorage.setItem('userName', response.userName);
        }
        if (response.schoolId) {
          localStorage.setItem('schoolId', response.schoolId);
        }
      })
    );
  }

  register(user: User): Observable<any> {
    return this.API.http.post(`${this.API.baseUrl}/account/register`, user, {
      responseType: 'json' // ✅ Ensures response is treated as JSON
    });
  }


  logout() {
    localStorage.removeItem('token');
    // Navigate to the login page
    this.API.router.navigate(['/']);
    localStorage.removeItem('token');
  }
}
