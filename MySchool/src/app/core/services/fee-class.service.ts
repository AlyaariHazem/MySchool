import { inject, Injectable } from '@angular/core';
import { catchError, map, Observable } from 'rxjs';

import { BackendAspService } from '../../environments/ASP.NET/backend-asp.service';
import { FeeClass } from '../models/Fee.model';

@Injectable({
  providedIn: 'root'
})
export class FeeClassService {
  private API = inject(BackendAspService);
  constructor() { }

  getAllFeeClass(): Observable<any> {
    return this.API.http.get(`${this.API.baseUrl}/FeeClass`).pipe(
      map(res => res),
      catchError(error => {
        throw error;
      })
    );
  }
  AddFeeClass(feeClass: FeeClass): Observable<any> {
    return this.API.http.post(`${this.API.baseUrl}/FeeClass`, feeClass).pipe(
      catchError(error => {
        console.error("Error adding Fee:", error);
        throw error;
      })
    );
  }
  UpdateFeeCass(ClassID:number,FeeID:number,feeClass:FeeClass):Observable<any>{
    return this.API.http.put(`${this.API.baseUrl}/FeeClass/${ClassID}/${FeeID}`,feeClass).pipe(
      map(res=>res),
      catchError(err => {
        console.error("Error updating FeeClass:", err);
        throw err;
      })
    );
  }

  DeleteFeeClass(ClassID:number,FeeID:number):Observable<any>{
    return this.API.http.delete(`${this.API.baseUrl}/FeeClass/${ClassID}/${FeeID}`).pipe(
      map(res=>res)
    );
  }
  GetByID(ClassID:number,FeeID:number):Observable<any>{
    return this.API.http.get(`${this.API.baseUrl}/FeeClass/${ClassID}/${FeeID}`).pipe(
      map(res=>res)
    );
  }
}
