import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpResponse } from '@angular/common/http';
import { Observable, catchError, map, throwError } from 'rxjs';
import { School } from '../models/school.modul';
import { BackendAspService } from '../../ASP.NET/backend-asp.service';

/** Request body for POST api/School/page (backend GenericQueryRequest, camelCase JSON). */
export interface SchoolQueryRequest {
  filters?: Record<string, string>;
  orders?: Record<string, number>;
  pageIndex: number;
  pageSize: number;
}

/** Response from POST api/School/page (backend PagedResult, camelCase JSON). */
export interface PagedResult<T> {
  data: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

@Injectable({
  providedIn: 'root',
})
export class SchoolService {
  private ApI = inject(BackendAspService);
  private http = inject(HttpClient);
  result: School[] = [];

  constructor() { }

  getAllSchools(): Observable<School[]> {
    return this.http.get<{ result: School[] }>(`${this.ApI.baseUrl}/School`).pipe(
      map(response => response.result),
      catchError((error) => {
        return throwError(() => new Error(error.message));
      })
    )
  }

  /** POST api/School/page — generic CRUD paged list (pageIndex is zero-based). */
  getSchoolsPage(
    request: SchoolQueryRequest,
  ): Observable<PagedResult<School>> {
    const body: SchoolQueryRequest = {
      filters: request.filters ?? {},
      orders: request.orders ?? {},
      pageIndex: request.pageIndex,
      pageSize: request.pageSize,
    };
    return this.http
      .post<PagedResult<School>>(`${this.ApI.baseUrl}/School/page`, body)
      .pipe(
        catchError((error: HttpErrorResponse) => throwError(() => error)),
      );
  }
  getSchoolByID(id:number): Observable<School> {
    return this.http.get<{result: School}>(`${this.ApI.baseUrl}/School/${id}`).pipe(
      map(response => response.result),
      catchError((error) => {
        return throwError(() => new Error(error.message));
      })
    )
  }

  addSchool(school: School): Observable<School> {
    return this.http.post<School>(`${this.ApI.baseUrl}/School`, school).pipe(
      catchError((error) => {
        return throwError(() => new Error(error.message));
      })
    );
  }

  updateSchool(id: number, school: School): Observable<any> {
    return this.http.put(`${this.ApI.baseUrl}/School`, school).pipe(
      map(res => res),
      catchError((error) => {
        return throwError(() => new Error(error.message));
      })
    );
  }

  /** Accepts 204 No Content (generic CRUD) or empty 200; rethrows HttpErrorResponse for detailed UI messages. */
  deleteSchool(id: number): Observable<void> {
    return this.http
      .delete(`${this.ApI.baseUrl}/School/${id}`, { observe: 'response' })
      .pipe(
        map((_res: HttpResponse<unknown>) => void 0),
        catchError((error: HttpErrorResponse) => throwError(() => error)),
      );
  }
}
