import { Injectable, inject } from '@angular/core';
import { catchError, map, Observable } from 'rxjs';

import { BackendAspService } from '../../../environments/ASP.NET/backend-asp.service';
import { manager } from '../../models/manage/manager.model';

@Injectable({
  providedIn: 'root'
})
export class ManagerService {
  private API = inject(BackendAspService);
  constructor() { }

  getAllManagers(): Observable<any> {
    return this.API.http.get<any>(`${this.API.baseUrl}/Manager`).pipe(
      map(response => response.result),
      catchError(error => {
        console.error("Error fetching managers:", error);
        throw error;
      })
    );
  }

  getManagerById(id: number): Observable<any> {
    return this.API.http.get<any>(`${this.API.baseUrl}/Manager/${id}`).pipe(
      map(response => response.result),
      catchError(error => {
        console.error("Error fetching manager:", error);
        throw error;
      })
    );
  }
  
  deleteManager(id: number): Observable<any> {
    return this.API.http.delete(`${this.API.baseUrl}/Manager/${id}`).pipe(
      catchError(error => {
        console.error("Error deleting manager:", error);
        throw error;
      })
    );
  }
  
  addManager(manager: manager): Observable<any> {
    return this.API.http.post(`${this.API.baseUrl}/Manager`, manager).pipe(
      catchError(error => {
        console.error("Error adding manager:", error);
        throw error;
      })
    );
  }

}
