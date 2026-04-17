import { NgIf } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { ToastrService } from 'ngx-toastr';
import { finalize } from 'rxjs/operators';

import { PagePermission, PermissionService } from 'app/core/services/permission.service';
import { SchoolService } from 'app/core/services/school.service';
import { YearService } from 'app/core/services/year.service';
import { Year } from 'app/core/models/year.model';
import { School } from 'app/core/models/school.modul';
import { ShardModule } from 'app/shared/shard.module';

import { EmployeeProfileCreateDto } from '../employees-hr.models';
import { EmployeesHrProfileFormComponent } from '../employees-hr-profile-form/employees-hr-profile-form.component';
import { EmployeesHrService, readHttpError } from '../employees-hr.service';

@Component({
  selector: 'app-employees-hr-create',
  standalone: true,
  imports: [
    ShardModule,
    NgIf,
    TranslateModule,
    ButtonModule,
    RouterLink,
    ProgressSpinnerModule,
    EmployeesHrProfileFormComponent,
  ],
  templateUrl: './employees-hr-create.component.html',
  styleUrl: './employees-hr-create.component.scss',
})
export class EmployeesHrCreateComponent implements OnInit {
  private readonly employeesHr = inject(EmployeesHrService);
  private readonly schoolService = inject(SchoolService);
  private readonly yearService = inject(YearService);
  private readonly router = inject(Router);
  private readonly toastr = inject(ToastrService);
  private readonly perm = inject(PermissionService);

  schools: School[] = [];
  years: Year[] = [];
  loadingMeta = true;
  submitting = false;

  canCreate = this.perm.hasPermission(PagePermission.Employees.Create);

  cancel(): void {
    this.router.navigate(['/school/employees-hr']).catch(() => undefined);
  }

  ngOnInit(): void {
    if (!this.canCreate) {
      this.loadingMeta = false;
      return;
    }
    this.schoolService.getAllSchools().subscribe({
      next: (s) => (this.schools = s ?? []),
      error: () => this.toastr.error('employeesHr.errors.loadSchools'),
    });
    this.yearService
      .getAllYears()
      .pipe(finalize(() => (this.loadingMeta = false)))
      .subscribe({
        next: (y) => (this.years = y ?? []),
        error: () => this.toastr.error('employeesHr.errors.loadYears'),
      });
  }

  onSubmit(dto: EmployeeProfileCreateDto): void {
    this.submitting = true;
    this.employeesHr
      .createEmployee(dto)
      .pipe(finalize(() => (this.submitting = false)))
      .subscribe({
        next: (row) => {
          this.toastr.success('employeesHr.toast.created');
          this.router.navigate(['/school/employees-hr', row.employeeProfileID]).catch(() => undefined);
        },
        error: (err) => this.toastr.error(readHttpError(err)),
      });
  }
}
