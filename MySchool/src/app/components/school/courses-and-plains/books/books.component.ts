import { Component, inject, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { PaginatorState } from 'primeng/paginator';

import { LanguageService } from '../../../../core/services/language.service';
import { SubjectService } from '../../core/services/subject.service';
import { Subjects } from '../../core/models/subjects.model';
import { Paginates } from '../../core/models/Pagination.model';

@Component({
  selector: 'app-books',
  templateUrl: './books.component.html',
  styleUrls: ['./books.component.scss', './../../../../shared/styles/style-table.scss']
})
export class BooksComponent implements OnInit {
  subjectService = inject(SubjectService);
  languageService = inject(LanguageService);

  subjects: Subjects[] = [];
  paginates!: Paginates;
  editMode: boolean = false;
  search: any;

  subjectData: Subjects = {
    subjectName: '',
    subjectReplacement: '',
    note: '',
    hireDate: ''
  };

  first: number = 0;
  rows: number = 4;

  constructor(public dialog: MatDialog) {}

  ngOnInit(): void {
    this.updatePaginatedData();
    this.languageService.currentLanguage();
  }

  addSubject(): void {
    if (!this.subjectData.subjectName || this.subjectData.subjectName.trim() === '') {
      console.log('اسم الكتاب مطلوب');
      return;
    }

    const newSubject: Subjects = {
      ...this.subjectData,
      hireDate: new Date().toISOString()
    };

    this.subjectService.addSubject(newSubject).subscribe({
      next: (res) => {
        if (res.isSuccess) {
          this.subjects.push(res.result);
          this.updatePaginatedData();
          this.resetForm();
        }
      },
      error: () => console.error('فشل في إضافة الكتاب')
    });
  }

  updateSubject(): void {
    const subjectID = this.subjectData.subjectID;

    if (!subjectID) {
      console.log('subjectID is missing!');
      return;
    }

    this.subjectService.updateSubject(subjectID, this.subjectData).subscribe({
      next: (res) => {
        if (res.isSuccess) {
          this.subjects = this.subjects.map(subj =>
            subj.subjectID === subjectID ? { ...subj, ...this.subjectData } : subj
          );
          this.updatePaginatedData();
          this.resetForm();
        }
      },
      error: () => console.error('فشل في تحديث الكتاب')
    });
  }

  editSubject(subject: Subjects): void {
    this.editMode = true;
    this.subjectData = { ...subject }; // نسخة مستقلة
  }

  deleteSubject(id: number): void {
    this.subjectService.deleteSubject(id).subscribe({
      next: (res) => {
        if (res.isSuccess) {
          this.subjects = this.subjects.filter(subject => subject.subjectID !== id);
          this.updatePaginatedData();
        }
      },
      error: () => console.error('فشل في حذف الكتاب')
    });
  }

  updatePaginatedData(): void {
    const page = this.first / this.rows + 1;
    this.subjectService.getPaginatedSubjects(page, this.rows).subscribe({
      next: (res) => {
        if (res.isSuccess) {
          this.paginates = res.result;
          this.subjects = res.result.data;
        }
      },
      error: () => console.error('فشل في تحميل البيانات')
    });
  }

  onPageChange(event: PaginatorState): void {
    this.first = event.first || 0;
    this.rows = event.rows || 4;
    this.updatePaginatedData();
  }

  resetForm(): void {
    this.subjectData = {
      subjectName: '',
      subjectReplacement: '',
      note: '',
      hireDate: ''
    };
    this.editMode = false;
  }
}
