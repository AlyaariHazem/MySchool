import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { PageNotFoundComponent } from '../../shared/components/page-not-found/page-not-found.component';
import { AttendanceComponent } from '../school/attendance/attendance.component';
import { GradesMonthComponent } from '../school/grades-mange/grades-month/grades-month.component';
import { GradesTermComponent } from '../school/grades-mange/grades-term/grades-term.component';
import { NotificationsComponent } from '../school/notifications/notifications.component';
import { StudentMonthResultComponent } from '../school/report/student-month-result/student-month-result.component';
import { WeeklyScheduleComponent } from '../school/weekly-schedule/weekly-schedule.component';
import { StudentHomeComponent } from './student-home/student-home.component';
import { StudentLayoutComponent } from './student-layout/student-layout.component';

const routes: Routes = [
  {
    path: '',
    component: StudentLayoutComponent,
    data: { breadcrumb: 'الرئيسية  / ' },
    children: [
      { path: '', redirectTo: 'home', pathMatch: 'full' },
      { path: 'home', component: StudentHomeComponent, data: { breadcrumb: 'الرئيسية' } },
      { path: 'notifications', component: NotificationsComponent, data: { breadcrumb: 'الإشعارات' } },
      { path: 'schedule', component: WeeklyScheduleComponent, data: { breadcrumb: 'جدول الحصص الأسبوعي' } },
      { path: 'grades/month', component: GradesMonthComponent, data: { breadcrumb: 'الدرجات الشهرية' } },
      { path: 'grades/term', component: GradesTermComponent, data: { breadcrumb: 'الدرجات الفصلية' } },
      {
        path: 'reports/monthly',
        component: StudentMonthResultComponent,
        data: { breadcrumb: 'تقارير شهرية' },
      },
      { path: 'attendance', component: AttendanceComponent, data: { breadcrumb: 'الحضور والغياب' } },
      { path: 'not-found', component: PageNotFoundComponent },
      { path: '**', redirectTo: 'not-found', pathMatch: 'full' },
    ],
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class StudentsRoutingModule {}
