import { NgFor, NgIf } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { FloatLabelModule } from 'primeng/floatlabel';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { Select } from 'primeng/select';
import { TextareaModule } from 'primeng/textarea';
import { ToastrService } from 'ngx-toastr';
import { of } from 'rxjs';
import { catchError, finalize, map } from 'rxjs/operators';
import { HttpClient } from '@angular/common/http';

import { isSchoolManagerUser } from 'app/core/utils/school-role.util';
import { PagePermission, PermissionService } from 'app/core/services/permission.service';
import { SchoolService } from 'app/core/services/school.service';
import { YearService } from 'app/core/services/year.service';
import { School } from 'app/core/models/school.modul';
import { Year } from 'app/core/models/year.model';
import { ShardModule } from 'app/shared/shard.module';
import { BackendAspService } from 'app/ASP.NET/backend-asp.service';
import { ApiResponse } from 'app/core/models/response.model';
import { PagedResultDto } from 'app/core/models/students.model';

import { EmployeeProfileListFilterDto, EmployeeProfileOptionDto } from '../../employees-hr/employees-hr.models';
import { EmployeesHrService } from '../../employees-hr/employees-hr.service';
import {
  RecommendationImplementationStatus,
  RecommendationFollowUpWriteDto,
  SupervisorVisitStatus,
  SupervisorVisitWriteDto,
  VisitObservationWriteDto,
  VisitRecommendationWriteDto,
} from '../supervisor-visits.models';
import { SupervisorVisitsService, readSupervisorVisitHttpError } from '../supervisor-visits.service';

interface ClassOption {
  classID: number;
  className: string;
}

interface SubjectOption {
  subjectID: number;
  subjectName: string;
}

@Component({
  selector: 'app-supervisor-visits-form',
  standalone: true,
  imports: [
    ShardModule,
    NgIf,
    NgFor,
    FormsModule,
    TranslateModule,
    RouterLink,
    ButtonModule,
    Select,
    FloatLabelModule,
    InputTextModule,
    TextareaModule,
    InputNumberModule,
    ProgressSpinnerModule,
  ],
  templateUrl: './supervisor-visits-form.component.html',
  styleUrl: './supervisor-visits-form.component.scss',
})
export class SupervisorVisitsFormComponent implements OnInit {
  private readonly svc = inject(SupervisorVisitsService);
  private readonly employeesHr = inject(EmployeesHrService);
  private readonly schoolService = inject(SchoolService);
  private readonly yearService = inject(YearService);
  private readonly http = inject(HttpClient);
  private readonly api = inject(BackendAspService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly toastr = inject(ToastrService);
  private readonly translate = inject(TranslateService);
  private readonly perm = inject(PermissionService);

  readonly filterSelectPanelStyle: Record<string, string> = {
    maxWidth: 'min(22rem, calc(100vw - 2rem))',
  };

  loading = false;
  saving = false;
  visitId: number | null = null;

  schoolID: number | null = null;
  academicYearID: number | null = null;
  visitedTeacherID: number | null = null;
  classID: number | null = null;
  subjectID: number | null = null;
  supervisorEmployeeProfileID: number | null = null;
  visitDate = '';
  status: SupervisorVisitStatus = SupervisorVisitStatus.Draft;
  overallScoreOutOf100 = 0;
  summaryNotes = '';

  observations: VisitObservationWriteDto[] = [];
  recommendations: VisitRecommendationWriteDto[] = [];

  schoolOptions: { label: string; value: number }[] = [];
  yearOptions: { label: string; value: number }[] = [];
  teacherOptions: { label: string; value: number }[] = [];
  classOptions: { label: string; value: number }[] = [];
  subjectOptions: { label: string; value: number }[] = [];
  supervisorOptions: { label: string; value: number }[] = [];

  statusOptions: { label: string; value: number }[] = [];
  implStatusOptions: { label: string; value: number }[] = [];

  private allYears: Year[] = [];

  get isSchoolManager(): boolean {
    return isSchoolManagerUser();
  }

  get isEdit(): boolean {
    return this.visitId != null && this.visitId > 0;
  }

  get canSubmit(): boolean {
    return this.isEdit
      ? this.perm.hasPermission(PagePermission.Employees.Update)
      : this.perm.hasPermission(PagePermission.Employees.Create);
  }

  ngOnInit(): void {
    this.visitId = Number(this.route.snapshot.paramMap.get('id')) || null;
    if (this.visitId && !this.perm.hasPermission(PagePermission.Employees.Update)) {
      this.router.navigate(['/school/supervisor-visits']).catch(() => undefined);
      return;
    }
    if (!this.visitId && !this.perm.hasPermission(PagePermission.Employees.Create)) {
      this.router.navigate(['/school/supervisor-visits']).catch(() => undefined);
      return;
    }

    this.statusOptions = [
      { label: this.translate.instant('supervisorVisits.status.draft'), value: SupervisorVisitStatus.Draft },
      { label: this.translate.instant('supervisorVisits.status.submitted'), value: SupervisorVisitStatus.Submitted },
      { label: this.translate.instant('supervisorVisits.status.archived'), value: SupervisorVisitStatus.Archived },
    ];
    this.implStatusOptions = [
      { label: this.translate.instant('supervisorVisits.impl.pending'), value: RecommendationImplementationStatus.Pending },
      { label: this.translate.instant('supervisorVisits.impl.inProgress'), value: RecommendationImplementationStatus.InProgress },
      { label: this.translate.instant('supervisorVisits.impl.completed'), value: RecommendationImplementationStatus.Completed },
      { label: this.translate.instant('supervisorVisits.impl.deferred'), value: RecommendationImplementationStatus.Deferred },
      { label: this.translate.instant('supervisorVisits.impl.na'), value: RecommendationImplementationStatus.NotApplicable },
    ];

    if (!this.isSchoolManager) {
      this.schoolService.getAllSchools().subscribe({
        next: (schools: School[]) => {
          this.schoolOptions = (schools ?? [])
            .filter((s) => s.schoolID != null && s.schoolID > 0)
            .map((s) => ({
              label: s.schoolName ?? String(s.schoolID),
              value: s.schoolID as number,
            }));
        },
        error: () => undefined,
      });
    }

    this.yearService.getAllYears().subscribe({
      next: (years: Year[]) => {
        this.allYears = years ?? [];
        this.refreshYearOptions();
      },
      error: () => undefined,
    });

    this.loadTeachers();
    this.loadClasses();
    this.loadSubjects();
    this.loadSupervisors();

    if (this.visitId) this.loadVisit();
    else {
      this.observations = [this.emptyObservation()];
      this.recommendations = [this.emptyRecommendation()];
    }
  }

  onSchoolChange(): void {
    this.refreshYearOptions();
    this.loadSupervisors();
  }

  private refreshYearOptions(): void {
    const sid = this.schoolID;
    const list = !sid ? this.allYears : this.allYears.filter((y) => y.schoolID === sid);
    this.yearOptions = list.map((y) => ({
      label: String(y.yearID),
      value: y.yearID,
    }));
  }

  private unwrap<T>(body: ApiResponse<T> | Record<string, unknown>): T {
    const b = body as Record<string, unknown>;
    const ok = (b['isSuccess'] ?? b['IsSuccess']) !== false;
    const errs = (b['errorMasseges'] ?? b['ErrorMasseges']) as string[] | undefined;
    if (!ok && errs?.length) throw new Error(errs.join('; '));
    return (b['result'] ?? b['Result']) as T;
  }

  private loadTeachers(): void {
    this.http
      .post<ApiResponse<PagedResultDto<{ teacherID?: number; TeacherID?: number; fullName?: string; FullName?: string }>>>(
        `${this.api.baseUrl}/Teacher/names/page`,
        { pageIndex: 0, pageSize: 500, search: null },
      )
      .pipe(
        map((r) => {
          const p = this.unwrap<PagedResultDto<Record<string, unknown>>>(r as ApiResponse<PagedResultDto<Record<string, unknown>>>);
          const rows = p?.data ?? [];
          return rows.map((raw) => {
            const o = raw as Record<string, unknown>;
            const id = Number(o['teacherID'] ?? o['TeacherID']);
            const name = String(o['fullName'] ?? o['FullName'] ?? '');
            return { label: name || `#${id}`, value: id };
          });
        }),
        catchError(() => of([] as { label: string; value: number }[])),
      )
      .subscribe((opts) => (this.teacherOptions = opts.filter((x) => x.value > 0)));
  }

  private loadClasses(): void {
    this.http
      .get<ApiResponse<ClassOption[] | Record<string, unknown>[]>>(`${this.api.baseUrl}/classes`)
      .pipe(
        map((r) => {
          const rows = this.unwrap<Record<string, unknown>[]>(r as ApiResponse<Record<string, unknown>[]>);
          return (rows ?? []).map((c) => ({
            label: String(c['className'] ?? c['ClassName'] ?? ''),
            value: Number(c['classID'] ?? c['ClassID']),
          }));
        }),
        catchError(() => of([])),
      )
      .subscribe((o) => (this.classOptions = o.filter((x) => x.value > 0)));
  }

  private loadSubjects(): void {
    this.http
      .get<ApiResponse<SubjectOption[] | Record<string, unknown>[]>>(`${this.api.baseUrl}/subject/AllSubjects`)
      .pipe(
        map((r) => {
          const rows = this.unwrap<Record<string, unknown>[]>(r as ApiResponse<Record<string, unknown>[]>);
          return (rows ?? []).map((s) => ({
            label: String(s['subjectName'] ?? s['SubjectName'] ?? ''),
            value: Number(s['subjectID'] ?? s['SubjectID']),
          }));
        }),
        catchError(() => of([])),
      )
      .subscribe((o) => (this.subjectOptions = o.filter((x) => x.value > 0)));
  }

  private loadSupervisors(): void {
    const filter: EmployeeProfileListFilterDto = {};
    if (this.schoolID) filter.schoolID = this.schoolID;
    this.employeesHr
      .getEmployeesPage({
        pageIndex: 0,
        pageSize: 500,
        filter,
      })
      .pipe(
        map((p) => {
          const rows = p?.data ?? [];
          return rows.map((e: EmployeeProfileOptionDto) => ({
            label: this.formatEmployeeName(e),
            value: e.id,
          }));
        }),
        catchError(() => of([] as { label: string; value: number }[])),
      )
      .subscribe((o) => (this.supervisorOptions = o.filter((x) => x.value > 0)));
  }

  private formatEmployeeName(e: EmployeeProfileOptionDto): string {
    const n = e.fullName;
    if (!n) return `#${e.id}`;
    return [n.firstName, n.middleName, n.lastName].filter(Boolean).join(' ').trim() || `#${e.id}`;
  }

  private emptyObservation(): VisitObservationWriteDto {
    return { category: '', observationText: '', sortOrder: 0 };
  }

  private emptyFollowUp(): RecommendationFollowUpWriteDto {
    return { followUpNote: '', followUpDate: this.visitDate || new Date().toISOString().slice(0, 10), followUpByEmployeeProfileID: null };
  }

  private emptyRecommendation(): VisitRecommendationWriteDto {
    return {
      recommendationText: '',
      implementationStatus: RecommendationImplementationStatus.Pending,
      dueDate: null,
      completedAtUtc: null,
      sortOrder: 0,
      followUps: [],
    };
  }

  addObservation(): void {
    this.observations.push(this.emptyObservation());
  }

  removeObservation(i: number): void {
    this.observations.splice(i, 1);
    if (this.observations.length === 0) this.observations.push(this.emptyObservation());
  }

  addRecommendation(): void {
    this.recommendations.push(this.emptyRecommendation());
  }

  removeRecommendation(i: number): void {
    this.recommendations.splice(i, 1);
    if (this.recommendations.length === 0) this.recommendations.push(this.emptyRecommendation());
  }

  addFollowUp(rec: VisitRecommendationWriteDto): void {
    rec.followUps = [...(rec.followUps ?? []), this.emptyFollowUp()];
  }

  removeFollowUp(rec: VisitRecommendationWriteDto, j: number): void {
    rec.followUps.splice(j, 1);
  }

  private loadVisit(): void {
    if (!this.visitId) return;
    this.loading = true;
    this.svc
      .getById(this.visitId)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (d) => {
          this.schoolID = d.schoolID;
          this.academicYearID = d.academicYearID;
          this.visitedTeacherID = d.visitedTeacherID;
          this.classID = d.classID ?? null;
          this.subjectID = d.subjectID ?? null;
          this.supervisorEmployeeProfileID = d.supervisorEmployeeProfileID;
          this.visitDate = (d.visitDate || '').slice(0, 10);
          this.status = d.status as SupervisorVisitStatus;
          this.overallScoreOutOf100 = d.overallScoreOutOf100;
          this.summaryNotes = d.summaryNotes ?? '';
          this.observations =
            d.observations?.length > 0
              ? d.observations.map((o, idx) => ({
                  category: o.category ?? '',
                  observationText: o.observationText,
                  sortOrder: o.sortOrder ?? idx,
                }))
              : [this.emptyObservation()];
          this.recommendations =
            d.recommendations?.length > 0
              ? d.recommendations.map((r, idx) => ({
                  recommendationText: r.recommendationText,
                  implementationStatus: r.implementationStatus,
                  dueDate: r.dueDate ? String(r.dueDate).slice(0, 10) : null,
                  completedAtUtc: (r.completedAtUtc as string | null | undefined) ?? null,
                  sortOrder: r.sortOrder ?? idx,
                  followUps: (r.followUps ?? []).map((f) => ({
                    followUpNote: f.followUpNote,
                    followUpDate: String(f.followUpDate).slice(0, 10),
                    followUpByEmployeeProfileID: f.followUpByEmployeeProfileID ?? null,
                  })),
                }))
              : [this.emptyRecommendation()];
          this.refreshYearOptions();
          this.loadSupervisors();
        },
        error: (e) => {
          this.toastr.error(readSupervisorVisitHttpError(e));
          this.router.navigate(['/school/supervisor-visits']).catch(() => undefined);
        },
      });
  }

  cancel(): void {
    this.router.navigate(['/school/supervisor-visits']).catch(() => undefined);
  }

  save(): void {
    if (!this.canSubmit) return;
    if (!this.schoolID || !this.academicYearID || !this.visitedTeacherID || !this.supervisorEmployeeProfileID || !this.visitDate) {
      this.toastr.warning(this.translate.instant('supervisorVisits.form.validationRequired'));
      return;
    }

    const dto: SupervisorVisitWriteDto = {
      schoolID: this.schoolID,
      academicYearID: this.academicYearID,
      visitedTeacherID: this.visitedTeacherID,
      classID: this.classID,
      subjectID: this.subjectID,
      supervisorEmployeeProfileID: this.supervisorEmployeeProfileID,
      visitDate: this.visitDate,
      status: this.status,
      overallScoreOutOf100: this.overallScoreOutOf100,
      summaryNotes: this.summaryNotes || null,
      observations: this.observations
        .filter((o) => (o.observationText || '').trim().length > 0)
        .map((o, i) => ({
          ...o,
          category: (o.category || '').trim() || null,
          sortOrder: i,
        })),
      recommendations: this.recommendations
        .filter((r) => (r.recommendationText || '').trim().length > 0)
        .map((r, i) => ({
          ...r,
          sortOrder: i,
          followUps: (r.followUps ?? [])
            .filter((f) => (f.followUpNote || '').trim().length > 0)
            .map((f) => ({
              followUpNote: f.followUpNote.trim(),
              followUpDate: f.followUpDate,
              followUpByEmployeeProfileID: f.followUpByEmployeeProfileID ?? null,
            })),
        })),
    };

    this.saving = true;
    const req = this.visitId
      ? this.svc.update(this.visitId, dto)
      : this.svc.create(dto);
    req.pipe(finalize(() => (this.saving = false))).subscribe({
      next: () => {
        this.toastr.success(this.visitId ? 'supervisorVisits.toast.updated' : 'supervisorVisits.toast.created');
        this.router.navigate(['/school/supervisor-visits']).catch(() => undefined);
      },
      error: (e) => this.toastr.error(readSupervisorVisitHttpError(e)),
    });
  }
}
