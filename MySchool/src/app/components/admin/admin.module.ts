import { CUSTOM_ELEMENTS_SCHEMA, NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDialogActions, MatDialogClose, MatDialogContent, MatDialogTitle } from '@angular/material/dialog';
import { AsyncPipe, CommonModule } from '@angular/common';
import { RouterLinkActive, RouterOutlet } from '@angular/router';
import { MatDialogModule } from '@angular/material/dialog';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatOption, MatOptionModule } from '@angular/material/core';
import { MatSelectModule } from '@angular/material/select';
import { PaginatorModule } from 'primeng/paginator';
import { TranslateModule } from '@ngx-translate/core';
import { MatPaginatorModule } from '@angular/material/paginator';
import { EditorModule } from 'primeng/editor';
import { DropdownModule } from 'primeng/dropdown';
import { MatTabsModule } from '@angular/material/tabs';
import { BreadcrumbModule } from 'primeng/breadcrumb';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { PanelModule } from 'primeng/panel';
import { InputMaskModule } from 'primeng/inputmask';
import { CardModule } from 'primeng/card';
import { MatTableModule } from '@angular/material/table';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { InputNumberModule } from 'primeng/inputnumber';

import { MatIconModule } from '@angular/material/icon';

import { MatMenuModule } from '@angular/material/menu';

import { HeaderComponent } from './header/header.component';
import { ActionComponent } from './action/action.component';
import { SchoolsComponent } from './schools/schools.component';
import { UsersComponent } from './users/users.component';
import { MessagesComponent } from './messages/messages.component';
import { NavigateComponent } from './navigate/navigate.component';
import { MediaPartalComponent } from './media-partal/media-partal.component';
import { CustomerComponent } from './customer/customer.component';
import { FileManagerComponent } from './file-manager/file-manager.component';
import { SittingComponent } from './sitting/sitting.component';
import { SidebarComponent } from './sidebar/sidebar.component';
import { PageHeaderComponent } from './page-header/page-header.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import { AdminRoutingModule } from './admin-routing.module';
import { SchoolInfoComponent } from './school-info/school-info.component';
import { AddManagerComponent } from './users/add-manager/add-manager.component';
import { ChartModule } from 'primeng/chart';


const components = [
  DashboardComponent,
  SchoolsComponent,
  UsersComponent,
  MessagesComponent,
  SchoolInfoComponent,
  SittingComponent,
  FileManagerComponent,
  MediaPartalComponent,
  CustomerComponent,
  HeaderComponent,
  PageHeaderComponent,
  SidebarComponent,
  NavigateComponent,
  ActionComponent,
  AddManagerComponent
];

const modules = [
  FormsModule,
  CommonModule,
  MatOptionModule,
  AdminRoutingModule,
  MatButtonModule,
  MatIconModule,
  MatMenuModule,
  ReactiveFormsModule,
  MatTableModule,
  ChartModule,
  MatFormFieldModule,
  MatInputModule,
  TranslateModule,
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
  PaginatorModule,
  MatPaginatorModule,
  MatOption,
  EditorModule,
  DropdownModule,
  MatTabsModule,
  BreadcrumbModule,
  MatAutocompleteModule,
  AsyncPipe,
  RouterLinkActive,
  DialogModule,
  ButtonModule,
  PanelModule,
  InputMaskModule,
  CardModule,
  InputTextModule,
  PasswordModule,
  InputNumberModule,
];

@NgModule({
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
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