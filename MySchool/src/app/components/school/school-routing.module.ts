import {  NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { DashboardComponent } from './dashboard/dashboard.component';
import { PageHeaderComponent } from './page-header/page-header.component';
import { NavigateComponent } from './navigate/navigate.component';
import { AboutStudentComponent } from './students/about-student/about-student.component';
import { AddStudentComponent } from './students/add-student/add-student.component';
import { AllStudentsComponent } from './students/all-students/all-students.component';
import { TeachersComponent } from './teachers/teachers.component';
import { StudyYearComponent } from './sittings/study-year/study-year.component';
import { SchoolInfoComponent } from './sittings/school-info/school-info.component';
import { StagesGradesComponent } from './sittings/stages-grades/stages-grades.component';
import { ChartForStudentComponent } from './students/chart-for-student/chart-for-student.component';
import { StudentsComponent } from './students/students.component';
import { FeeClassComponent } from './sittings/fee-class/fee-class.component';
import { ActionComponent } from './action/action.component';
import { AddParentsComponent } from './parents/add-parents/add-parents.component';
import { FeesComponent } from './fees/fees.component';

const routes: Routes = [
  {
    path: '',
    component: NavigateComponent, data: { breadcrumb: 'الرئيسية  / ' },
    children: [
      { path: 'dashboard', component: DashboardComponent, data: { breadcrumb: '' } },
      { path: 'sidebar', component: PageHeaderComponent , data: { breadcrumb: 'Sidebar' } },
      { path: 'students', data: { breadcrumb: 'الطلاب' }, children: [
          { path: 'all-students', component: StudentsComponent, data: { breadcrumb: 'جميع الطلاب' } },
          { path: 'about-students', component: AboutStudentComponent, data: { breadcrumb: 'عن الطلاب' } },
          { path: 'add-student', component: AddStudentComponent, data: { breadcrumb: 'إضافة طالب' } },
          { path: 'edit-student', component: AllStudentsComponent, data: { breadcrumb: 'تعديل طالب' } },
          { path: 'chart-for-student', component: ChartForStudentComponent, data: { breadcrumb: ' طالب' } },
          { path: '', redirectTo: 'all-students', pathMatch: 'full' }
        ]
      },
      {
        path:'sitting',data:{breadcrumb:'الإعدادات'},children:[
          {path:'years',component:StudyYearComponent,data:{breadcrumb:'السنوات الدراسية'}},
          {path:'schoolInfo',component:SchoolInfoComponent,data:{breadcrumb:'معلومات المدرسة'}},
          {path:'stages',component:StagesGradesComponent,data:{breadcrumb:'المراحل والفصول'}},
          {path:'feeClass',component:FeeClassComponent,data:{breadcrumb:'رسوم الصـفوف'}},
          {path:'',redirectTo:'years',pathMatch:'full'}
        ]
      },
      { path: 'teacher', data: { breadcrumb: 'الإستاذ' }, children: [
          {path:'',component:TeachersComponent,data:{breadcrumb:'الإستاذ'}},
          {path:'action',component:ActionComponent,data:{breadcrumb:'حدث'}},
          {path:'**' ,redirectTo:'action',pathMatch:'full'}

        ]},
        {path:'CaptureBonds',component:FeesComponent,data:{breadcrumb:'سند قبظ'}},
        {path:'parants',component:AddParentsComponent,data:{breadcrumb:'ولي الأمر'}},
        { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
    ]
  },
  { path: '', redirectTo: '/school/dashboard', pathMatch: 'full' }
];
@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class SchoolRoutingModule { }
