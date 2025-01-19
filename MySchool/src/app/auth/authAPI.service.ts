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

  login(user:User): Observable<any> {
    return this.API.http.post(`${this.API.baseUrl}/account/login`, user).pipe(
      tap((response: any) => {
        if (response && response.token) {
          localStorage.setItem('token', response.token);
        }
      })
    );
  }

  register(user:User): Observable<any> {
    return this.API.http.post(`${this.API.baseUrl}/account/register`, user);
  }
  
  logout() {
    localStorage.removeItem('token');
    // Navigate to the login page
    this.API.router.navigate(['/login']);
  //  localStorage.removeItem('token'); 
  }
}
