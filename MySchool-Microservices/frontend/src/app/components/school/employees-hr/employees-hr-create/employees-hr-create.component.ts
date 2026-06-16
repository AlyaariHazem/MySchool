import { NgIf } from '@angular/common';
import { Component, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { ToastrService } from 'ngx-toastr';
import { finalize } from 'rxjs/operators';

import { PagePermission, PermissionService } from 'app/core/services/permission.service';
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
    EmployeesHrProfileFormComponent,
  ],
  templateUrl: './employees-hr-create.component.html',
  styleUrl: './employees-hr-create.component.scss',
})
export class EmployeesHrCreateComponent {
  private readonly employeesHr = inject(EmployeesHrService);
  private readonly router = inject(Router);
  private readonly toastr = inject(ToastrService);
  private readonly perm = inject(PermissionService);

  submitting = false;

  canCreate = this.perm.hasPermission(PagePermission.Employees.Create);

  cancel(): void {
    this.router.navigate(['/school/employees-hr']).catch(() => undefined);
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
