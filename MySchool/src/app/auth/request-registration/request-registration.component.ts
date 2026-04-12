import { Component, ElementRef, inject, viewChild } from '@angular/core';
import { finalize } from 'rxjs';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';

import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { MatIconModule } from '@angular/material/icon';
import { ShardModule } from '../../shared/shard.module';
import { RegistrationRequestService } from '../../core/services/registration-request.service';
import { PublicSchoolOption } from '../../core/models/registration-request.model';

@Component({
  selector: 'app-request-registration',
  standalone: true,
  imports: [
    ShardModule,
    FormsModule,
    RouterLink,
    MatButtonModule,
    MatCardModule,
    MatDividerModule,
    MatIconModule,
  ],
  templateUrl: './request-registration.component.html',
  styleUrls: ['./request-registration.component.scss', '../../shared/styles/style-select.scss'],
})
export class RequestRegistrationComponent {
  private readonly registration = inject(RegistrationRequestService);
  private readonly toastr = inject(ToastrService);

  readonly fileInput = viewChild<ElementRef<HTMLInputElement>>('fileInput');

  schools: PublicSchoolOption[] = [];
  schoolsLoading = true;
  tenantId: number | null = null;
  userName = '';
  phoneNumber = '';
  password = '';
  confirmPassword = '';
  requestedRole: 'STUDENT' | 'GUARDIAN' | '' = '';
  fullName = '';
  gender: string = '';
  dateOfBirth: string | null = null;

  selectedFiles: File[] = [];
  readonly maxFiles = 12;
  readonly maxFileMb = 10;

  isSubmitting = false;
  submitted = false;
  /** Highlight drop zone while dragging files over it */
  fileDropActive = false;

  roleOptions = [
    { label: 'طالب', value: 'STUDENT' as const },
    { label: 'ولي أمر', value: 'GUARDIAN' as const },
  ];

  genderOptions = [
    { label: 'ذكر', value: 'ذكر' },
    { label: 'أنثى', value: 'أنثى' },
  ];

  constructor() {
    this.registration.getPublicSchools().subscribe({
      next: (list) => {
        this.schools = Array.isArray(list) ? list : [];
        this.schoolsLoading = false;
      },
      error: () => {
        this.schoolsLoading = false;
        this.toastr.error('تعذر تحميل قائمة المدارس');
      },
    });
  }

  /** Native &lt;input type="date"&gt; often makes NgForm invalid when empty; use explicit readiness for the submit button. */
  get canSubmit(): boolean {
    if (this.isSubmitting || this.schoolsLoading) {
      return false;
    }
    if (
      this.tenantId == null ||
      !this.userName?.trim() ||
      this.userName.trim().length < 3 ||
      !this.phoneNumber?.trim() ||
      !this.password ||
      this.password.length < 6 ||
      !this.confirmPassword ||
      !this.gender ||
      !this.requestedRole
    ) {
      return false;
    }
    const digits = this.phoneNumber.replace(/\D/g, '');
    if (digits.length < 8) {
      return false;
    }
    return this.password === this.confirmPassword;
  }

  openFilePicker(): void {
    this.fileInput()?.nativeElement.click();
  }

  addFilesFromList(fileList: FileList | null): void {
    if (!fileList?.length) {
      return;
    }
    const next: File[] = [...this.selectedFiles];
    const maxBytes = this.maxFileMb * 1024 * 1024;
    const allowed = /\.(pdf|jpe?g|png|webp)$/i;

    for (let i = 0; i < fileList.length; i++) {
      const f = fileList.item(i)!;
      if (f.size > maxBytes) {
        this.toastr.error(`الملف ${f.name} أكبر من ${this.maxFileMb} ميجابايت`);
        continue;
      }
      if (!allowed.test(f.name)) {
        this.toastr.error(`نوع الملف غير مسموح: ${f.name}`);
        continue;
      }
      next.push(f);
    }
    this.selectedFiles = next.slice(0, this.maxFiles);
  }

  onFilesPicked(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.addFilesFromList(input.files);
    input.value = '';
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.fileDropActive = false;
    if (event.dataTransfer?.files?.length) {
      this.addFilesFromList(event.dataTransfer.files);
    }
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.fileDropActive = true;
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    this.fileDropActive = false;
  }

  removeFile(index: number): void {
    this.selectedFiles = this.selectedFiles.filter((_, i) => i !== index);
  }

  formatFileSize(bytes: number): string {
    if (bytes < 1024) {
      return `${bytes} بايت`;
    }
    if (bytes < 1024 * 1024) {
      return `${(bytes / 1024).toFixed(1)} ك.ب`;
    }
    return `${(bytes / (1024 * 1024)).toFixed(1)} م.ب`;
  }

  submit(): void {
    if (this.isSubmitting || !this.canSubmit) {
      return;
    }
    if (!this.tenantId) {
      this.toastr.error('اختر المدرسة');
      return;
    }
    if (!this.requestedRole) {
      this.toastr.error('اختر نوع الحساب');
      return;
    }
    if (!this.gender) {
      this.toastr.error('اختر الجنس');
      return;
    }
    const digits = this.phoneNumber.replace(/\D/g, '');
    if (digits.length < 8) {
      this.toastr.error('أدخل رقم هاتف صالحاً');
      return;
    }
    if (this.password !== this.confirmPassword) {
      this.toastr.error('كلمة السر وتأكيدها غير متطابقين');
      return;
    }

    this.isSubmitting = true;
    this.registration
      .requestRegistration({
        tenantId: this.tenantId,
        userName: this.userName.trim(),
        phoneNumber: this.phoneNumber.trim(),
        password: this.password,
        confirmPassword: this.confirmPassword,
        requestedRole: this.requestedRole,
        fullName: this.fullName.trim() || null,
        gender: this.gender,
        dateOfBirth: this.dateOfBirth || null,
        files: this.selectedFiles,
      })
      .pipe(finalize(() => (this.isSubmitting = false)))
      .subscribe({
        next: () => {
          this.submitted = true;
          this.toastr.success('طلبك قيد مراجعة المدرسة');
        },
        error: (err) => {
          const msg =
            err?.error?.message ||
            err?.error?.title ||
            'تعذر إرسال الطلب';
          this.toastr.error(msg);
        },
      });
  }
}
