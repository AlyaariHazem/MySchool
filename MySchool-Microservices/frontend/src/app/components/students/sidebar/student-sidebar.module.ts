import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

import { StudentSidebarComponent } from './student-sidebar.component';

@NgModule({
  declarations: [StudentSidebarComponent],
  imports: [CommonModule, RouterModule, TranslateModule],
  exports: [StudentSidebarComponent],
})
export class StudentSidebarModule {}
