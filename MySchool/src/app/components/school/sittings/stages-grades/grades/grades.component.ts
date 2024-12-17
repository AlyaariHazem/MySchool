import { Component, HostListener, inject, OnInit } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { Stage } from '../../../../../core/models/stages-grades.modul';
import { ClassService } from '../../../../../core/services/class.service';
import { CLass, ClassDTO, updateClass } from '../../../../../core/models/class.model';
import { StageService } from '../../../../../core/services/stage.service';
import { PageEvent } from '@angular/material/paginator';

@Component({
    selector: 'app-grades',
    templateUrl: './grades.component.html',
    styleUrls: ['./grades.component.scss']
})
export class GradesComponent implements OnInit {
  stages: Array<Stage> = [];
  classes: ClassDTO[] = [];
  DisplayClasses: ClassDTO[] = [];
  AddClass?: CLass;
  form: FormGroup;
  isEditMode = false;
  classToEditId: number | null = null;

  currentPage: number = 0; // Current page index
  pageSize: number = 5; // Number of items per page
  length: number = 0; // Total number of items

  private toastr = inject(ToastrService);
  private classService = inject(ClassService);
  private stageService = inject(StageService);
  errorMessage: string = "";

  constructor(private fb: FormBuilder) {
    this.form = this.fb.group({
      className: ['', Validators.required],
      stageID: ['', Validators.required]
    });
  }

  ngOnInit(): void {
    this.getAllClasses();
  }

  openOuterDropdown: any = null;
  currentClassPage: { [key: string]: number } = {};
  maxClassesPerPage = 3;

  getAllClasses(): void {
    this.classService.GetAll().subscribe({
      next: (res) => {
        this.classes = res;
        this.length = this.classes.length; // Set total item count
        this.updateDisplayedClass(); // Initialize displayed divisions
      },
      error: (err) => {
        console.error('Error fetching classes:', err);
        this.toastr.error('Error fetching classes');
      }
    });

    this.stageService.getAllStages().subscribe({
      next: (res) => this.stages = res.stagesInfo,
      error: (err) => this.toastr.error('Error fetching Stages ', err)
    });
  }

  addClass(): void {
    if (this.form.valid) {
      const addClassData: CLass = this.form.value;
      this.classService.Add(addClassData).subscribe({
        next: (res) => {
          this.getAllClasses();
          this.form.reset();
          this.isEditMode = false;
          this.toastr.success('Stage Added successfully');
        },
        error: () => this.toastr.error('Something went wrong')
      });
    } else {
      this.errorMessage = 'Please fill in the required fields';
      this.isEditMode = false;
    }
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
    if (this.form.valid && this.classToEditId !== null) {
      const updateData: updateClass = this.form.value;
      this.classService.Update(this.classToEditId, updateData).subscribe({
        next: (response) => {
          if (response.success) {
            this.toastr.success(response.success, "Stage Updated Successfully");
            this.form.reset();
            this.getAllClasses();
          }
        },
        error: () => this.toastr.error('Failed to update stage', 'Error')
      });
      this.toastr.success('Stage updated successfully');
      this.form.reset();
      this.getAllClasses();
      this.isEditMode = false;
    }
  }

  changeState(Class: ClassDTO, isActive: boolean): void {
    const patchDoc = [
      { op: "replace", path: "/state", value: isActive }
    ];

    this.classService.partialUpdate(Class.classID, patchDoc).subscribe({
      next: (response) => {
        if (response.success) {
          this.toastr.success(response.message);
          this.getAllClasses(); // Refresh the list to show updated data
        }
      },
      error: () => this.toastr.error('Failed to update Class', 'Error')
    });

    this.isEditMode = false;
  }

  // Method to delete a Class by ID
  deleteClass(id: number): void {
    this.classService.Delete(id).subscribe({
      next: (response) => {
        if (response.success) {
          this.toastr.success(response.message, 'Class Deleted');
          this.getAllClasses(); // Refresh the list after deletion
        }
      },
      error: () => this.toastr.error('Failed to delete Class', 'Error')
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

  updateDisplayedClass(): void {
    const startIndex = this.currentPage * this.pageSize;
    const endIndex = startIndex + this.pageSize;
    this.DisplayClasses = this.classes.slice(startIndex, endIndex);
  }

  // Handle paginator events
  onPageChange(event: PageEvent): void {
    this.currentPage = event.pageIndex;
    this.pageSize = event.pageSize;
    this.updateDisplayedClass();
  }
  toggleStateDropdown(item: any): void {
    item.isDropdownOpen = !item.isDropdownOpen;
  }

}
