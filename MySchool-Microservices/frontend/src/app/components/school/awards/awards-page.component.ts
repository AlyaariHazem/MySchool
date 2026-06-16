import { AsyncPipe, DatePipe, NgIf } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Store } from '@ngrx/store';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { CheckboxModule } from 'primeng/checkbox';
import { DialogModule } from 'primeng/dialog';
import { FloatLabelModule } from 'primeng/floatlabel';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { Select } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { TabsModule } from 'primeng/tabs';
import { TextareaModule } from 'primeng/textarea';
import { ToastrService } from 'ngx-toastr';
import { finalize, map } from 'rxjs/operators';

import { isSchoolManagerUser } from 'app/core/utils/school-role.util';
import { School } from 'app/core/models/school.modul';
import { PagePermission, PermissionService } from 'app/core/services/permission.service';
import { SchoolService } from 'app/core/services/school.service';
import { selectLanguage } from 'app/core/store/language/language.selectors';
import { ShardModule } from 'app/shared/shard.module';

import {
  AwardCycleDto,
  AwardCycleKind,
  AwardCycleStatus,
  AwardCycleWriteDto,
  AwardDto,
  AwardNominationDto,
  AwardNominationStatus,
  AwardWinnerDto,
  AwardWriteDto,
} from './awards.models';
import { AwardsService, readAwardsHttpError } from './awards.service';

@Component({
  selector: 'app-awards-page',
  standalone: true,
  imports: [
    ShardModule,
    NgIf,
    AsyncPipe,
    DatePipe,
    FormsModule,
    TranslateModule,
    TabsModule,
    TableModule,
    ButtonModule,
    Select,
    FloatLabelModule,
    InputTextModule,
    InputNumberModule,
    TextareaModule,
    CheckboxModule,
    DialogModule,
  ],
  templateUrl: './awards-page.component.html',
  styleUrl: './awards-page.component.scss',
})
export class AwardsPageComponent implements OnInit {
  private readonly svc = inject(AwardsService);
  private readonly schoolService = inject(SchoolService);
  private readonly toastr = inject(ToastrService);
  private readonly translate = inject(TranslateService);
  private readonly perm = inject(PermissionService);
  private readonly store = inject(Store);

  readonly dir$ = this.store.select(selectLanguage).pipe(map((l) => (l === 'ar' ? 'rtl' : 'ltr')));
  readonly filterSelectPanelStyle: Record<string, string> = { maxWidth: 'min(22rem, calc(100vw - 2rem))' };

  activeTab = '0';
  schoolOptions: { label: string; value: number }[] = [];
  cycleKindOptions: { label: string; value: AwardCycleKind }[] = [];
  cycleStatusOptions: { label: string; value: AwardCycleStatus }[] = [];

  filterSchoolID: number | null = null;

  awardsRows: AwardDto[] = [];
  cyclesRows: AwardCycleDto[] = [];
  nominationsRows: AwardNominationDto[] = [];
  winnersRows: AwardWinnerDto[] = [];
  awardsLoading = false;
  cyclesLoading = false;
  nominationsLoading = false;
  winnersLoading = false;

  awardDialogVisible = false;
  awardEditID: number | null = null;
  awardSaving = false;
  awardForm: AwardWriteDto = this.emptyAwardForm();

  cycleDialogVisible = false;
  cycleEditID: number | null = null;
  cycleSaving = false;
  cycleForm: AwardCycleWriteDto = this.emptyCycleForm();

  get isSchoolManager(): boolean {
    return isSchoolManagerUser();
  }
  get canView(): boolean {
    return this.perm.hasPermission(PagePermission.Employees.View);
  }
  get canCreate(): boolean {
    return this.perm.hasPermission(PagePermission.Employees.Create);
  }
  get canUpdate(): boolean {
    return this.perm.hasPermission(PagePermission.Employees.Update);
  }

  ngOnInit(): void {
    this.cycleKindOptions = [
      { label: this.translate.instant('awards.cycle.week'), value: AwardCycleKind.Week },
      { label: this.translate.instant('awards.cycle.month'), value: AwardCycleKind.Month },
      { label: this.translate.instant('awards.cycle.term'), value: AwardCycleKind.Term },
      { label: this.translate.instant('awards.cycle.year'), value: AwardCycleKind.Year },
    ];
    this.cycleStatusOptions = [
      { label: this.translate.instant('awards.cycleStatus.draft'), value: AwardCycleStatus.Draft },
      { label: this.translate.instant('awards.cycleStatus.open'), value: AwardCycleStatus.Open },
      { label: this.translate.instant('awards.cycleStatus.nominationsClosed'), value: AwardCycleStatus.NominationsClosed },
      { label: this.translate.instant('awards.cycleStatus.completed'), value: AwardCycleStatus.Completed },
    ];

    if (!this.isSchoolManager) {
      this.schoolService.getAllSchools().subscribe({
        next: (schools: School[]) => {
          this.schoolOptions = (schools ?? [])
            .filter((s) => s.schoolID != null && s.schoolID > 0)
            .map((s) => ({ label: s.schoolName ?? String(s.schoolID), value: s.schoolID as number }));
        },
      });
    } else {
      const raw = typeof localStorage !== 'undefined' ? localStorage.getItem('schoolId') : null;
      const sid = raw != null ? Number(raw) : NaN;
      if (Number.isFinite(sid) && sid > 0) this.filterSchoolID = sid;
    }

    if (this.canView) this.loadAwards();
  }

  loadAwards(): void {
    this.awardsLoading = true;
    this.svc
      .listAwards(this.filterSchoolID)
      .pipe(finalize(() => (this.awardsLoading = false)))
      .subscribe({
        next: (rows) => (this.awardsRows = rows ?? []),
        error: (e) => this.toastr.error(readAwardsHttpError(e)),
      });
  }

  loadCycles(): void {
    this.cyclesLoading = true;
    this.svc
      .listCycles(this.filterSchoolID)
      .pipe(finalize(() => (this.cyclesLoading = false)))
      .subscribe({
        next: (rows) => (this.cyclesRows = rows ?? []),
        error: (e) => this.toastr.error(readAwardsHttpError(e)),
      });
  }

  loadNominations(): void {
    this.nominationsLoading = true;
    this.svc
      .listNominations(this.filterSchoolID)
      .pipe(finalize(() => (this.nominationsLoading = false)))
      .subscribe({
        next: (rows) => (this.nominationsRows = rows ?? []),
        error: (e) => this.toastr.error(readAwardsHttpError(e)),
      });
  }

  loadWinners(): void {
    this.winnersLoading = true;
    this.svc
      .listWinners(this.filterSchoolID)
      .pipe(finalize(() => (this.winnersLoading = false)))
      .subscribe({
        next: (rows) => (this.winnersRows = rows ?? []),
        error: (e) => this.toastr.error(readAwardsHttpError(e)),
      });
  }

  onTabChange(tab: string): void {
    if (tab === '1' && this.cyclesRows.length === 0) this.loadCycles();
    if (tab === '2' && this.nominationsRows.length === 0) this.loadNominations();
    if (tab === '3' && this.winnersRows.length === 0) this.loadWinners();
  }

  openAwardCreate(): void {
    this.awardEditID = null;
    this.awardForm = this.emptyAwardForm();
    if (this.filterSchoolID != null) this.awardForm.schoolID = this.filterSchoolID;
    this.awardDialogVisible = true;
  }

  openAwardEdit(row: AwardDto): void {
    this.awardEditID = row.awardID;
    this.awardForm = {
      schoolID: row.schoolID,
      code: row.code,
      title: row.title,
      description: row.description ?? null,
      cycleKind: row.cycleKind,
      isActive: row.isActive,
      sortOrder: row.sortOrder,
    };
    this.awardDialogVisible = true;
  }

  saveAward(): void {
    if (!this.awardForm.schoolID || !this.awardForm.code?.trim() || !this.awardForm.title?.trim()) {
      this.toastr.warning(this.translate.instant('awards.validation.awardRequired'));
      return;
    }
    this.awardSaving = true;
    const onDone = (): void => {
      this.toastr.success(this.translate.instant(this.awardEditID ? 'awards.toast.updated' : 'awards.toast.created'));
      this.awardDialogVisible = false;
      this.loadAwards();
    };
    const onErr = (e: unknown): void => {
      this.toastr.error(readAwardsHttpError(e));
    };
    if (this.awardEditID != null && this.awardEditID > 0) {
      this.svc
        .updateAward(this.awardEditID, this.awardForm)
        .pipe(finalize(() => (this.awardSaving = false)))
        .subscribe({ next: () => onDone(), error: onErr });
    } else {
      this.svc
        .createAward(this.awardForm)
        .pipe(finalize(() => (this.awardSaving = false)))
        .subscribe({ next: () => onDone(), error: onErr });
    }
  }

  openCycleCreate(): void {
    this.cycleEditID = null;
    this.cycleForm = this.emptyCycleForm();
    this.cycleDialogVisible = true;
  }

  openCycleEdit(row: AwardCycleDto): void {
    this.cycleEditID = row.awardCycleID;
    this.cycleForm = {
      awardID: row.awardID,
      academicYearID: row.academicYearID,
      termID: row.termID ?? null,
      periodStartUtc: row.periodStartUtc,
      periodEndUtc: row.periodEndUtc,
      status: row.status,
    };
    this.cycleDialogVisible = true;
  }

  saveCycle(): void {
    if (!this.cycleForm.awardID || !this.cycleForm.academicYearID || !this.cycleForm.periodStartUtc || !this.cycleForm.periodEndUtc) {
      this.toastr.warning(this.translate.instant('awards.validation.cycleRequired'));
      return;
    }
    this.cycleSaving = true;
    const onDone = (): void => {
      this.toastr.success(this.translate.instant(this.cycleEditID ? 'awards.toast.cycleUpdated' : 'awards.toast.cycleCreated'));
      this.cycleDialogVisible = false;
      this.loadCycles();
    };
    const onErr = (e: unknown): void => {
      this.toastr.error(readAwardsHttpError(e));
    };
    if (this.cycleEditID != null && this.cycleEditID > 0) {
      this.svc
        .updateCycle(this.cycleEditID, this.cycleForm)
        .pipe(finalize(() => (this.cycleSaving = false)))
        .subscribe({ next: () => onDone(), error: onErr });
    } else {
      this.svc
        .createCycle(this.cycleForm)
        .pipe(finalize(() => (this.cycleSaving = false)))
        .subscribe({ next: () => onDone(), error: onErr });
    }
  }

  cycleKindLabel(v: AwardCycleKind): string {
    const m: Record<number, string> = {
      [AwardCycleKind.Week]: 'awards.cycle.week',
      [AwardCycleKind.Month]: 'awards.cycle.month',
      [AwardCycleKind.Term]: 'awards.cycle.term',
      [AwardCycleKind.Year]: 'awards.cycle.year',
    };
    return this.translate.instant(m[v] ?? '');
  }

  cycleStatusLabel(v: AwardCycleStatus): string {
    const m: Record<number, string> = {
      [AwardCycleStatus.Draft]: 'awards.cycleStatus.draft',
      [AwardCycleStatus.Open]: 'awards.cycleStatus.open',
      [AwardCycleStatus.NominationsClosed]: 'awards.cycleStatus.nominationsClosed',
      [AwardCycleStatus.Completed]: 'awards.cycleStatus.completed',
    };
    return this.translate.instant(m[v] ?? '');
  }

  nominationStatusLabel(v: AwardNominationStatus): string {
    const m: Record<number, string> = {
      [AwardNominationStatus.Pending]: 'awards.nominationStatus.pending',
      [AwardNominationStatus.Shortlisted]: 'awards.nominationStatus.shortlisted',
      [AwardNominationStatus.Rejected]: 'awards.nominationStatus.rejected',
      [AwardNominationStatus.Withdrawn]: 'awards.nominationStatus.withdrawn',
    };
    return this.translate.instant(m[v] ?? '');
  }

  private emptyAwardForm(): AwardWriteDto {
    return {
      schoolID: this.filterSchoolID ?? 0,
      code: '',
      title: '',
      description: null,
      cycleKind: AwardCycleKind.Week,
      isActive: true,
      sortOrder: 0,
    };
  }

  private emptyCycleForm(): AwardCycleWriteDto {
    return {
      awardID: 0,
      academicYearID: 0,
      termID: null,
      periodStartUtc: '',
      periodEndUtc: '',
      status: AwardCycleStatus.Draft,
    };
  }
}
