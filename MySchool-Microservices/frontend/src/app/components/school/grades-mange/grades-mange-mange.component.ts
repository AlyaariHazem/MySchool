import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Store } from '@ngrx/store';
import { ToastrService } from 'ngx-toastr';
import { PaginatorState } from 'primeng/paginator';
import { ConfirmationService } from 'primeng/api';
import { map } from 'rxjs';

import { GradeTypeService } from '../core/services/grade-type.service';
import { GradeType } from '../core/models/gradeType.model';
import { ApiResponse } from '../../../core/models/response.model';
import { selectLanguage } from '../../../core/store/language/language.selectors';

@Component({
  selector: 'app-grades-mange',
  templateUrl: './grades-mange.component.html',
  styleUrls: ['./grades-mange.component.scss', '../../../shared/styles/style-table.scss']
})
export class GradesMangeComponent implements OnInit {
  form: FormGroup;
  search: unknown;
  gradeTypeService = inject(GradeTypeService);
  private toastr = inject(ToastrService);
  private confirmationService = inject(ConfirmationService);

  gradeTypes: GradeType[] = [];
  paginatedGradeTypes: GradeType[] = [];
  apiErrorMessages: string[] = [];
  editingGradeTypeId: number | null = null;
  isLoading = true;

  first = 0;
  rows = 4;

  readonly dir$ = this.store.select(selectLanguage).pipe(
    map(l => (l === 'ar' ? 'rtl' : 'ltr')),
  );

  constructor(
    private formBuilder: FormBuilder,
    private store: Store,
  ) {
    this.form = this.formBuilder.group({
      name: ['', Validators.required],
      maxGrade: [null as number | null, [Validators.required, Validators.min(0)]],
      isActive: [true],
    });
  }

  ngOnInit(): void {
    this.getAllGradeTypes();
  }

  getAllGradeTypes(): void {
    this.gradeTypeService.getAllGradeType().subscribe({
      next: res => {
        if (res.isSuccess && res.result) {
          this.apiErrorMessages = [];
          this.gradeTypes = res.result;
          this.clampPaginator();
          this.updatePaginatedData();
        } else {
          this.gradeTypes = [];
          this.apiErrorMessages = res.errorMasseges?.length
            ? [...res.errorMasseges]
            : ['تعذر تحميل بنود الدرجات'];
          this.updatePaginatedData();
        }
        this.isLoading = false;
      },
      error: (err: HttpErrorResponse) => {
        this.applyHttpError(err);
        this.gradeTypes = [];
        this.updatePaginatedData();
        this.isLoading = false;
      },
    });
  }

  edit(gradeType: GradeType): void {
    this.editingGradeTypeId = gradeType.gradeTypeID;
    this.apiErrorMessages = [];
    this.form.patchValue({
      name: gradeType.name,
      maxGrade: gradeType.maxGrade,
      isActive: gradeType.isActive,
    });
  }

  cancelEdit(): void {
    this.editingGradeTypeId = null;
    this.apiErrorMessages = [];
    this.form.reset({ name: '', maxGrade: null, isActive: true });
  }

  submit(): void {
    this.apiErrorMessages = [];
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { name, maxGrade, isActive } = this.form.value;
    const payload: Partial<GradeType> = {
      name,
      maxGrade: Number(maxGrade),
      isActive: !!isActive,
    };

    if (this.editingGradeTypeId != null) {
      this.gradeTypeService
        .updateGradeType(this.editingGradeTypeId, {
          ...payload,
          gradeTypeID: this.editingGradeTypeId,
        })
        .subscribe({
          next: res => this.handleMutationResponse(res, 'تم تحديث البند'),
          error: (err: HttpErrorResponse) => this.handleMutationHttpError(err),
        });
    } else {
      this.gradeTypeService.createGradeType(payload).subscribe({
        next: res => this.handleMutationResponse(res, 'تم إضافة البند'),
        error: (err: HttpErrorResponse) => this.handleMutationHttpError(err),
      });
    }
  }

  deleteGradeType(gradeType: GradeType): void {
    this.confirmationService.confirm({
      message: 'هل أنت متأكد من حذف هذا البند؟',
      header: 'تأكيد الحذف',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'نعم',
      rejectLabel: 'لا',
      accept: () => {
        this.gradeTypeService.deleteGradeType(gradeType.gradeTypeID).subscribe({
          next: res => {
            if (!res.isSuccess) {
              this.apiErrorMessages = res.errorMasseges?.length
                ? [...res.errorMasseges]
                : ['فشل الحذف'];
              this.toastr.error(this.apiErrorMessages[0]);
              return;
            }
            this.apiErrorMessages = [];
            this.toastr.success('تم حذف البند');
            this.cancelEdit();
            this.getAllGradeTypes();
          },
          error: (err: HttpErrorResponse) => {
            this.applyHttpError(err);
            this.toastr.error(this.apiErrorMessages[0] || 'فشل الحذف');
          },
        });
      },
    });
  }

  updatePaginatedData(): void {
    const start = this.first;
    const end = this.first + this.rows;
    this.paginatedGradeTypes = this.gradeTypes.slice(start, end);
  }

  onPageChange(event: PaginatorState): void {
    this.first = event.first ?? 0;
    this.rows = event.rows ?? 4;
    this.updatePaginatedData();
  }

  toggleIsActive(gradeType: GradeType): void {
    const next = !gradeType.isActive;
    const payload: Partial<GradeType> = {
      gradeTypeID: gradeType.gradeTypeID,
      name: gradeType.name,
      maxGrade: gradeType.maxGrade,
      isActive: next,
    };
    this.gradeTypeService.updateGradeType(gradeType.gradeTypeID, payload).subscribe({
      next: res => {
        if (!res.isSuccess) {
          this.toastr.error(res.errorMasseges?.[0] ?? 'فشل تحديث الحالة');
          return;
        }
        gradeType.isActive = next;
      },
      error: (err: HttpErrorResponse) => {
        const msg = this.messageFromHttpError(err);
        this.toastr.error(msg);
      },
    });
  }

  private handleMutationResponse(res: ApiResponse<unknown>, successMsg: string): void {
    if (!res.isSuccess) {
      this.apiErrorMessages = res.errorMasseges?.length
        ? [...res.errorMasseges]
        : ['تعذر إكمال العملية'];
      this.toastr.error(this.apiErrorMessages[0]);
      return;
    }
    this.apiErrorMessages = [];
    this.toastr.success(successMsg);
    this.cancelEdit();
    this.getAllGradeTypes();
  }

  private handleMutationHttpError(err: HttpErrorResponse): void {
    this.applyHttpError(err);
    this.toastr.error(this.apiErrorMessages[0] || 'تعذر إكمال العملية');
  }

  private applyHttpError(err: HttpErrorResponse): void {
    this.apiErrorMessages = [this.messageFromHttpError(err)];
  }

  private messageFromHttpError(err: HttpErrorResponse): string {
    const body = err.error as ApiResponse<unknown> | string | null | undefined;
    if (body && typeof body === 'object' && Array.isArray(body.errorMasseges) && body.errorMasseges.length) {
      return body.errorMasseges[0];
    }
    if (typeof body === 'string' && body.trim()) {
      return body;
    }
    return 'تعذر الاتصال بالخادم. تحقق من الشبكة أو الصلاحيات.';
  }

  private clampPaginator(): void {
    if (this.gradeTypes.length === 0) {
      this.first = 0;
      return;
    }
    const lastPageFirst = Math.floor((this.gradeTypes.length - 1) / this.rows) * this.rows;
    if (this.first > lastPageFirst) {
      this.first = lastPageFirst;
    }
  }
}
