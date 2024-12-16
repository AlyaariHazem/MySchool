import { CUSTOM_ELEMENTS_SCHEMA, NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { AsyncPipe } from '@angular/common';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatCardModule } from '@angular/material/card';
import { MatDialogActions, MatDialogClose, MatDialogContent, MatDialogTitle } from '@angular/material/dialog';
import {MatTabsModule} from '@angular/material/tabs';
import { DropdownModule } from 'primeng/dropdown';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { MatDialogModule } from '@angular/material/dialog';
import { MatOption } from '@angular/material/core';
import { PanelModule } from 'primeng/panel';
import { ChipsModule } from 'primeng/chips';
import { InputMaskModule } from 'primeng/inputmask';
import { CardModule } from 'primeng/card';
import { EditorModule } from 'primeng/editor';
import { PaginatorModule } from 'primeng/paginator';

import { SchoolRoutingModule } from './school-routing.module';
import { HeaderComponent } from './header/header.component';
import { SidebarComponent } from './sidebar/sidebar.component';
import { PageHeaderComponent } from './page-header/page-header.component';
import { ChartForStudentComponent } from './students/chart-for-student/chart-for-student.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import { AllParentsComponent } from './parents/all-parents/all-parents.component';
import { EditParentsComponent } from './parents/edit-parents/edit-parents.component';
import { AboutStudentComponent } from './students/about-student/about-student.component';
import { AddStudentComponent } from './students/add-student/add-student.component';
import { EditStudentComponent } from './students/edit-student/edit-student.component';
import { TeachersComponent } from './teachers/teachers.component';
import { NavigateComponent } from './navigate/navigate.component';
import { RouterLinkActive, RouterOutlet } from '@angular/router';
import { StudyYearComponent } from './sittings/study-year/study-year.component';
import { StagesGradesComponent } from './sittings/stages-grades/stages-grades.component';
import { SchoolInfoComponent } from './sittings/school-info/school-info.component';
import { GradesComponent } from './sittings/stages-grades/grades/grades.component';
import { DivisionComponent } from './sittings/stages-grades/division/division.component';
import { ActionComponent } from './action/action.component';
import { StudentsComponent } from './students/students.component';
import { NewStudentComponent } from './students/new-student/new-student.component';
import { PrimaryDataComponent } from './students/new-student/primary-data/primary-data.component';
import { DocumentComponent } from './students/new-student/document/document.component';
import { GuardianComponent } from './students/new-student/guardian/guardian.component';
import { OptionDataComponent } from './students/new-student/option-data/option-data.component';
import { HeadComponent } from './students/new-student/head/head.component';
import { ShardModule } from '../../shared/shard.module';
import { MatSelectModule } from '@angular/material/select';
import { FeeClassComponent } from './sittings/fee-class/fee-class.component';
import { NewYearComponent } from './sittings/study-year/new-year/new-year.component';
import { NewCaptureComponent } from './fees/new-capture/new-capture.component';
import { FeesComponent } from './fees/fees.component';
import { FeeComponent } from './students/new-student/fee/fee.component';
import { CoursesComponent } from './courses-and-plains/courses/courses.component';
import { PlainsComponent } from './courses-and-plains/plains/plains.component';
import { CoursesAndPlains } from './courses-and-plains/courses-and-plains.component';
import { BooksComponent } from './courses-and-plains/books/books.component';
import { AccountsComponent } from './accounts/accounts.component';
import { BillsComponent } from './accounts/bills/bills.component';
import { AddAccountComponent } from './accounts/add-account/add-account.component';
import { AllotmentComponent } from './allotment-report/allotment/allotment.component';
import { CountDirective } from '../../directives/count.directive';



const components = [
  DashboardComponent,
  HeaderComponent,
  PageHeaderComponent,
  AllParentsComponent,
  EditParentsComponent,
  SidebarComponent,
  AboutStudentComponent,
  AddStudentComponent,
  ChartForStudentComponent,
  EditStudentComponent,
  TeachersComponent,
  NavigateComponent,
  StudyYearComponent,
  StagesGradesComponent,
  SchoolInfoComponent,
  StudentsComponent,
  GradesComponent,
  ActionComponent,
  DivisionComponent,
  NewStudentComponent,
  FeeClassComponent,
  PrimaryDataComponent,
  DocumentComponent,
  NewYearComponent,
  FeesComponent,
  GuardianComponent,
  OptionDataComponent,
  HeadComponent,
  NewCaptureComponent,
  FeeClassComponent,
  FeeComponent,
  NewStudentComponent,
  EditStudentComponent,
  CoursesComponent,
  PlainsComponent,
  CoursesAndPlains,
  BooksComponent,
  AccountsComponent,
  BillsComponent,
  AddAccountComponent,
  AllotmentComponent

];

const modules = [
  ShardModule,
  ReactiveFormsModule,
  MatFormFieldModule,
  MatPaginatorModule,
  MatInputModule,
  MatOption,
  EditorModule,
  FormsModule,
  DropdownModule,
  MatSelectModule,
  MatTabsModule,
  MatAutocompleteModule,
  AsyncPipe,
  MatCardModule,
  MatDialogTitle,
  MatDialogContent,
  MatDialogActions,
  MatDialogModule,
  MatDialogClose,
  RouterOutlet,
  PaginatorModule,
  RouterLinkActive,
  DialogModule,
  ButtonModule,
  PanelModule,
  ChipsModule,
  InputMaskModule,
  CardModule,
  CountDirective, 
];

@NgModule({
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  declarations: components,
  imports: [
    ...modules,
    SchoolRoutingModule,
  ],
  exports: [
    ...components, ...modules
  ]
})
export class SchoolModule {

}
