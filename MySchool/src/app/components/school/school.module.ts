import { CUSTOM_ELEMENTS_SCHEMA, NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { AsyncPipe, DatePipe } from '@angular/common';
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
import { TabsModule } from 'primeng/tabs';
import { InputMaskModule } from 'primeng/inputmask';
import { CardModule } from 'primeng/card';
import { EditorModule } from 'primeng/editor';
import { PaginatorModule } from 'primeng/paginator';
import { BreadcrumbModule } from 'primeng/breadcrumb';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { TextareaModule } from 'primeng/textarea';
import { FloatLabel } from "primeng/floatlabel"
import { TableModule }       from 'primeng/table';
import { Tooltip } from 'primeng/tooltip';
import { Select } from 'primeng/select';
import { ChartModule } from 'primeng/chart';
import { MatSelectModule } from '@angular/material/select';

import { SchoolRoutingModule } from './school-routing.module';
import { HeaderComponent } from './header/header.component';
import { SidebarComponent } from './sidebar/sidebar.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import { AllParentsComponent } from './parents/all-parents/all-parents.component';
import { EditParentsComponent } from './parents/edit-parents/edit-parents.component';
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
import { GradesMangeComponent } from './grades-mange/grades-mange-mange.component';
import { GradesMonthComponent } from './grades-mange/grades-month/grades-month.component';
import { StudentMonthResultComponent } from './report/student-month-result/student-month-result.component';
import { ReportComponent } from './report/report.component';
import { CustomDatePipe } from '../../Pipes/customDate.pipe';
import { InputTextModule } from 'primeng/inputtext';
import { NumberToArabicTextPipe } from '../../Pipes/number-to-arabic-text.pipe';
import { AccountReportComponent } from './report/account-report/account-report.component';
import { EmployeeComponent } from './teachers/employee/employee.component';
import { AgePipe } from '../../Pipes/age.pipe';
import {  TermResultComponent } from './report/termly-result/term-result.component';
import { GradesTermComponent } from './grades-mange/grades-term/grades-term.component';
import { BreadcrumbComponent } from './breadcrumb/breadcrumb.component';
import { HeaderReportComponent } from './report/header-report/header-report.component';
import { AllStudentsComponent } from './students/all-students/all-students.component';
import { DatePicker } from 'primeng/datepicker';
import { CustomTableComponent } from '../../shared/components/custom-table/custom-table.component';



const components = [
  DashboardComponent,
  HeaderComponent,
  AllParentsComponent,
  EditParentsComponent,
  SidebarComponent,
  TeachersComponent,
  StudentMonthResultComponent,
  NavigateComponent,
  StudyYearComponent,
  StagesGradesComponent,
  SchoolInfoComponent,
  AllStudentsComponent,
  StudentsComponent,
  EmployeeComponent,
  GradesComponent,
  ActionComponent,
  DivisionComponent,
  NewStudentComponent,
  FeeClassComponent,
  PrimaryDataComponent,
  DocumentComponent,
  BreadcrumbComponent,
  NewYearComponent,
  FeesComponent,
  GuardianComponent,
  ReportComponent,
  OptionDataComponent,
  HeadComponent,
  NewCaptureComponent,
  FeeClassComponent,
  FeeComponent,
  CoursesComponent,
  PlainsComponent,
  CoursesAndPlains,
  BooksComponent,
  AccountsComponent,
  BillsComponent,
  GradesMonthComponent,
  AddAccountComponent,
  GradesMangeComponent,
  AllotmentComponent,
  TermResultComponent,
  GradesTermComponent,
  HeaderReportComponent,
  AccountReportComponent

];

const modules = [
  ShardModule,
  ReactiveFormsModule,
  CustomTableComponent,
  MatFormFieldModule,
  MatPaginatorModule,
  TabsModule,
  TableModule,
  CustomDatePipe,
  MatInputModule,
  MatOption,
  EditorModule,
  FormsModule,
  DatePicker,
  ChartModule,
  DropdownModule,
  PaginatorModule,
  MatSelectModule,
  MatTabsModule,
  BreadcrumbModule,  
  MatAutocompleteModule,
  AsyncPipe,
  TextareaModule,
  InputTextModule,
  MatCardModule,
  MatDialogTitle,
  MatDialogContent,
  MatDialogActions,
  MatDialogModule,
  MatDialogClose,
  RouterOutlet,
  RouterLinkActive,
  DialogModule,
  ButtonModule,
  PanelModule,
  InputMaskModule,
  CardModule,
  CountDirective,
  MatDatepickerModule,
  MatNativeDateModule,
  FloatLabel,
  Select,
  DatePipe,
  NumberToArabicTextPipe,
  AgePipe,
  Tooltip
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
