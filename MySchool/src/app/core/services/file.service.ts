import { inject, Injectable } from '@angular/core';
import { catchError, Observable, throwError } from 'rxjs';

import { BackendAspService } from '../../ASP.NET/backend-asp.service';
import { HttpParams } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class FileService {
  private API = inject(BackendAspService);
  constructor() { }
  // Upload student images
  uploadFiles(files: File[], folderName: string, itemId: number = 0): Observable<any> {
    const formData = new FormData();
    files.forEach(file => formData.append('files', file));
    formData.append('folderName', folderName.toString());
    formData.append('itemId', itemId.toString());

    return this.API.http.post(`${this.API.baseUrl}/File/uploadFiles`, formData).pipe(
      catchError(error => {
        console.error("Error uploading Files:", error);
        return throwError(() => error);
      })
    );
  }
  uploadFile(file: File, folderName: string, itemId: number = 0): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('folderName', folderName.toString());
    formData.append('itemId', itemId.toString());

    return this.API.http.post(`${this.API.baseUrl}/File/uploadImage`, formData).pipe(
      catchError(error => {
        console.error("Error uploading Files:", error);
        return throwError(() => error);
      })
    );
  }
  DeleteFile(folderName: string, itemId: number = 0): Observable<any> {
    const params = new HttpParams()
      .set('folderName', folderName)
      .set('itemId', itemId.toString());
  
    return this.API.http.delete(`${this.API.baseUrl}/File/deleteFile`, { params }).pipe(
      catchError(error => {
        console.error("Error deleting file:", error);
        return throwError(() => error);
      })
    );
  }
  

}

