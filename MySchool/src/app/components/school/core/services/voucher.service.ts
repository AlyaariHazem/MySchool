import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map, catchError } from 'rxjs/operators';

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

  getPaginatedVouchers(pageNumber: number = 1, pageSize: number = 4, filters: Record<string, string> = {}): Observable<any> {
    // Transform filters from Record<string, string> to backend FilterRequest format
    const filtersDict: Record<string, { value: string }> = {};
    Object.entries(filters).forEach(([key, value]) => {
      if (value && value.trim() !== '') {
        filtersDict[key] = { value: value };
      }
    });

    const requestBody = {
      pageNumber: pageNumber,
      pageSize: pageSize,
      filters: filtersDict
    };

    return this.API.http.post<any>(`${this.API.baseUrl}/Vouchers/page`, requestBody).pipe(
      map((response: any) => {
        // Handle both wrapped (APIResponse) and unwrapped responses
        const data = response.result || response;
        // Map PagedResult properties (C# uses PascalCase, but JSON might be camelCase)
        return {
          data: data.Data || data.data || [],
          pageNumber: data.PageNumber ?? data.pageNumber ?? pageNumber,
          pageSize: data.PageSize ?? data.pageSize ?? pageSize,
          totalCount: data.TotalCount ?? data.totalCount ?? 0,
          totalPages: data.TotalPages ?? data.totalPages ?? 0
        };
      }),
      catchError((error: any) => {
        console.error("Error fetching paginated Vouchers:", error);
        throw error;
      })
    );
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