import { Component, HostListener, inject, Input, OnInit } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { Stage } from '../../../core/models/stages-grades.modul';
import { ClassService } from '../../../core/services/class.service';
import { CLass, ClassDTO, updateClass } from '../../../core/models/class.model';
import { PaginatorState } from 'primeng/paginator';
import { ConfirmDialogComponent } from '../../../../../shared/components/confirm-dialog/confirm-dialog.component';
import { DialogService } from 'primeng/dynamicdialog';

@Component({
  selector: 'app-grades',
  templateUrl: './grades.component.html',
  styleUrls: ['./grades.component.scss',
    '../../../../../shared/styles/button.scss'
  ]
})
export class GradesComponent implements OnInit {
  @Input() stages: Stage[] = [];
  paginatedGrade: ClassDTO[] = [];
  classes: ClassDTO[] = [];
  DisplayClasses: ClassDTO[] = [];
  AddClass?: CLass;
  form: FormGroup;
  isEditMode = false;
  classToEditId: number | null = null;
  YearID!: number;

  currentPage: number = 0; // Current page index
  pageSize: number = 5; // Number of items per page
  length: number = 0; // Total number of items

  private toastr = inject(ToastrService);
  private classService = inject(ClassService);
  errorMessage: string = "";

  constructor(private fb: FormBuilder,private dialogService: DialogService) {
    this.form = this.fb.group({
      className: ['', Validators.required],
      stageID: ['', Validators.required]
    });
  }

  ngOnInit(): void {
    this.getAllClasses();
    this.form.reset();
    this.YearID = Number(localStorage.getItem("yearId"));
  }

  openOuterDropdown: any = null;
  currentClassPage: { [key: string]: number } = {};
  maxClassesPerPage = 3;

  getAllClasses(): void {
    this.classService.GetAll().subscribe({
      next: (res) => {
        if (!res.isSuccess) {
          this.toastr.warning(res.errorMasseges[0] || 'فشل تحميل الصفوف');
          this.classes = [];
          return;
        }

        this.classes = res.result;
        this.length = this.classes.length;
        this.updatePaginatedData();
      },
      error: () => {
        this.toastr.error('Failed to load classes');
        this.classes = [];
      }
    });
  }

  addClass(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.errorMessage = 'Please fill in the required fields';
      return;
    }

    const addClassData: CLass = {
      ...this.form.value,
      yearID: this.YearID
    };

    this.classService.Add(addClassData).subscribe({
      next: (res) => {
        if (!res.isSuccess) {
          this.toastr.warning(res.errorMasseges[0] || 'Failed to add class');
          return;
        }

        this.toastr.success('Class added successfully');
        this.getAllClasses();
        this.form.reset();
        this.isEditMode = false;
      },
      error: () => this.toastr.error('Something went wrong')
    });
  }

  editClass(Class: ClassDTO): void {
    this.form.patchValue({
      stageID: Class.stageID,
      className: Class.className
    });
    this.isEditMode = true;  // Enter edit mode
    this.classToEditId = Class.classID;  // Set the ID for editing
  }

  updateClass(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid || this.classToEditId === null) return;

    const updateData: updateClass = this.form.value;

    this.classService.Update(this.classToEditId, updateData).subscribe({
      next: (res) => {
        if (!res.isSuccess) {
          this.toastr.warning(res.errorMasseges[0] || 'Failed to update class');
          return;
        }

        this.toastr.success(res.result, 'Class Updated Successfully');
        this.form.reset();
        this.getAllClasses();
        this.isEditMode = false;
      },
      error: () => this.toastr.error('Failed to update Class', 'Error')
    });
  }

  changeState(Class: ClassDTO, isActive: boolean): void {
    const patchDoc = [
      { op: "replace", path: "/state", value: isActive }
    ];

    this.classService.partialUpdate(Class.classID, patchDoc).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.toastr.success(response.result);
          this.getAllClasses(); // Refresh the list to show updated data
        }
      },
      error: () => this.toastr.error('Failed to update Class', 'Error')
    });

    this.isEditMode = false;
  }
  
    deleteClass(id: number): void {
      const ref = this.dialogService.open(ConfirmDialogComponent, {
        header: 'Delete Class',
        width: 'auto',
        data: {
          title: 'Delete Class',
          message: 'هل أنت متأكد من أنك تريد حذف هذه الصف؟',
          deleteFn: () => this.classService.Delete(id),
          successMessage: 'class deleted successfully'
        }
      });
  
      ref.onClose.subscribe((confirmed: boolean) => {
        if (confirmed) {
          this.paginatedGrade = this.paginatedGrade.filter(s => s.classID !== id);
        }
      });
    }
  

  toggleOuterDropdown(item: any): void {
    this.openOuterDropdown = this.openOuterDropdown === item ? null : item;
  }

  getPaginatedClasses(item: any) {
    if (this.currentClassPage[item.classID] === undefined) {
      this.currentClassPage[item.classID] = 0;
    }
    const startIndex = this.currentClassPage[item.classID] * this.maxClassesPerPage;
    const endIndex = startIndex + this.maxClassesPerPage;
    console.log('divisions data are', item);
    return item.divisions.slice(startIndex, endIndex);
  }

  previousClassPage(item: any) {
    if (this.currentClassPage[item.classID] > 0) {
      this.currentClassPage[item.classID]--;
    }
  }

  nextClassPage(item: any) {
    if ((this.currentClassPage[item.classID] + 1) * this.maxClassesPerPage < item.divisions.length) {
      this.currentClassPage[item.classID]++;
    }
  }

  getTotalClassPages(item: any): number {
    return Math.ceil(item.divisions.length / this.maxClassesPerPage);
  }

  isOuterDropdownOpen(item: any): boolean {
    return this.openOuterDropdown === item;
  }


  @HostListener('document:click', ['$event'])
  clickout(event: Event) {
    const target = event.target as HTMLElement;
    if (!target.closest('.dropdown-menu') && !target.closest('.btn')) {
      this.openOuterDropdown = null;
    }
  }

  // Handle paginator events
  first: number = 0; // Current starting index
  rows: number = 4; // Number of rows per page
  updatePaginatedData(): void {
    const start = this.first;
    const end = this.first + this.rows;
    this.paginatedGrade = this.classes.slice(start, end);
  }

  // Handle page change event from PrimeNG paginator
  onPageChange(event: PaginatorState): void {
    this.first = event.first || 0; // Default to 0 if undefined
    this.rows = event.rows || 4; // Default to 4 rows
    this.updatePaginatedData();
  }

}
