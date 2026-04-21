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
import { StudentExamsComponent } from './student-exams/student-exams.component';
import { StudentHomeworkComponent } from './student-homework/student-homework.component';

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
      { path: 'exams', component: StudentExamsComponent, data: { breadcrumb: 'الامتحانات' } },
      { path: 'homework', component: StudentHomeworkComponent, data: { breadcrumb: 'الواجبات' } },
      { path: 'grades/month', component: GradesMonthComponent, data: { breadcrumb: 'الدرجات الشهرية' } },
      { path: 'grades/term', component: GradesTermComponent, data: { breadcrumb: 'الدرجات الفصلية' } },
      {
        path: 'reports/monthly',
        component: StudentMonthResultComponent,
        data: { breadcrumb: 'تقارير شهرية' },
      },
      { path: 'attendance', component: AttendanceComponent, data: { breadcrumb: 'الحضور والغياب' } },
      {
        path: 'daily-evaluations',
        data: { breadcrumb: 'التقييم اليومي' },
        children: [
          {
            path: 'new',
            loadComponent: () =>
              import('../school/daily-evaluations/daily-evaluations-form/daily-evaluations-form.component').then(
                (m) => m.DailyEvaluationsFormComponent,
              ),
            data: { breadcrumb: 'تقييم يومي جديد' },
          },
          {
            path: ':evaluationId/edit',
            loadComponent: () =>
              import('../school/daily-evaluations/daily-evaluations-form/daily-evaluations-form.component').then(
                (m) => m.DailyEvaluationsFormComponent,
              ),
            data: { breadcrumb: 'تعديل تقييم يومي' },
          },
          {
            path: ':evaluationId',
            loadComponent: () =>
              import('../school/daily-evaluations/daily-evaluations-detail/daily-evaluations-detail.component').then(
                (m) => m.DailyEvaluationsDetailComponent,
              ),
            data: { breadcrumb: 'تقييم يومي' },
          },
          {
            path: '',
            loadComponent: () =>
              import('../school/daily-evaluations/daily-evaluations-list/daily-evaluations-list.component').then(
                (m) => m.DailyEvaluationsListComponent,
              ),
            data: { breadcrumb: 'التقييمات اليومية' },
          },
          { path: 'not-found', component: PageNotFoundComponent },
          { path: '**', redirectTo: 'not-found', pathMatch: 'full' },
        ],
      },
      {
        path: 'teacher-feedback',
        data: { breadcrumb: 'تقييم المعلم', tfParticipant: 'student' },
        children: [
          {
            path: '',
            loadComponent: () =>
              import('../school/teacher-feedback/teacher-feedback-portal/teacher-feedback-portal.component').then(
                (m) => m.TeacherFeedbackPortalComponent,
              ),
            data: { breadcrumb: 'الاستبيانات المفتوحة' },
          },
          {
            path: 'fill/:cycleId',
            loadComponent: () =>
              import('../school/teacher-feedback/teacher-feedback-fill/teacher-feedback-fill.component').then(
                (m) => m.TeacherFeedbackFillComponent,
              ),
            data: { breadcrumb: 'تعبئة الاستبيان' },
          },
          { path: 'not-found', component: PageNotFoundComponent },
          { path: '**', redirectTo: 'not-found', pathMatch: 'full' },
        ],
      },
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
