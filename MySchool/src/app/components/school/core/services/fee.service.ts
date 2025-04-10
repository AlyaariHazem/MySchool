import { inject, Injectable } from '@angular/core';
import { BackendAspService } from '../../../../environments/ASP.NET/backend-asp.service';
import {  Observable } from 'rxjs';

import { Fee } from '../models/Fee.model';

@Injectable({
  providedIn: 'root'
})
export class FeeService {
  private ApI=inject(BackendAspService)

  getAllFee():Observable<any>{
    return this.ApI.getRequest<Fee[]>("Fees");
  }

 AddFee(newFee:Fee):Observable<any>{
  return this.ApI.postRequest<Fee>("Fees",newFee);
 }

 DeleteFee(id:number):Observable<any>{
  return this.ApI.deleteRequest<Fee>(`Fees/${id}`);
 }

 Update(id:number,fee:Fee):Observable<any>{
  return this.ApI.putRequest<Fee>(`Fees/${id}`,fee);
 }

}
