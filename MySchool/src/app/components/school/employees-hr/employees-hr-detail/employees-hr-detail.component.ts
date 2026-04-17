import { DatePipe } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { ToastrService } from 'ngx-toastr';
import { finalize } from 'rxjs/operators';

import { PagePermission, PermissionService } from 'app/core/services/permission.service';
import { ShardModule } from 'app/shared/shard.module';

import { EmployeeProfileReadDto } from '../employees-hr.models';
import { EmployeesHrService, readHttpError } from '../employees-hr.service';

@Component({
  selector: 'app-employees-hr-detail',
  standalone: true,
  imports: [
    ShardModule,
    TranslateModule,
    ButtonModule,
    RouterLink,
    ProgressSpinnerModule,
    DatePipe,
  ],
  templateUrl: './employees-hr-detail.component.html',
  styleUrl: './employees-hr-detail.component.scss',
})
export class EmployeesHrDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly employeesHr = inject(EmployeesHrService);
  private readonly toastr = inject(ToastrService);
  private readonly perm = inject(PermissionService);

  id = 0;
  row: EmployeeProfileReadDto | null = null;
  loading = true;

  canView = this.perm.hasPermission(PagePermission.Employees.View);
  canUpdate = this.perm.hasPermission(PagePermission.Employees.Update);
  canDelete = this.perm.hasPermission(PagePermission.Employees.Delete);

  ngOnInit(): void {
    const p = this.route.snapshot.paramMap.get('id');
    this.id = p ? +p : 0;
    if (!this.id || !this.canView) {
      this.loading = false;
      return;
    }
    this.employeesHr
      .getEmployeeById(this.id)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (r) => (this.row = r),
        error: (err) => {
          this.toastr.error(readHttpError(err));
          this.row = null;
        },
      });
  }

  displayName(p: EmployeeProfileReadDto): string {
    const n = p.fullName;
    return [n?.firstName, n?.middleName, n?.lastName].filter(Boolean).join(' ');
  }
}
