import { Component, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';

import { PagePermission, PermissionService } from '../../../../core/services/permission.service';

/**
 * Navigates from <c>/school/reports</c> to the first sub-report the user may open.
 */
@Component({
  selector: 'app-reports-entry',
  standalone: true,
  template: '',
})
export class ReportsEntryComponent implements OnInit {
  private readonly perm = inject(PermissionService);
  private readonly router = inject(Router);

  ngOnInit(): void {
    const pairs: readonly [string, string][] = [
      [PagePermission.ReportsFinancial.View, 'account'],
      [PagePermission.ReportsTerm.View, 'term-result'],
      [PagePermission.ReportsMonthly.View, 'grades-month'],
      [PagePermission.ReportsRegistration.View, 'registration'],
      [PagePermission.ReportsAllotment.View, 'allotment'],
    ];

    if (this.perm.hasPermission(PagePermission.Reports.View)) {
      this.router.navigate(['/school/reports', 'account'], { replaceUrl: true });
      return;
    }

    for (const [viewPerm, segment] of pairs) {
      if (this.perm.hasPermission(viewPerm)) {
        this.router.navigate(['/school/reports', segment], { replaceUrl: true });
        return;
      }
    }

    this.router.navigate(['/school/dashboard']);
  }
}
