import { Component, inject, OnInit } from '@angular/core';
import { finalize, map } from 'rxjs';
import { FormsModule } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { Store } from '@ngrx/store';
import { MatCardModule } from '@angular/material/card';
import { PaginatorModule } from 'primeng/paginator';
import { PaginatorState } from 'primeng/paginator';
import { ButtonModule } from 'primeng/button';

import { ShardModule } from 'app/shared/shard.module';
import { selectLanguage } from 'app/core/store/language/language.selectors';
import { RegistrationRequestService } from 'app/core/services/registration-request.service';
import { PendingRegistrationRequest } from 'app/core/models/registration-request.model';

@Component({
  selector: 'app-pending-registration-requests',
  standalone: true,
  imports: [ShardModule, FormsModule, MatCardModule, PaginatorModule, ButtonModule],
  templateUrl: './pending-registration-requests.component.html',
  styleUrls: ['./pending-registration-requests.component.scss'],
})
export class PendingRegistrationRequestsComponent implements OnInit {
  private readonly registration = inject(RegistrationRequestService);
  private readonly toastr = inject(ToastrService);
  private readonly store = inject(Store);

  readonly dir$ = this.store.select(selectLanguage).pipe(
    map((l) => (l === 'ar' ? 'rtl' : 'ltr')),
  );

  rows: PendingRegistrationRequest[] = [];
  loading = false;
  busyId: number | null = null;

  first = 0;
  rowsPerPage = 10;

  filters = {
    userName: '',
    phoneNumber: '',
    gender: '',
    dob: '',
    role: '',
    schoolName: '',
    createdAt: '',
    attachments: '',
  };

  rejectReason = '';
  rejectForId: number | null = null;
  rejectDialogVisible = false;

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.registration
      .getPendingRequests()
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (list) => {
          this.rows = Array.isArray(list) ? list : [];
          this.first = 0;
          this.clampPage();
        },
        error: (err) => {
          const msg = err?.error?.message || err?.statusText || 'تعذر تحميل الطلبات';
          this.toastr.error(msg);
        },
      });
  }

  onFilterChange(): void {
    this.first = 0;
  }

  onPageChange(event: PaginatorState): void {
    this.first = event.first ?? 0;
    this.rowsPerPage = event.rows ?? this.rowsPerPage;
  }

  get filteredRows(): PendingRegistrationRequest[] {
    const f = this.filters;
    const match = (needle: string, haystack: string) =>
      !needle.trim() || haystack.toLowerCase().includes(needle.trim().toLowerCase());

    return this.rows.filter((row) => {
      if (!match(f.userName, row.userName || '')) return false;
      if (!match(f.phoneNumber, row.phoneNumber || '')) return false;
      if (!match(f.gender, row.gender || '')) return false;
      if (!match(f.dob, this.formatDob(row.dateOfBirth))) return false;
      if (!match(f.role, this.roleLabel(row.requestedRole))) return false;
      if (!match(f.schoolName, row.schoolName || '')) return false;
      if (!match(f.createdAt, this.formatDate(row.createdAt))) return false;
      const attachNames = (row.attachments ?? []).map((a) => a.fileName ?? '').join(' ');
      if (!match(f.attachments, attachNames)) return false;
      return true;
    });
  }

  get pagedRows(): PendingRegistrationRequest[] {
    const fr = this.filteredRows;
    return fr.slice(this.first, this.first + this.rowsPerPage);
  }

  get totalFiltered(): number {
    return this.filteredRows.length;
  }

  private clampPage(): void {
    const total = this.filteredRows.length;
    if (total === 0) {
      this.first = 0;
      return;
    }
    if (this.first >= total) {
      this.first = Math.max(0, total - this.rowsPerPage);
    }
  }

  formatDate(iso: string): string {
    try {
      return new Date(iso).toLocaleString('ar');
    } catch {
      return iso;
    }
  }

  formatDob(iso: string | null | undefined): string {
    if (iso == null || iso === '') {
      return '—';
    }
    try {
      return new Date(iso).toLocaleDateString('ar');
    } catch {
      return iso;
    }
  }

  roleLabel(role: string): string {
    if (role === 'STUDENT') return 'طالب';
    if (role === 'GUARDIAN') return 'ولي أمر';
    return role;
  }

  isPdfFile(fileName: string | null | undefined): boolean {
    return (fileName ?? '').toLowerCase().endsWith('.pdf');
  }

  approve(row: PendingRegistrationRequest): void {
    if (this.busyId != null) {
      return;
    }
    this.busyId = row.id;
    this.registration
      .approveRequest(row.id)
      .pipe(finalize(() => (this.busyId = null)))
      .subscribe({
        next: () => {
          this.toastr.success('تمت الموافقة وإنشاء المستخدم');
          this.rows = this.rows.filter((r) => r.id !== row.id);
          this.clampPage();
        },
        error: (err) => {
          const msg = err?.error?.message || 'فشلت الموافقة';
          this.toastr.error(msg);
        },
      });
  }

  openReject(row: PendingRegistrationRequest): void {
    this.rejectForId = row.id;
    this.rejectReason = '';
    this.rejectDialogVisible = true;
  }

  cancelReject(): void {
    this.rejectDialogVisible = false;
    this.rejectForId = null;
    this.rejectReason = '';
  }

  confirmReject(): void {
    if (this.rejectForId == null || this.busyId != null) {
      return;
    }
    const id = this.rejectForId;
    this.busyId = id;
    this.registration
      .rejectRequest(id, this.rejectReason.trim() || null)
      .pipe(finalize(() => (this.busyId = null)))
      .subscribe({
        next: () => {
          this.toastr.success('تم رفض الطلب');
          this.rows = this.rows.filter((r) => r.id !== id);
          this.clampPage();
          this.cancelReject();
        },
        error: (err) => {
          const msg = err?.error?.message || 'فشل الرفض';
          this.toastr.error(msg);
        },
      });
  }
}
