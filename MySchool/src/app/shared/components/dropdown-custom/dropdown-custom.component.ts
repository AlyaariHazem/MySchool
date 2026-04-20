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
import { ClassNameLookupRow, ClassService } from '../../../components/school/core/services/class.service';
import { TeacherNameLookupRow, TeacherService } from '../../../components/school/core/services/teacher.service';

/**
 * Searchable lazy dropdown:
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
export class DropdownCustomComponent implements ControlValueAccessor {
  @ViewChild('dropdownAutocomplete')
  dropdownAutocomplete?: AutoComplete;

  @Input() inputId = 'dropdownCustomSearch';
  @Input() placeholder = '';
  @Input() disabled = false;
  /** student: POST Students/names-ids; class: POST Classes/GetAllNameClasses/page; teacher: POST Teacher/names/page */
  @Input() resource: 'student' | 'class' | 'teacher' = 'student';
  @Input() pageSize = 5;
  @Input() searchDelayMs = 400;
  @Input() scrollHeight = '240px';
  @Input() virtualScrollItemSize = 48;
  @Input() inputStyleClass = 'dropdown-custom__input p-inputtext p-component w-full';

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
  private skipKeyUpAfterSelect = false;

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

  onCompleteSearch(event: AutoCompleteCompleteEvent): void {
    const raw = (event.query || '').trim();
    if (this.resource === 'class') {
      this.loadClassSuggestionsFromQuery(raw, false);
    } else if (this.resource === 'teacher') {
      this.loadTeacherSuggestionsFromQuery(raw, false);
    } else {
      this.loadStudentSuggestionsFromQuery(raw, false);
    }
  }

  onInputFocus(_event: Event): void {
    if (this.disabled) {
      return;
    }
    if (this.resource === 'class') {
      this.loadClassSuggestionsFromQuery('', true);
    } else if (this.resource === 'teacher') {
      this.loadTeacherSuggestionsFromQuery('', true);
    } else {
      this.loadStudentSuggestionsFromQuery('', true);
    }
  }

  private loadStudentSuggestionsFromQuery(raw: string, showPanelAfterLoad: boolean): void {
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
        if (showPanelAfterLoad) {
          setTimeout(() => this.dropdownAutocomplete?.show(), 0);
        }
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

  private loadClassSuggestionsFromQuery(raw: string, showPanelAfterLoad: boolean): void {
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
        if (showPanelAfterLoad) {
          setTimeout(() => this.dropdownAutocomplete?.show(), 0);
        }
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

  private loadTeacherSuggestionsFromQuery(raw: string, showPanelAfterLoad: boolean): void {
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
        if (showPanelAfterLoad) {
          setTimeout(() => this.dropdownAutocomplete?.show(), 0);
        }
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

  onLazyLoad(event: AutoCompleteLazyLoadEvent): void {
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
    event: AutoCompleteLazyLoadEvent,
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
    this.filteredPanel = [];
    this.lastStudentQuery = null;
    this.lastClassSearchNormalized = undefined;
    this.lastClassQueryReady = false;
    this.lastTeacherSearchNormalized = undefined;
    this.lastTeacherQueryReady = false;
    this.loadedPage = 0;
    this.totalPages = 0;
    this.loadingMore = false;
    this.noScrollAppendDone = false;
    this.requestSeq++;
    this.selectedPanel = null;
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
      if (this.resource === 'class') {
        this.loadClassSuggestionsFromQuery('', false);
      } else if (this.resource === 'teacher') {
        this.loadTeacherSuggestionsFromQuery('', false);
      } else {
        this.filteredPanel = [];
      }
      return;
    }
    ac.search(event, raw, 'input');
  }

  onRowSelect(event: AutoCompleteSelectEvent): void {
    if (this.resource === 'class') {
      const row = event.value as ClassNameLookupRow;
      if (!row?.classID) {
        return;
      }
      this.skipKeyUpAfterSelect = true;
      this.onChange(row.classID);
      this.onTouched();
      return;
    }

    if (this.resource === 'teacher') {
      const row = event.value as TeacherNameLookupRow;
      if (!row?.teacherID) {
        return;
      }
      this.skipKeyUpAfterSelect = true;
      this.onChange(row.teacherID);
      this.onTouched();
      return;
    }

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
