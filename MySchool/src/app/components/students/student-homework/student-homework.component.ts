import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';

import { BackendAspService } from '../../../ASP.NET/backend-asp.service';
import { FileService } from '../../../core/services/file.service';
import { HomeworkService } from '../../../core/services/homework.service';
import {
  homeworkStatusLabelAr,
  StudentHomeworkDetail,
  StudentHomeworkItem,
} from '../../../core/models/homework.model';

@Component({
  selector: 'app-student-homework',
  templateUrl: './student-homework.component.html',
  styleUrls: ['./student-homework.component.scss'],
})
export class StudentHomeworkComponent implements OnInit {
  private readonly homework = inject(HomeworkService);
  private readonly files = inject(FileService);
  private readonly api = inject(BackendAspService);
  private readonly toastr = inject(ToastrService);
  private readonly fb = inject(FormBuilder);

  readonly filterOptions: { label: string; value: string }[] = [
    { label: 'الكل', value: 'all' },
    { label: 'اليوم', value: 'today' },
    { label: 'قادمة', value: 'upcoming' },
    { label: 'متأخرة', value: 'overdue' },
    { label: 'مكتملة', value: 'completed' },
    { label: 'معلقة', value: 'pending' },
  ];

  /** For file input `accept` — matches server allowed extensions. */
  readonly homeworkFileAccept =
    '.pdf,.doc,.docx,.txt,.zip,.rar,.jpg,.jpeg,.png,.gif,.webp,application/pdf';

  filterForm = this.fb.group({
    view: ['all'],
  });

  rows: StudentHomeworkItem[] = [];
  loading = false;
  statusLabel = homeworkStatusLabelAr;

  showDetail = false;
  detail: StudentHomeworkDetail | null = null;
  loadingDetail = false;
  uploading = false;
  pendingHomeworkFile: File | null = null;
  pendingHomeworkFileName = '';

  submitForm = this.fb.group({
    answerText: [''],
    fileUrl: [''],
    fileName: [''],
  });

  ngOnInit(): void {
    this.load();
  }

  get dialogTitle(): string {
    return this.detail?.title?.trim() ? this.detail!.title : 'تفاصيل الواجب';
  }

  statusClass(status: number): string {
    switch (status) {
      case 0:
        return 'hw-status--pending';
      case 1:
        return 'hw-status--submitted';
      case 2:
        return 'hw-status--late';
      case 3:
        return 'hw-status--graded';
      case 4:
        return 'hw-status--completed';
      case 5:
        return 'hw-status--missing';
      default:
        return 'hw-status--default';
    }
  }

  load(): void {
    this.loading = true;
    const view = this.filterForm.get('view')?.value ?? 'all';
    const f = view === 'all' ? undefined : view;
    this.homework.getStudentTasks(f).subscribe({
      next: (r) => {
        this.rows = (r.result ?? []) as StudentHomeworkItem[];
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.toastr.error('تعذر تحميل الواجبات');
      },
    });
  }

  openDetail(row: StudentHomeworkItem): void {
    this.showDetail = true;
    this.loadingDetail = true;
    this.detail = null;
    this.resetPendingFile();
    this.submitForm.reset({ answerText: '', fileUrl: '', fileName: '' });
    this.homework.getStudentTask(row.homeworkTaskID).subscribe({
      next: (r) => {
        this.detail = (r.result ?? null) as StudentHomeworkDetail | null;
        this.submitForm.patchValue({
          answerText: this.detail?.answerText ?? '',
        });
        this.loadingDetail = false;
      },
      error: () => {
        this.loadingDetail = false;
        this.toastr.error('تعذر التحميل');
      },
    });
  }

  onHomeworkFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;
    this.pendingHomeworkFile = file;
    this.pendingHomeworkFileName = file?.name ?? '';
  }

  clearHomeworkFile(input: HTMLInputElement): void {
    input.value = '';
    this.pendingHomeworkFile = null;
    this.pendingHomeworkFileName = '';
  }

  private resetPendingFile(): void {
    this.pendingHomeworkFile = null;
    this.pendingHomeworkFileName = '';
  }

  /**
   * API returns physical paths under wwwroot; homework API expects public URLs.
   */
  private serverPathsToSubmissionFiles(paths: unknown): { fileUrl: string; fileName: string | null }[] {
    if (!paths || !Array.isArray(paths)) return [];
    const origin = this.api.baseUrl.replace(/\/api\/?$/, '');
    return paths.map((raw) => {
      const s = String(raw).replace(/\\/g, '/');
      const lower = s.toLowerCase();
      const u = lower.indexOf('/uploads/');
      const rel = u >= 0 ? s.slice(u) : `/uploads/${s.split('/').pop() ?? ''}`;
      const fileUrl = `${origin}${rel.startsWith('/') ? rel : `/${rel}`}`;
      const fileName = s.split('/').pop() ?? null;
      return { fileUrl, fileName };
    });
  }

  private manualLinkFiles(): { fileUrl: string; fileName: string | null }[] {
    const v = this.submitForm.getRawValue();
    if (!v.fileUrl?.trim()) return [];
    return [{ fileUrl: v.fileUrl.trim(), fileName: v.fileName?.trim() || null }];
  }

  submit(): void {
    if (!this.detail || this.uploading) return;
    const v = this.submitForm.getRawValue();
    const answerText = v.answerText?.trim() || null;
    const manual = this.manualLinkFiles();

    const send = (uploaded: { fileUrl: string; fileName: string | null }[]) => {
      const merged = [...uploaded, ...manual];
      this.homework
        .submitStudentTask(this.detail!.homeworkTaskID, {
          answerText,
          files: merged.length ? merged : undefined,
        })
        .subscribe({
          next: (r) => {
            this.detail = (r.result ?? null) as StudentHomeworkDetail | null;
            this.toastr.success('تم التسليم');
            this.resetPendingFile();
            this.load();
          },
          error: (e) => this.toastr.error(e?.error?.errorMasseges?.[0] ?? 'تعذر التسليم'),
        });
    };

    if (this.pendingHomeworkFile) {
      this.uploading = true;
      this.files
        .uploadFiles([this.pendingHomeworkFile], 'HomeworkSubmissions', this.detail.homeworkTaskID)
        .subscribe({
          next: (res) => {
            this.uploading = false;
            const paths = (res as { result?: unknown })?.result;
            send(this.serverPathsToSubmissionFiles(paths));
          },
          error: () => {
            this.uploading = false;
            this.toastr.error('تعذر رفع الملف');
          },
        });
    } else {
      send([]);
    }
  }
}
