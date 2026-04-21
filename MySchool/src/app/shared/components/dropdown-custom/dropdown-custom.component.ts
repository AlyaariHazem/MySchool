import {
  Component,
  EventEmitter,
  OnDestroy,
  Output,
  Input,
  ViewChild,
  forwardRef,
  inject
} from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { Select } from 'primeng/select';
import {
  SelectChangeEvent,
  SelectFilterEvent,
  SelectLazyLoadEvent
} from 'primeng/select';
import { ToastrService } from 'ngx-toastr';
import { StudentService } from '../../../core/services/student.service';
import { StudentNameIdDTO, StudentNameIdSearchRequest } from '../../../core/models/students.model';
import { ClassNameLookupRow, ClassService } from '../../../components/school/core/services/class.service';
import { TeacherNameLookupRow, TeacherService } from '../../../components/school/core/services/teacher.service';

/**
 * Searchable lazy dropdown (PrimeNG `p-select` + optional `p-floatlabel` like supervisor visits):
 * - `student` (default): POST Students/names-ids, value = StudentNameIdDTO
 * - `class`: POST Classes/GetAllNameClasses/page, value = classID (number) for reactive forms
 * - `teacher`: POST Teacher/names/page, value = teacherID (number) for reactive forms
 */
@Component({
  selector: 'app-dropdown-custom',
  templateUrl: './dropdown-custom.component.html',
  styleUrl: './dropdown-custom.component.scss',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => DropdownCustomComponent),
      multi: true
    }
  ]
})
export class DropdownCustomComponent implements ControlValueAccessor, OnDestroy {
  @ViewChild('dropdownSelect')
  dropdownSelect?: Select;

  @Input() inputId = 'dropdownCustomSearch';
  /** When set, wraps `p-select` in `p-floatlabel` + translated label (same pattern as supervisor visits). */
  @Input() labelKey = '';
  @Input() placeholder = '';
  @Input() disabled = false;
  /** Panel width cap; matches supervisor `filterSelectPanelStyle`. */
  @Input() selectPanelStyle: Record<string, string> | null = null;
  /** student: POST Students/names-ids; class: POST Classes/GetAllNameClasses/page; teacher: POST Teacher/names/page */
  @Input() resource: 'student' | 'class' | 'teacher' = 'student';
  @Input() pageSize = 5;
  @Input() searchDelayMs = 400;
  @Input() scrollHeight = '240px';
  /** Keep close to rendered `.dropdown-custom__item` row height when `lazyVirtualScroll` is true. */
  @Input() virtualScrollItemSize = 40;
  /** Applied as `p-select` `styleClass` (maps legacy `dropdown-custom__input` to the select shell). */
  @Input() inputStyleClass = 'w-full';
  /**
   * When false, turns off PrimeNG virtual scroll + lazy loading for the suggestion list.
   * Virtual + lazy inside overlays (e.g. p-dialog) can trigger repeated onLazyLoad and freeze the tab.
   */
  @Input() lazyVirtualScroll = true;
  /**
   * When true (default), first input focus loads suggestions then calls overlay.show() — in dialogs that can
   * re-trigger focus and loop GET Teacher/names/page. Set false for dropdowns inside p-dialog / modals.
   */
  @Input() openSuggestionPanelOnFocus = true;

  @Output() selectionChange = new EventEmitter<StudentNameIdDTO>();

  private studentService = inject(StudentService);
  private classService = inject(ClassService);
  private teacherService = inject(TeacherService);
  private toastr = inject(ToastrService);

  /** Panel model: student row, class row, or teacher row */
  selectedPanel: StudentNameIdDTO | ClassNameLookupRow | TeacherNameLookupRow | null = null;
  filteredPanel: Array<StudentNameIdDTO | ClassNameLookupRow | TeacherNameLookupRow> = [];

  get optionLabelField(): 'fullName' | 'className' {
    return this.resource === 'class' ? 'className' : 'fullName';
  }

  get minQueryLength(): number {
    return this.resource === 'class' || this.resource === 'teacher' ? 0 : 1;
  }

  /** PrimeNG option identity for virtual scroll / selection. */
  get suggestionDataKey(): string {
    if (this.resource === 'class') return 'classID';
    if (this.resource === 'teacher') return 'teacherID';
    return 'studentID';
  }

  suggestionRowId(row: StudentNameIdDTO | ClassNameLookupRow | TeacherNameLookupRow): number {
    if (this.resource === 'class') return (row as ClassNameLookupRow).classID;
    if (this.resource === 'teacher') return (row as TeacherNameLookupRow).teacherID;
    return (row as StudentNameIdDTO).studentID;
  }

  suggestionRowLabel(row: StudentNameIdDTO | ClassNameLookupRow | TeacherNameLookupRow): string {
    if (this.resource === 'class') return String((row as ClassNameLookupRow).className ?? '');
    return String((row as TeacherNameLookupRow | StudentNameIdDTO).fullName ?? '');
  }

  get selectPanelStyleResolved(): Record<string, string> {
    return this.selectPanelStyle ?? { maxWidth: 'min(22rem, calc(100vw - 2rem))' };
  }

  get selectControlStyleClass(): string {
    const ic = (this.inputStyleClass || '').trim();
    if (!ic) {
      return 'dropdown-custom__select w-full';
    }
    const stripped = ic
      .replace(/\bdropdown-custom__input\b/g, 'dropdown-custom__select')
      .replace(/\bp-inputtext\b/g, '')
      .replace(/\bp-component\b/g, '')
      .replace(/\s+/g, ' ')
      .trim();
    return [stripped, 'dropdown-custom__select'].filter(Boolean).join(' ');
  }

  private lastStudentQuery: Pick<StudentNameIdSearchRequest, 'studentID' | 'fullName'> | null = null;
  private lastClassSearchNormalized: string | undefined = undefined;
  /** After first successful class search; cleared on reset so lazy-load does not fire with stale state. */
  private lastClassQueryReady = false;
  private lastTeacherSearchNormalized: string | undefined = undefined;
  private lastTeacherQueryReady = false;
  private loadedPage = 0;
  private totalPages = 0;
  private loadingMore = false;
  private requestSeq = 0;
  private noScrollAppendDone = false;
  /** Skips the next writeValue for this teacher id — parent ngModel echoes selection before panel state is stable. */
  private pendingTeacherSelectEcho: number | null = null;
  private filterDebounceHandle: ReturnType<typeof setTimeout> | undefined;

  private onChange: (v: StudentNameIdDTO | number | null) => void = () => {};
  private onTouched: () => void = () => {};

  writeValue(obj: StudentNameIdDTO | number | null): void {
    if (this.resource === 'class') {
      const id = typeof obj === 'number' && obj > 0 ? obj : null;
      if (id == null) {
        this.selectedPanel = null;
        return;
      }
      this.selectedPanel = { classID: id, className: '' };
      this.classService.getClassById(id).subscribe({
        next: (res) => {
          if (!res.isSuccess || !res.result) {
            return;
          }
          const cur = this.selectedPanel as ClassNameLookupRow | null;
          if (!cur || cur.classID !== id) {
            return;
          }
          this.selectedPanel = {
            classID: id,
            className: res.result.className
          };
        },
        error: () => {}
      });
      return;
    }

    if (this.resource === 'teacher') {
      const id = typeof obj === 'number' && obj > 0 ? obj : null;
      if (id == null) {
        this.selectedPanel = null;
        return;
      }
      if (this.pendingTeacherSelectEcho === id) {
        return;
      }
      const existing = this.selectedPanel as TeacherNameLookupRow | null;
      // Avoid clearing the label after onSelect — that re-triggers autocomplete + forceSelection and can freeze the UI.
      if (existing?.teacherID === id && String(existing.fullName ?? '').trim() !== '') {
        return;
      }
      this.selectedPanel = { teacherID: id, fullName: '' };
      this.teacherService.getTeacherNameLookup(id).subscribe({
        next: (res) => {
          if (!res.isSuccess || !res.result) {
            return;
          }
          const cur = this.selectedPanel as TeacherNameLookupRow | null;
          if (!cur || cur.teacherID !== id) {
            return;
          }
          this.selectedPanel = {
            teacherID: id,
            fullName: res.result.fullName
          };
        },
        error: () => {}
      });
      return;
    }

    this.selectedPanel = (obj as StudentNameIdDTO | null) ?? null;
  }

  registerOnChange(fn: (v: StudentNameIdDTO | number | null) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }

  ngOnDestroy(): void {
    if (this.filterDebounceHandle != null) {
      clearTimeout(this.filterDebounceHandle);
      this.filterDebounceHandle = undefined;
    }
  }

  /** Prefetch when the closed control is focused (dialog / `openSuggestionPanelOnFocus=false` pattern). */
  onSelectTriggerFocus(): void {
    if (this.disabled) {
      return;
    }
    if (!this.openSuggestionPanelOnFocus) {
      if (this.resource === 'class') {
        this.loadClassSuggestionsFromQuery('');
      } else if (this.resource === 'teacher') {
        this.loadTeacherSuggestionsFromQuery('');
      } else {
        this.loadStudentSuggestionsFromQuery('');
      }
    }
  }

  onSelectShow(): void {
    if (this.disabled) {
      return;
    }
    if (this.resource === 'class') {
      this.loadClassSuggestionsFromQuery('');
    } else if (this.resource === 'teacher') {
      this.loadTeacherSuggestionsFromQuery('');
    } else {
      this.loadStudentSuggestionsFromQuery('');
    }
  }

  onSelectFilter(event: SelectFilterEvent): void {
    if (this.filterDebounceHandle != null) {
      clearTimeout(this.filterDebounceHandle);
    }
    const rawFilter = String(event.filter ?? '');
    this.filterDebounceHandle = setTimeout(() => {
      this.filterDebounceHandle = undefined;
      const raw = rawFilter.trim();
      if (this.resource === 'class') {
        this.loadClassSuggestionsFromQuery(raw);
      } else if (this.resource === 'teacher') {
        this.loadTeacherSuggestionsFromQuery(raw);
      } else if (raw.length === 0) {
        this.filteredPanel = [];
      } else {
        this.loadStudentSuggestionsFromQuery(raw);
      }
    }, this.searchDelayMs);
  }

  private loadStudentSuggestionsFromQuery(raw: string): void {
    const req: StudentNameIdSearchRequest = {
      pageNumber: 1,
      pageSize: this.pageSize
    };
    if (/^\d+$/.test(raw)) {
      req.studentID = parseInt(raw, 10);
    } else if (raw.length > 0) {
      req.fullName = raw;
    }

    const seq = ++this.requestSeq;
    this.loadingMore = false;
    this.loadedPage = 0;
    this.totalPages = 0;
    this.lastStudentQuery = null;
    this.noScrollAppendDone = false;

    this.studentService.searchStudentNamesAndIds(req).subscribe({
      next: (page) => {
        if (seq !== this.requestSeq) {
          return;
        }
        this.filteredPanel = page.data;
        this.loadedPage = page.pageNumber;
        this.totalPages = Math.max(1, page.totalPages);
        this.lastStudentQuery =
          req.studentID != null && req.studentID > 0
            ? { studentID: req.studentID }
            : req.fullName != null && String(req.fullName).trim() !== ''
              ? { fullName: String(req.fullName).trim() }
              : {};
      },
      error: () => {
        if (seq !== this.requestSeq) {
          return;
        }
        this.filteredPanel = [];
        this.lastStudentQuery = null;
        this.toastr.error('تعذّر البحث عن الطلاب', 'خطأ');
      }
    });
  }

  private loadClassSuggestionsFromQuery(raw: string): void {
    const seq = ++this.requestSeq;
    this.loadingMore = false;
    this.loadedPage = 0;
    this.totalPages = 0;
    this.lastClassSearchNormalized = undefined;
    this.lastClassQueryReady = false;
    this.noScrollAppendDone = false;

    const search = raw.length > 0 ? raw : undefined;

    this.classService.getClassNamesPage({ pageIndex: 0, pageSize: this.pageSize, search }).subscribe({
      next: (page) => {
        if (seq !== this.requestSeq) {
          return;
        }
        this.filteredPanel = page.data;
        this.loadedPage = page.pageNumber;
        this.totalPages = Math.max(1, page.totalPages);
        this.lastClassSearchNormalized = search;
        this.lastClassQueryReady = true;
      },
      error: () => {
        if (seq !== this.requestSeq) {
          return;
        }
        this.filteredPanel = [];
        this.lastClassSearchNormalized = undefined;
        this.lastClassQueryReady = false;
        this.toastr.error('تعذّر تحميل الصفوف', 'خطأ');
      }
    });
  }

  private loadTeacherSuggestionsFromQuery(raw: string): void {
    const seq = ++this.requestSeq;
    this.loadingMore = false;
    this.loadedPage = 0;
    this.totalPages = 0;
    this.lastTeacherSearchNormalized = undefined;
    this.lastTeacherQueryReady = false;
    this.noScrollAppendDone = false;

    const search = raw.length > 0 ? raw : undefined;

    this.teacherService.getTeacherNamesPage({ pageIndex: 0, pageSize: this.pageSize, search }).subscribe({
      next: (page) => {
        if (seq !== this.requestSeq) {
          return;
        }
        this.filteredPanel = page.data;
        this.loadedPage = page.pageNumber;
        this.totalPages = Math.max(1, page.totalPages);
        this.lastTeacherSearchNormalized = search;
        this.lastTeacherQueryReady = true;
      },
      error: () => {
        if (seq !== this.requestSeq) {
          return;
        }
        this.filteredPanel = [];
        this.lastTeacherSearchNormalized = undefined;
        this.lastTeacherQueryReady = false;
        this.toastr.error('تعذّر تحميل المعلمين', 'خطأ');
      }
    });
  }

  onLazyLoad(event: SelectLazyLoadEvent): void {
    if (!this.lazyVirtualScroll) {
      return;
    }
    if (this.resource === 'class') {
      this.onLazyLoadPaged(
        event,
        () => (this.lastClassQueryReady ? ({} as Record<string, never>) : null),
        () => this.loadNextClassPage()
      );
    } else if (this.resource === 'teacher') {
      this.onLazyLoadPaged(
        event,
        () => (this.lastTeacherQueryReady ? ({} as Record<string, never>) : null),
        () => this.loadNextTeacherPage()
      );
    } else {
      this.onLazyLoadPaged(event, () => this.lastStudentQuery, () => this.loadNextStudentPage());
    }
  }

  private onLazyLoadPaged(
    event: SelectLazyLoadEvent,
    lastQuery: () => unknown,
    loadNext: () => void
  ): void {
    const len = this.filteredPanel.length;
    if (len === 0 || this.loadingMore || lastQuery() == null) {
      return;
    }
    if (this.loadedPage >= this.totalPages) {
      return;
    }

    const first = Number(event.first);
    const last = Number(event.last);
    if (!Number.isFinite(first) || !Number.isFinite(last)) {
      return;
    }

    const nearEndOfLoaded = last >= len - 1;
    if (!nearEndOfLoaded) {
      return;
    }

    const scrolledAwayFromTop = first > 0;
    const canAppendWithoutScroll =
      first === 0 &&
      len === this.pageSize &&
      this.loadedPage < this.totalPages &&
      !this.noScrollAppendDone;

    if (!scrolledAwayFromTop && !canAppendWithoutScroll) {
      return;
    }

    if (canAppendWithoutScroll && !scrolledAwayFromTop) {
      this.noScrollAppendDone = true;
    }

    loadNext();
  }

  private loadNextStudentPage(): void {
    if (this.lastStudentQuery == null || this.loadingMore || this.loadedPage >= this.totalPages) {
      return;
    }

    const seq = this.requestSeq;
    const nextPage = this.loadedPage + 1;
    this.loadingMore = true;

    const req: StudentNameIdSearchRequest = {
      ...this.lastStudentQuery,
      pageNumber: nextPage,
      pageSize: this.pageSize
    };

    this.studentService.searchStudentNamesAndIds(req).subscribe({
      next: (page) => {
        this.loadingMore = false;
        if (seq !== this.requestSeq) {
          return;
        }
        const existing = new Set(
          this.filteredPanel.map((s) => (s as StudentNameIdDTO).studentID)
        );
        const merged = [...this.filteredPanel] as StudentNameIdDTO[];
        for (const row of page.data) {
          if (!existing.has(row.studentID)) {
            merged.push(row);
            existing.add(row.studentID);
          }
        }
        this.filteredPanel = merged;
        this.loadedPage = page.pageNumber;
        this.totalPages = Math.max(this.totalPages, Math.max(1, page.totalPages));
      },
      error: () => {
        this.loadingMore = false;
        if (seq === this.requestSeq) {
          this.toastr.error('تعذّر تحميل المزيد من الطلاب', 'خطأ');
        }
      }
    });
  }

  private loadNextClassPage(): void {
    if (!this.lastClassQueryReady || this.loadingMore || this.loadedPage >= this.totalPages) {
      return;
    }

    const seq = this.requestSeq;
    const nextPageIndex = this.loadedPage;
    this.loadingMore = true;

    this.classService
      .getClassNamesPage({
        pageIndex: nextPageIndex,
        pageSize: this.pageSize,
        search: this.lastClassSearchNormalized
      })
      .subscribe({
        next: (page) => {
          this.loadingMore = false;
          if (seq !== this.requestSeq) {
            return;
          }
          const existing = new Set(
            this.filteredPanel.map((c) => (c as ClassNameLookupRow).classID)
          );
          const merged = [...this.filteredPanel] as ClassNameLookupRow[];
          for (const row of page.data) {
            if (!existing.has(row.classID)) {
              merged.push(row);
              existing.add(row.classID);
            }
          }
          this.filteredPanel = merged;
          this.loadedPage = page.pageNumber;
          this.totalPages = Math.max(this.totalPages, Math.max(1, page.totalPages));
        },
        error: () => {
          this.loadingMore = false;
          if (seq === this.requestSeq) {
            this.toastr.error('تعذّر تحميل المزيد من الصفوف', 'خطأ');
          }
        }
      });
  }

  private loadNextTeacherPage(): void {
    if (!this.lastTeacherQueryReady || this.loadingMore || this.loadedPage >= this.totalPages) {
      return;
    }

    const seq = this.requestSeq;
    const nextPageIndex = this.loadedPage;
    this.loadingMore = true;

    this.teacherService
      .getTeacherNamesPage({
        pageIndex: nextPageIndex,
        pageSize: this.pageSize,
        search: this.lastTeacherSearchNormalized
      })
      .subscribe({
        next: (page) => {
          this.loadingMore = false;
          if (seq !== this.requestSeq) {
            return;
          }
          const existing = new Set(
            this.filteredPanel.map((t) => (t as TeacherNameLookupRow).teacherID)
          );
          const merged = [...this.filteredPanel] as TeacherNameLookupRow[];
          for (const row of page.data) {
            if (!existing.has(row.teacherID)) {
              merged.push(row);
              existing.add(row.teacherID);
            }
          }
          this.filteredPanel = merged;
          this.loadedPage = page.pageNumber;
          this.totalPages = Math.max(this.totalPages, Math.max(1, page.totalPages));
        },
        error: () => {
          this.loadingMore = false;
          if (seq === this.requestSeq) {
            this.toastr.error('تعذّر تحميل المزيد من المعلمين', 'خطأ');
          }
        }
      });
  }

  onClearSelection(): void {
    this.dropdownSelect?.resetFilter();
    this.filteredPanel = [];
    this.lastStudentQuery = null;
    this.lastClassSearchNormalized = undefined;
    this.lastClassQueryReady = false;
    this.lastTeacherSearchNormalized = undefined;
    this.lastTeacherQueryReady = false;
    this.pendingTeacherSelectEcho = null;
    this.loadedPage = 0;
    this.totalPages = 0;
    this.loadingMore = false;
    this.noScrollAppendDone = false;
    this.requestSeq++;
    this.selectedPanel = null;
    this.onChange(null);
    this.onTouched();
  }

  onPrimeSelectChange(event: SelectChangeEvent): void {
    const v = event.value as StudentNameIdDTO | ClassNameLookupRow | TeacherNameLookupRow | null;
    if (v == null) {
      return;
    }
    this.handleRowSelected(v);
  }

  private handleRowSelected(
    row: StudentNameIdDTO | ClassNameLookupRow | TeacherNameLookupRow
  ): void {
    if (this.resource === 'class') {
      const r = row as ClassNameLookupRow;
      if (!r?.classID) {
        return;
      }
      this.onChange(r.classID);
      this.onTouched();
      return;
    }

    if (this.resource === 'teacher') {
      const r = row as TeacherNameLookupRow;
      if (!r?.teacherID) {
        return;
      }
      const id = r.teacherID;
      this.pendingTeacherSelectEcho = id;
      this.requestSeq++;
      this.lastTeacherQueryReady = false;
      this.filteredPanel = [];
      this.selectedPanel = {
        teacherID: id,
        fullName: String(r.fullName ?? '').trim(),
      };
      setTimeout(() => {
        this.dropdownSelect?.hide(true);
        this.onChange(id);
        this.onTouched();
        queueMicrotask(() => {
          if (this.pendingTeacherSelectEcho === id) {
            this.pendingTeacherSelectEcho = null;
          }
        });
      }, 0);
      return;
    }

    const r = row as StudentNameIdDTO;
    if (!r?.studentID) {
      return;
    }
    this.onChange(r);
    this.onTouched();
    this.selectionChange.emit(r);
  }
}
