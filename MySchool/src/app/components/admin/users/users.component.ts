import { Component, inject } from '@angular/core';
import { PaginatorState } from 'primeng/paginator';
import { MatDialog, MatDialogConfig } from '@angular/material/dialog';
import { ToastrService } from 'ngx-toastr';

import { StudentDetailsDTO } from '../../../core/models/students.model';
import { StudentService } from '../../../core/services/student.service';
import { LanguageService } from '../../../core/services/language.service';
import { TranslationService } from '../../../core/services/translation.service';
import { AddManagerComponent } from './add-manager/add-manager.component';
import { managerInfo } from '../../../core/models/manage/managerInfo.model';
import { ManagerService } from '../../../core/services/Manage/manager.service';

@Component({
  selector: 'app-users',
  templateUrl: './users.component.html',
  styleUrl: './users.component.scss'
})
export class UsersComponent {
  constructor(
    private toastr: ToastrService,
    public dialog: MatDialog) {

  }
  ManagerInfo:managerInfo[]=[];
  ManagerService=inject(ManagerService);
  languageService = inject(LanguageService);
  translationService = inject(TranslationService);

  students: StudentDetailsDTO[] = [];
  studentService = inject(StudentService);

  ngOnInit(): void {
    this.getAllStudent();
    this.languageService.currentLanguage();
    this.getAllManagers();
    this.translationService.changeLanguage(this.languageService.langDir);
  }

  getAllStudent(): void {
    this.studentService.getAllStudents().subscribe(res => this.students = res);
  }
  getAllManagers(): void{
    this.ManagerService.getAllManagers().subscribe(res=>this.ManagerInfo=res);
  }

  first: number = 0;
  rows: number = 4;
  onPageChange(event: PaginatorState) {
    this.first = event.first || 0; // Default to 0 if undefined
    this.rows = event.rows!;
  }
  
  openDialog(): void {
    const dialogConfig = new MatDialogConfig();
    dialogConfig.width = '95%';
    dialogConfig.panelClass = 'custom-dialog-container';

    const dialogRef = this.dialog.open(AddManagerComponent, dialogConfig);

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.toastr.success('تم إضافة الطالب بنجاح');
      }
    });
  }
}
