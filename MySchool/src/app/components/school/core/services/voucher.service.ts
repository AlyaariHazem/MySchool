import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { BackendAspService } from '../../../../environments/ASP.NET/backend-asp.service';
import { Voucher, VoucherAdd } from '../models/voucher.model';

@Injectable({
  providedIn: 'root'
})
export class VoucherService {

  private API = inject(BackendAspService);

  constructor() { }
  getAll(): Observable<Voucher[]> {
    return this.API.getRequest<Voucher[]>("Vouchers");
  }
  Add(voucher: VoucherAdd): Observable<string> {
    return this.API.postRequest<string>("Vouchers", voucher);
  }
  Delete(id: number): Observable<any> {
    return this.API.deleteRequest<any>(`Vouchers/${id}`);
  }
  Update(id: number | undefined, voucher: any): Observable<any> {
    return this.API.putRequest<any>(`Vouchers/${id}`, voucher);
  }

}