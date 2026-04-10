import { Component, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { map, of, switchMap } from 'rxjs';

import { AuthAPIService } from '../../../auth/authAPI.service';
import { DashboardService } from '../../../core/services/dashboard.service';
import { StudentDashboardSnapshot } from '../../../core/models/dashboard.model';
import { ApiResponse } from '../../../core/models/response.model';
import { selectLanguage } from '../../../core/store/language/language.selectors';

@Component({
  selector: 'app-student-home',
  templateUrl: './student-home.component.html',
  styleUrl: './student-home.component.scss',
})
export class StudentHomeComponent implements OnInit {
  private dashboardService = inject(DashboardService);
  private authApi = inject(AuthAPIService);
  private router = inject(Router);
  private store = inject(Store);

  readonly dir$ = this.store
    .select(selectLanguage)
    .pipe(map((l) => (l === 'ar' ? 'rtl' : 'ltr')));

  loading = true;
  errorMessage = '';
  snapshot: StudentDashboardSnapshot | null = null;

  ngOnInit(): void {
    if (typeof window !== 'undefined' && localStorage.getItem('userType') !== 'STUDENT') {
      void this.router.navigateByUrl('/school/dashboard', { replaceUrl: true });
      return;
    }

    this.authApi
      .ensureTenantContext()
      .pipe(
        switchMap((ok) => {
          if (!ok) {
            return of({ kind: 'no-tenant' as const });
          }
          return this.dashboardService.getStudentDashboardData().pipe(
            map((res) => ({ kind: 'data' as const, res })),
          );
        }),
      )
      .subscribe({
        next: (x) => {
          this.loading = false;
          if (x.kind === 'no-tenant') {
            this.errorMessage =
              'لم يتم ربط حسابك بمدرسة. سجّل الخروج ثم أعد تسجيل الدخول. إذا استمرت المشكلة، تأكد أن سجل الطالب موجود في المدرسة وأن حساب المستخدم مرتبط به.';
            return;
          }
          const res = x.res as ApiResponse<StudentDashboardSnapshot>;
          if (res.isSuccess && res.result) {
            this.snapshot = res.result as StudentDashboardSnapshot;
          } else {
            this.errorMessage = res.errorMasseges?.[0] || 'تعذر تحميل بيانات الطالب.';
          }
        },
        error: () => {
          this.loading = false;
          this.errorMessage = 'تعذر الاتصال بالخادم.';
        },
      });
  }

  /** First letter for avatar circle (same idea as header notifications). */
  avatarLetter(name: string): string {
    const t = (name || '').trim();
    if (!t) {
      return '?';
    }
    return t.charAt(0).toUpperCase();
  }
}
