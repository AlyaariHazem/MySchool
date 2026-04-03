import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { TranslateService } from '@ngx-translate/core';
import { PaginatorState } from 'primeng/paginator';
import { Store } from '@ngrx/store';
import { forkJoin, map, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { selectLanguage } from '../../../core/store/language/language.selectors';
import { AttendanceService } from '../core/services/attendance.service';
import { ClassService } from '../core/services/class.service';
import { DivisionService } from '../core/services/division.service';
import { StudentService } from '../../../core/services/student.service';
import { CLass } from '../core/models/class.model';
import { divisions } from '../core/models/division.model';
import { StudentDetailsDTO } from '../../../core/models/students.model';
import { ApiResponse } from '../../../core/models/response.model';
import { AttendanceDto, AttendanceStatus, BulkAttendanceRequest } from '../core/models/attendance.model';

interface AttendanceRow {
  studentID: number;
  studentName: string;
  status: AttendanceStatus;
  remarks: string;
}

@Component({
  selector: 'app-attendance',
  templateUrl: './attendance.component.html',
  styleUrls: ['./attendance.component.scss']
})
export class AttendanceComponent implements OnInit {
  private fb = inject(FormBuilder);
  private toastr = inject(ToastrService);
  private store = inject(Store);
  private attendanceService = inject(AttendanceService);
  private classService = inject(ClassService);
  private divisionService = inject(DivisionService);
  private studentService = inject(StudentService);
  private translate = inject(TranslateService);

  filterForm: FormGroup;
  classes: CLass[] = [];
  allDivisions: divisions[] = [];
  filteredDivisions: divisions[] = [];
  rows: AttendanceRow[] = [];
  isLoading = false;
  saving = false;

  /** Server-side paging for POST /Students/page (backend max pageSize 100). */
  pageSize = 10;
  first = 0;
  totalRecords = 0;

  /** Attendance rows for the selected class+date; merged when changing table pages without re-fetching. */
  private attendanceByStudent = new Map<number, AttendanceDto>();

  statusOptions: { label: string; value: AttendanceStatus }[] = [
    { label: 'حاضر', value: AttendanceStatus.Present },
    { label: 'غائب', value: AttendanceStatus.Absent },
    { label: 'متأخر', value: AttendanceStatus.Late },
    { label: 'معذور', value: AttendanceStatus.Excused }
  ];

  readonly dir$ = this.store.select(selectLanguage).pipe(
    map(l => (l === 'ar' ? 'rtl' : 'ltr'))
  );

  constructor() {
    this.filterForm = this.fb.group({
      classId: [null as number | null],
      divisionId: [null as number | null],
      date: [new Date()]
    });
  }

  ngOnInit(): void {
    this.classService.GetAllNames().subscribe({
      next: res => {
        if (res.result) this.classes = res.result;
      },
      error: () => this.toastr.error('تعذر تحميل الصفوف', 'خطأ')
    });

    this.divisionService.GetAll().subscribe({
      next: res => {
        if (res.result) {
          this.allDivisions = res.result;
          this.updateDivisionsByClass(this.filterForm.get('classId')?.value ?? null);
        }
      },
      error: () => {}
    });

    this.filterForm.get('classId')?.valueChanges.subscribe(classId => {
      this.updateDivisionsByClass(classId);
      this.rows = [];
      this.totalRecords = 0;
      this.first = 0;
    });
  }

  private updateDivisionsByClass(classId: number | null): void {
    if (classId == null) {
      this.filteredDivisions = [];
      this.filterForm.patchValue({ divisionId: null }, { emitEvent: false });
      return;
    }
    const cid = Number(classId);
    this.filteredDivisions = this.allDivisions.filter(d => Number(d.classID) === cid);
    this.filterForm.patchValue({ divisionId: null }, { emitEvent: false });
  }

  private formatStudentName(s: StudentDetailsDTO): string {
    const n = s.fullName;
    if (!n) return '';
    return `${n.firstName} ${n.middleName ?? ''} ${n.lastName}`.replace(/\s+/g, ' ').trim();
  }

  private toDateOnlyString(d: Date): string {
    const y = d.getFullYear();
    const m = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${y}-${m}-${day}`;
  }

  private divisionFilterValue(): number | null {
    const divisionId = this.filterForm.get('divisionId')?.value as number | null;
    if (divisionId == null || divisionId === ('' as unknown)) return null;
    const n = Number(divisionId);
    return Number.isNaN(n) ? null : n;
  }

  private fillAttendanceMap(res: ApiResponse<AttendanceDto[]>): void {
    this.attendanceByStudent.clear();
    const list = res.result as AttendanceDto[] | null | undefined;
    if (!Array.isArray(list)) return;
    for (const a of list) {
      const raw = a as AttendanceDto & { StudentID?: number };
      const sid = raw.studentID ?? raw.StudentID;
      if (sid != null) this.attendanceByStudent.set(Number(sid), a);
    }
  }

  private studentsToRows(students: StudentDetailsDTO[]): AttendanceRow[] {
    return students.map(s => {
      const existing = this.attendanceByStudent.get(s.studentID);
      const ex = existing as (AttendanceDto & { Status?: number; Remarks?: string }) | undefined;
      const st = ex?.status ?? ex?.Status ?? AttendanceStatus.Present;
      const status = typeof st === 'number' ? st : AttendanceStatus.Present;
      const remarks = ex?.remarks ?? ex?.Remarks ?? '';
      return {
        studentID: s.studentID,
        studentName: this.formatStudentName(s),
        status,
        remarks
      };
    });
  }

  /** Initial load: refresh attendance map + first page of students from POST /Students/page (server filters). */
  loadRoster(): void {
    const classId = this.filterForm.get('classId')?.value as number | null;
    const dateVal = this.filterForm.get('date')?.value as Date | null;

    if (classId == null || !dateVal) {
      this.toastr.warning('اختر الصف والتاريخ', 'تنبيه');
      return;
    }

    const classIdNum = Number(classId);
    const dateStr = this.toDateOnlyString(dateVal);
    const divisionId = this.divisionFilterValue();

    this.isLoading = true;
    this.rows = [];
    this.first = 0;

    forkJoin({
      attendance: this.attendanceService.getByClassAndDate(classIdNum, dateStr).pipe(
        catchError(() =>
          of({
            result: [] as AttendanceDto[],
            isSuccess: true,
            statusCode: 200,
            errorMasseges: []
          } as ApiResponse<AttendanceDto[]>)
        )
      ),
      studentsPage: this.studentService
        .getStudentsPageForAttendance(1, this.pageSize, { classId: classIdNum, divisionId })
        .pipe(
          catchError(() =>
            of({
              data: [] as StudentDetailsDTO[],
              totalCount: 0,
              totalPages: 0,
              pageNumber: 1,
              pageSize: this.pageSize
            })
          )
        )
    }).subscribe({
      next: ({ attendance, studentsPage }) => {
        this.fillAttendanceMap(attendance);
        this.totalRecords = studentsPage.totalCount;
        this.rows = this.studentsToRows(studentsPage.data);
        this.isLoading = false;
        if (this.totalRecords === 0) {
          this.toastr.info('لا يوجد طلاب مطابقين لهذا الصف والشعبة', 'معلومة');
        }
      },
      error: () => {
        this.isLoading = false;
        this.toastr.error('تعذر تحميل الحضور', 'خطأ');
      }
    });
  }

  /** Other pages: only POST /Students/page; merge with cached attendance for this class/date. */
  private loadStudentsPage(pageNumber: number): void {
    const classId = this.filterForm.get('classId')?.value as number | null;
    const dateVal = this.filterForm.get('date')?.value as Date | null;
    if (classId == null || !dateVal) {
      return;
    }

    const classIdNum = Number(classId);
    const divisionId = this.divisionFilterValue();

    this.isLoading = true;
    this.studentService
      .getStudentsPageForAttendance(pageNumber, this.pageSize, { classId: classIdNum, divisionId })
      .pipe(
        catchError(() =>
          of({
            data: [] as StudentDetailsDTO[],
            totalCount: this.totalRecords,
            totalPages: 0,
            pageNumber,
            pageSize: this.pageSize
          })
        )
      )
      .subscribe({
        next: p => {
          this.totalRecords = p.totalCount;
          this.rows = this.studentsToRows(p.data);
          this.isLoading = false;
        },
        error: () => {
          this.isLoading = false;
          this.toastr.error('تعذر تحميل الصفحة', 'خطأ');
        }
      });
  }

  onStudentsPageChange(event: PaginatorState): void {
    const first = event.first ?? 0;
    const rows = event.rows ?? this.pageSize;
    this.first = first;
    this.pageSize = rows;
    const pageNumber =
      event.page != null ? event.page + 1 : Math.floor(first / rows) + 1;
    this.loadStudentsPage(pageNumber);
  }

  saveBulk(): void {
    const classId = this.filterForm.get('classId')?.value as number | null;
    const dateVal = this.filterForm.get('date')?.value as Date | null;
    if (classId == null || !dateVal || this.rows.length === 0) {
      this.toastr.warning('حمّل القائمة أولاً', 'تنبيه');
      return;
    }

    const body: BulkAttendanceRequest = {
      classID: Number(classId),
      date: this.toDateOnlyString(dateVal),
      entries: this.rows.map(r => ({
        studentID: r.studentID,
        status: r.status,
        remarks: r.remarks?.trim() ? r.remarks.trim() : null
      }))
    };

    this.saving = true;
    this.attendanceService.bulkUpsert(body).subscribe({
      next: res => {
        this.saving = false;
        if (res.isSuccess !== false) {
          this.toastr.success(
            this.translate.instant('attendance.saveSuccessPage'),
            this.translate.instant('attendance.saveSuccessTitle')
          );
          this.loadRoster();
        } else {
          const msg = res.errorMasseges?.[0] ?? 'فشل الحفظ';
          this.toastr.error(msg, 'خطأ');
        }
      },
      error: err => {
        this.saving = false;
        const msg = err?.error?.errorMasseges?.[0] ?? err?.message ?? 'فشل الحفظ';
        this.toastr.error(msg, 'خطأ');
      }
    });
  }
}
