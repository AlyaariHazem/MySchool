import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { DashboardComponent } from './dashboard/dashboard.component';
import { NavigateComponent } from './navigate/navigate.component';
import { TeachersComponent } from './teachers/teachers.component';
import { StudyYearComponent } from './sittings/study-year/study-year.component';
import { SchoolInfoComponent } from './sittings/school-info/school-info.component';
import { StagesGradesComponent } from './sittings/stages-grades/stages-grades.component';
import { StudentsComponent } from './students/students.component';
import { FeeClassComponent } from './sittings/fee-class/fee-class.component';
import { ActionComponent } from './action/action.component';
import { FeesComponent } from './fees/fees.component';
import { CoursesComponent } from './courses-and-plains/courses/courses.component';
import { PlainsComponent } from './courses-and-plains/plains/plains.component';
import { BooksComponent } from './courses-and-plains/books/books.component';
import { AllParentsComponent } from './parents/all-parents/all-parents.component';
import { AccountsComponent } from './accounts/accounts.component';
import { BillsComponent } from './accounts/bills/bills.component';
import { AllotmentComponent } from './allotment-report/allotment/allotment.component';
import { GradesMangeComponent } from './grades-mange/grades-mange-mange.component';
import { GradesMonthComponent } from './grades-mange/grades-month/grades-month.component';
import { PageNotFoundComponent } from '../../shared/components/page-not-found/page-not-found.component';
import { AccountReportComponent } from './report/account-report/account-report.component';
import { StudentMonthResultComponent } from './report/student-month-result/student-month-result.component';
import { RegistrationReportComponent } from './report/registration-report/registration-report.component';
import { TermResultComponent } from './report/termly-result/term-result.component';
import { GradesTermComponent } from './grades-mange/grades-term/grades-term.component';
import { BreadcrumbComponent } from './breadcrumb/breadcrumb.component';
import { StudentPromotionComponent } from './students/student-promotion/student-promotion.component';
import { WeeklyScheduleComponent } from './weekly-schedule/weekly-schedule.component';
import { AttendanceComponent } from './attendance/attendance.component';
import { DatabaseRestoreComponent } from './sittings/database-restore/database-restore.component';
import { NotificationsComponent } from './notifications/notifications.component';
import { SchoolLandingComponent } from './school-landing/school-landing.component';
import { ExamsAdminComponent } from './exams/exams-admin.component';
import { HomeworkAdminComponent } from './homework/homework-admin.component';
import { permissionGuard } from '../../core/guards/permission.guard';
import { PagePermission } from '../../core/services/permission.service';
const routes: Routes = [
  {
    path: '',
    component: NavigateComponent, data: { breadcrumb: 'الرئيسية  / ' },
    children: [
      {
        path: 'dashboard',
        component: DashboardComponent,
        data: { breadcrumb: '', permission: 'Dashboard.View' },
        canMatch: [permissionGuard],
      },
      { path: 'sidebar', component: BreadcrumbComponent, data: { breadcrumb: 'Sidebar' } },
      {
        path: 'reports',
        data: { breadcrumb: 'تقارير' },
        children: [
          {
            path: '',
            loadComponent: () =>
              import('./report/reports-entry/reports-entry.component').then((m) => m.ReportsEntryComponent),
          },
          {
            path: 'account',
            component: AccountReportComponent,
            data: { breadcrumb: 'حسابات', permission: 'ReportsFinancial.View' },
            canMatch: [permissionGuard],
          },
          {
            path: 'allotment',
            component: AllotmentComponent,
            data: { breadcrumb: 'تخصيص التقارير', permission: 'ReportsAllotment.View' },
            canMatch: [permissionGuard],
          },
          {
            path: 'grades-month',
            component: StudentMonthResultComponent,
            data: { breadcrumb: 'تقارير شهرية', permission: 'ReportsMonthly.View' },
            canMatch: [permissionGuard],
          },
          {
            path: 'registration',
            component: RegistrationReportComponent,
            data: { breadcrumb: 'استمارة التسجيل', permission: 'ReportsRegistration.View' },
            canMatch: [permissionGuard],
          },
          {
            path: 'term-result',
            component: TermResultComponent,
            data: { breadcrumb: 'الدرجات الفصلية', permission: 'ReportsTerm.View' },
            canMatch: [permissionGuard],
          },
          { path: 'not-found', component: PageNotFoundComponent },
          { path: '**', redirectTo: 'not-found', pathMatch: 'full' },
        ],
      },
      {
        path: 'students', data: { breadcrumb: 'الطلاب' }, children: [
          { path: 'all-students', component: StudentsComponent, data: { breadcrumb: 'جميع الطلاب' } },
          { path: 'about-students', component: StudentsComponent, data: { breadcrumb: 'عن الطلاب' } },
          { path: 'add-student/:id', component: StudentsComponent, data: { breadcrumb: 'إضافة طالب' } },
          {
            path: 'pending-registrations',
            loadComponent: () =>
              import('./sittings/pending-registration-requests/pending-registration-requests.component').then(
                (m) => m.PendingRegistrationRequestsComponent,
              ),
            data: { breadcrumb: 'طلبات التسجيل المعلقة' },
          },
          { path: 'un-reg-students', component: StudentPromotionComponent, data: { breadcrumb: 'الطلاب غير المسجلين' } },
          { path: '', redirectTo: 'all-students', pathMatch: 'full' },
          { path: 'not-found', component: PageNotFoundComponent },
          { path: '**', redirectTo: 'not-found', pathMatch: 'full' }
        ]
      },
      {
        path: 'sitting', data: { breadcrumb: 'الإعدادات' }, children: [
          { path: 'years', component: StudyYearComponent, data: { breadcrumb: 'السنوات الدراسية' } },
          { path: 'schoolInfo', component: SchoolInfoComponent, data: { breadcrumb: 'معلومات المدرسة' } },
          { path: 'stages', component: StagesGradesComponent, data: { breadcrumb: 'المراحل والفصول' } },
          { path: 'feeClass', component: FeeClassComponent, data: { breadcrumb: 'رسوم الصـفوف' } },
          { path: 'databaseRestore', component: DatabaseRestoreComponent, data: { breadcrumb: 'استعادة قاعدة البيانات' } },
          {
            path: 'pending-registrations',
            redirectTo: '/school/students/pending-registrations',
            pathMatch: 'full',
          },
          { path: '', redirectTo: 'years', pathMatch: 'full' },
          { path: 'not-found', component: PageNotFoundComponent },
          { path: '**', redirectTo: 'not-found', pathMatch: 'full' }
        ]
      },
      {
        path: 'teacher', data: { breadcrumb: 'الموظفون' }, children: [
          { path: '', component: TeachersComponent, data: { breadcrumb: 'قائمة الموظفين' } },
          { path: 'action', component: ActionComponent, data: { breadcrumb: 'حدث' } },
          { path: 'not-found', component: PageNotFoundComponent },
          { path: '**', redirectTo: 'not-found', pathMatch: 'full' }

        ]
      },
      {
        path: 'grade', data: { breadcrumb: 'الدرجات' }, children: [
          { path: '', component: GradesMangeComponent, data: { breadcrumb: 'بنود الدرجات' } },
          { path: 'GradeMonth', component: GradesMonthComponent, data: { breadcrumb: 'الدرجات الشهرية' } },
          { path: 'GradeTerm', component: GradesTermComponent, data: { breadcrumb: 'الدرجات الفصلية' } },
          { path: 'not-found', component: PageNotFoundComponent },
          { path: '**', redirectTo: 'not-found', pathMatch: 'full' }

        ]
      },
      {
        path: 'guardian', data: { breadcrumb: 'أولياء الأمور' }, children: [
          { path: '', component: AllParentsComponent, data: { breadcrumb: 'عرض أولياء الأمور' } },
          { path: 'not-found', component: PageNotFoundComponent },
          { path: '**', redirectTo: 'not-found', pathMatch: 'full' }

        ]
      },
      {
        path: 'account', data: { breadcrumb: 'الحسابات' }, children: [
          { path: '', component: AccountsComponent, data: { breadcrumb: 'الحسابات' } },
          { path: 'bill', component: BillsComponent, data: { breadcrumb: 'الفواتير' } },
          { path: 'not-found', component: PageNotFoundComponent },
          { path: '**', redirectTo: 'not-found', pathMatch: 'full' }

        ]
      },
      {
        path: 'allotment', component: AllotmentComponent, data: { breadcrumb: 'تخصيص' }
      },
      { path: 'schedule', component: WeeklyScheduleComponent, data: { breadcrumb: 'جدول الحصص الأسبوعي' } },
      { path: 'exams', component: ExamsAdminComponent, data: { breadcrumb: 'الامتحانات' } },
      { path: 'homework', component: HomeworkAdminComponent, data: { breadcrumb: 'الواجبات' } },
      { path: 'attendance', component: AttendanceComponent, data: { breadcrumb: 'الحضور والغياب' } },
      { path: 'notifications', component: NotificationsComponent, data: { breadcrumb: 'الإشعارات' } },
      {
        path: 'employees-hr',
        data: { breadcrumb: 'الموارد البشرية' },
        children: [
          {
            path: '',
            loadComponent: () =>
              import('./employees-hr/employees-hr-list/employees-hr-list.component').then(
                (m) => m.EmployeesHrListComponent,
              ),
            data: { breadcrumb: 'سجل الموظفين', permission: PagePermission.Employees.View },
            canMatch: [permissionGuard],
          },
          {
            path: 'new',
            loadComponent: () =>
              import('./employees-hr/employees-hr-create/employees-hr-create.component').then(
                (m) => m.EmployeesHrCreateComponent,
              ),
            data: { breadcrumb: 'إضافة موظف', permission: PagePermission.Employees.Create },
            canMatch: [permissionGuard],
          },
          {
            path: ':id/edit',
            loadComponent: () =>
              import('./employees-hr/employees-hr-edit/employees-hr-edit.component').then(
                (m) => m.EmployeesHrEditComponent,
              ),
            data: { breadcrumb: 'تعديل موظف', permission: PagePermission.Employees.Update },
            canMatch: [permissionGuard],
          },
          {
            path: ':id/profile',
            loadComponent: () =>
              import('./employees-hr/employees-hr-full-profile/employees-hr-full-profile.component').then(
                (m) => m.EmployeesHrFullProfileComponent,
              ),
            data: { breadcrumb: 'الملف الكامل', permission: PagePermission.Employees.View },
            canMatch: [permissionGuard],
          },
          {
            path: ':id',
            loadComponent: () =>
              import('./employees-hr/employees-hr-detail/employees-hr-detail.component').then(
                (m) => m.EmployeesHrDetailComponent,
              ),
            data: { breadcrumb: 'تفاصيل موظف', permission: PagePermission.Employees.View },
            canMatch: [permissionGuard],
          },
          { path: 'not-found', component: PageNotFoundComponent },
          { path: '**', redirectTo: 'not-found', pathMatch: 'full' },
        ],
      },
      {
        path: 'daily-evaluations',
        data: { breadcrumb: 'التقييم اليومي' },
        children: [
          {
            path: 'templates/new',
            loadComponent: () =>
              import('./daily-evaluations/daily-eval-template-form/daily-eval-template-form.component').then(
                (m) => m.DailyEvalTemplateFormComponent,
              ),
            data: { breadcrumb: 'قالب تقييم جديد', permission: PagePermission.Evaluations.Update },
            canMatch: [permissionGuard],
          },
          {
            path: 'templates/:templateId/edit',
            loadComponent: () =>
              import('./daily-evaluations/daily-eval-template-form/daily-eval-template-form.component').then(
                (m) => m.DailyEvalTemplateFormComponent,
              ),
            data: { breadcrumb: 'تعديل قالب', permission: PagePermission.Evaluations.Update },
            canMatch: [permissionGuard],
          },
          {
            path: 'templates/:templateId',
            loadComponent: () =>
              import('./daily-evaluations/daily-eval-template-detail/daily-eval-template-detail.component').then(
                (m) => m.DailyEvalTemplateDetailComponent,
              ),
            data: { breadcrumb: 'قالب تقييم', permission: PagePermission.Evaluations.View },
            canMatch: [permissionGuard],
          },
          {
            path: 'templates',
            loadComponent: () =>
              import('./daily-evaluations/daily-eval-templates-list/daily-eval-templates-list.component').then(
                (m) => m.DailyEvalTemplatesListComponent,
              ),
            data: { breadcrumb: 'قوالب التقييم اليومي', permission: PagePermission.Evaluations.View },
            canMatch: [permissionGuard],
          },
          {
            path: 'new',
            loadComponent: () =>
              import('./daily-evaluations/daily-evaluations-form/daily-evaluations-form.component').then(
                (m) => m.DailyEvaluationsFormComponent,
              ),
            data: { breadcrumb: 'تقييم يومي جديد', permission: PagePermission.Evaluations.Create },
            canMatch: [permissionGuard],
          },
          {
            path: ':evaluationId/edit',
            loadComponent: () =>
              import('./daily-evaluations/daily-evaluations-form/daily-evaluations-form.component').then(
                (m) => m.DailyEvaluationsFormComponent,
              ),
            data: { breadcrumb: 'تعديل تقييم يومي', permission: PagePermission.Evaluations.Update },
            canMatch: [permissionGuard],
          },
          {
            path: ':evaluationId',
            loadComponent: () =>
              import('./daily-evaluations/daily-evaluations-detail/daily-evaluations-detail.component').then(
                (m) => m.DailyEvaluationsDetailComponent,
              ),
            data: { breadcrumb: 'تقييم يومي', permission: PagePermission.Evaluations.View },
            canMatch: [permissionGuard],
          },
          {
            path: '',
            loadComponent: () =>
              import('./daily-evaluations/daily-evaluations-list/daily-evaluations-list.component').then(
                (m) => m.DailyEvaluationsListComponent,
              ),
            data: { breadcrumb: 'التقييمات اليومية', permission: PagePermission.Evaluations.View },
            canMatch: [permissionGuard],
          },
          { path: 'not-found', component: PageNotFoundComponent },
          { path: '**', redirectTo: 'not-found', pathMatch: 'full' },
        ],
      },
      {
        path: 'supervisor-visits',
        data: { breadcrumb: 'زيارات المشرف' },
        children: [
          {
            path: 'new',
            redirectTo: '',
            pathMatch: 'full',
          },
          {
            path: ':id/edit',
            redirectTo: '',
            pathMatch: 'full',
          },
          {
            path: '',
            loadComponent: () =>
              import('./supervisor-visits/supervisor-visits-list/supervisor-visits-list.component').then(
                (m) => m.SupervisorVisitsListComponent,
              ),
            data: { breadcrumb: 'قائمة الزيارات', permission: PagePermission.Employees.View },
            canMatch: [permissionGuard],
          },
          { path: 'not-found', component: PageNotFoundComponent },
          { path: '**', redirectTo: 'not-found', pathMatch: 'full' },
        ],
      },
      {
        path: 'teacher-feedback',
        data: { breadcrumb: 'تقييم المعلم' },
        children: [
          {
            path: '',
            loadComponent: () =>
              import('./teacher-feedback/teacher-feedback-list/teacher-feedback-list.component').then(
                (m) => m.TeacherFeedbackListComponent,
              ),
            data: { breadcrumb: 'دورات التقييم', permission: PagePermission.Employees.View },
            canMatch: [permissionGuard],
          },
          { path: 'not-found', component: PageNotFoundComponent },
          { path: '**', redirectTo: 'not-found', pathMatch: 'full' },
        ],
      },
      {
        path: 'recruitment',
        data: { breadcrumb: 'التوظيف' },
        children: [
          {
            path: 'about',
            loadComponent: () =>
              import('./recruitment/recruitment-about/recruitment-about.component').then(
                (m) => m.RecruitmentAboutComponent,
              ),
            data: { breadcrumb: 'عن المدرسة' },
          },
          {
            path: 'job-postings',
            loadComponent: () =>
              import('./recruitment/job-postings-list/job-postings-list.component').then(
                (m) => m.JobPostingsListComponent,
              ),
            data: { breadcrumb: 'الوظائف الشاغرة' },
          },
          {
            path: 'job-postings/create',
            loadComponent: () =>
              import('./recruitment/job-posting-create/job-posting-create.component').then(
                (m) => m.JobPostingCreateComponent,
              ),
            data: {
              breadcrumb: 'إعلان وظيفة',
              permissions: [PagePermission.Recruitment.Create, PagePermission.Employees.Create],
            },
            canMatch: [permissionGuard],
          },
          {
            path: 'job-postings/:id/edit',
            loadComponent: () =>
              import('./recruitment/job-posting-edit/job-posting-edit.component').then(
                (m) => m.JobPostingEditComponent,
              ),
            data: {
              breadcrumb: 'تعديل إعلان',
              permissions: [PagePermission.Recruitment.Update, PagePermission.Employees.Update],
            },
            canMatch: [permissionGuard],
          },
          {
            path: 'job-postings/:id',
            loadComponent: () =>
              import('./recruitment/job-posting-detail/job-posting-detail.component').then(
                (m) => m.JobPostingDetailComponent,
              ),
            data: { breadcrumb: 'تفاصيل الإعلان' },
          },
          {
            path: 'job-applications/create',
            loadComponent: () =>
              import('./recruitment/job-application-create/job-application-create.component').then(
                (m) => m.JobApplicationCreateComponent,
              ),
            data: { breadcrumb: 'طلب توظيف جديد' },
          },
          {
            path: 'job-applications',
            loadComponent: () =>
              import('./recruitment/job-applications-list/job-applications-list.component').then(
                (m) => m.JobApplicationsListComponent,
              ),
            data: {
              breadcrumb: 'طلبات التوظيف',
              permissions: [PagePermission.Recruitment.View, PagePermission.Employees.View],
            },
            canMatch: [permissionGuard],
          },
          {
            path: 'job-applications/:id/edit',
            loadComponent: () =>
              import('./recruitment/job-application-edit/job-application-edit.component').then(
                (m) => m.JobApplicationEditComponent,
              ),
            data: {
              breadcrumb: 'تعديل طلب',
              permissions: [PagePermission.Recruitment.Update, PagePermission.Employees.Update],
            },
            canMatch: [permissionGuard],
          },
          {
            path: 'job-applications/:id',
            loadComponent: () =>
              import('./recruitment/job-application-detail/job-application-detail.component').then(
                (m) => m.JobApplicationDetailComponent,
              ),
            data: {
              breadcrumb: 'مسار الطلب',
              permissions: [PagePermission.Recruitment.View, PagePermission.Employees.View],
            },
            canMatch: [permissionGuard],
          },
          { path: 'not-found', component: PageNotFoundComponent },
          { path: '**', redirectTo: 'not-found', pathMatch: 'full' },
        ],
      },
      {
        path: 'course', data: { breadcrumb: 'المقررات والخطط' }, children: [
          { path: '', component: BooksComponent, data: { breadcrumb: 'الكتب' } },
          { path: 'courses', component: CoursesComponent, data: { breadcrumb: 'المقررات' } },
          { path: 'plain', component: PlainsComponent, data: { breadcrumb: 'الخطط' } },
          { path: 'not-found', component: PageNotFoundComponent },
          { path: '**', redirectTo: 'not-found', pathMatch: 'full' }
        ]
      },
      { path: 'CaptureBonds', component: FeesComponent, data: { breadcrumb: 'سند قبض' } },
      { path: '', component: SchoolLandingComponent, pathMatch: 'full' },
      { path: 'not-found', component: PageNotFoundComponent },
      { path: '**', redirectTo: 'not-found', pathMatch: 'full' }
    ]
  },
  { path: '', redirectTo: '/school', pathMatch: 'full' },
  { path: 'not-found', component: PageNotFoundComponent },
  { path: '**', redirectTo: 'not-found', pathMatch: 'full' }
];
@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class SchoolRoutingModule {}
