import {
  Component,
  ElementRef,
  EventEmitter,
  HostListener,
  Input,
  OnChanges,
  OnDestroy,
  OnInit,
  Output,
  Renderer2,
  SimpleChanges,
  ViewChild,
  inject,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import {
  MatDatepickerInputEvent,
  MatDatepickerModule,
} from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { PaginatorModule } from 'primeng/paginator';
import { ButtonModule } from 'primeng/button';
import { DatePipe } from '@angular/common';
import { StudentsDataService } from '../../../core/services/students-data.service';
import { AgePipe } from '../../../Pipes/age.pipe';

/** Internal key for the actions column width map (not a data field). */
export const CUSTOM_TABLE_ACTIONS_FIELD = '__ct_actions__';

export type TableColumnFilterType = 'text' | 'select' | 'date';

export interface TableColumnFilterSelectOption {
  value: string;
  label: string;
}

export interface TableColumn {
  field: string;
  header: string;
  sortable?: boolean;
  filterable?: boolean;
  /**
   * Per-column filter UI when `filterable` is true. Default `text`.
   * Values are stored as strings in `filterChange` (`yyyy-MM-dd` for `date`).
   */
  filterType?: TableColumnFilterType;
  /** Required when `filterType === 'select'`. */
  filterSelectOptions?: TableColumnFilterSelectOption[];
  /** rowIndex = 1-based page index; debitBadge / statusToggle for accounts-style cells */
  template?:
    | 'text'
    | 'date'
    | 'custom'
    | 'rowIndex'
    | 'debitBadge'
    | 'statusToggle'
    | 'attachmentLinks'
    | 'rolePill';
  formatter?: (value: any, row: any) => string;
}

@Component({
  selector: 'app-custom-table',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
    PaginatorModule,
    ButtonModule,
    DatePipe,
    AgePipe,
  ],
  templateUrl: './custom-table.component.html',
  styleUrls: ['./custom-table.component.scss'],
})
export class CustomTableComponent implements OnInit, OnChanges, OnDestroy {
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
  /** When true, row action menu triggers are disabled (e.g. while a parent request is in flight). */
  @Input() actionsDisabled: boolean = false;
  @Input() actionHeader: string = 'العملية';
  @Input() tableDir: 'rtl' | 'ltr' = 'rtl';
  /** When false, rows are not clickable and do not use pointer cursor. */
  @Input() rowClickable: boolean = true;
  /** Drag the trailing edge of a header cell to resize; first drag snapshots all column widths. */
  @Input() resizableColumns: boolean = true;
  /**
   * When true (default), column filters sync with `StudentsDataService` (students list).
   * Set false for standalone tables (e.g. pending registration) that only use `filterChange`.
   */
  @Input() syncFiltersWithStudentsDataService: boolean = true;
  /**
   * Row actions: `editDelete` = ellipsis menu; `approveReject` = inline موافقة / رفض buttons
   * (for `rowApprove` / `rowReject`).
   */
  @Input() actionMenuMode: 'editDelete' | 'approveReject' = 'editDelete';
  /** Message shown in a single spanning row while `loading` is true. */
  @Input() loadingMessage: string = 'جاري التحميل…';

  @Output() pageChange = new EventEmitter<any>();
  @Output() rowEdit = new EventEmitter<any>();
  @Output() rowDelete = new EventEmitter<any>();
  @Output() rowClick = new EventEmitter<any>();
  @Output() filterChange = new EventEmitter<Record<string, string>>();
  @Output() statusToggle = new EventEmitter<any>();
  @Output() rowApprove = new EventEmitter<any>();
  @Output() rowReject = new EventEmitter<any>();

  private studentsDataService = inject(StudentsDataService);
  private renderer = inject(Renderer2);

  @ViewChild('tableScrollHost', { read: ElementRef }) tableScrollHost?: ElementRef<HTMLElement>;
  @ViewChild('dataTable', { read: ElementRef }) dataTableRef?: ElementRef<HTMLTableElement>;

  globalFilterValue: string = '';
  columnFilters: Record<string, string> = {};
  openDropdownIndex: number | null = null;
  /** Viewport-fixed position for the row action menu (avoids overflow inside scrollable table). */
  dropdownPanelStyle: { top: string; left: string } | null = null;

  /** Which column’s filter popover is open (field name); only one at a time. */
  activeFilterField: string | null = null;

  /**
   * Stable `Date` for datepicker `[ngModel]` (same ref while filter string unchanged).
   * A fresh `new Date()` each CD tick was confusing MatDatepicker and could freeze the UI.
   */
  private dateFilterModelCache: Record<string, { key: string; date: Date | null }> = {};

  private scrollUnlisten?: () => void;
  private readonly menuMinWidthPx = 170;
  private readonly menuHeightEstimatePx = 120;

  /** Pixel widths after first resize snapshot; drives <colgroup> and fixed layout. */
  columnWidthsPx: Partial<Record<string, number>> = {};
  actionColumnWidthPx: number | null = null;

  private resizeField: string | null = null;
  private resizeStartX = 0;
  private resizeStartW = 0;
  private resizeUnlistenMove?: () => void;
  private resizeUnlistenUp?: () => void;
  private readonly colResizeMinPx = 48;

  readonly actionsFieldKey = CUSTOM_TABLE_ACTIONS_FIELD;

  ngOnInit() {
    if (!this.columns || this.columns.length === 0) {
      console.warn('CustomTableComponent: No columns provided');
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['columns'] && !changes['columns'].firstChange) {
      this.columnWidthsPx = {};
      this.actionColumnWidthPx = null;
      this.activeFilterField = null;
      this.dateFilterModelCache = {};
      this.updateTableScrollListener();
    }
    if (changes['actionsDisabled']?.currentValue === true) {
      this.closeDropdown();
    }
  }

  get useFixedColumnLayout(): boolean {
    return Object.keys(this.columnWidthsPx).length > 0;
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
    if (this.syncFiltersWithStudentsDataService) {
      this.studentsDataService.setFilter(field, value);
    }
    this.filterChange.emit({ ...this.columnFilters });
  }

  toggleColumnFilter(field: string, event: MouseEvent): void {
    event.stopPropagation();
    if (!this.showHeaderFilters) {
      return;
    }
    this.activeFilterField = this.activeFilterField === field ? null : field;
    this.updateTableScrollListener();
  }

  hasColumnFilterValue(field: string): boolean {
    const v = this.columnFilters[field];
    return typeof v === 'string' && v.trim() !== '';
  }

  /** `yyyy-MM-dd` string from `columnFilters` as a stable local `Date` for the datepicker. */
  getDateFilterModel(field: string): Date | null {
    const raw =
      this.columnFilters[field] && typeof this.columnFilters[field] === 'string'
        ? this.columnFilters[field].trim()
        : '';
    const cached = this.dateFilterModelCache[field];
    if (cached && cached.key === raw) {
      return cached.date;
    }
    let date: Date | null = null;
    if (raw) {
      const m = /^(\d{4})-(\d{2})-(\d{2})/.exec(raw);
      if (m) {
        const d = new Date(+m[1], +m[2] - 1, +m[3]);
        date = isNaN(d.getTime()) ? null : d;
      }
    }
    this.dateFilterModelCache[field] = { key: raw, date };
    return date;
  }

  /** Use `(dateChange)` only — `(ngModelChange)` fires during calendar navigation and can hang the page. */
  onDateFilterInputChange(field: string, e: MatDatepickerInputEvent<Date | null>): void {
    const d = e.value;
    if (!d) {
      this.onColumnFilter(field, '');
      return;
    }
    const y = d.getFullYear();
    const mo = d.getMonth() + 1;
    const day = d.getDate();
    const iso = `${y}-${String(mo).padStart(2, '0')}-${String(day).padStart(2, '0')}`;
    this.onColumnFilter(field, iso);
  }

  clearFilters() {
    this.globalFilterValue = '';
    this.columnFilters = {};
    this.dateFilterModelCache = {};
    this.activeFilterField = null;
    if (this.syncFiltersWithStudentsDataService) {
      this.studentsDataService.clearFilters();
    }
    this.filterChange.emit({});
    this.updateTableScrollListener();
  }

  hasActiveFilters(): boolean {
    return this.globalFilterValue !== '' || Object.keys(this.columnFilters).length > 0;
  }

  toggleDropdown(index: number, event: Event) {
    event.stopPropagation();
    if (this.actionsDisabled) {
      return;
    }
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
    this.updateTableScrollListener();
  }

  closeDropdown() {
    this.openDropdownIndex = null;
    this.dropdownPanelStyle = null;
    this.updateTableScrollListener();
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

  /** One listener: closes row menu and/or column filter popover on table scroll. */
  private updateTableScrollListener(): void {
    this.detachScrollClose();
    if (this.openDropdownIndex === null && this.activeFilterField === null) {
      return;
    }
    const el = this.tableScrollHost?.nativeElement;
    if (!el) {
      return;
    }
    this.scrollUnlisten = this.renderer.listen(el, 'scroll', () => {
      this.openDropdownIndex = null;
      this.dropdownPanelStyle = null;
      this.activeFilterField = null;
      this.detachScrollClose();
    });
  }

  private detachScrollClose(): void {
    if (this.scrollUnlisten) {
      this.scrollUnlisten();
      this.scrollUnlisten = undefined;
    }
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    const t = event.target as HTMLElement | null;
    if (!t) {
      this.closeDropdown();
      this.activeFilterField = null;
      this.detachScrollClose();
      return;
    }

    if (this.openDropdownIndex !== null) {
      if (!t.closest('.ct-action-menu-panel') && !t.closest('.ct-actions-trigger')) {
        this.closeDropdown();
      }
    }

    if (this.activeFilterField !== null) {
      const inFilterWrap = !!t.closest('.ct-col-filter-wrap');
      const inOverlay =
        !!t.closest('.cdk-overlay-container') ||
        !!t.closest('.mat-mdc-select-panel') ||
        !!t.closest('.mat-datepicker-content');
      if (!inFilterWrap && !inOverlay) {
        this.activeFilterField = null;
        this.updateTableScrollListener();
      }
    }
  }

  @HostListener('window:resize')
  onWindowResize(): void {
    if (this.openDropdownIndex !== null) {
      this.closeDropdown();
    }
    this.activeFilterField = null;
    this.updateTableScrollListener();
  }

  ngOnDestroy() {
    this.activeFilterField = null;
    this.openDropdownIndex = null;
    this.dropdownPanelStyle = null;
    this.detachScrollClose();
    this.endColumnResize();
  }

  onColResizePointerDown(field: string, event: PointerEvent): void {
    if (!this.resizableColumns || event.button !== 0) {
      return;
    }
    event.preventDefault();
    event.stopPropagation();
    this.snapshotColumnWidthsFromDom();
    this.resizeField = field;
    this.resizeStartX = event.clientX;
    if (field === CUSTOM_TABLE_ACTIONS_FIELD) {
      this.resizeStartW = this.actionColumnWidthPx ?? this.colResizeMinPx;
    } else {
      this.resizeStartW = this.columnWidthsPx[field] ?? this.colResizeMinPx;
    }
    this.resizeUnlistenMove = this.renderer.listen('document', 'pointermove', (e: Event) =>
      this.onColResizePointerMove(e as PointerEvent),
    );
    this.resizeUnlistenUp = this.renderer.listen('document', 'pointerup', (e: Event) =>
      this.onColResizePointerUp(e as PointerEvent),
    );
  }

  private snapshotColumnWidthsFromDom(): void {
    if (Object.keys(this.columnWidthsPx).length > 0) {
      return;
    }
    const table = this.dataTableRef?.nativeElement;
    if (!table || !this.columns?.length) {
      return;
    }
    const ths = table.querySelectorAll<HTMLElement>('thead tr th');
    let i = 0;
    for (const col of this.columns) {
      const th = ths[i++];
      if (th) {
        this.columnWidthsPx[col.field] = Math.max(
          this.colResizeMinPx,
          Math.round(th.getBoundingClientRect().width),
        );
      }
    }
    if (this.showActions) {
      const th = ths[i];
      if (th) {
        this.actionColumnWidthPx = Math.max(
          this.colResizeMinPx,
          Math.round(th.getBoundingClientRect().width),
        );
      }
    }
  }

  private onColResizePointerMove(event: PointerEvent): void {
    if (this.resizeField === null) {
      return;
    }
    const delta =
      this.tableDir === 'rtl' ? this.resizeStartX - event.clientX : event.clientX - this.resizeStartX;
    const w = Math.max(this.colResizeMinPx, Math.round(this.resizeStartW + delta));
    if (this.resizeField === CUSTOM_TABLE_ACTIONS_FIELD) {
      this.actionColumnWidthPx = w;
    } else {
      this.columnWidthsPx = { ...this.columnWidthsPx, [this.resizeField]: w };
    }
  }

  private onColResizePointerUp(_event: PointerEvent): void {
    this.endColumnResize();
  }

  private endColumnResize(): void {
    this.resizeField = null;
    if (this.resizeUnlistenMove) {
      this.resizeUnlistenMove();
      this.resizeUnlistenMove = undefined;
    }
    if (this.resizeUnlistenUp) {
      this.resizeUnlistenUp();
      this.resizeUnlistenUp = undefined;
    }
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

  onApproveRow(row: any, event: Event): void {
    event.stopPropagation();
    this.rowApprove.emit(row);
  }

  onRejectRow(row: any, event: Event): void {
    event.stopPropagation();
    this.rowReject.emit(row);
  }

  getTrackByValue(index: number, item: any): any {
    return this.getFieldValue(item, this.trackByField) ?? index;
  }
}

