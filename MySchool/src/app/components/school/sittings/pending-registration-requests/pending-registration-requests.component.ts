import { Component, inject, OnInit } from '@angular/core';
import { finalize, map } from 'rxjs';
import { FormsModule } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { Store } from '@ngrx/store';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDividerModule } from '@angular/material/divider';
import { PaginatorModule } from 'primeng/paginator';
import { PaginatorState } from 'primeng/paginator';
import { ButtonModule } from 'primeng/button';

import { ShardModule } from 'app/shared/shard.module';
import { selectLanguage } from 'app/core/store/language/language.selectors';
import { RegistrationRequestService } from 'app/core/services/registration-request.service';
import {
  ApproveRegistrationPayload,
  PendingRegistrationRequest,
} from 'app/core/models/registration-request.model';
import { DivisionService } from '../../core/services/division.service';
import { GuardianService } from '../../core/services/guardian.service';
import { divisions } from '../../core/models/division.model';
import { Guardians } from '../../core/models/guardian.model';

@Component({
  selector: 'app-pending-registration-requests',
  standalone: true,
  imports: [
    ShardModule,
    FormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDividerModule,
    PaginatorModule,
    ButtonModule,
  ],
  templateUrl: './pending-registration-requests.component.html',
  styleUrls: ['./pending-registration-requests.component.scss'],
})
export class PendingRegistrationRequestsComponent implements OnInit {
  private readonly registration = inject(RegistrationRequestService);
  private readonly toastr = inject(ToastrService);
  private readonly store = inject(Store);
  private readonly divisionService = inject(DivisionService);
  private readonly guardianService = inject(GuardianService);

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

  /** Approve student: class + division + guardian */
  approveDialogVisible = false;
  approveRow: PendingRegistrationRequest | null = null;
  allDivisions: divisions[] = [];
  classOptions: { classID: number; label: string }[] = [];
  divisionsForClass: divisions[] = [];
  guardiansList: Guardians[] = [];

  approveClassId: number | null = null;
  approveDivisionId: number | null = null;
  guardianMode: 'existing' | 'new' = 'existing';
  existingGuardianId: number | null = null;
  amount = 0;
  studentFirstName = '';
  studentMiddleName = '';
  studentLastName = '';
  guardianEmail = '';
  guardianPassword = 'Guardian';
  guardianFullName = '';
  guardianPhone = '';
  guardianGender = 'Male';
  guardianAddress = '';
  guardianType = '';

  ngOnInit(): void {
    this.load();
    this.loadLookupData();
  }

  private loadLookupData(): void {
    this.divisionService.GetAll().subscribe({
      next: (res) => {
        this.allDivisions = res.result ?? [];
        const map = new Map<number, string>();
        for (const d of this.allDivisions) {
          const label = [d.stageName, d.classesName].filter(Boolean).join(' · ') || `صف ${d.classID}`;
          map.set(d.classID, label);
        }
        this.classOptions = [...map.entries()].map(([classID, label]) => ({ classID, label }));
      },
      error: () => this.toastr.warning('تعذر تحميل الصفوف والشُعب'),
    });
    this.guardianService.getAllGuardians().subscribe({
      next: (res) => {
        this.guardiansList = res.result ?? [];
      },
      error: () => this.toastr.warning('تعذر تحميل قائمة أولياء الأمور'),
    });
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

  onApproveClassChange(classId: number | null): void {
    this.approveClassId = classId;
    this.approveDivisionId = null;
    if (classId == null) {
      this.divisionsForClass = [];
      return;
    }
    this.divisionsForClass = this.allDivisions.filter((d) => d.classID === classId);
    if (this.divisionsForClass.length === 1) {
      this.approveDivisionId = this.divisionsForClass[0].divisionID;
    }
  }

  openApproveStudent(row: PendingRegistrationRequest): void {
    this.approveRow = row;
    this.approveClassId = null;
    this.approveDivisionId = null;
    this.divisionsForClass = [];
    this.guardianMode = 'existing';
    this.existingGuardianId = null;
    this.amount = 0;
    this.guardianEmail = '';
    this.guardianPassword = 'Guardian';
    this.guardianFullName = '';
    this.guardianPhone = '';
    this.guardianGender = 'Male';
    this.guardianAddress = '';
    this.guardianType = '';
    const parts = (row.fullName ?? '').trim().split(/\s+/).filter(Boolean);
    if (parts.length >= 3) {
      this.studentFirstName = parts[0];
      this.studentMiddleName = parts.slice(1, -1).join(' ');
      this.studentLastName = parts[parts.length - 1];
    } else if (parts.length === 2) {
      this.studentFirstName = parts[0];
      this.studentMiddleName = '';
      this.studentLastName = parts[1];
    } else if (parts.length === 1) {
      this.studentFirstName = parts[0];
      this.studentMiddleName = '';
      this.studentLastName = '';
    } else {
      this.studentFirstName = row.userName;
      this.studentMiddleName = '';
      this.studentLastName = '';
    }
    this.approveDialogVisible = true;
  }

  cancelApproveStudent(): void {
    this.approveDialogVisible = false;
    this.approveRow = null;
  }

  confirmApproveStudent(): void {
    if (this.approveRow == null || this.busyId != null) {
      return;
    }
    if (this.approveDivisionId == null || this.approveDivisionId <= 0) {
      this.toastr.warning('اختر الصف ثم الشعبة');
      return;
    }
    if (this.guardianMode === 'existing') {
      if (this.existingGuardianId == null || this.existingGuardianId <= 0) {
        this.toastr.warning('اختر ولي أمراً من القائمة');
        return;
      }
    } else {
      if (!this.guardianEmail?.trim() || !this.guardianFullName?.trim()) {
        this.toastr.warning('أدخل البريد واسم ولي الأمر الجديد');
        return;
      }
    }

    const id = this.approveRow.id;
    const body: ApproveRegistrationPayload = {
      divisionID: this.approveDivisionId,
      amount: this.amount,
      studentFirstName: this.studentFirstName.trim() || undefined,
      studentMiddleName: this.studentMiddleName.trim() || undefined,
      studentLastName: this.studentLastName.trim() || undefined,
    };

    if (this.guardianMode === 'existing') {
      body.existingGuardianId = this.existingGuardianId;
    } else {
      body.guardianEmail = this.guardianEmail.trim();
      body.guardianPassword = this.guardianPassword || 'Guardian';
      body.guardianFullName = this.guardianFullName.trim();
      body.guardianPhone = this.guardianPhone.trim() || undefined;
      body.guardianGender = this.guardianGender || 'Male';
      body.guardianAddress = this.guardianAddress.trim() || undefined;
      body.guardianType = this.guardianType.trim() || undefined;
    }

    this.busyId = id;
    this.registration
      .approveRequest(id, body)
      .pipe(finalize(() => (this.busyId = null)))
      .subscribe({
        next: () => {
          this.toastr.success('تمت الموافقة وتسجيل الطالب');
          this.rows = this.rows.filter((r) => r.id !== id);
          this.clampPage();
          this.cancelApproveStudent();
        },
        error: (err) => {
          const msg = err?.error?.message || err?.error?.error || 'فشلت الموافقة';
          this.toastr.error(typeof msg === 'string' ? msg : JSON.stringify(msg));
        },
      });
  }

  /** ولي أمر طلب التسجيل: موافقة مباشرة بدون صف وشعبة */
  approveGuardian(row: PendingRegistrationRequest): void {
    if (this.busyId != null) {
      return;
    }
    this.busyId = row.id;
    this.registration
      .approveRequest(row.id, {})
      .pipe(finalize(() => (this.busyId = null)))
      .subscribe({
        next: () => {
          this.toastr.success('تمت الموافقة وإنشاء الحساب');
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
