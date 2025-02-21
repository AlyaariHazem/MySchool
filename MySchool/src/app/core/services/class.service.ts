import { inject, Injectable } from '@angular/core';
import { BackendAspService } from '../../environments/ASP.NET/backend-asp.service';
import { catchError, map, Observable } from 'rxjs';
import { CLass, updateClass } from '../models/class.model';

@Injectable({
  providedIn: 'root'
})
export class ClassService {
  private API = inject(BackendAspService);
  constructor() { }

  GetAll(): Observable<any> {
    return this.API.http.get<any>(`${this.API.baseUrl}/Classes`).pipe(
      map(response => response.result)
    );
  }

  Add(Class: CLass): Observable<any> {
    return this.API.http.post(`${this.API.baseUrl}/Classes`, Class).pipe(
      catchError(error => {
        console.error("Error adding Class:", error);
        throw error; // Optionally rethrow or handle the error here
      })
    )
  }

  Delete(id: number): Observable<any> {
    return this.API.http.delete(`${this.API.baseUrl}/Classes/${id}`);
  }

  Update(id: number, update: updateClass): Observable<any> {
    return this.API.http.put(`${this.API.baseUrl}/Classes/${id}`, update).pipe(
      map(response => response), // Optionally process the response if needed
      catchError(error => {
        console.error("Error updating Class:", error);
        throw error;
      })
    );
  }

  partialUpdate(id: number, patchDoc: any): Observable<any> {
    return this.API.http.patch(`${this.API.baseUrl}/Classes/${id}`, patchDoc).pipe(
      map(response => response),
      catchError(error => {
        console.error("Error with partial update:", error);
        throw error;
      })
    );
  }
  
}
