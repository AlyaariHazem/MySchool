import { Component, ElementRef, ViewChild, inject } from '@angular/core';
import { Store } from '@ngrx/store';
import { map } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { ToastrService } from 'ngx-toastr';
import { TranslateService } from '@ngx-translate/core';

import { DatabaseRestoreService } from '../../../../core/services/database-restore.service';
import {
  DatabaseRestoreResultDTO,
  RestoreHistoryRow,
} from '../../../../core/models/database-restore.model';
import { ApiResponse } from '../../../../core/models/response.model';
import { selectLanguage } from '../../../../core/store/language/language.selectors';

@Component({
  selector: 'app-database-restore',
  templateUrl: './database-restore.component.html',
  styleUrls: ['./database-restore.component.scss', '../../../../shared/styles/style-input.scss'],
})
export class DatabaseRestoreComponent {
  private readonly restoreService = inject(DatabaseRestoreService);
  private readonly toastr = inject(ToastrService);
  private readonly translate = inject(TranslateService);
  private readonly store = inject(Store);

  readonly dir$ = this.store.select(selectLanguage).pipe(map((l) => (l === 'ar' ? 'rtl' : 'ltr')));

  @ViewChild('fileInput') fileInput?: ElementRef<HTMLInputElement>;

  selectedFile: File | null = null;
  databaseNameBase = '';
  submitting = false;

  lastResult: DatabaseRestoreResultDTO | null = null;
  history: RestoreHistoryRow[] = [];

  pickFile(): void {
    this.fileInput?.nativeElement.click();
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) {
      this.selectedFile = null;
      return;
    }
    if (!file.name.toLowerCase().endsWith('.bak')) {
      this.selectedFile = null;
      input.value = '';
      this.toastr.warning(this.translate.instant('databaseRestore.onlyBak'));
      return;
    }
    this.selectedFile = file;
  }

  clearFile(): void {
    this.selectedFile = null;
    if (this.fileInput?.nativeElement) {
      this.fileInput.nativeElement.value = '';
    }
  }

  submit(): void {
    if (!this.selectedFile || this.submitting) {
      return;
    }

    this.submitting = true;
    this.lastResult = null;

    const fileName = this.selectedFile.name;
    this.restoreService.restoreFromBackup(this.selectedFile, this.databaseNameBase || undefined).subscribe({
      next: (res) => this.handleSuccessResponse(res, fileName),
      error: (err: HttpErrorResponse) => this.handleError(err, fileName),
    });
  }

  private handleSuccessResponse(res: ApiResponse<DatabaseRestoreResultDTO>, fileName: string): void {
    this.submitting = false;

    const messages = res.errorMasseges ?? [];
    const failed = res.isSuccess === false || messages.length > 0;

    if (failed) {
      const text = messages.length ? messages.join(' ') : this.translate.instant('databaseRestore.failed');
      this.toastr.error(text);
      this.unshiftHistory({
        at: new Date(),
        success: false,
        fileName,
        errorMessages: messages.length ? messages : [text],
      });
      return;
    }

    const result = res.result as DatabaseRestoreResultDTO | undefined;
    if (!result) {
      this.toastr.error(this.translate.instant('databaseRestore.noResult'));
      this.unshiftHistory({
        at: new Date(),
        success: false,
        fileName,
        errorMessages: [this.translate.instant('databaseRestore.noResult')],
      });
      return;
    }

    this.lastResult = result;
    this.toastr.success(this.translate.instant('databaseRestore.successToast', { name: result.databaseName }));
    this.unshiftHistory({
      at: new Date(),
      success: true,
      fileName,
      databaseName: result.databaseName,
      result,
    });
    this.clearFile();
  }

  private handleError(err: HttpErrorResponse, fileName: string): void {
    this.submitting = false;

    const body = err.error as ApiResponse<unknown> | undefined;
    const fromApi = body?.errorMasseges?.filter(Boolean) ?? [];
    const msg =
      fromApi.length > 0
        ? fromApi.join(' ')
        : err.message || this.translate.instant('databaseRestore.failed');

    this.toastr.error(msg);
    this.unshiftHistory({
      at: new Date(),
      success: false,
      fileName,
      errorMessages: fromApi.length ? fromApi : [msg],
    });
  }

  private unshiftHistory(row: RestoreHistoryRow): void {
    this.history = [row, ...this.history].slice(0, 25);
  }
}
