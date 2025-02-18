import { Component, inject } from '@angular/core';
import { StudentDetailsDTO } from '../../../core/models/students.model';
import { StudentService } from '../../../core/services/student.service';
import { LanguageService } from '../../../core/services/language.service';
import { TranslationService } from '../../../core/services/translation.service';
import { PaginatorState } from 'primeng/paginator';
import { MatDialog, MatDialogConfig } from '@angular/material/dialog';
import { RegisterComponent } from '../../../auth/register/register.component';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-users',
  templateUrl: './users.component.html',
  styleUrl: './users.component.scss'
})
export class UsersComponent {
constructor(
    private toastr: ToastrService,
    public dialog: MatDialog){

}
  languageService=inject(LanguageService);
  translationService=inject(TranslationService);
  
  students: StudentDetailsDTO[] = [];
  studentService = inject(StudentService);

  ngOnInit(): void {
    this.getAllStudent();
    this.languageService.currentLanguage();
    this.translationService.changeLanguage(this.languageService.langDir);
  }

  getAllStudent(): void {
    this.studentService.getAllStudents().subscribe(res => this.students = res);
  }

  first: number = 0;
  rows: number = 4;
  onPageChange(event: PaginatorState) {
    this.first = event.first || 0; // Default to 0 if undefined
    this.rows = event.rows!;
  }
  openDialog(): void {
      const dialogConfig = new MatDialogConfig();
      dialogConfig.width = '400px';
      dialogConfig.panelClass = 'custom-dialog-container';
  
      const dialogRef = this.dialog.open(RegisterComponent, dialogConfig);
  
      dialogRef.afterClosed().subscribe((result) => {
        if (result) {
          this.toastr.success('تم إضافة الطالب بنجاح');
        }
      });
    }
}
