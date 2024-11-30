import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';

import { Division, divisions } from '../../../../../core/models/division.model';
import { DivisionService } from '../../../../../core/services/division.service';
import { PageEvent } from '@angular/material/paginator';
import { ClassService } from '../../../../../core/services/class.service';
import { ClassDTO } from '../../../../../core/models/class.model';

@Component({
  selector: 'app-division',
  templateUrl: './division.component.html',
  styleUrls: ['./division.component.scss']
})
export class DivisionComponent implements OnInit {
  divisions: Array<divisions> = [];
  classes: ClassDTO[] = [];
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
      ClassID: ['', Validators.required],
      divisionName: ['',Validators.required]
    });
  }

  editDivision(division: divisions): void {
    this.form.patchValue({
      ClassID: division.classID,
      divisionName: division.divisionName
    });
    this.isEditMode = true;
    this.divisionToEditId = division.divisionID;
  }

  ngOnInit(): void {
    this.getAllDivisions();
    this.getAllClass();
  }

  getAllDivisions(): void {
    this.divisionService.GetAll().subscribe({
      next: (res) => {
        this.divisions = res.divisionInfo;
        this.length = this.divisions.length; // Set total item count
        this.updateDisplayedDivisions(); // Initialize displayed divisions
      },
      error: (err) => {
        this.toastr.error('Error fetching divisions', err);
      }
    });
  }

  getAllClass(): void {
    this.classService.GetAll().subscribe({
      next: (res) => this.classes = res,
      error: (err) => this.toastr.error('Error feched', 'Error', err)
    });
  }
  
  AddDivision(): void {
    if (this.form.valid) {
      const addClassData: Division = this.form.value;
      this.divisionService.Add(addClassData).subscribe({
        next: (res) => {
          this.getAllDivisions();
          this.form.reset();
          this.isEditMode = false;
          this.toastr.success('Stage Added successfully', res);
        },
        error: () => this.toastr.error('Something went wrong')
      });
    } else {
      this.isEditMode = false;
    }
  }

  DeleteDivision(id: number): void {
    this.divisionService.Delete(id).subscribe({
      next: (response) => {
        if (response.success) {
          this.toastr.success(response.message, 'Division Deleted');
          this.getAllDivisions(); // Refresh the list after deletion
        }
      },
      error: () => this.toastr.error('Failed to delete Division', 'Error')
    });
  }

  updateDivision(): void {
    this.form.markAllAsTouched();
    if (this.form.valid && this.divisionToEditId !== null) {
      const updateDivision:Division = this.form.value;
      this.divisionService.UpdateDivision(this.divisionToEditId, updateDivision).subscribe({
        next: (res) =>{
          if(res.success){
            this.toastr.success("Division updated successfully");
            this.form.reset();
            this.getAllDivisions();
          }
        },
        error: ()=> this.toastr.error("Failed to Update Division")
    });
    this.form.reset();
    this.getAllDivisions();
    this.isEditMode=false;
    }
  }

  changeState(division: divisions, isActive: boolean): void {
    const patchDoc = [
      { op: "replace", path: "/state", value: isActive }
    ];

    this.divisionService.partialUpdate(division.divisionID, patchDoc).subscribe({
      next: (response) => {
        if (response.success) {
          this.toastr.success(response.message);
          this.getAllDivisions(); // Refresh the list to show updated data
        }
      },
      error: () => this.toastr.error('Failed to update Division', 'Error')
    });
  }

  // Update displayed data based on pagination
  updateDisplayedDivisions(): void {
    const startIndex = this.currentPage * this.pageSize;
    const endIndex = startIndex + this.pageSize;
    this.displayedDivisions = this.divisions.slice(startIndex, endIndex);
  }

  // Handle paginator events
  onPageChange(event: PageEvent): void {
    this.currentPage = event.pageIndex;
    this.pageSize = event.pageSize;
    this.updateDisplayedDivisions();
  }

  toggleStateDropdown(item: any): void {
    item.isDropdownOpen = !item.isDropdownOpen;
  }
}
