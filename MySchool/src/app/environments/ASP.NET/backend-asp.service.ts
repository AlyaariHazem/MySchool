import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, map, Observable, throwError } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class BackendAspService {

  public baseUrl = 'https://localhost:7258/api';

  constructor(public http: HttpClient, public router: Router) { }

  getRequest<T>(name: string): Observable<T> {
    return this.http.get<{ result: T }>(`${this.baseUrl}/${name}`).pipe(
      map(response => response.result),
      catchError(error => {
        throw error;
      })
    );
  }

  postRequest<T>(name: string, data: any): Observable<T> {
    return this.http.post<{ result: T }>(`${this.baseUrl}/${name}`, data).pipe(
      map(response => response.result),
      catchError(error => {
        console.error("Error when adding:", error);
        throw error;
      })
    );
  }

  putRequest<T>(name: string, data: any): Observable<T> {
    return this.http.put<T>(`${this.baseUrl}/${name}`, data);
  }

  deleteRequest<T>(name: string): Observable<T> {
    return this.http.delete<T>(`${this.baseUrl}/${name}`).pipe(
      catchError(err => {
        console.error("Error when Delete:", err);
        throw err;
      })
    );
  }

  patchRequest<T>(name: string, body: any): Observable<T> {
    return this.http.patch<T>(`${this.baseUrl}/${name}`, body).pipe(
      catchError(error => {
        console.error("Error with partial update:", error);
        throw error;
      })
    );
  }
  uploadFile(file: File, studentId: number, voucherID: number = 0): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('voucherID', voucherID.toString());
    formData.append('studentId', studentId.toString());

    return this.http.post(`${this.baseUrl}/File/uploadImage`, formData).pipe(
      catchError(error => {
        console.error("Error uploading File:", error);
        return throwError(() => error);
      })
    );
  }
  uploadFiles(files: File[], studentId: number, voucherID: number = 0): Observable<any> {
    const formData = new FormData();
    files.forEach(file => formData.append('files', file));
    formData.append('studentId', studentId.toString());
    formData.append('voucherID', voucherID.toString());

    return this.http.post(`${this.baseUrl}/File/uploadAttachments`, formData).pipe(
      catchError(error => {
        console.error("Error uploading Files:", error);
        return throwError(() => error);
      })
    );
  }
}
