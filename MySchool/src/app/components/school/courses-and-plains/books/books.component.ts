import { Component, OnInit, inject } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { PaginatorState } from 'primeng/paginator';
import { ToastrService } from 'ngx-toastr';
import { NgForm } from '@angular/forms';
import { Store } from '@ngrx/store';
import { map } from 'rxjs';

import { SubjectService } from '../../core/services/subject.service';
import { Subjects } from '../../core/models/subjects.model';
import { selectLanguage } from '../../../../core/store/language/language.selectors';

@Component({
  selector: 'app-books',
  templateUrl: './books.component.html',
  styleUrls: ['./books.component.scss', './../../../../shared/styles/style-table.scss']
})
export class BooksComponent implements OnInit {
  /* ───────── injections ───────── */
  private subjectService = inject(SubjectService);
  private toastr = inject(ToastrService);

  /* ───────── data ───────── */
  subjects: Subjects[] = [];
  totalRecords = 0;

  /* paginator state (lazy-load from server) */
  first = 0;
  rows = 4;

  /* form helpers */
  editMode = false;
  subjectData: Subjects = { subjectName: '', subjectReplacement: '', note: '', hireDate: '' };

  readonly dir$ = this.store.select(selectLanguage).pipe(
    map(l => (l === 'ar' ? 'rtl' : 'ltr')),
  );

  constructor(public dialog: MatDialog,private store:Store) { }

  /* ───────── lifecycle ───────── */
  ngOnInit(): void {
    this.loadPage();
  }

  /* ───────── CRUD ───────── */
  addSubject(form: NgForm): void {
    if (!this.subjectData.subjectName?.trim()) {
      this.toastr.warning('اسم الكتاب مطلوب');
      return;
    }
  
    const dto: Subjects = { ...this.subjectData, hireDate: new Date().toISOString() };
  
    this.subjectService.addSubject(dto).subscribe({
      next: res => {
        if (res.isSuccess) {
          this.toastr.success('تمت إضافة الكتاب');
          this.loadPage();
          this.resetForm(form);
        }
      },
      error: () => this.toastr.error('فشل في إضافة الكتاب')
    });
  }
  

  updateSubject(form:NgForm): void {
    if (!this.subjectData.subjectID) return;

    this.subjectService.updateSubject(this.subjectData.subjectID, this.subjectData).subscribe({
      next: res => {
        if (res.isSuccess) {
          this.toastr.success('تم التعديل الكتاب بنجاح');
          this.loadPage();
          this.resetForm(form);
        }
      },
      error: () => this.toastr.error('فشل في التعديل')
    });
  }

  deleteSubject(id: number): void {
    this.subjectService.deleteSubject(id).subscribe({
      next: res => {
        if (res.isSuccess) {
          this.toastr.success(' تم الحذف الكتاب بنجاح');
          this.loadPage();
        }
      },
      error: () => this.toastr.error('فشل في الحذف')
    });
  }

  editSubject(row: Subjects): void {
    this.editMode = true;
    this.subjectData = { ...row };   // deep copy
  }

  /* ───────── pagination ───────── */
  handlePageChange(evt: PaginatorState): void {
    this.first = evt.first ?? 0;
    this.rows = evt.rows ?? 4;
    this.loadPage();
  }

  private loadPage(): void {
    const page = this.first / this.rows + 1;

    this.subjectService
      .getPaginatedSubjects(page, this.rows)
      .subscribe({
        next: res => {
          if (res.isSuccess && res.result) {
            this.subjects = res.result.data;
            this.totalRecords = res.result.totalCount;
          } else {
            this.subjects = [];
            this.totalRecords = 0;
          }
        },
        error: () => this.toastr.error('فشل في تحميل البيانات')
      });
  }

  /* ───────── utils ───────── */
  resetForm(form:NgForm): void {
    this.subjectData = { subjectName: '', subjectReplacement: '', note: '', hireDate: '' };
    this.editMode = false;
    form.reset();

  }
}
