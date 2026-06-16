import { Component, inject, OnInit } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { PaginatorState } from 'primeng/paginator';
import { MatDialog, MatDialogConfig } from '@angular/material/dialog';
import { ToastrService } from 'ngx-toastr';
import { map } from 'rxjs';
import { finalize } from 'rxjs';
import { Store } from '@ngrx/store';

import { AddManagerComponent } from './add-manager/add-manager.component';
import { managerInfo } from '../core/models/managerInfo.model';
import { ManagerService } from '../../../core/services/manager.service';
import { selectLanguage } from '../../../core/store/language/language.selectors';

@Component({
  selector: 'app-users',
  templateUrl: './users.component.html',
  styleUrl: './users.component.scss',
})
export class UsersComponent implements OnInit {
  private readonly toastr = inject(ToastrService);
  private readonly store = inject(Store);
  readonly dialog = inject(MatDialog);
  readonly managerService = inject(ManagerService);

  readonly dir$ = this.store.select(selectLanguage).pipe(
    map((l) => (l === 'ar' ? 'rtl' : 'ltr')),
  );

  /** Current page from POST api/Manager/page */
  managerPage: managerInfo[] = [];
  totalRecords = 0;
  first = 0;
  rows = 10;
  isListLoading = false;
  isMutating = false;

  get isBusy(): boolean {
    return this.isListLoading || this.isMutating;
  }

  ngOnInit(): void {
    this.loadManagersPage();
  }

  loadManagersPage(): void {
    const pageIndex = Math.floor(this.first / this.rows);
    this.isListLoading = true;
    this.managerService.getManagersPage({ pageIndex, pageSize: this.rows }).pipe(
      finalize(() => {
        this.isListLoading = false;
      }),
    ).subscribe({
      next: (page) => {
        if (
          page.totalCount > 0 &&
          page.data.length === 0 &&
          page.totalPages > 0 &&
          pageIndex >= page.totalPages
        ) {
          const lastPageIndex = page.totalPages - 1;
          this.first = lastPageIndex * this.rows;
          this.loadManagersPage();
          return;
        }
        this.managerPage = (page.data ?? []) as managerInfo[];
        this.totalRecords = page.totalCount;
        if (page.totalCount === 0) {
          this.first = 0;
        }
      },
      error: (err) => {
        this.toastr.error(this.formatHttpError(err), 'تعذر تحميل المستخدمين');
      },
    });
  }

  private formatHttpError(err: unknown): string {
    if (err instanceof HttpErrorResponse) {
      const body = err.error;
      if (typeof body === 'string' && body.trim().length > 0 && body.length < 600) {
        return body.trim();
      }
      if (body && typeof body === 'object') {
        const o = body as {
          errorMasseges?: string[];
          message?: string;
          title?: string;
        };
        if (o.errorMasseges?.length) {
          return o.errorMasseges.join(' ');
        }
        if (o.message) {
          return o.message;
        }
        if (o.title) {
          return o.title;
        }
      }
      if (err.message) {
        return err.message;
      }
    }
    return 'حدث خطأ غير متوقع';
  }

  onPageChange(event: PaginatorState): void {
    if (this.isListLoading) {
      return;
    }
    this.first = event.first ?? 0;
    this.rows = event.rows ?? 10;
    this.loadManagersPage();
  }

  openDialog(): void {
    if (this.isBusy) {
      return;
    }
    const dialogConfig = new MatDialogConfig();
    dialogConfig.width = '95%';
    dialogConfig.panelClass = 'custom-dialog-container';

    const dialogRef = this.dialog.open(AddManagerComponent, dialogConfig);

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.toastr.success('تم إضافة الطالب بنجاح');
      }
      this.loadManagersPage();
    });
  }

  editUser(manager: managerInfo): void {
    if (this.isBusy) {
      return;
    }
    const dialogConfig = new MatDialogConfig();
    dialogConfig.width = '95%';
    dialogConfig.panelClass = 'custom-dialog-container';
    dialogConfig.data = { manager, isEditMode: true };

    const dialogRef = this.dialog.open(AddManagerComponent, dialogConfig);

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.toastr.success('تم تحديث الطالب بنجاح');
      }
      this.loadManagersPage();
    });
  }

  deleteUser(userID: number): void {
    if (this.isBusy) {
      return;
    }
    if (!confirm('هل أنت متأكد من حذف هذا المستخدم؟')) {
      return;
    }
    this.isMutating = true;
    this.managerService.deleteManager(userID).pipe(
      finalize(() => {
        this.isMutating = false;
      }),
    ).subscribe({
      next: () => {
        this.toastr.success('تم حذف المستخدم');
        this.loadManagersPage();
      },
      error: (err) => {
        this.toastr.error(this.formatHttpError(err), 'فشل الحذف');
      },
    });
  }
}
