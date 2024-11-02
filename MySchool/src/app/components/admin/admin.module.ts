import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldControl, MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDialogActions, MatDialogClose, MatDialogContent, MatDialogTitle } from '@angular/material/dialog';

import { HeaderComponent } from './header/header.component';
import { SidebarComponent } from './sidebar/sidebar.component';
import { PageHeaderComponent } from './page-header/page-header.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import { AdminRoutingModule } from './admin-routing.module';
import { CommonModule } from '@angular/common';
import { NavigateComponent } from './navigate/navigate.component';
import { RouterOutlet } from '@angular/router';
import { ActionComponent } from './action/action.component';
import { MatDialogModule } from '@angular/material/dialog';
import { SchoolsComponent } from './schools/schools.component';
import { UsersComponent } from './users/users.component';
import { MessagesComponent } from './messages/messages.component';
import { MediaPartalComponent } from './media-partal/media-partal.component';
import { CustomerComponent } from './customer/customer.component';
import { FileManagerComponent } from './file-manager/file-manager.component';
import { SittingComponent } from './sitting/sitting.component';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatOptionModule } from '@angular/material/core';
import { MatSelectModule } from '@angular/material/select';


const components = [
  DashboardComponent,
  SchoolsComponent,
  UsersComponent,
  MessagesComponent,
  SittingComponent,
  FileManagerComponent,
  MediaPartalComponent,
  CustomerComponent,
  HeaderComponent,
  PageHeaderComponent,
  SidebarComponent,
  NavigateComponent,
  ActionComponent
];

const modules = [
  FormsModule,
  CommonModule,
  MatOptionModule,
  ReactiveFormsModule,
  MatFormFieldModule,
  MatInputModule,
  MatDialogTitle,
  MatDatepickerModule,
  MatDialogContent,
  MatDialogActions,
  MatSelectModule,
  MatDialogModule,
  MatDialogClose,
  MatCardModule,
  MatFormFieldModule,
  MatInputModule,
  MatButtonModule,
  RouterOutlet,
];

@NgModule({
  declarations: components,
  imports: [
      ...modules,
      AdminRoutingModule,
  ],
  exports: [
      ...components,
      ...modules
  ]
})
export class AdminModule {

}