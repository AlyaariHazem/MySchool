import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, Observable, tap, throwError } from 'rxjs';
import { ToastrService } from 'ngx-toastr';

import { environment } from '../../environments/environment';
import { ApiResponse } from '../core/models/response.model';

@Injectable({
  providedIn: 'root'
})
export class BackendAspService {

  public baseUrl = environment.baseUrl;

  constructor(public http: HttpClient, public router: Router, private toastr: ToastrService) { }

  getRequest<T>(name: string): Observable<ApiResponse<T>> {
    return this.http.get<ApiResponse<T>>(`${this.baseUrl}/${name}`).pipe(
      catchError(error => {
        console.error('HTTP GET error:', error);
        return throwError(() => error);
      })
    );
  }

  getRequestByID<T>(name: string, id1: number, id2: number): Observable<ApiResponse<T>> {
    return this.http.get<ApiResponse<T>>(`${this.baseUrl}/${name}/${id1}/${id2}`).pipe(
      catchError(error => {
        console.error("GET by ID error:", error);
        return throwError(() => error);
      })
    );
  }

  postRequest<T>(name: string, data: any): Observable<ApiResponse<T>> {
    return this.http.post<ApiResponse<T>>(`${this.baseUrl}/${name}`, data).pipe(
      catchError(error => {
        console.error("POST error:", error);
        return throwError(() => error);
      })
    );
  }


  putRequest<T>(name: string, data: any): Observable<ApiResponse<T>> {
    return this.http.put<ApiResponse<T>>(`${this.baseUrl}/${name}`, data).pipe(
      catchError(error => {
        console.error("PUT error:", error);
        return throwError(() => error);
      })
    );
  }

  deleteRequest<T>(name: string): Observable<ApiResponse<T>> {
    return this.http.delete<ApiResponse<T>>(`${this.baseUrl}/${name}`).pipe(
      tap(res => {
        if (res.isSuccess) {
          this.toastr.success(res.result);
        } else {
          this.toastr.error(res.errorMasseges[0]);
        }
      }),
      catchError(err => {
        console.error("Error when Delete:", err);
        throw err;
      })
    );
  }

  patchRequest<T>(name: string, body: any): Observable<ApiResponse<T>> {
    return this.http.patch<ApiResponse<T>>(`${this.baseUrl}/${name}`, body).pipe(
      catchError(error => {
        console.error("PATCH error:", error);
        return throwError(() => error);
      })
    );
  }

  putRequestWithToParms<T>(name: string, id1: number, id2: number, data: any): Observable<ApiResponse<T>> {
    return this.http.put<ApiResponse<T>>(`${this.baseUrl}/${name}/${id1}/${id2}`, data).pipe(
      catchError(error => {
        console.error("PUT with 2 params error:", error);
        return throwError(() => error);
      })
    );
  }

  uploadFile(file: File, folderName: string, itemId: number): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('itemId', itemId.toString());
    formData.append('folderName', folderName.toString());

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
    formData.append('voucherID', voucherID.toString());
    formData.append('studentId', studentId.toString());

    return this.http.post(`${this.baseUrl}/File/uploadFiles`, formData).pipe(
      catchError(error => {
        console.error("Error uploading Files:", error);
        return throwError(() => error);
      })
    );
  }
}
