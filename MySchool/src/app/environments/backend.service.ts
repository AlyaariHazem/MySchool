import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { RegisterStudent, UserForLoginDto } from '../core/models/RegisterStudent.model';
import { Observable } from 'rxjs';
import { BackendUrl } from './environment.prod';

@Injectable({
  providedIn: 'root'
})
export class BackendService {

  constructor(private http: HttpClient) {}

  register(registerStudent: RegisterStudent): Observable<any> {
    return this.http.post(`${BackendUrl}/register`, registerStudent, {
      headers: new HttpHeaders({
        'Content-Type': 'application/json'
      })
    });
  }

  login(userForLoginDto: UserForLoginDto): Observable<any> {
    return this.http.post(`${BackendUrl}/login`, userForLoginDto, {
      headers: new HttpHeaders({
        'Content-Type': 'application/json'
      })
    });
  }
}
