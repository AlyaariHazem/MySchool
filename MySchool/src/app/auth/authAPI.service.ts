import { Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';

import { FirebaseService } from '../firebase/firebase.service';
import { signIn, signUp } from '../firebase/firebase-config';
import { HttpClient } from '@angular/common/http';



@Injectable({
  providedIn: 'root'
})
export class AuthAPIService {
  private baseUrl = 'http://localhost:5180/api';

  constructor(private http: HttpClient, public router: Router) { }

  login(credentials: { username: string; password: string }): Observable<any> {
    return this.http.post(`${this.baseUrl}/account/login`, credentials).pipe(
      tap((response: any) => {
        if (response && response.token) {
          localStorage.setItem('token', response.token);
        }
      })
    );
  }
  logout() {
    
    localStorage.removeItem('token');
    // Navigate to the login page
    this.router.navigate(['/login']);
  }
}
