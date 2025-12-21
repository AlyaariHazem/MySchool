import { Component, EventEmitter, Input, Output, OnInit, OnDestroy, inject, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { PaginatorModule } from 'primeng/paginator';
import { DatePipe } from '@angular/common';
import { StudentsDataService } from '../../../core/services/students-data.service';

export interface TableColumn {
  field: string;
  header: string;
  sortable?: boolean;
  filterable?: boolean;
  template?: 'text' | 'date' | 'custom';
  formatter?: (value: any, row: any) => string;
}

@Component({
  selector: 'app-custom-table',
  standalone: true,
  imports: [CommonModule, FormsModule, MatCardModule, PaginatorModule, DatePipe],
  templateUrl: './custom-table.component.html',
  styleUrls: ['./custom-table.component.scss'],
})
export class CustomTableComponent implements OnInit, OnDestroy {
  @Input() data: any[] = [];
  @Input() columns: TableColumn[] = [];
  @Input() loading: boolean = false;
  @Input() totalRecords: number = 0;
  @Input() rows: number = 8;
  @Input() first: number = 0;
  @Input() rowsPerPageOptions: number[] = [8, 16, 25];
  @Input() showGlobalFilter: boolean = false;
  @Input() showActions: boolean = true;
  @Input() emptyMessage: string = 'لا توجد بيانات متاحة في الجدول';
  @Input() trackByField: string = 'id'; // Field to use for tracking items
  @Input() actionHeader: string = 'العملية';

  @Output() pageChange = new EventEmitter<any>();
  @Output() rowEdit = new EventEmitter<any>();
  @Output() rowDelete = new EventEmitter<any>();
  @Output() rowClick = new EventEmitter<any>();
  @Output() filterChange = new EventEmitter<Record<string, string>>();

  private studentsDataService = inject(StudentsDataService);

  globalFilterValue: string = '';
  columnFilters: Record<string, string> = {};
  openDropdownIndex: number | null = null;

  ngOnInit() {
    if (!this.columns || this.columns.length === 0) {
      console.warn('CustomTableComponent: No columns provided');
    }
  }

  onPageChange(event: any) {
    this.pageChange.emit(event);
  }

  onGlobalFilter(event: Event) {
    const value = (event.target as HTMLInputElement).value;
    this.globalFilterValue = value;
    // Emit filter event if needed
  }

  onColumnFilter(field: string, value: string) {
    if (value && value.trim() !== '') {
      this.columnFilters[field] = value;
    } else {
      delete this.columnFilters[field];
    }
    // Apply filter through service
    this.studentsDataService.setFilter(field, value);
    // Also emit the filter change with current state
    this.filterChange.emit({ ...this.columnFilters });
  }

  clearFilters() {
    this.globalFilterValue = '';
    this.columnFilters = {};
    this.studentsDataService.clearFilters();
    this.filterChange.emit({});
  }

  hasActiveFilters(): boolean {
    return this.globalFilterValue !== '' || Object.keys(this.columnFilters).length > 0;
  }

  toggleDropdown(index: number, event: Event) {
    event.stopPropagation();
    this.openDropdownIndex = this.openDropdownIndex === index ? null : index;
  }

  closeDropdown() {
    this.openDropdownIndex = null;
  }

  isDropdownOpen(index: number): boolean {
    return this.openDropdownIndex === index;
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    // Close dropdown when clicking outside
    if (this.openDropdownIndex !== null) {
      this.closeDropdown();
    }
  }

  ngOnDestroy() {
    this.closeDropdown();
  }

  getFieldValue(row: any, field: string): any {
    // Support nested fields like 'fullName.firstName'
    const fields = field.split('.');
    let value = row;
    for (const f of fields) {
      value = value?.[f];
    }
    return value ?? null;
  }

  getFormattedValue(row: any, column: TableColumn): string {
    const value = this.getFieldValue(row, column.field);
    
    if (column.formatter) {
      return column.formatter(value, row);
    }
    
    if (column.template === 'date' && value) {
      return new Date(value).toLocaleDateString('ar-EG');
    }
    
    return value ?? '-';
  }

  onEdit(row: any) {
    this.rowEdit.emit(row);
  }

  onDelete(row: any) {
    this.rowDelete.emit(row);
  }

  onRowClick(row: any) {
    this.rowClick.emit(row);
  }

  getTrackByValue(index: number, item: any): any {
    return this.getFieldValue(item, this.trackByField) ?? index;
  }
}

