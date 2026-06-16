import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BackendAspService } from '../../../../ASP.NET/backend-asp.service';
import { Fee } from '../models/Fee.model';
import { ApiResponse } from '../../../../core/models/response.model';

@Injectable({
  providedIn: 'root'
})
export class FeeService {
  private API = inject(BackendAspService);

  getAllFee(): Observable<ApiResponse<Fee[]>> {
    return this.API.getRequest<Fee[]>('Fees');
  }

  AddFee(newFee: Fee): Observable<ApiResponse<Fee>> {
    return this.API.postRequest<Fee>('Fees', newFee);
  }

  DeleteFee(id: number): Observable<ApiResponse<any>> {
    return this.API.deleteRequest<any>(`Fees/${id}`);
  }

  Update(id: number, fee: Fee): Observable<ApiResponse<Fee>> {
    return this.API.putRequest<Fee>(`Fees/${id}`, fee);
  }
  partialUpdate(id: number, patchDoc: any): Observable<ApiResponse<any>> {
    return this.API.patchRequest<any>(`Fees/${id}`, patchDoc);
  }
}