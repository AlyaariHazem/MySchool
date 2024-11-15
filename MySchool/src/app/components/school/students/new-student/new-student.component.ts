import { Component, AfterViewInit, OnInit, Inject, ChangeDetectorRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Observable, combineLatest, map } from 'rxjs';
import { ToastrService } from 'ngx-toastr';
import { StageService } from '../../../../core/services/stage.service';

@Component({
  selector: 'app-new-student',
  templateUrl: './new-student.component.html',
  styleUrls: ['./new-student.component.scss']
})
export class NewStudentComponent implements OnInit, AfterViewInit {
  activeTab: string = 'DataStudent'; // Default active tab
  form: FormGroup;
  combinedData$: Observable<any[]> | undefined;
  currentPage: { [key: string]: number } = {};

  constructor(
    private formBuilder: FormBuilder,
    private stageService: StageService,
    private toastr: ToastrService,
    private changeDetectorRef: ChangeDetectorRef,
    public dialogRef: MatDialogRef<NewStudentComponent>, // Inject MatDialogRef
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {
    this.form = this.formBuilder.group({
      id: '',
      stage: ['', Validators.required],
      note: '',
      state: true
    });
  }

  ngOnInit(): void {
   
  }

  ngAfterViewInit(): void {
    setTimeout(() => {
      const defaultOpen = document.getElementById('defaultOpen');
      if (defaultOpen) {
        defaultOpen.click();
      }
    }, 0);
  }

  openPage(pageName: string, elmnt: EventTarget | null): void {
    this.activeTab = pageName; // Update activeTab property

    // Remove active class from all buttons
    const tablinks = document.getElementsByClassName("tablink") as HTMLCollectionOf<HTMLElement>;
    for (let i = 0; i < tablinks.length; i++) {
      tablinks[i].classList.remove('active');
    }

    // Add active class to the clicked button
    if (elmnt instanceof HTMLElement) {
      elmnt.classList.add('active');
    }

    // Force change detection (optional)
    this.changeDetectorRef.detectChanges();
  }

  closeModal(): void {
    this.dialogRef.close(); // Close the modal
  }

  

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

  isInnerDropdownOpen(item: any, division: any): boolean {
    return this.openInnerDropdown === item && this.openInnerDivision === division;
  }

  maxRowsPerPage = 3;

  getPaginatedGrades(item: any) {
    const startIndex = this.currentPage[item.id] * this.maxRowsPerPage;
    const endIndex = startIndex + this.maxRowsPerPage;
    return item.grades.slice(startIndex, endIndex);
  }

  nextPage(item: any) {
    if ((this.currentPage[item.id] + 1) * this.maxRowsPerPage < item.grades.length) {
      this.currentPage[item.id]++;
    }
  }

  previousPage(item: any) {
    if (this.currentPage[item.id] > 0) {
      this.currentPage[item.id]--;
    }
  }

  getTotalPages(item: any): number {
    return Math.ceil(item.grades.length / this.maxRowsPerPage);
  }
}
