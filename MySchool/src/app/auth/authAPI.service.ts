import { Injectable, inject } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { URLAPIService } from '../ASP.NET API/urlapi.service';
import { Router } from '@angular/router';


@Injectable({
  providedIn: 'root'
})
export class AuthAPIService {
  private API = inject(URLAPIService);  
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
  }
}
