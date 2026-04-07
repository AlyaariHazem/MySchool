import {
  Component,
  EventEmitter,
  Output,
  Input,
  ViewChild,
  forwardRef,
  inject
} from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { AutoComplete } from 'primeng/autocomplete';
import {
  AutoCompleteCompleteEvent,
  AutoCompleteLazyLoadEvent,
  AutoCompleteSelectEvent
} from 'primeng/autocomplete';
import { ToastrService } from 'ngx-toastr';
import { StudentService } from '../../../core/services/student.service';
import { StudentNameIdDTO, StudentNameIdSearchRequest } from '../../../core/models/students.model';

/**
 * Reusable searchable dropdown for students (POST Students/names-ids): paging, lazy scroll, focus load.
 * Uses ControlValueAccessor — bind with [(ngModel)] or formControlName.
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
export class DropdownCustomComponent implements ControlValueAccessor {
  @ViewChild('dropdownAutocomplete')
  dropdownAutocomplete?: AutoComplete;

  @Input() inputId = 'dropdownCustomSearch';
  @Input() placeholder = '';
  @Input() disabled = false;
  /** Page size for POST Students/names-ids */
  @Input() pageSize = 5;
  @Input() searchDelayMs = 400;
  @Input() scrollHeight = '240px';
  @Input() virtualScrollItemSize = 48;
  @Input() inputStyleClass = 'dropdown-custom__input p-inputtext p-component w-full';

  /** Emitted when the user picks a row (not on programmatic writes). */
  @Output() selectionChange = new EventEmitter<StudentNameIdDTO>();

  private studentService = inject(StudentService);
  private toastr = inject(ToastrService);

  filteredStudents: StudentNameIdDTO[] = [];
  selectedStudent: StudentNameIdDTO | null = null;

  private lastQuery: Pick<StudentNameIdSearchRequest, 'studentID' | 'fullName'> | null = null;
  private loadedPage = 0;
  private totalPages = 0;
  private loadingMore = false;
  private requestSeq = 0;
  private noScrollAppendDone = false;
  private skipKeyUpAfterSelect = false;

  private onChange: (v: StudentNameIdDTO | null) => void = () => {};
  private onTouched: () => void = () => {};

  writeValue(obj: StudentNameIdDTO | null): void {
    this.selectedStudent = obj ?? null;
  }

  registerOnChange(fn: (v: StudentNameIdDTO | null) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }

  onStudentSearch(event: AutoCompleteCompleteEvent): void {
    const raw = (event.query || '').trim();
    this.loadSuggestionsFromQuery(raw, false);
  }

  onInputFocus(_event: Event): void {
    if (this.disabled) {
      return;
    }
    this.loadSuggestionsFromQuery('', true);
  }

  private loadSuggestionsFromQuery(raw: string, showPanelAfterLoad: boolean): void {
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
    this.lastQuery = null;
    this.noScrollAppendDone = false;

    this.studentService.searchStudentNamesAndIds(req).subscribe({
      next: (page) => {
        if (seq !== this.requestSeq) {
          return;
        }
        this.filteredStudents = page.data;
        this.loadedPage = page.pageNumber;
        this.totalPages = Math.max(1, page.totalPages);
        this.lastQuery =
          req.studentID != null && req.studentID > 0
            ? { studentID: req.studentID }
            : req.fullName != null && String(req.fullName).trim() !== ''
              ? { fullName: String(req.fullName).trim() }
              : {};
        if (showPanelAfterLoad) {
          setTimeout(() => this.dropdownAutocomplete?.show(), 0);
        }
      },
      error: () => {
        if (seq !== this.requestSeq) {
          return;
        }
        this.filteredStudents = [];
        this.lastQuery = null;
        this.toastr.error('تعذّر البحث عن الطلاب', 'خطأ');
      }
    });
  }

  onLazyLoad(event: AutoCompleteLazyLoadEvent): void {
    const len = this.filteredStudents.length;
    if (len === 0 || this.loadingMore || this.lastQuery == null) {
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

    this.loadNextPage();
  }

  private loadNextPage(): void {
    if (
      this.lastQuery == null ||
      this.loadingMore ||
      this.loadedPage >= this.totalPages
    ) {
      return;
    }

    const seq = this.requestSeq;
    const nextPage = this.loadedPage + 1;
    this.loadingMore = true;

    const req: StudentNameIdSearchRequest = {
      ...this.lastQuery,
      pageNumber: nextPage,
      pageSize: this.pageSize
    };

    this.studentService.searchStudentNamesAndIds(req).subscribe({
      next: (page) => {
        this.loadingMore = false;
        if (seq !== this.requestSeq) {
          return;
        }
        const existing = new Set(this.filteredStudents.map((s) => s.studentID));
        const merged = [...this.filteredStudents];
        for (const row of page.data) {
          if (!existing.has(row.studentID)) {
            merged.push(row);
            existing.add(row.studentID);
          }
        }
        this.filteredStudents = merged;
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

  onStudentClear(): void {
    this.filteredStudents = [];
    this.lastQuery = null;
    this.loadedPage = 0;
    this.totalPages = 0;
    this.loadingMore = false;
    this.noScrollAppendDone = false;
    this.requestSeq++;
    this.selectedStudent = null;
    this.onChange(null);
    this.onTouched();
  }

  onSearchKeyUp(event: KeyboardEvent): void {
    if (event.key !== 'Enter' && event.key !== 'NumpadEnter') {
      return;
    }
    if (this.skipKeyUpAfterSelect) {
      this.skipKeyUpAfterSelect = false;
      return;
    }
    const ac = this.dropdownAutocomplete;
    if (!ac) {
      return;
    }
    const raw = ((event.target as HTMLInputElement)?.value ?? '').trim();
    if (!raw.length) {
      this.filteredStudents = [];
      return;
    }
    ac.search(event, raw, 'input');
  }

  onStudentSelect(event: AutoCompleteSelectEvent): void {
    const row = event.value as StudentNameIdDTO;
    if (!row?.studentID) {
      return;
    }
    this.skipKeyUpAfterSelect = true;
    this.onChange(row);
    this.onTouched();
    this.selectionChange.emit(row);
  }
}
