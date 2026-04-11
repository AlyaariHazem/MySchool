import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { PageNotFoundComponent } from '../../shared/components/page-not-found/page-not-found.component';
import { NotificationsComponent } from '../school/notifications/notifications.component';
import { GuardianHomeComponent } from './guardian-home/guardian-home.component';
import { GuardianLayoutComponent } from './guardian-layout/guardian-layout.component';
import { GuardianHomeworkComponent } from './guardian-homework/guardian-homework.component';
import { GuardianExamsComponent } from './guardian-exams/guardian-exams.component';
import { GuardianStubPageComponent } from './guardian-stub-page/guardian-stub-page.component';

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
        component: GuardianStubPageComponent,
        data: {
          breadcrumb: 'الدرجات الشهرية',
          stubTitle: 'الدرجات الشهرية للأبناء',
          stubMessage: 'عرض الدرجات الشهرية لأبنائك سيتم تفعيله في تحديث قادم.',
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
        component: GuardianStubPageComponent,
        data: {
          breadcrumb: 'تقارير شهرية',
          stubTitle: 'تقارير شهرية',
          stubMessage: 'التقارير الشهرية الخاصة بأبنائك ستتوفر هنا لاحقاً.',
        },
      },
      {
        path: 'attendance',
        component: GuardianStubPageComponent,
        data: {
          breadcrumb: 'الحضور والغياب',
          stubTitle: 'حضور الأبناء',
          stubMessage: 'متابعة الحضور والغياب لأبنائك ستتوفر هنا لاحقاً.',
        },
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
export class GuardianRoutingModule {}
