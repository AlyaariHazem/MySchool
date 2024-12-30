import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { DashboardComponent } from './dashboard/dashboard.component';
import { PageHeaderComponent } from './page-header/page-header.component';
import { NavigateComponent } from './navigate/navigate.component';
import { UsersComponent } from './users/users.component';
import { MediaPartalComponent } from './media-partal/media-partal.component';
import { CustomerComponent } from './customer/customer.component';
import { MessagesComponent } from './messages/messages.component';
import { FileManagerComponent } from './file-manager/file-manager.component';
import { SittingComponent } from './sitting/sitting.component';
import { SchoolsComponent } from './schools/schools.component';

const routes: Routes = [
  {
    path: '',
    component: NavigateComponent, data: { breadcrumb: 'الرئيسية  / ' },
    children: [
      { path: 'dashboard', component: DashboardComponent, data: { breadcrumb: '' } },
      { path: 'sidebar', component: PageHeaderComponent , data: { breadcrumb: 'Sidebar' } },
      { path: 'school', component: SchoolsComponent , data: { breadcrumb: 'المدارس' } },
      { path: 'user', component: UsersComponent , data: { breadcrumb: 'المستخدمين' } },
      { path: 'media', component: MediaPartalComponent , data: { breadcrumb: 'بوابة الإعلام' } },
      { path: 'customer', component: CustomerComponent , data: { breadcrumb: 'الزبائن' } },
      { path: 'message', component: MessagesComponent , data: { breadcrumb: 'الرسائل' } },
      { path: 'file', component: FileManagerComponent , data: { breadcrumb: 'ملف المدير' } },
      { path: 'sitting', component:SittingComponent  , data: { breadcrumb: 'الإعدادات' } },
      
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
    ]
  },
  { path: '', redirectTo: '/admin/dashboard', pathMatch: 'full' }
];
@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AdminRoutingModule { }
