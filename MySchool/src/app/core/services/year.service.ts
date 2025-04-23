import { inject, Injectable } from '@angular/core';
import { BackendAspService } from '../../ASP.NET/backend-asp.service';
import { catchError, map, Observable } from 'rxjs';
import { Year } from '../models/year.model';

@Injectable({
  providedIn: 'root'
})
export class YearService {
  private API = inject(BackendAspService);

  addYear(year: Year) {
    return this.API.http.post(`${this.API.baseUrl}/Year`, year).pipe(
      catchError(error => {
        console.error("Error adding Year:", error);
        throw error; // Optionally handle the error or rethrow
      })
    );
  }

  updateYear(year: Year, id: number): void {
    this.API.http.put(`${this.API.baseUrl}/Year/${id}`, year).subscribe(res => {
      console.log("updated year is ", res);
    });
  }

  getAllYears(): Observable<Year[]> {
    return this.API.http.get<{ result: Year[] }>(`${this.API.baseUrl}/Year`).pipe(
      map(response => response.result),
      catchError(error => {
        console.error("Error fetching Year Details:", error);
        throw error; // Optionally handle the error or rethrow
      })
    );
  }

  getYearById(id: number) {
    return this.API.http.get(`${this.API.baseUrl}/Year/${id}`).pipe(
      catchError(error => {
        console.error("Error fetching Year Details:", error);
        throw error; // Optionally handle the error or rethrow
      })
    );
  }

  deleteYear(id: number) {
   return this.API.http.delete(`${this.API.baseUrl}/Year/${id}`).pipe(
      catchError(error => {
        console.error("Error deleting Year:", error);
        throw error; // Optionally handle the error or rethrow
      })
    );
  }

  partialUpdate(id: number, patchDoc: any): Observable<any> {
    return this.API.http.patch<any>(`${this.API.baseUrl}/year/${id}`, patchDoc).pipe(
      map(response => response.result),
      catchError(error => {
        console.error("Error with partial update:", error);
        throw error;
      })
    );
  }
}
