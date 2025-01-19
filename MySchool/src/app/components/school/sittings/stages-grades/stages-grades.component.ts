import { AfterViewInit, Component, HostListener, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { Observable } from 'rxjs';

import { AddStage, Stage, updateStage } from '../../../../core/models/stages-grades.modul';
import { StageService } from '../../../../core/services/stage.service';
import { ClassService } from '../../../../core/services/class.service';
import { PaginatorState } from 'primeng/paginator';

@Component({
  selector: 'app-stages-grades',
  templateUrl: './stages-grades.component.html',
  styleUrls: ['./stages-grades.component.scss',
    '../../../../shared/styles/button.scss'
  ]
})
export class StagesGradesComponent implements AfterViewInit, OnInit {
  activeTab: string = 'News';
  form: FormGroup;
  isEditMode = false;  // Track if we're in edit mode
  stageToEditId: number | null = null;
  combinedData$: Observable<any[]> | undefined;
  outerDropdownState: { [key: string]: boolean } = {};
  innerDropdownState: { [key: string]: { [key: string]: boolean } } = {};
  currentPage: { [key: string]: number } = {};
  stages: Stage[] = [];
  paginatedStage: Stage[] = [];
  update!: updateStage;
  errorMessage: string = '';

  private classService = inject(ClassService);
  
  constructor(
    private stageService: StageService,
    private formBuilder: FormBuilder,
    private toastr: ToastrService
  ) {
    this.form = this.formBuilder.group({
      StageName: ['', Validators.required],
      Note: ''
    });
  }

  ngOnInit(): void {
    this.getStage();
  }

  getStage(): void {
    this.stageService.getAllStages().subscribe({
      next: (data) =>{
        this.stages = data.stagesInfo;
        this.length = this.stages.length; // Set total item count
        this.updateDisplayedDivisions(); // Initialize displayed divisions
      },
      error: () => this.errorMessage = 'Failed to load stages'
    });
  }

  addStage(): void {
    this.form.markAllAsTouched();

    if (this.form.valid) {
      const addStageData: AddStage = this.form.value;
      this.stageService.AddStage(addStageData).subscribe({
        next: () => {
          this.getStage();
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

  editStage(stage: Stage): void {
    this.form.patchValue({
      StageName: stage.stageName,
      Note: stage.note
    });
    this.isEditMode = true;  // Enter edit mode
    this.stageToEditId = stage.stageID;  // Set the ID for editing
  }

  updateStage(): void {
    this.form.markAllAsTouched();
    if (this.form.valid && this.stageToEditId !== null) {
      const updateData: updateStage = this.form.value;
      this.stageService.Update(this.stageToEditId, updateData).subscribe({
        next: (response) => {
          if (response.success) {
           this.toastr.success(response.success,"Stage Updated Successfully");
           this.form.reset();
           this.getStage();  
          }
        },
        error: () => this.toastr.error('Failed to update stage', 'Error')
      });
      this.toastr.success('Stage updated successfully');
      this.form.reset();
      this.getStage(); 
      this.isEditMode = false;
    }
  }
  
  changeState(stage: Stage, isActive: boolean): void {
    const patchDoc = [
      { op: "replace", path: "/active", value: isActive }
    ];
  
    this.stageService.partialUpdate(stage.stageID, patchDoc).subscribe({
      next: (response) => {
        if (response.success) {
          this.toastr.success(response.message);
          this.getStage(); // Refresh the list to show updated data
        }
      },
      error: () => this.toastr.error('Failed to update stage', 'Error')
    });
  
    this.isEditMode = false;
  }
  


  // Method to delete a stage by ID
  deleteStage(id: number): void {
    this.stageService.DeleteStage(id).subscribe({
      next: (response) => {
        if (response.success) {
          this.toastr.success(response.message, 'Stage Deleted');
          this.getStage(); // Refresh the list after deletion
        }
      },
      error: () => this.toastr.error('Failed to delete stage', 'Error')
    });
  }

  deleteClass(ID: number): void {
    this.classService.Delete(ID).subscribe({
      next: (response) => {
        if (response.success) {
        }
      },
      error: () => this.toastr.error('يجب أن يكون الصف فارغ', 'خطأ')
    });
  }


  ngAfterViewInit(): void {
    const defaultOpen = document.getElementById('defaultOpen');
    if (defaultOpen) {
      defaultOpen.click();
    }
  }


  openPage(pageName: string, elmnt: EventTarget | null): void {
    let i: number;
    let tabcontent: HTMLCollectionOf<HTMLElement>;
    let tablinks: HTMLCollectionOf<HTMLElement>;

    tabcontent = document.getElementsByClassName("tabcontent") as HTMLCollectionOf<HTMLElement>;
    for (i = 0; i < tabcontent.length; i++) {
      tabcontent[i].style.display = "none";
    }

    tablinks = document.getElementsByClassName("tablink") as HTMLCollectionOf<HTMLElement>;
    for (i = 0; i < tablinks.length; i++) {
      tablinks[i].classList.remove('active'); // Remove active class from all buttons
    }

    document.getElementById(pageName)!.style.display = "block";
    if (elmnt instanceof HTMLElement) {
      elmnt.classList.add('active'); // Add active class to the clicked button
    }

    this.activeTab = pageName;
  }



  // Manage dropdown states
  openOuterDropdown: any = null;
  openInnerDropdown: any = null;
  openInnerDivision: any = null;

  toggleOuterDropdown(item: any): void {
    if (this.openOuterDropdown === item) {
      this.openOuterDropdown = null;
    } else {
      this.openOuterDropdown = item;
    }
  }

  isOuterDropdownOpen(item: any): boolean {
    return this.openOuterDropdown === item;
  }

  toggleInnerDropdown(item: any, division: any): void {
    if (this.openInnerDropdown === item && this.openInnerDivision === division) {
      this.openInnerDropdown = null;
      this.openInnerDivision = null;
    } else {
      this.openInnerDropdown = item;
      this.openInnerDivision = division;
    }
  }
  // Updated maxRowsPerPage for 4 rows per page
  maxStagesPerPage = 6; // Number of stages to display per page
  maxClassesPerPage = 3; // Number of classes to display per page in dropdown
  currentStagePage = 0; // Track the current page for stages
  currentClassPage: { [key: string]: number } = {}; // Track the current page for each class (stageID as key)


  // Method to initialize pagination for items
  initializePagination(item: any): void {
    if (this.currentPage[item.stageID] === undefined) {
      this.currentPage[item.stageID] = 0; // Initialize to first page
    }
  }

  // Get paginated stages based on current page
  getPaginatedStages() {
    const startIndex = this.currentStagePage * this.maxStagesPerPage;
    const endIndex = startIndex + this.maxStagesPerPage;
    return this.stages.slice(startIndex, endIndex);
  }

  // Get paginated classes for each stage based on current page
  getPaginatedClasses(item: any) {
    if (this.currentClassPage[item.stageID] === undefined) {
      this.currentClassPage[item.stageID] = 0; // Initialize if not set
    }
    const startIndex = this.currentClassPage[item.stageID] * this.maxClassesPerPage;
    const endIndex = startIndex + this.maxClassesPerPage;
    return item.classes.slice(startIndex, endIndex);
  }

  // Navigate to the next/previous stage page
  nextStagePage() {
    if ((this.currentStagePage + 1) * this.maxStagesPerPage < this.stages.length) {
      this.currentStagePage++;
    }
  }

  previousStagePage() {
    if (this.currentStagePage > 0) {
      this.currentStagePage--;
    }
  }

  // Navigate to the next/previous class page
  nextClassPage(item: any) {
    if ((this.currentClassPage[item.stageID] + 1) * this.maxClassesPerPage < item.classes.length) {
      this.currentClassPage[item.stageID]++;
    }
  }

  previousClassPage(item: any) {
    if (this.currentClassPage[item.stageID] > 0) {
      this.currentClassPage[item.stageID]--;
    }
  }

  // Total pages calculation
  getTotalStagePages(): number {
    return Math.ceil(this.stages.length / this.maxStagesPerPage);
  }

  getTotalClassPages(item: any): number {
    return Math.ceil(item.classes.length / this.maxClassesPerPage);
  }

  @HostListener('document:click', ['$event'])
  clickout(event: Event) {
    const target = event.target as HTMLElement;

    if (!target.closest('.dropdown-menu') && !target.closest('.btn')) {
      this.openOuterDropdown = null;
      this.openInnerDropdown = null;
      this.openInnerDivision = null;
    }
  }

  currentPages: number = 0; // Current page index
  pageSize: number = 4; // Number of items per page
  length: number = 0; // Total number of items

  updateDisplayedDivisions(): void {
    const startIndex = this.currentPages * this.pageSize;
    const endIndex = startIndex + this.pageSize;
    this.paginatedStage = this.stages.slice(startIndex, endIndex);
  }


  toggleStateDropdown(item: any): void {
    item.isDropdownOpen = !item.isDropdownOpen;
  }
  
  first: number = 0; // Current starting index
  rows: number = 4; // Number of rows per page
  updatePaginatedData(): void {
    const start = this.first;
    const end = this.first + this.rows;
    this.paginatedStage = this.stages.slice(start, end);
  }

  // Handle page change event from PrimeNG paginator
  onPageChange(event: PaginatorState): void {
    this.first = event.first || 0; // Default to 0 if undefined
    this.rows = event.rows || 4; // Default to 4 rows
    this.updatePaginatedData();
  }

}
