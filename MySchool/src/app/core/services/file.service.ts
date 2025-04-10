import { inject, Injectable } from '@angular/core';
import { catchError, Observable, throwError } from 'rxjs';

import { BackendAspService } from '../../environments/ASP.NET/backend-asp.service';

@Injectable({
  providedIn: 'root'
})
export class FileService {
  private API = inject(BackendAspService);
  constructor() { }
  // Upload student images
  uploadFile(files: File[], studentId: number, voucherID: number = 0): Observable<any> {
    const formData = new FormData();
    files.forEach(file => formData.append('files', file));
    formData.append('studentId', studentId.toString());
    formData.append('voucherID', voucherID.toString());

    return this.API.http.post(`${this.API.baseUrl}/File/uploadAttachments`, formData).pipe(
      catchError(error => {
        console.error("Error uploading Files:", error);
        return throwError(() => error);
      })
    );
  }
}

