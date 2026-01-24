import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { VouchersGuardian } from '../models/vouchers-guardian.model';
import { VoucherService } from './voucher.service';
import { ApiResponse } from '../../../../core/models/response.model';

@Injectable({
  providedIn: 'root'
})
export class VouchersGuardianStoreService {
  // Store vouchers by guardianID
  private vouchersByGuardian = new Map<number, VouchersGuardian[]>();
  
  // BehaviorSubject for current guardian's vouchers
  private currentVouchersSubject = new BehaviorSubject<VouchersGuardian[]>([]);
  public currentVouchers$ = this.currentVouchersSubject.asObservable();
  
  // BehaviorSubject for loading state
  private loadingSubject = new BehaviorSubject<boolean>(false);
  public loading$ = this.loadingSubject.asObservable();
  
  // Track current guardian ID
  private currentGuardianID: number | undefined;

  constructor(private voucherService: VoucherService) {}

  /**
   * Get vouchers for a specific guardian
   * Returns cached data if available, otherwise fetches from API
   */
  getVouchersGuardian(guardianID?: number): Observable<VouchersGuardian[]> {
    // If no guardianID, return empty array
    if (!guardianID || guardianID <= 0) {
      this.currentGuardianID = undefined;
      this.currentVouchersSubject.next([]);
      return of([]);
    }

    // If same guardian and data is cached, return cached data
    if (this.currentGuardianID === guardianID && this.vouchersByGuardian.has(guardianID)) {
      const cachedData = this.vouchersByGuardian.get(guardianID)!;
      this.currentVouchersSubject.next(cachedData);
      return of(cachedData);
    }

    // If data is cached for this guardian, use it
    if (this.vouchersByGuardian.has(guardianID)) {
      const cachedData = this.vouchersByGuardian.get(guardianID)!;
      this.currentGuardianID = guardianID;
      this.currentVouchersSubject.next(cachedData);
      return of(cachedData);
    }

    // Fetch from API
    this.loadingSubject.next(true);
    this.currentGuardianID = guardianID;

    return this.voucherService.getAllVouchersGuardian(guardianID).pipe(
      map((response: ApiResponse<VouchersGuardian[]>) => {
        if (!response.isSuccess) {
          return [];
        }
        return response.result || [];
      }),
      tap((vouchers: VouchersGuardian[]) => {
        // Cache the data
        this.vouchersByGuardian.set(guardianID, vouchers);
        // Emit to subscribers
        this.currentVouchersSubject.next(vouchers);
        this.loadingSubject.next(false);
      }),
      tap({
        error: () => {
          this.loadingSubject.next(false);
        }
      })
    );
  }

  /**
   * Clear vouchers for a specific guardian (force refresh on next call)
   */
  clearVouchersForGuardian(guardianID: number): void {
    this.vouchersByGuardian.delete(guardianID);
    if (this.currentGuardianID === guardianID) {
      this.currentVouchersSubject.next([]);
      this.currentGuardianID = undefined;
    }
  }

  /**
   * Clear all cached vouchers
   */
  clearAllVouchers(): void {
    this.vouchersByGuardian.clear();
    this.currentVouchersSubject.next([]);
    this.currentGuardianID = undefined;
  }

  /**
   * Get current vouchers synchronously
   */
  getCurrentVouchers(): VouchersGuardian[] {
    return this.currentVouchersSubject.value;
  }
}

