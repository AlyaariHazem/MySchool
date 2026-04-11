import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { forkJoin } from 'rxjs';

import { ExamsService } from '../../../core/services/exams.service';
import { CreateScheduledExam, ExamType, ScheduledExamList } from '../../../core/models/exams.model';
import { YearService } from '../../../core/services/year.service';
import { TermService } from '../core/services/term.service';
import { ClassService } from '../core/services/class.service';
import { DivisionService } from '../core/services/division.service';
import { SubjectService } from '../core/services/subject.service';
import { TeacherService } from '../core/services/teacher.service';
import { Terms } from '../core/models/term.model';
import { Subjects } from '../core/models/subjects.model';
import { divisions } from '../core/models/division.model';
import { Teachers } from '../core/models/teacher.model';
import { Year } from '../../../core/models/year.model';

@Component({
  selector: 'app-exams-admin',
  templateUrl: './exams-admin.component.html',
  styleUrls: ['./exams-admin.component.scss'],
})
export class ExamsAdminComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly exams = inject(ExamsService);
  private readonly years = inject(YearService);
  private readonly terms = inject(TermService);
  private readonly classes = inject(ClassService);
  private readonly divisions = inject(DivisionService);
  private readonly subjects = inject(SubjectService);
  private readonly teachers = inject(TeacherService);
  private readonly toastr = inject(ToastrService);

  yearList: Year[] = [];
  termList: Terms[] = [];
  classList: { classID: number; className: string }[] = [];
  allDivisions: divisions[] = [];
  divisionList: divisions[] = [];
  subjectList: Subjects[] = [];
  teacherList: Teachers[] = [];
  examTypes: { examTypeID: number; name: string }[] = [];

  scheduled: ScheduledExamList[] = [];
  loading = false;
  showDialog = false;

  filterForm = this.fb.group({
    yearID: [null as number | null],
    termID: [null as number | null],
  });

  form = this.fb.group({
    examTypeID: [1, Validators.required],
    yearID: [null as number | null, Validators.required],
    termID: [null as number | null, Validators.required],
    classID: [null as number | null, Validators.required],
    divisionID: [null as number | null, Validators.required],
    subjectID: [null as number | null, Validators.required],
    teacherID: [null as number | null, Validators.required],
    examDate: [new Date().toISOString().slice(0, 10), Validators.required],
    startTime: ['08:00', Validators.required],
    endTime: ['09:00', Validators.required],
    room: [''],
    totalMarks: [100, [Validators.required, Validators.min(1)]],
    passingMarks: [50, [Validators.required, Validators.min(0)]],
    schedulePublished: [false],
    resultsPublished: [false],
    notes: [''],
  });

  ngOnInit(): void {
    this.loadLookups();
  }

  loadLookups(): void {
    forkJoin({
      years: this.years.getAllYears(),
      terms: this.terms.getAllTerm(),
      classes: this.classes.GetAllNames(),
      divisions: this.divisions.GetAll(),
      subjects: this.subjects.getAllSubjects(),
      teachers: this.teachers.getAllTeacher(),
      types: this.exams.getExamTypes(false),
    }).subscribe({
      next: (res) => {
        this.yearList = res.years ?? [];
        this.termList = res.terms?.result ?? [];
        this.classList = res.classes?.result ?? [];
        this.allDivisions = res.divisions?.result ?? [];
        this.subjectList = res.subjects?.result ?? [];
        this.teacherList = res.teachers?.result ?? [];
        this.examTypes = (res.types?.result ?? []).map((t: ExamType) => ({ examTypeID: t.examTypeID, name: t.name }));
        const y = this.yearList.find((x) => x.active);
        if (y) this.filterForm.patchValue({ yearID: y.yearID });
        if (this.termList.length) this.filterForm.patchValue({ termID: this.termList[0].termID });
        this.loadScheduled();
      },
      error: () => this.toastr.error('تعذر تحميل الإعدادات'),
    });
  }

  onClassChange(): void {
    const cid = this.form.get('classID')?.value;
    if (!cid) {
      this.divisionList = [];
      return;
    }
    this.divisionList = this.allDivisions
      .filter((d) => d.classID === cid)
      .sort((a, b) => (a.divisionName || '').localeCompare(b.divisionName || ''));
  }

  loadScheduled(): void {
    const { yearID, termID } = this.filterForm.getRawValue();
    this.loading = true;
    this.exams
      .getScheduled({
        yearID: yearID ?? undefined,
        termID: termID ?? undefined,
      })
      .subscribe({
        next: (r) => {
          this.scheduled = (r.result ?? []) as ScheduledExamList[];
          this.loading = false;
        },
        error: () => {
          this.loading = false;
          this.toastr.error('تعذر تحميل الامتحانات');
        },
      });
  }

  openCreate(): void {
    const y = this.filterForm.get('yearID')?.value;
    const t = this.filterForm.get('termID')?.value;
    this.form.patchValue({
      yearID: y ?? null,
      termID: t ?? null,
      classID: null,
      divisionID: null,
      subjectID: null,
      teacherID: null,
    });
    this.divisionList = [];
    this.showDialog = true;
  }

  saveCreate(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const v = this.form.getRawValue();
    const body: CreateScheduledExam = {
      examSessionID: null,
      examTypeID: v.examTypeID!,
      yearID: v.yearID!,
      termID: v.termID!,
      classID: v.classID!,
      divisionID: v.divisionID!,
      subjectID: v.subjectID!,
      teacherID: v.teacherID!,
      examDate: v.examDate!,
      startTime: v.startTime!,
      endTime: v.endTime!,
      room: v.room || null,
      totalMarks: Number(v.totalMarks),
      passingMarks: Number(v.passingMarks),
      schedulePublished: !!v.schedulePublished,
      resultsPublished: !!v.resultsPublished,
      notes: v.notes || null,
    };
    this.exams.createScheduled(body).subscribe({
      next: () => {
        this.toastr.success('تم إنشاء الامتحان');
        this.showDialog = false;
        this.loadScheduled();
      },
      error: (e) => this.toastr.error(e?.error?.errorMasseges?.[0] ?? 'تعذر الحفظ'),
    });
  }

  deleteRow(row: ScheduledExamList): void {
    if (!confirm('حذف هذا الامتحان؟')) return;
    this.exams.deleteScheduled(row.scheduledExamID).subscribe({
      next: () => {
        this.toastr.success('تم الحذف');
        this.loadScheduled();
      },
      error: () => this.toastr.error('تعذر الحذف'),
    });
  }

  publishSchedule(row: ScheduledExamList, publish: boolean): void {
    this.exams.publishSchedule(row.scheduledExamID, publish).subscribe({
      next: () => {
        this.toastr.success(publish ? 'تم نشر الجدول' : 'تم إخفاء الجدول');
        this.loadScheduled();
      },
      error: () => this.toastr.error('تعذر التحديث'),
    });
  }
}
