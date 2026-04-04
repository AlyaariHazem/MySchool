import {
  Component,
  ElementRef,
  EventEmitter,
  HostListener,
  Input,
  OnDestroy,
  OnInit,
  Output,
  Renderer2,
  ViewChild,
  inject,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { PaginatorModule } from 'primeng/paginator';
import { DatePipe } from '@angular/common';
import { StudentsDataService } from '../../../core/services/students-data.service';
import { AgePipe } from '../../../Pipes/age.pipe';

export interface TableColumn {
  field: string;
  header: string;
  sortable?: boolean;
  filterable?: boolean;
  /** rowIndex = 1-based page index; debitBadge / statusToggle for accounts-style cells */
  template?: 'text' | 'date' | 'custom' | 'rowIndex' | 'debitBadge' | 'statusToggle';
  formatter?: (value: any, row: any) => string;
}

@Component({
  selector: 'app-custom-table',
  standalone: true,
  imports: [CommonModule, FormsModule, MatCardModule, PaginatorModule, DatePipe, AgePipe],
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
  /** When false, per-column filter inputs in the header are hidden (column.filterable is ignored). */
  @Input() showHeaderFilters: boolean = true;
  @Input() showActions: boolean = true;
  @Input() emptyMessage: string = 'لا توجد بيانات متاحة في الجدول';
  @Input() trackByField: string = 'id'; // Field to use for tracking items
  @Input() actionHeader: string = 'العملية';
  @Input() tableDir: 'rtl' | 'ltr' = 'rtl';
  /** When false, rows are not clickable and do not use pointer cursor. */
  @Input() rowClickable: boolean = true;

  @Output() pageChange = new EventEmitter<any>();
  @Output() rowEdit = new EventEmitter<any>();
  @Output() rowDelete = new EventEmitter<any>();
  @Output() rowClick = new EventEmitter<any>();
  @Output() filterChange = new EventEmitter<Record<string, string>>();
  @Output() statusToggle = new EventEmitter<any>();

  private studentsDataService = inject(StudentsDataService);
  private renderer = inject(Renderer2);

  @ViewChild('tableScrollHost', { read: ElementRef }) tableScrollHost?: ElementRef<HTMLElement>;

  globalFilterValue: string = '';
  columnFilters: Record<string, string> = {};
  openDropdownIndex: number | null = null;
  /** Viewport-fixed position for the row action menu (avoids overflow inside scrollable table). */
  dropdownPanelStyle: { top: string; left: string } | null = null;

  private scrollUnlisten?: () => void;
  private readonly menuMinWidthPx = 170;
  private readonly menuHeightEstimatePx = 120;

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
    if (this.openDropdownIndex === index) {
      this.closeDropdown();
      return;
    }
    const btn = event.currentTarget as HTMLElement | null;
    if (!btn) {
      return;
    }
    const r = btn.getBoundingClientRect();
    const { top, left } = this.computeMenuPosition(r);
    this.dropdownPanelStyle = { top: `${top}px`, left: `${left}px` };
    this.openDropdownIndex = index;
    this.attachScrollClose();
  }

  closeDropdown() {
    this.openDropdownIndex = null;
    this.dropdownPanelStyle = null;
    this.detachScrollClose();
  }

  isDropdownOpen(index: number): boolean {
    return this.openDropdownIndex === index;
  }

  private computeMenuPosition(trigger: DOMRect): { top: number; left: number } {
    const gap = 6;
    const vw = typeof window !== 'undefined' ? window.innerWidth : 1024;
    const vh = typeof window !== 'undefined' ? window.innerHeight : 768;
    const w = this.menuMinWidthPx;
    let left = this.tableDir === 'rtl' ? trigger.right - w : trigger.left;
    left = Math.max(8, Math.min(left, vw - w - 8));
    let top = trigger.bottom + gap;
    if (top + this.menuHeightEstimatePx > vh) {
      top = trigger.top - this.menuHeightEstimatePx - gap;
    }
    top = Math.max(8, Math.min(top, vh - this.menuHeightEstimatePx - 8));
    return { top, left };
  }

  private attachScrollClose(): void {
    this.detachScrollClose();
    const el = this.tableScrollHost?.nativeElement;
    if (!el) {
      return;
    }
    this.scrollUnlisten = this.renderer.listen(el, 'scroll', () => this.closeDropdown());
  }

  private detachScrollClose(): void {
    if (this.scrollUnlisten) {
      this.scrollUnlisten();
      this.scrollUnlisten = undefined;
    }
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    if (this.openDropdownIndex === null) {
      return;
    }
    const t = event.target as HTMLElement | null;
    if (!t) {
      this.closeDropdown();
      return;
    }
    if (t.closest('.ct-action-menu-panel') || t.closest('.ct-actions-trigger')) {
      return;
    }
    this.closeDropdown();
  }

  @HostListener('window:resize')
  onWindowResize(): void {
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
    this.closeDropdown();
    if (!this.rowClickable) {
      return;
    }
    this.rowClick.emit(row);
  }

  onStatusToggleClick(row: any, event: Event) {
    event.stopPropagation();
    this.statusToggle.emit(row);
  }

  getTrackByValue(index: number, item: any): any {
    return this.getFieldValue(item, this.trackByField) ?? index;
  }
}

