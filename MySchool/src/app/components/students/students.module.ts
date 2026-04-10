import { CUSTOM_ELEMENTS_SCHEMA, NgModule } from '@angular/core';

import { SchoolComponentsModule } from '../school/school-components.module';
import { StudentHeaderComponent } from './header/student-header.component';
import { StudentsRoutingModule } from './students-routing.module';
import { StudentHomeComponent } from './student-home/student-home.component';
import { StudentLayoutComponent } from './student-layout/student-layout.component';

@NgModule({
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  declarations: [
    StudentLayoutComponent,
    StudentHeaderComponent,
    StudentHomeComponent,
  ],
  imports: [SchoolComponentsModule, StudentsRoutingModule],
})
export class StudentsModule {}
