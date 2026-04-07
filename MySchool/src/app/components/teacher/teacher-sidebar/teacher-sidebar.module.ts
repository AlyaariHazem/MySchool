import { NgModule } from '@angular/core';
import { RouterLinkActive } from '@angular/router';

import { ShardModule } from '../../../shared/shard.module';
import { TeacherSidebarComponent } from './teacher-sidebar.component';

/** Isolated so `TeacherModule` → `SchoolComponentsModule` does not create a circular NgModule graph. */
@NgModule({
  declarations: [TeacherSidebarComponent],
  imports: [ShardModule, RouterLinkActive],
  exports: [TeacherSidebarComponent],
})
export class TeacherSidebarModule {}
