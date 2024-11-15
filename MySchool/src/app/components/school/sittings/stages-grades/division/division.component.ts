import { Component, inject, OnInit, ViewEncapsulation } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { divisions } from '../../../../../core/models/division.model';
import { DivisionService } from '../../../../../core/services/division.service';
import { PageEvent } from '@angular/material/paginator';

@Component({
  selector: 'app-division',
  templateUrl: './division.component.html',
  styleUrls: ['./division.component.scss']
})
export class DivisionComponent implements OnInit {
  divisions: Array<divisions> = [];
  displayedDivisions: Array<divisions> = [];
  form: FormGroup;
  
  currentPage: number = 0; // Current page index
  pageSize: number = 5; // Number of items per page
  length: number = 0; // Total number of items

  toastr = inject(ToastrService);
  divisionService = inject(DivisionService);

  constructor(private formBuilder: FormBuilder) {
    this.form = this.formBuilder.group({
      id: '',
      division: '',
      grade: '',
      state: true
    });
  }

  ngOnInit(): void {
    this.getAllDivisions();
  }

  getAllDivisions(): void {
    this.divisionService.getAll().subscribe({
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
