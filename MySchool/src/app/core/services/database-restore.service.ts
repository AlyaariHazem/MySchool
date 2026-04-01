import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { catchError, Observable, throwError } from 'rxjs';

import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/response.model';
import { DatabaseRestoreResultDTO } from '../models/database-restore.model';

@Injectable({
  providedIn: 'root',
})
export class DatabaseRestoreService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.baseUrl;

  /**
   * POST multipart: file (.bak), optional databaseName (base name; server appends unique suffix).
   */
  restoreFromBackup(file: File, databaseName?: string | null): Observable<ApiResponse<DatabaseRestoreResultDTO>> {
    const formData = new FormData();
    formData.append('file', file, file.name);
    if (databaseName != null && databaseName.trim() !== '') {
      formData.append('databaseName', databaseName.trim());
    }

    return this.http
      .post<ApiResponse<DatabaseRestoreResultDTO>>(`${this.baseUrl}/DatabaseRestore/restore`, formData)
      .pipe(
        catchError((err) => {
          console.error('Database restore request failed:', err);
          return throwError(() => err);
        }),
      );
  }
}
