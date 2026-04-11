import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { forkJoin } from 'rxjs';

import { HomeworkService } from '../../../core/services/homework.service';
import {
  CreateHomeworkTask,
  HomeworkActivitySummary,
  HomeworkSubmissionRow,
  HomeworkTaskDetail,
  HomeworkTaskList,
  homeworkStatusLabelAr,
} from '../../../core/models/homework.model';
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
  selector: 'app-homework-admin',
  templateUrl: './homework-admin.component.html',
  styleUrls: ['./homework-admin.component.scss'],
})
export class HomeworkAdminComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly homework = inject(HomeworkService);
  private readonly years = inject(YearService);
  private readonly terms = inject(TermService);
  private readonly classes = inject(ClassService);
  private readonly divisions = inject(DivisionService);
  private readonly subjects = inject(SubjectService);
  private readonly teachers = inject(TeacherService);
  private readonly toastr = inject(ToastrService);

  yearOptions: { label: string; yearID: number }[] = [];
  yearList: Year[] = [];
  termList: Terms[] = [];
  classList: { classID: number; className: string }[] = [];
  allDivisions: divisions[] = [];
  divisionList: divisions[] = [];
  subjectList: Subjects[] = [];
  teacherList: Teachers[] = [];
  /** p-select label/value for teachers filter & create */
  teacherOptions: { label: string; teacherID: number }[] = [];

  tasks: HomeworkTaskList[] = [];
  summary: HomeworkActivitySummary | null = null;
  loading = false;
  loadingSummary = false;

  showCreate = false;
  showSubmissions = false;
  selectedTask: HomeworkTaskList | null = null;
  taskDetail: HomeworkTaskDetail | null = null;
  submissions: HomeworkSubmissionRow[] = [];
  loadingSubmissions = false;

  statusLabel = homeworkStatusLabelAr;

  filterForm = this.fb.group({
    yearID: [null as number | null],
    termID: [null as number | null],
    classID: [null as number | null],
    teacherID: [null as number | null],
  });

  createForm = this.fb.group({
    teacherID: [null as number | null, Validators.required],
    termID: [null as number | null, Validators.required],
    classID: [null as number | null, Validators.required],
    divisionID: [null as number | null, Validators.required],
    subjectID: [null as number | null, Validators.required],
    title: ['', Validators.required],
    description: [''],
    dueDateUtc: [new Date().toISOString().slice(0, 16), Validators.required],
    submissionRequired: [true],
    linkUrl: [''],
    linkLabel: [''],
  });

  ngOnInit(): void {
    this.loadLookups();
    this.createForm.get('classID')?.valueChanges.subscribe((classID) => {
      this.divisionList = classID ? this.allDivisions.filter((d) => d.classID === classID) : [];
      this.createForm.patchValue({ divisionID: null }, { emitEvent: false });
    });
  }

  loadLookups(): void {
    forkJoin({
      years: this.years.getAllYears(),
      terms: this.terms.getAllTerm(),
      classes: this.classes.GetAllNames(),
      divisions: this.divisions.GetAll(),
      subjects: this.subjects.getAllSubjects(),
      teachers: this.teachers.getAllTeacher(),
    }).subscribe({
      next: (res) => {
        this.yearList = res.years ?? [];
        this.yearOptions = this.yearList.map((y) => ({
          label: String(new Date(y.yearDateStart).getFullYear()),
          yearID: y.yearID,
        }));
        this.termList = res.terms?.result ?? [];
        this.classList = res.classes?.result ?? [];
        this.allDivisions = res.divisions?.result ?? [];
        this.subjectList = res.subjects?.result ?? [];
        this.teacherList = res.teachers?.result ?? [];
        this.teacherOptions = this.teacherList.map((t) => ({
          teacherID: t.teacherID,
          label: [t.firstName, t.middleName, t.lastName].filter(Boolean).join(' ').trim() || String(t.teacherID),
        }));
      },
      error: () => this.toastr.error('تعذر تحميل القوائم'),
    });
  }

  load(): void {
    const f = this.filterForm.getRawValue();
    if (!f.yearID || !f.termID) {
      this.tasks = [];
      this.summary = null;
      return;
    }
    this.loading = true;
    this.loadingSummary = true;
    this.homework.getTeacherTasks({
      yearID: f.yearID,
      termID: f.termID,
      classID: f.classID ?? undefined,
      teacherID: f.teacherID ?? undefined,
    }).subscribe({
      next: (r) => {
        this.tasks = (r.result ?? []) as HomeworkTaskList[];
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.toastr.error('تعذر تحميل الواجبات');
      },
    });
    this.homework.getActivityReport(f.yearID, f.termID, f.classID ?? undefined, f.teacherID ?? undefined).subscribe({
      next: (r) => {
        this.summary = (r.result ?? null) as HomeworkActivitySummary | null;
        this.loadingSummary = false;
      },
      error: () => {
        this.loadingSummary = false;
      },
    });
  }

  openCreate(): void {
    const ft = this.filterForm.value.termID;
    this.createForm.reset({
      teacherID: null,
      termID: ft ?? null,
      classID: null,
      divisionID: null,
      subjectID: null,
      title: '',
      description: '',
      dueDateUtc: new Date().toISOString().slice(0, 16),
      submissionRequired: true,
      linkUrl: '',
      linkLabel: '',
    });
    this.divisionList = [];
    this.showCreate = true;
  }

  saveCreate(): void {
    if (this.createForm.invalid) {
      this.createForm.markAllAsTouched();
      return;
    }
    const v = this.createForm.getRawValue();
    const links =
      v.linkUrl?.trim()
        ? [{ url: v.linkUrl.trim(), label: v.linkLabel?.trim() || null, sortOrder: 0 }]
        : [];
    const body: CreateHomeworkTask = {
      teacherID: v.teacherID!,
      termID: v.termID!,
      classID: v.classID!,
      divisionID: v.divisionID!,
      subjectID: v.subjectID!,
      title: v.title!.trim(),
      description: v.description?.trim() || null,
      dueDateUtc: new Date(v.dueDateUtc!).toISOString(),
      submissionRequired: v.submissionRequired ?? false,
      links,
    };
    this.homework.createTask(body).subscribe({
      next: () => {
        this.toastr.success('تم إنشاء الواجب');
        this.showCreate = false;
        this.load();
      },
      error: (e) => this.toastr.error(e?.error?.errorMasseges?.[0] ?? 'تعذر الحفظ'),
    });
  }

  deleteRow(row: HomeworkTaskList): void {
    if (!confirm('حذف هذا الواجب؟')) return;
    this.homework.deleteTask(row.homeworkTaskID).subscribe({
      next: () => {
        this.toastr.success('تم الحذف');
        this.load();
        if (this.selectedTask?.homeworkTaskID === row.homeworkTaskID) {
          this.showSubmissions = false;
          this.selectedTask = null;
        }
      },
      error: () => this.toastr.error('تعذر الحذف'),
    });
  }

  openSubmissions(row: HomeworkTaskList): void {
    this.selectedTask = row;
    this.taskDetail = null;
    this.showSubmissions = true;
    this.loadingSubmissions = true;
    this.homework.getTeacherTask(row.homeworkTaskID).subscribe({
      next: (r) => {
        this.taskDetail = (r.result ?? null) as HomeworkTaskDetail | null;
      },
      error: () => {},
    });
    this.homework.getTaskSubmissions(row.homeworkTaskID).subscribe({
      next: (r) => {
        this.submissions = (r.result ?? []) as HomeworkSubmissionRow[];
        this.loadingSubmissions = false;
      },
      error: () => {
        this.loadingSubmissions = false;
        this.toastr.error('تعذر تحميل التسليمات');
      },
    });
  }

  saveReview(sub: HomeworkSubmissionRow): void {
    this.homework
      .reviewSubmission(sub.homeworkSubmissionID, {
        status: sub.status,
        teacherFeedback: sub.teacherFeedback ?? null,
        score: sub.score ?? null,
        feedbackPublished: sub.feedbackPublished,
      })
      .subscribe({
        next: () => this.toastr.success('تم الحفظ'),
        error: (e) => this.toastr.error(e?.error?.errorMasseges?.[0] ?? 'تعذر الحفظ'),
      });
  }
}
