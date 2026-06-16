import { CUSTOM_ELEMENTS_SCHEMA, NgModule } from '@angular/core';

import { SchoolComponentsModule } from '../school/school-components.module';
import { GuardianRoutingModule } from './guardian-routing.module';
import { GuardianHeaderComponent } from './header/guardian-header.component';
import { GuardianHomeComponent } from './guardian-home/guardian-home.component';
import { GuardianLayoutComponent } from './guardian-layout/guardian-layout.component';
import { GuardianHomeworkComponent } from './guardian-homework/guardian-homework.component';
import { GuardianExamsComponent } from './guardian-exams/guardian-exams.component';
import { GuardianAttendanceComponent } from './guardian-attendance/guardian-attendance.component';
import { GuardianMonthlyReportsComponent } from './guardian-monthly-reports/guardian-monthly-reports.component';
import { GuardianStudentMonthResultComponent } from './guardian-student-month-result/guardian-student-month-result.component';
import { GuardianStubPageComponent } from './guardian-stub-page/guardian-stub-page.component';

@NgModule({
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  declarations: [
    GuardianLayoutComponent,
    GuardianHeaderComponent,
    GuardianHomeComponent,
    GuardianHomeworkComponent,
    GuardianExamsComponent,
    GuardianAttendanceComponent,
    GuardianMonthlyReportsComponent,
    GuardianStudentMonthResultComponent,
    GuardianStubPageComponent,
  ],
  imports: [SchoolComponentsModule, GuardianRoutingModule],
})
export class GuardianModule {}
