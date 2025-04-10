import { inject, Injectable } from '@angular/core';
import { catchError, map, Observable } from 'rxjs';
import { AddStage, updateStage } from '../models/stages-grades.modul';
import { BackendAspService } from '../../../../environments/ASP.NET/backend-asp.service';

@Injectable({
  providedIn: 'root'
})
export class StageService {
  private API = inject(BackendAspService);

  // I want this to display my stages 
  getAllStages(): Observable<any> {
    return this.API.http.get<any>(`${this.API.baseUrl}/stages`).pipe(
      map(response => response.result), // Process or map the response here if needed
      catchError(error => {
        console.error("Error fetching stages:", error);
        throw error; // Optionally handle the error or rethrow
      })
    );
  }

  AddStage(stage: AddStage): Observable<any> {
    return this.API.http.post<any>(`${this.API.baseUrl}/stages`, stage).pipe(
      map(response => response.result),
      catchError(error => {
        console.error("Error adding stage:", error);
        throw error; // Optionally rethrow or handle the error here
      })
    )
  }

  DeleteStage(id: number): Observable<any> {
    return this.API.http.delete<any>(`${this.API.baseUrl}/stages/${id}`).pipe(
      map(response => response.result),
      catchError(error => {
        console.error("Error deleting stage:", error);
        throw error; // Optionally rethrow or handle the error here
      })
    );
  }

  Update(id: number, update: updateStage): Observable<any> {
    return this.API.http.put<any>(`${this.API.baseUrl}/stages/${id}`, update).pipe(
      map(response => response.result),
      catchError(error => {
        console.error("Error updating stage:", error);
        throw error;
      })
    );
  }
  
  partialUpdate(id: number, patchDoc: any): Observable<any> {
    return this.API.http.patch<any>(`${this.API.baseUrl}/stages/${id}`, patchDoc).pipe(
      map(response => response.result),
      catchError(error => {
        console.error("Error with partial update:", error);
        throw error;
      })
    );
  }

}
