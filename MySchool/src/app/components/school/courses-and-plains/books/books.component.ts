import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
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

  form: FormGroup;
  subjects: Subjects[] = [];
  paginates!: Paginates;
  search: any;
  editMode: boolean = false;

  isSmallScreen = false;
  langDir!: string;
  subjectData!: Subjects;

  first: number = 0; // Current starting index for pagination
  rows: number = 4; // Number of rows per page

  constructor(
    private formBuilder: FormBuilder,
    public dialog: MatDialog
  ) {
    this.form = this.formBuilder.group({
      name: ['', Validators.required],
      nameReplacement: [''],
      note: [''],
    });
  }

  ngOnInit(): void {
    this.updatePaginatedData();
    this.languageService.currentLanguage();
  }

  // Adding new subject
  addSubject(formGroup: FormGroup) {
    if (formGroup.invalid) {
      console.log('Form is invalid');
      return;
    }
    this.subjectData = {
      subjectName: formGroup.value.name,
      subjectReplacement: formGroup.value.nameReplacement,
      note: formGroup.value.note,
      hireDate: new Date().toISOString(),
    };

    this.subjectService.addSubject(this.subjectData).subscribe({
      next: (res) => {
        if (res.isSuccess) {
          this.subjects.push(res.result);
          this.updatePaginatedData();
        }
      },
      error: () => console.error('Failed to add subject')
    });
  }

  // Fetch paginated data based on page number and rows per page
  updatePaginatedData(): void {
    const page = this.first / this.rows + 1;
    this.subjectService.getPaginatedSubjects(page, this.rows).subscribe({
      next: (res) => {
        if (res.isSuccess) {
          this.paginates = res.result;
          this.subjects = res.result.data;
        }
      },
      error: () => console.error('Failed to load paginated subjects')
    });

  }

  // Deleting a subject
  deleteSubject(id: number) {
    this.subjectService.deleteSubject(id).subscribe({
      next: (res) => {
        if (res.isSuccess) {
          this.subjects = this.subjects.filter(subject => subject.subjectID !== id);
          this.updatePaginatedData();
        }
      },
      error: () => console.error('Failed to delete subject')
    });

  }

  // Updating a subject
  updateSubject(formGroup: FormGroup) {
    if (formGroup.invalid) {
      console.log('Form is invalid');
      return;
    }

    this.subjectData = {
      ...this.subjectData,  // Keep the existing values of subjectData
      subjectName: formGroup.value.name,
      subjectReplacement: formGroup.value.nameReplacement,
      note: formGroup.value.note,
    };

    const subjectID = this.subjectData.subjectID;

    if (!subjectID) {
      console.log('subjectID is missing!');
      return; // Return early if subjectID is not set
    }

    this.subjectService.updateSubject(subjectID, this.subjectData).subscribe({
      next: (res) => {
        if (res.isSuccess) {
          this.subjects = this.subjects.map(subj =>
            subj.subjectID === subjectID ? { ...subj, ...this.subjectData } : subj
          );
          this.updatePaginatedData();
        }
      },
      error: () => console.error('Failed to update subject')
    });    

    this.form.reset();
    this.editMode = false;
  }

  // Setting the subject for editing
  editSubject(subject: Subjects) {
    this.editMode = true;
    this.subjectData = { ...subject };  // Create a copy of the subject to avoid mutation
    this.form.patchValue({
      name: subject.subjectName,
      nameReplacement: subject.subjectReplacement,
      note: subject.note,
    });
  }

  // Handle page change event from PrimeNG paginator
  onPageChange(event: PaginatorState): void {
    this.first = event.first || 0; // Update first index based on page
    this.rows = event.rows || 4; // Update rows per page
    this.updatePaginatedData(); // Fetch new page data
  }
}
