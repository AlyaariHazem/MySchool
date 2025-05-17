import { Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';

import { User } from '../core/models/user.model';
import { BackendAspService } from '../ASP.NET/backend-asp.service';


@Injectable({
  providedIn: 'root'
})
export class AuthAPIService {
  private API = inject(BackendAspService);

  constructor(public router: Router) { }

  login(user: User): Observable<any> {
    return this.API.http.post(`${this.API.baseUrl}/auth/login`, user).pipe(
      tap((response: any) => {
        if (response && response.token) {
          localStorage.setItem('token', response.token);
        }
        if (response.managerName) {
          localStorage.setItem('managerName', response.managerName);
        }
        if (response.yearId) {
          localStorage.setItem('yearId', response.yearId);
        }
        if (response.managerName === " ") {
          localStorage.setItem('managerName', "Admin");
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
    return this.API.http.post(`${this.API.baseUrl}/auth/register`, user, {
      responseType: 'json' // ✅ Ensures response is treated as JSON
    });
  }


  logout(): Observable<void> {
     return this.API.http.post<void>(`${this.API.baseUrl}/auth/logout`, {},
      { withCredentials: true })               // what this will do?
      .pipe(
        tap(() => {
          ['token', 'managerName', 'yearId', 'schoolName', 'userName', 'schoolId'].forEach(item => localStorage.removeItem(item));
          this.router.navigate(['/']);
        })
      );
  }
}
