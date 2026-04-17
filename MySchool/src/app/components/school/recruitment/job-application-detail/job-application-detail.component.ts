import { DatePipe, NgIf } from '@angular/common';
import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ConfirmationService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { CheckboxModule } from 'primeng/checkbox';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { DatePicker } from 'primeng/datepicker';
import { DialogModule } from 'primeng/dialog';
import { FloatLabelModule } from 'primeng/floatlabel';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { Select } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { TextareaModule } from 'primeng/textarea';
import { MatTabsModule } from '@angular/material/tabs';
import { ToastrService } from 'ngx-toastr';
import { finalize } from 'rxjs/operators';
import { Subscription } from 'rxjs';

import { PagePermission, PermissionService } from 'app/core/services/permission.service';
import { ShardModule } from 'app/shared/shard.module';

import { EmployeeJobTypeDto } from '../../employees-hr/employees-hr.models';
import { EmployeesHrService } from '../../employees-hr/employees-hr.service';
import {
  CandidateEvaluationCreateDto,
  CandidateEvaluationReadDto,
  CandidateEvaluationUpdateDto,
  ConvertApplicantToEmployeeDto,
  EvaluationRecommendation,
  HiringDecisionCreateDto,
  HiringDecisionStatus,
  InterviewCreateDto,
  InterviewReadDto,
  InterviewStatus,
  InterviewUpdateDto,
  JobApplicationFullDto,
  JobApplicationStatus,
  JobApplicationStatusMoveDto,
  JobPostingStatus,
} from '../recruitment.models';
import { RecruitmentService, readRecruitmentHttpError } from '../recruitment.service';
import { EmploymentStatus } from '../../employees-hr/employees-hr.models';

@Component({
  selector: 'app-job-application-detail',
  standalone: true,
  imports: [
    ShardModule,
    NgIf,
    FormsModule,
    TranslateModule,
    ButtonModule,
    RouterLink,
    ProgressSpinnerModule,
    DatePipe,
    TagModule,
    MatTabsModule,
    TableModule,
    DialogModule,
    InputTextModule,
    TextareaModule,
    Select,
    FloatLabelModule,
    InputNumberModule,
    DatePicker,
    CheckboxModule,
    ConfirmDialogModule,
  ],
  providers: [ConfirmationService],
  templateUrl: './job-application-detail.component.html',
  styleUrl: './job-application-detail.component.scss',
})
export class JobApplicationDetailComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly recruitment = inject(RecruitmentService);
  private readonly employeesHr = inject(EmployeesHrService);
  private readonly toastr = inject(ToastrService);
  private readonly translate = inject(TranslateService);
  private readonly confirm = inject(ConfirmationService);
  private readonly perm = inject(PermissionService);

  private langSub?: Subscription;

  id = 0;
  full: JobApplicationFullDto | null = null;
  loading = true;

  readonly canView = this.perm.hasAny([PagePermission.Recruitment.View, PagePermission.Employees.View]);
  readonly canUpdate = this.perm.hasAny([PagePermission.Recruitment.Update, PagePermission.Employees.Update]);

  jobTypes: EmployeeJobTypeDto[] = [];
  jobTypeOptions: { label: string; value: number }[] = [];

  JobApplicationStatus = JobApplicationStatus;
  InterviewStatus = InterviewStatus;
  HiringDecisionStatus = HiringDecisionStatus;
  EvaluationRecommendation = EvaluationRecommendation;
  JobPostingStatus = JobPostingStatus;
  EmploymentStatus = EmploymentStatus;

  // Dialogs
  showInterview = false;
  interviewEdit: InterviewReadDto | null = null;
  /** Dialog model; datepicker may bind Date; API expects ISO string on save. */
  interviewDraft: InterviewCreateDto & { interviewDate?: string | Date } = {
    interviewDate: new Date().toISOString(),
  };

  evalRecommendationOptions: { label: string; value: EvaluationRecommendation }[] = [];

  showEval = false;
  evalEdit: CandidateEvaluationReadDto | null = null;
  evalDraft: CandidateEvaluationCreateDto = {
    recommendation: EvaluationRecommendation.Consider,
  };

  showDecision = false;
  decisionDraft: HiringDecisionCreateDto = {
    decisionStatus: HiringDecisionStatus.Pending,
    offerJobTypeID: 0,
    skipEvaluationCheck: false,
  };

  showStatus = false;
  statusMove: JobApplicationStatusMoveDto = { newStatus: JobApplicationStatus.UnderReview };

  showConvert = false;
  convertDraft: ConvertApplicantToEmployeeDto = {
    employmentStatus: EmploymentStatus.Active,
    mapQualificationAndSpecialization: true,
  };

  saving = false;

  ngOnInit(): void {
    this.ngOnInitEvalLabels();
    const p = this.route.snapshot.paramMap.get('id');
    this.id = p ? +p : 0;
    this.loadJobTypes();
    this.langSub = this.translate.onLangChange.subscribe(() => {
      this.rebuildJobTypeLabels();
      this.ngOnInitEvalLabels();
    });
    if (!this.id || !this.canView) {
      this.loading = false;
      return;
    }
    this.reload();
  }

  ngOnDestroy(): void {
    this.langSub?.unsubscribe();
  }

  private loadJobTypes(): void {
    this.employeesHr.getEmployeeJobTypes().subscribe({
      next: (rows) => {
        this.jobTypes = rows;
        this.rebuildJobTypeLabels();
      },
      error: () => (this.jobTypes = []),
    });
  }

  private rebuildJobTypeLabels(): void {
    const lang = this.translate.currentLang;
    this.jobTypeOptions = this.jobTypes.map((j) => ({
      value: j.employeeJobTypeID,
      label: lang === 'ar' && j.nameAr ? j.nameAr : j.name,
    }));
  }

  reload(): void {
    if (!this.id) return;
    this.loading = true;
    this.recruitment
      .getJobApplicationFull(this.id)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (f) => {
          this.full = f;
          if (f.decision) {
            this.decisionDraft = {
              decisionStatus: f.decision.decisionStatus,
              offerJobTypeID: f.decision.offerJobTypeID,
              proposedHireDate: f.decision.proposedHireDate ?? null,
              proposedSalaryNotes: f.decision.proposedSalaryNotes ?? null,
              reason: f.decision.reason ?? null,
              internalNotes: f.decision.internalNotes ?? null,
              skipEvaluationCheck: false,
            };
          }
        },
        error: (err) => {
          this.toastr.error(readRecruitmentHttpError(err));
          this.full = null;
        },
      });
  }

  appStatusLabelKey(s: JobApplicationStatus): string {
    const m: Record<number, string> = {
      [JobApplicationStatus.Submitted]: 'submitted',
      [JobApplicationStatus.UnderReview]: 'underReview',
      [JobApplicationStatus.InterviewScheduled]: 'interviewScheduled',
      [JobApplicationStatus.Evaluated]: 'evaluated',
      [JobApplicationStatus.Accepted]: 'accepted',
      [JobApplicationStatus.Rejected]: 'rejected',
      [JobApplicationStatus.ConvertedToEmployee]: 'convertedToEmployee',
      [JobApplicationStatus.Withdrawn]: 'withdrawn',
    };
    return m[s] ?? 'submitted';
  }

  openPostingOk(): boolean {
    return this.full?.posting?.status === JobPostingStatus.Open;
  }

  // --- Interviews ---
  private ngOnInitEvalLabels(): void {
    this.evalRecommendationOptions = [
      { label: this.translate.instant('recruitment.eval.recStrongReject'), value: EvaluationRecommendation.StrongReject },
      { label: this.translate.instant('recruitment.eval.recReject'), value: EvaluationRecommendation.Reject },
      { label: this.translate.instant('recruitment.eval.recConsider'), value: EvaluationRecommendation.Consider },
      { label: this.translate.instant('recruitment.eval.recRecommend'), value: EvaluationRecommendation.Recommend },
      { label: this.translate.instant('recruitment.eval.recStrongRecommend'), value: EvaluationRecommendation.StrongRecommend },
    ];
  }

  postingStatusKey(s: JobPostingStatus): string {
    switch (s) {
      case JobPostingStatus.Draft:
        return 'draft';
      case JobPostingStatus.Open:
        return 'open';
      case JobPostingStatus.Closed:
        return 'closed';
      case JobPostingStatus.Archived:
        return 'archived';
      default:
        return 'draft';
    }
  }

  private toIsoStringOrUndefined(raw: string | Date | undefined | null): string | undefined {
    if (raw == null || raw === '') return undefined;
    if (typeof raw === 'string') return raw;
    if (raw instanceof Date) return raw.toISOString();
    return new Date(raw as unknown as string).toISOString();
  }

  private toIsoStringRequired(raw: string | Date | undefined | null): string {
    if (raw == null || raw === '') return new Date().toISOString();
    if (typeof raw === 'string') return raw;
    if (raw instanceof Date) return raw.toISOString();
    return new Date(String(raw)).toISOString();
  }

  openNewInterview(): void {
    this.interviewEdit = null;
    this.interviewDraft = { interviewDate: new Date().toISOString() };
    this.showInterview = true;
  }

  editInterview(row: InterviewReadDto): void {
    this.interviewEdit = row;
    this.interviewDraft = {
      interviewDate: row.interviewDate,
      interviewType: row.interviewType,
      locationOrMeetingLink: row.locationOrMeetingLink,
      interviewerName: row.interviewerName,
      interviewerUserID: row.interviewerUserID,
      interviewerEmployeeProfileID: row.interviewerEmployeeProfileID,
      notes: row.notes,
    };
    this.showInterview = true;
  }

  saveInterview(): void {
    if (!this.full) return;
    this.saving = true;
    const done = () => {
      this.saving = false;
      this.showInterview = false;
      this.reload();
    };
    if (this.interviewEdit) {
      const u: InterviewUpdateDto = {
        interviewDate: this.toIsoStringOrUndefined(this.interviewDraft.interviewDate),
        interviewType: this.interviewDraft.interviewType,
        locationOrMeetingLink: this.interviewDraft.locationOrMeetingLink,
        interviewerName: this.interviewDraft.interviewerName,
        interviewerUserID: this.interviewDraft.interviewerUserID,
        interviewerEmployeeProfileID: this.interviewDraft.interviewerEmployeeProfileID,
        notes: this.interviewDraft.notes,
      };
      this.recruitment
        .updateInterview(this.interviewEdit.interviewID, u)
        .pipe(finalize(done))
        .subscribe({
          next: () => this.toastr.success(this.translate.instant('recruitment.workflow.interviewSaved')),
          error: (e) => this.toastr.error(readRecruitmentHttpError(e)),
        });
    } else {
      const raw = this.interviewDraft.interviewDate;
      const iso = this.toIsoStringRequired(raw);
      const createPayload: InterviewCreateDto = {
        ...this.interviewDraft,
        interviewDate: iso,
      };
      this.recruitment
        .createInterview(this.id, createPayload)
        .pipe(finalize(done))
        .subscribe({
          next: () => this.toastr.success(this.translate.instant('recruitment.workflow.interviewSaved')),
          error: (e) => this.toastr.error(readRecruitmentHttpError(e)),
        });
    }
  }

  confirmCompleteInterview(row: InterviewReadDto): void {
    this.confirm.confirm({
      message: this.translate.instant('recruitment.workflow.confirmCompleteInterview'),
      accept: () =>
        this.recruitment.completeInterview(row.interviewID).subscribe({
          next: () => {
            this.toastr.success(this.translate.instant('recruitment.workflow.done'));
            this.reload();
          },
          error: (e) => this.toastr.error(readRecruitmentHttpError(e)),
        }),
    });
  }

  cancelInterviewRow(row: InterviewReadDto): void {
    this.recruitment.cancelInterview(row.interviewID).subscribe({
      next: () => {
        this.toastr.success(this.translate.instant('recruitment.workflow.done'));
        this.reload();
      },
      error: (e) => this.toastr.error(readRecruitmentHttpError(e)),
    });
  }

  noShowInterviewRow(row: InterviewReadDto): void {
    this.recruitment.noShowInterview(row.interviewID).subscribe({
      next: () => {
        this.toastr.success(this.translate.instant('recruitment.workflow.done'));
        this.reload();
      },
      error: (e) => this.toastr.error(readRecruitmentHttpError(e)),
    });
  }

  // --- Evaluations ---
  openNewEval(): void {
    this.evalEdit = null;
    this.evalDraft = { recommendation: EvaluationRecommendation.Consider };
    this.showEval = true;
  }

  editEval(row: CandidateEvaluationReadDto): void {
    this.evalEdit = row;
    this.evalDraft = {
      interviewID: row.interviewID,
      technicalScore: row.technicalScore,
      communicationScore: row.communicationScore,
      classManagementScore: row.classManagementScore,
      cultureFitScore: row.cultureFitScore,
      overallScore: row.overallScore,
      strengths: row.strengths,
      weaknesses: row.weaknesses,
      recommendation: row.recommendation,
      notes: row.notes,
    };
    this.showEval = true;
  }

  saveEval(): void {
    this.saving = true;
    const done = () => {
      this.saving = false;
      this.showEval = false;
      this.reload();
    };
    if (this.evalEdit) {
      const u: CandidateEvaluationUpdateDto = { ...this.evalDraft };
      this.recruitment
        .updateEvaluation(this.evalEdit.candidateEvaluationID, u)
        .pipe(finalize(done))
        .subscribe({
          next: () => this.toastr.success(this.translate.instant('recruitment.workflow.evalSaved')),
          error: (e) => this.toastr.error(readRecruitmentHttpError(e)),
        });
    } else {
      this.recruitment
        .createEvaluation(this.id, this.evalDraft)
        .pipe(finalize(done))
        .subscribe({
          next: () => this.toastr.success(this.translate.instant('recruitment.workflow.evalSaved')),
          error: (e) => this.toastr.error(readRecruitmentHttpError(e)),
        });
    }
  }

  // --- Decision ---
  openDecisionDialog(): void {
    const d = this.full?.decision;
    this.decisionDraft = {
      decisionStatus: d?.decisionStatus ?? HiringDecisionStatus.Pending,
      offerJobTypeID: d?.offerJobTypeID ?? this.full?.posting?.employeeJobTypeID ?? 0,
      proposedHireDate: d?.proposedHireDate ?? null,
      proposedSalaryNotes: d?.proposedSalaryNotes ?? null,
      reason: d?.reason ?? null,
      internalNotes: d?.internalNotes ?? null,
      skipEvaluationCheck: false,
    };
    this.showDecision = true;
  }

  saveDecision(): void {
    this.saving = true;
    if (!this.full?.decision) {
      this.recruitment
        .createDecision(this.id, this.decisionDraft)
        .pipe(finalize(() => (this.saving = false)))
        .subscribe({
          next: () => {
            this.toastr.success(this.translate.instant('recruitment.workflow.decisionSaved'));
            this.showDecision = false;
            this.reload();
          },
          error: (e) => this.toastr.error(readRecruitmentHttpError(e)),
        });
    } else {
      this.recruitment
        .updateDecision(this.full.decision.hiringDecisionID, {
          decisionStatus: this.decisionDraft.decisionStatus,
          offerJobTypeID: this.decisionDraft.offerJobTypeID,
          proposedHireDate: this.decisionDraft.proposedHireDate,
          proposedSalaryNotes: this.decisionDraft.proposedSalaryNotes,
          reason: this.decisionDraft.reason,
          internalNotes: this.decisionDraft.internalNotes,
        })
        .pipe(finalize(() => (this.saving = false)))
        .subscribe({
          next: () => {
            this.toastr.success(this.translate.instant('recruitment.workflow.decisionSaved'));
            this.showDecision = false;
            this.reload();
          },
          error: (e) => this.toastr.error(readRecruitmentHttpError(e)),
        });
    }
  }

  accept(): void {
    const payload: HiringDecisionCreateDto = {
      decisionStatus: HiringDecisionStatus.Accepted,
      offerJobTypeID: this.decisionDraft.offerJobTypeID || this.full?.posting?.employeeJobTypeID || 0,
      skipEvaluationCheck: this.decisionDraft.skipEvaluationCheck,
      reason: this.decisionDraft.reason,
      internalNotes: this.decisionDraft.internalNotes,
      proposedHireDate: this.decisionDraft.proposedHireDate,
      proposedSalaryNotes: this.decisionDraft.proposedSalaryNotes,
    };
    this.recruitment.acceptApplication(this.id, payload).subscribe({
      next: () => {
        this.toastr.success(this.translate.instant('recruitment.workflow.accepted'));
        this.reload();
      },
      error: (e) => this.toastr.error(readRecruitmentHttpError(e)),
    });
  }

  reject(): void {
    const payload: HiringDecisionCreateDto = {
      decisionStatus: HiringDecisionStatus.Rejected,
      offerJobTypeID: this.decisionDraft.offerJobTypeID || this.full?.posting?.employeeJobTypeID || 0,
      skipEvaluationCheck: this.decisionDraft.skipEvaluationCheck,
      reason: this.decisionDraft.reason,
      internalNotes: this.decisionDraft.internalNotes,
    };
    this.recruitment.rejectApplication(this.id, payload).subscribe({
      next: () => {
        this.toastr.success(this.translate.instant('recruitment.workflow.rejected'));
        this.reload();
      },
      error: (e) => this.toastr.error(readRecruitmentHttpError(e)),
    });
  }

  // --- Status ---
  openStatusMove(): void {
    this.statusMove = { newStatus: JobApplicationStatus.UnderReview };
    this.showStatus = true;
  }

  saveStatusMove(): void {
    this.recruitment.moveJobApplicationStatus(this.id, this.statusMove).subscribe({
      next: () => {
        this.toastr.success(this.translate.instant('recruitment.workflow.statusUpdated'));
        this.showStatus = false;
        this.reload();
      },
      error: (e) => this.toastr.error(readRecruitmentHttpError(e)),
    });
  }

  // --- Convert ---
  openConvert(): void {
    this.convertDraft = {
      employmentStatus: EmploymentStatus.Active,
      mapQualificationAndSpecialization: true,
      employeeJobTypeID: this.full?.decision?.offerJobTypeID ?? this.full?.posting?.employeeJobTypeID,
    };
    this.showConvert = true;
  }

  convert(): void {
    this.saving = true;
    this.recruitment
      .convertApplicantToEmployee(this.id, this.convertDraft)
      .pipe(finalize(() => (this.saving = false)))
      .subscribe({
        next: () => {
          this.toastr.success(this.translate.instant('recruitment.workflow.converted'));
          this.showConvert = false;
          this.reload();
        },
        error: (e) => this.toastr.error(readRecruitmentHttpError(e)),
      });
  }

  canConvert(): boolean {
    const a = this.full?.application;
    if (!a || a.convertedEmployeeProfileID) return false;
    if (a.status !== JobApplicationStatus.Accepted) return false;
    if (this.full?.decision?.decisionStatus !== HiringDecisionStatus.Accepted) return false;
    return true;
  }
}
