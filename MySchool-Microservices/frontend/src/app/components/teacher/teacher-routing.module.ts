import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { permissionGuard } from '../../core/guards/permission.guard';
import { PagePermission } from '../../core/services/permission.service';
import { PageNotFoundComponent } from '../../shared/components/page-not-found/page-not-found.component';
import { AttendanceComponent } from '../school/attendance/attendance.component';
import { GradesMonthComponent } from '../school/grades-mange/grades-month/grades-month.component';
import { GradesTermComponent } from '../school/grades-mange/grades-term/grades-term.component';
import { NotificationsComponent } from '../school/notifications/notifications.component';
import { StudentMonthResultComponent } from '../school/report/student-month-result/student-month-result.component';
import { WeeklyScheduleComponent } from '../school/weekly-schedule/weekly-schedule.component';
import { TeacherLayoutComponent } from './teacher-layout/teacher-layout.component';
import { TeacherWorkspaceComponent } from './pages/teacher-workspace/teacher-workspace.component';
import { TeacherExamsComponent } from './pages/teacher-exams/teacher-exams.component';
import { TeacherHomeworkComponent } from './pages/teacher-homework/teacher-homework.component';

const routes: Routes = [
  {
    path: '',
    component: TeacherLayoutComponent,
    data: { breadcrumb: 'الرئيسية  / ' },
    children: [
      { path: '', redirectTo: 'workspace', pathMatch: 'full' },
      {
        path: 'workspace',
        component: TeacherWorkspaceComponent,
        data: { breadcrumb: 'مساحة المعلم' },
      },
      {
        path: 'time-capsule',
        loadComponent: () =>
          import('./pages/teacher-time-capsule-redirect/teacher-time-capsule-redirect.component').then(
            (m) => m.TeacherTimeCapsuleRedirectComponent,
          ),
        data: { breadcrumb: 'كبسولة الزمن' },
      },
      {
        path: 'time-capsule/:id',
        loadComponent: () =>
          import('../school/employees-hr/time-capsule/employees-hr-time-capsule.component').then(
            (m) => m.EmployeesHrTimeCapsuleComponent,
          ),
        data: { breadcrumb: 'كبسولة الزمن', timeCapsuleTeacherShell: true },
      },
      { path: 'attendance', component: AttendanceComponent, data: { breadcrumb: 'الحضور والغياب' } },
      {
        path: 'schedule',
        component: WeeklyScheduleComponent,
        data: { breadcrumb: 'جدول الحصص الأسبوعي', teacherPersonalSchedule: true },
      },
      { path: 'exams', component: TeacherExamsComponent, data: { breadcrumb: 'الامتحانات' } },
      { path: 'homework', component: TeacherHomeworkComponent, data: { breadcrumb: 'الواجبات' } },
      { path: 'grades/month', component: GradesMonthComponent, data: { breadcrumb: 'الدرجات الشهرية' } },
      { path: 'grades/term', component: GradesTermComponent, data: { breadcrumb: 'الدرجات الفصلية' } },
      {
        path: 'reports/monthly',
        component: StudentMonthResultComponent,
        data: { breadcrumb: 'تقارير شهرية' },
      },
      {
        path: 'daily-evaluations',
        data: { breadcrumb: 'التقييم اليومي' },
        children: [
          {
            path: 'templates/new',
            loadComponent: () =>
              import('../school/daily-evaluations/daily-eval-template-form/daily-eval-template-form.component').then(
                (m) => m.DailyEvalTemplateFormComponent,
              ),
            data: { breadcrumb: 'قالب تقييم جديد', permission: PagePermission.Evaluations.Update },
            canMatch: [permissionGuard],
          },
          {
            path: 'templates/:templateId/edit',
            loadComponent: () =>
              import('../school/daily-evaluations/daily-eval-template-form/daily-eval-template-form.component').then(
                (m) => m.DailyEvalTemplateFormComponent,
              ),
            data: { breadcrumb: 'تعديل قالب', permission: PagePermission.Evaluations.Update },
            canMatch: [permissionGuard],
          },
          {
            path: 'templates/:templateId',
            loadComponent: () =>
              import('../school/daily-evaluations/daily-eval-template-detail/daily-eval-template-detail.component').then(
                (m) => m.DailyEvalTemplateDetailComponent,
              ),
            data: { breadcrumb: 'قالب تقييم', permission: PagePermission.Evaluations.View },
            canMatch: [permissionGuard],
          },
          {
            path: 'templates',
            loadComponent: () =>
              import('../school/daily-evaluations/daily-eval-templates-list/daily-eval-templates-list.component').then(
                (m) => m.DailyEvalTemplatesListComponent,
              ),
            data: { breadcrumb: 'قوالب التقييم اليومي', permission: PagePermission.Evaluations.View },
            canMatch: [permissionGuard],
          },
          {
            path: 'new',
            loadComponent: () =>
              import('../school/daily-evaluations/daily-evaluations-form/daily-evaluations-form.component').then(
                (m) => m.DailyEvaluationsFormComponent,
              ),
            data: { breadcrumb: 'تقييم يومي جديد', permission: PagePermission.Evaluations.Create },
            canMatch: [permissionGuard],
          },
          {
            path: ':evaluationId/edit',
            loadComponent: () =>
              import('../school/daily-evaluations/daily-evaluations-form/daily-evaluations-form.component').then(
                (m) => m.DailyEvaluationsFormComponent,
              ),
            data: { breadcrumb: 'تعديل تقييم يومي', permission: PagePermission.Evaluations.Update },
            canMatch: [permissionGuard],
          },
          {
            path: ':evaluationId',
            loadComponent: () =>
              import('../school/daily-evaluations/daily-evaluations-detail/daily-evaluations-detail.component').then(
                (m) => m.DailyEvaluationsDetailComponent,
              ),
            data: { breadcrumb: 'تقييم يومي', permission: PagePermission.Evaluations.View },
            canMatch: [permissionGuard],
          },
          {
            path: '',
            loadComponent: () =>
              import('../school/daily-evaluations/daily-evaluations-list/daily-evaluations-list.component').then(
                (m) => m.DailyEvaluationsListComponent,
              ),
            data: { breadcrumb: 'التقييمات اليومية', permission: PagePermission.Evaluations.View },
            canMatch: [permissionGuard],
          },
          { path: 'not-found', component: PageNotFoundComponent },
          { path: '**', redirectTo: 'not-found', pathMatch: 'full' },
        ],
      },
      { path: 'notifications', component: NotificationsComponent, data: { breadcrumb: 'الإشعارات' } },
      { path: 'not-found', component: PageNotFoundComponent },
      { path: '**', redirectTo: 'not-found', pathMatch: 'full' },
    ],
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class TeacherRoutingModule {}
