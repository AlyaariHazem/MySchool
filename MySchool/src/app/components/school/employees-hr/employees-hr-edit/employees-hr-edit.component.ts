import { NgIf } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { ToastrService } from 'ngx-toastr';
import { finalize } from 'rxjs/operators';

import { PagePermission, PermissionService } from 'app/core/services/permission.service';
import { ShardModule } from 'app/shared/shard.module';

import { EmployeeProfileCreateDto, EmployeeProfileReadDto } from '../employees-hr.models';
import { EmployeesHrProfileFormComponent } from '../employees-hr-profile-form/employees-hr-profile-form.component';
import { EmployeesHrService, readHttpError } from '../employees-hr.service';

@Component({
  selector: 'app-employees-hr-edit',
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
  templateUrl: './employees-hr-edit.component.html',
  styleUrl: './employees-hr-edit.component.scss',
})
export class EmployeesHrEditComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  readonly router = inject(Router);
  private readonly employeesHr = inject(EmployeesHrService);
  private readonly toastr = inject(ToastrService);
  private readonly perm = inject(PermissionService);

  id = 0;
  profile: EmployeeProfileReadDto | null = null;
  loading = true;
  submitting = false;

  canUpdate = this.perm.hasPermission(PagePermission.Employees.Update);

  ngOnInit(): void {
    const p = this.route.snapshot.paramMap.get('id');
    this.id = p ? +p : 0;
    if (!this.id || !this.canUpdate) {
      this.loading = false;
      return;
    }
    this.employeesHr
      .getEmployeeById(this.id)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (row) => (this.profile = row),
        error: (err) => {
          this.toastr.error(readHttpError(err));
          this.profile = null;
        },
      });
  }

  onSubmit(dto: EmployeeProfileCreateDto): void {
    this.submitting = true;
    this.employeesHr
      .updateEmployee(this.id, dto)
      .pipe(finalize(() => (this.submitting = false)))
      .subscribe({
        next: () => {
          this.toastr.success('employeesHr.toast.updated');
          this.router.navigate(['/school/employees-hr', this.id]).catch(() => undefined);
        },
        error: (err) => this.toastr.error(readHttpError(err)),
      });
  }
}
