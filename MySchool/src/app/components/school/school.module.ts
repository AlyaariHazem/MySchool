import { NgModule } from '@angular/core';

import { SchoolComponentsModule } from './school-components.module';
import { SchoolRoutingModule } from './school-routing.module';

@NgModule({
  imports: [SchoolComponentsModule, SchoolRoutingModule],
  exports: [SchoolComponentsModule],
})
export class SchoolModule {}
