import { Component, inject, Input, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';

import { Division, divisions } from '../../../core/models/division.model';
import { DivisionService } from '../../../core/services/division.service';
import { ClassService } from '../../../core/services/class.service';
import { ClassDTO } from '../../../core/models/class.model';
import { PaginatorState } from 'primeng/paginator';

@Component({
  selector: 'app-division',
  templateUrl: './division.component.html',
  styleUrls: ['./division.component.scss',
    '../../../../../shared/styles/button.scss'
  ]
})
export class DivisionComponent implements OnInit {
  divisions: Array<divisions> = [];
  PaginatedDivisions: Array<divisions> = [];
  @Input() classes: ClassDTO[] = [];
  displayedDivisions: Array<divisions> = [];
  form: FormGroup;
  isEditMode: boolean = false;
  divisionToEditId: number | null = null;

  currentPage: number = 0; // Current page index
  pageSize: number = 5; // Number of items per page
  length: number = 0; // Total number of items

  toastr = inject(ToastrService);
  divisionService = inject(DivisionService);
  classService = inject(ClassService);

  constructor(private formBuilder: FormBuilder) {
    this.form = this.formBuilder.group({
      classID: ['', Validators.required],
      divisionName: ['', Validators.required]
    });
  }

  editDivision(division: divisions): void {
    this.form.patchValue({
      classID: division.classID,
      divisionName: division.divisionName
    });
    this.isEditMode = true;
    this.divisionToEditId = division.divisionID;
  }

  ngOnInit(): void {
    this.getAllDivisions();
    this.form.reset();
    this.classService.GetAll().subscribe({
      next: (res) => {
        if (!res.isSuccess) {
          this.toastr.warning(res.errorMasseges[0] || 'فشل تحميل الصفوف');
          this.classes = [];
          return;
        }
        this.classes = res.result;
      },
      error: () => {
        this.toastr.error('Failed to load classes');
        this.classes = [];
      }
    });
  }

  getAllDivisions(): void {
    this.divisionService.GetAll().subscribe({
      next: (res) => {
        if (!res.isSuccess) {
          this.toastr.warning(res.errorMasseges[0] || 'Failed to load divisions');
          this.divisions = [];
          return;
        }
        this.divisions = res.result;
        this.updatePaginatedData();
      },
      error: () => {
        this.toastr.error('Server error while loading divisions');
        this.divisions = [];
      }
    });
  }

  AddDivision(): void {
    if (this.form.valid) {
      const addClassData: Division = this.form.value;
      console.log('the date are ',addClassData);
      this.divisionService.Add(addClassData).subscribe({
        next: (res) => {
          if (res.isSuccess) {
            this.toastr.success(res.result);
            this.form.reset();
            this.getAllDivisions();
          }
        },
        error: () => this.toastr.error('Failed to add Division')
      });
    } else {
      this.isEditMode = false;
    }
  }

  DeleteDivision(id: number): void {
    this.divisionService.Delete(id).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.toastr.success(response.result, 'Division Deleted');
          this.getAllDivisions(); // Refresh the list after deletion
        }
      },
      error: () => this.toastr.error('Failed to delete Division', 'Error')
    });
  }

  updateDivision(): void {
    this.form.markAllAsTouched();
    if (this.form.valid && this.divisionToEditId !== null) {
      const updateDivision: Division = this.form.value;
      this.divisionService.UpdateDivision(this.divisionToEditId, updateDivision).subscribe({
        next: (res) => {
          if (res.isSuccess) {
            this.toastr.success("Division updated successfully");
            this.form.reset();
            this.getAllDivisions();
          }
        },
        error: () => this.toastr.error("Failed to Update Division")
      });
      this.form.reset();
      this.getAllDivisions();
      this.isEditMode = false;
    }
  }

  changeState(division: divisions, isActive: boolean): void {
    const patchDoc = [
      { op: "replace", path: "/state", value: isActive }
    ];

    this.divisionService.partialUpdate(division.divisionID, patchDoc).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.toastr.success(response.result);
          this.getAllDivisions(); // Refresh the list to show updated data
        }
      },
      error: () => this.toastr.error('Failed to update Division', 'Error')
    });
  }

  // Update displayed data based on pagination
  first: number = 0; // Current starting index
  rows: number = 4; // Number of rows per page
  updatePaginatedData(): void {
    const start = this.first;
    const end = this.first + this.rows;
    this.PaginatedDivisions = this.divisions.slice(start, end);
  }

  // Handle paginator events
  onPageChange(event: PaginatorState): void {
      this.first = event.first || 0; // Default to 0 if undefined
      this.rows = event.rows || 4; // Default to 4 rows
      this.updatePaginatedData();
    }

  toggleStateDropdown(item: any): void {
    item.isDropdownOpen = !item.isDropdownOpen;
  }
}
