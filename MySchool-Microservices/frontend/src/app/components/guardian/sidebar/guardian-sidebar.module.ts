import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

import { GuardianSidebarComponent } from './guardian-sidebar.component';

@NgModule({
  declarations: [GuardianSidebarComponent],
  imports: [CommonModule, RouterModule, TranslateModule],
  exports: [GuardianSidebarComponent],
})
export class GuardianSidebarModule {}
