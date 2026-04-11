import { CUSTOM_ELEMENTS_SCHEMA, NgModule } from '@angular/core';

import { SchoolComponentsModule } from '../school/school-components.module';
import { TeacherRoutingModule } from './teacher-routing.module';
import { TeacherLayoutComponent } from './teacher-layout/teacher-layout.component';
import { TeacherWorkspaceComponent } from './pages/teacher-workspace/teacher-workspace.component';
import { TeacherExamsComponent } from './pages/teacher-exams/teacher-exams.component';

@NgModule({
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  declarations: [TeacherLayoutComponent, TeacherWorkspaceComponent, TeacherExamsComponent],
  imports: [SchoolComponentsModule, TeacherRoutingModule],
})
export class TeacherModule {}
