import { Injectable, inject } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { Router } from '@angular/router';
import { BackendAspService } from '../environments/ASP.NET/backend-asp.service';


@Injectable({
  providedIn: 'root'
})
export class AuthAPIService {
  private API = inject(BackendAspService);  
  constructor(public router: Router) { }

  login(credentials: { username: string; password: string }): Observable<any> {
    return this.API.http.post(`${this.API.baseUrl}/account/login`, credentials).pipe(
      tap((response: any) => {
        if (response && response.token) {
          localStorage.setItem('token', response.token);
        }
      })
    );
  }

  register(credentials: { userName: string; email: string; password: string }): Observable<any> {
    return this.API.http.post(`${this.API.baseUrl}/account/register`, credentials);
  }
  
  logout() {
    localStorage.removeItem('token');
    // Navigate to the login page
    this.API.router.navigate(['/login']);
  //  localStorage.removeItem('token'); 
  }
}
