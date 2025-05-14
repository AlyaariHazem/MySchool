import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { BackendAspService } from '../../../../ASP.NET/backend-asp.service';
import { Voucher, VoucherAddUpdate } from '../models/voucher.model';
import { VouchersGuardian } from '../models/vouchers-guardian.model';
import { ApiResponse } from '../../../../core/models/response.model';

@Injectable({
  providedIn: 'root'
})
export class VoucherService {

  private API = inject(BackendAspService);

  getAll(): Observable<ApiResponse<Voucher[]>> {
    return this.API.getRequest<Voucher[]>('Vouchers');
  }

  getAllVouchersGuardian(): Observable<ApiResponse<VouchersGuardian[]>> {
    return this.API.getRequest<VouchersGuardian[]>('Vouchers/vouchersGuardian');
  }

  Add(voucher: VoucherAddUpdate): Observable<ApiResponse<string>> {
    return this.API.postRequest<string>('Vouchers', voucher);
  }

  Delete(id: number): Observable<ApiResponse<any>> {
    return this.API.deleteRequest<any>(`Vouchers/${id}`);
  }

  Update(id: number | undefined, voucher: VoucherAddUpdate): Observable<ApiResponse<any>> {
    return this.API.putRequest<any>(`Vouchers/${id}`, voucher);
  }
}