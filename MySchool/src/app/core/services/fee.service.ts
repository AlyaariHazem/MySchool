import { inject, Injectable } from '@angular/core';
import { BackendAspService } from '../../environments/ASP.NET/backend-asp.service';
import { catchError, map, Observable } from 'rxjs';

import { Fee } from '../models/Fee.model';

@Injectable({
  providedIn: 'root'
})
export class FeeService {
  private ApI=inject(BackendAspService)
  constructor() { }

  getAllFee():Observable<any>{
    return this.ApI.http.get(`${this.ApI.baseUrl}/Fees`).pipe(
      map(res=>res),
      catchError(error=>{
        throw error;
      })
    );
  }
 AddFee(newFee:Fee):Observable<any>{
  return this.ApI.http.post(`${this.ApI.baseUrl}/Fees`,newFee).pipe(
    catchError(error => {
      console.error("Error adding Fee:", error);
      throw error;
    })
  )
 }

 DeleteFee(id:number):Observable<any>{
  return this.ApI.http.delete(`${this.ApI.baseUrl}/Fees/${id}`).pipe(
    catchError(err=>{
      console.error("Error Delete Fee:", err);
      throw err;
    })
    );
 }

 Update(id:number,fee:Fee):Observable<any>{
  return this.ApI.http.put(`${this.ApI.baseUrl}/Fees/${id}`,fee).pipe(
    map(res=>res),
    catchError(err => {
      console.error("Error updating Fee:", err);
      throw err;
    })
  );
 }

}
