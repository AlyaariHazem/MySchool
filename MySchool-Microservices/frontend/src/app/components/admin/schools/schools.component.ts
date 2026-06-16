import { Component, inject, OnInit } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { MatDialog } from '@angular/material/dialog';
import { ToastrService } from 'ngx-toastr';

import { SchoolInfoComponent } from '../school-info/school-info.component';
import { SchoolService } from '../../../core/services/school.service';
import { School } from '../../../core/models/school.modul';
import { PaginatorService } from '../../../core/services/paginator.service';
import { PaginatorState } from 'primeng/paginator';

@Component({
  selector: 'app-schools',
  templateUrl: './schools.component.html',
  styleUrls: ['./schools.component.scss'],
})
export class SchoolsComponent implements OnInit {
  private readonly schoolService = inject(SchoolService);
  private readonly dialog = inject(MatDialog);
  private readonly toastr = inject(ToastrService);

  paginatorService = inject(PaginatorService);
  /** Current page rows from POST api/School/page */
  paginatedSchools: School[] = [];
  /** Total rows across all pages (server) */
  totalRecords = 0;

  displayedColumns: string[] = [
    'schoolName',
    'schoolNameEn',
    'schoolCreaDate',
    'schoolType',
    'city',
    'schoolPhone',
    'email',
    'actions',
  ];

  ngOnInit(): void {
    this.loadSchoolsPage();
  }

  loadSchoolsPage(): void {
    const rows = this.paginatorService.rows();
    const first = this.paginatorService.first();
    const pageIndex = Math.floor(first / rows);

    this.schoolService
      .getSchoolsPage({ pageIndex, pageSize: rows })
      .subscribe({
        next: (page) => {
          if (
            page.totalCount > 0 &&
            page.data.length === 0 &&
            page.totalPages > 0 &&
            pageIndex >= page.totalPages
          ) {
            const lastPageIndex = page.totalPages - 1;
            this.paginatorService.first.set(lastPageIndex * rows);
            this.loadSchoolsPage();
            return;
          }

          this.paginatedSchools = page.data ?? [];
          this.totalRecords = page.totalCount;
          if (page.totalCount === 0) {
            this.paginatorService.first.set(0);
          }
        },
        error: (err) => {
          this.toastr.error(this.formatHttpError(err), 'تعذر تحميل المدارس');
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

  handlePageChange(event: PaginatorState): void {
    this.paginatorService.onPageChange(event);
    this.loadSchoolsPage();
  }

  openAddSchoolForm(): void {
    this.dialog
      .open(SchoolInfoComponent, {
        width: '80%',
        height: '80%',
        data: { isEditMode: false },
      })
      .afterClosed()
      .subscribe(() => this.loadSchoolsPage());
  }

  openEditSchoolForm(school: School): void {
    this.dialog
      .open(SchoolInfoComponent, {
        width: '80%',
        height: '80%',
        data: { isEditMode: true, schoolData: school },
      })
      .afterClosed()
      .subscribe(() => this.loadSchoolsPage());
  }

  deleteSchool(school: School): void {
    const id = school.schoolID;
    if (id == null) {
      this.toastr.warning('معرف المدرسة غير صالح');
      return;
    }
    if (!confirm('هل أنت متأكد من حذف هذه المدرسة؟')) {
      return;
    }

    this.schoolService.deleteSchool(id).subscribe({
      next: () => {
        this.toastr.success('تم حذف المدرسة');
        this.loadSchoolsPage();
      },
      error: (err) => {
        this.toastr.error(this.formatHttpError(err), 'فشل الحذف');
      },
    });
  }
}
