import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { Store } from '@ngrx/store';
import { ToastrService } from 'ngx-toastr';
import { map } from 'rxjs';

import { AttendanceService } from '../../school/core/services/attendance.service';
import { AttendanceDto, AttendanceStatus } from '../../school/core/models/attendance.model';
import { selectLanguage } from '../../../core/store/language/language.selectors';

@Component({
  selector: 'app-guardian-attendance',
  templateUrl: './guardian-attendance.component.html',
  styleUrls: ['./guardian-attendance.component.scss'],
})
export class GuardianAttendanceComponent implements OnInit {
  private readonly attendance = inject(AttendanceService);
  private readonly toastr = inject(ToastrService);
  private readonly fb = inject(FormBuilder);
  private readonly store = inject(Store);

  readonly dir$ = this.store.select(selectLanguage).pipe(map((l) => (l === 'ar' ? 'rtl' : 'ltr')));

  filterForm = this.fb.group({
    fromDate: [this.defaultFrom()],
    toDate: [this.defaultTo()],
  });

  rows: AttendanceDto[] = [];
  loading = false;

  ngOnInit(): void {
    this.load();
  }

  private defaultTo(): Date {
    return new Date();
  }

  private defaultFrom(): Date {
    const d = new Date();
    d.setDate(d.getDate() - 30);
    return d;
  }

  private toIsoDate(d: Date | null | undefined): string | undefined {
    if (!d || !(d instanceof Date) || Number.isNaN(d.getTime())) {
      return undefined;
    }
    const y = d.getFullYear();
    const m = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${y}-${m}-${day}`;
  }

  load(): void {
    const from = this.toIsoDate(this.filterForm.get('fromDate')?.value as Date);
    const to = this.toIsoDate(this.filterForm.get('toDate')?.value as Date);
    this.loading = true;
    this.attendance.getGuardianMy(from, to).subscribe({
      next: (r) => {
        this.rows = (r.result ?? []) as AttendanceDto[];
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.toastr.error('تعذر تحميل الحضور والغياب');
      },
    });
  }

  statusLabel(status: AttendanceStatus): string {
    switch (status) {
      case AttendanceStatus.Present:
        return 'حاضر';
      case AttendanceStatus.Absent:
        return 'غائب';
      case AttendanceStatus.Late:
        return 'متأخر';
      case AttendanceStatus.Excused:
        return 'معذور';
      default:
        return '—';
    }
  }

  statusClass(status: AttendanceStatus): string {
    switch (status) {
      case AttendanceStatus.Present:
        return 'ga-status--present';
      case AttendanceStatus.Absent:
        return 'ga-status--absent';
      case AttendanceStatus.Late:
        return 'ga-status--late';
      case AttendanceStatus.Excused:
        return 'ga-status--excused';
      default:
        return 'ga-status--default';
    }
  }
}
