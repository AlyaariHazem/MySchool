import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, of, forkJoin } from 'rxjs';
import { map, tap, switchMap, catchError } from 'rxjs/operators';
import { VouchersGuardian } from '../models/vouchers-guardian.model';
import { VoucherService } from './voucher.service';

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

    // Fetch from API (POST body + pagination; load all pages up to backend max page size)
    this.loadingSubject.next(true);
    this.currentGuardianID = guardianID;

    const pageSize = 100;

    return this.voucherService.getVouchersGuardianPage(guardianID, 1, pageSize).pipe(
      switchMap((first) => {
        const firstRows = [...(first.data || [])];
        const totalPages = Math.max(1, first.totalPages ?? 1);
        if (totalPages <= 1) {
          return of(firstRows);
        }
        const restCalls = [];
        for (let p = 2; p <= totalPages; p++) {
          restCalls.push(this.voucherService.getVouchersGuardianPage(guardianID, p, pageSize));
        }
        return forkJoin(restCalls).pipe(
          map((pages) => {
            const rest = pages.flatMap((r) => r.data || []);
            return [...firstRows, ...rest];
          }),
        );
      }),
      tap((vouchers: VouchersGuardian[]) => {
        this.vouchersByGuardian.set(guardianID, vouchers);
        this.currentVouchersSubject.next(vouchers);
        this.loadingSubject.next(false);
      }),
      catchError(() => {
        this.loadingSubject.next(false);
        return of([]);
      }),
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

