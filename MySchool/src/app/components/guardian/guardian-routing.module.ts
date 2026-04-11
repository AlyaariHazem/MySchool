import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { PageNotFoundComponent } from '../../shared/components/page-not-found/page-not-found.component';
import { NotificationsComponent } from '../school/notifications/notifications.component';
import { GuardianHomeComponent } from './guardian-home/guardian-home.component';
import { GuardianLayoutComponent } from './guardian-layout/guardian-layout.component';
import { GuardianHomeworkComponent } from './guardian-homework/guardian-homework.component';
import { GuardianExamsComponent } from './guardian-exams/guardian-exams.component';
import { GuardianAttendanceComponent } from './guardian-attendance/guardian-attendance.component';
import { GuardianMonthlyReportsComponent } from './guardian-monthly-reports/guardian-monthly-reports.component';
import { GuardianStubPageComponent } from './guardian-stub-page/guardian-stub-page.component';
import { GuardianStudentMonthResultComponent } from './guardian-student-month-result/guardian-student-month-result.component';

const routes: Routes = [
  {
    path: '',
    component: GuardianLayoutComponent,
    data: { breadcrumb: 'الرئيسية  / ' },
    children: [
      { path: '', redirectTo: 'home', pathMatch: 'full' },
      { path: 'home', component: GuardianHomeComponent, data: { breadcrumb: 'الرئيسية' } },
      { path: 'notifications', component: NotificationsComponent, data: { breadcrumb: 'الإشعارات' } },
      { path: 'exams', component: GuardianExamsComponent, data: { breadcrumb: 'الامتحانات' } },
      { path: 'homework', component: GuardianHomeworkComponent, data: { breadcrumb: 'الواجبات' } },
      {
        path: 'grades/month',
        component: GuardianMonthlyReportsComponent,
        data: {
          breadcrumb: 'الدرجات الشهرية',
          pageTitle: 'الدرجات الشهرية للأبناء',
        },
      },
      {
        path: 'grades/term',
        component: GuardianStubPageComponent,
        data: {
          breadcrumb: 'الدرجات الفصلية',
          stubTitle: 'الدرجات الفصلية للأبناء',
          stubMessage: 'عرض الدرجات الفصلية لأبنائك سيتم تفعيله في تحديث قادم.',
        },
      },
      {
        path: 'reports/monthly',
        component: GuardianStudentMonthResultComponent,
        data: {
          breadcrumb: 'تقارير شهرية',
          pageTitle: 'تقارير شهرية',
          showPrintableCertificate: true,
        },
      },
      { path: 'attendance', component: GuardianAttendanceComponent, data: { breadcrumb: 'الحضور والغياب' } },
      { path: 'not-found', component: PageNotFoundComponent },
      { path: '**', redirectTo: 'not-found', pathMatch: 'full' },
    ],
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class GuardianRoutingModule {}
