import { Component, inject, OnInit } from '@angular/core';
import { finalize } from 'rxjs';
import { FormsModule } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';

import { ShardModule } from '../../../../shared/shard.module';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { RegistrationRequestService } from '../../../../core/services/registration-request.service';
import { PendingRegistrationRequest } from '../../../../core/models/registration-request.model';

@Component({
  selector: 'app-pending-registration-requests',
  standalone: true,
  imports: [ShardModule, TableModule, ButtonModule, FormsModule],
  templateUrl: './pending-registration-requests.component.html',
  styleUrls: ['./pending-registration-requests.component.scss'],
})
export class PendingRegistrationRequestsComponent implements OnInit {
  private readonly registration = inject(RegistrationRequestService);
  private readonly toastr = inject(ToastrService);

  rows: PendingRegistrationRequest[] = [];
  loading = false;
  busyId: number | null = null;

  rejectReason = '';
  rejectForId: number | null = null;

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
        },
        error: (err) => {
          const msg = err?.error?.message || err?.statusText || 'تعذر تحميل الطلبات';
          this.toastr.error(msg);
        },
      });
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
  }

  cancelReject(): void {
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
          this.cancelReject();
        },
        error: (err) => {
          const msg = err?.error?.message || 'فشل الرفض';
          this.toastr.error(msg);
        },
      });
  }
}
