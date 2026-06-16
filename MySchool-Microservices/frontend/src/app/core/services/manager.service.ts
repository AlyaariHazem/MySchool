import { Injectable, inject } from '@angular/core';
import {
  HttpClient,
  HttpErrorResponse,
  HttpResponse,
} from '@angular/common/http';
import { catchError, map, Observable, throwError } from 'rxjs';

import { BackendAspService } from '../../ASP.NET/backend-asp.service';
import { manager } from '../../components/admin/core/models/manager.model';

/** Request body for POST api/Manager/page. */
export interface ManagerQueryRequest {
  filters?: Record<string, string>;
  orders?: Record<string, number>;
  pageIndex: number;
  pageSize: number;
}

/** Response from POST api/Manager/page. */
export interface PagedResult<T> {
  data: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

/** Body for PUT api/Manager (generic CRUD update). */
export interface ManagerDto {
  managerID: number;
  fullName: {
    firstName: string;
    middleName?: string;
    lastName: string;
  };
  hireDate: string;
  schoolName: string;
  tenantID?: number;
  tenantName?: string;
  userName: string;
  email?: string;
  userType: string;
  phoneNumber?: string | number;
}

@Injectable({
  providedIn: 'root',
})
export class ManagerService {
  private API = inject(BackendAspService);
  private http = inject(HttpClient);

  getAllManagers(): Observable<any> {
    return this.http.get<any>(`${this.API.baseUrl}/Manager`).pipe(
      map((response) => response.result),
      catchError((error) => {
        console.error('Error fetching managers:', error);
        return throwError(() => error);
      }),
    );
  }

  /** POST api/Manager/page — paged list (zero-based pageIndex). */
  getManagersPage(request: ManagerQueryRequest): Observable<PagedResult<any>> {
    const body: ManagerQueryRequest = {
      filters: request.filters ?? {},
      orders: request.orders ?? {},
      pageIndex: request.pageIndex,
      pageSize: request.pageSize,
    };
    return this.http
      .post<PagedResult<any>>(`${this.API.baseUrl}/Manager/page`, body)
      .pipe(
        catchError((error: HttpErrorResponse) => throwError(() => error)),
      );
  }

  getManagerById(id: number): Observable<any> {
    return this.http.get<any>(`${this.API.baseUrl}/Manager/${id}`).pipe(
      map((response) => response.result),
      catchError((error) => {
        console.error('Error fetching manager:', error);
        return throwError(() => error);
      }),
    );
  }

  /** DELETE api/Manager/{id} — 204 No Content from generic CRUD. */
  deleteManager(id: number): Observable<void> {
    return this.http
      .delete(`${this.API.baseUrl}/Manager/${id}`, { observe: 'response' })
      .pipe(
        map((_res: HttpResponse<unknown>) => void 0),
        catchError((error: HttpErrorResponse) => throwError(() => error)),
      );
  }

  /** POST api/Manager/add — create (AddManagerDTO). */
  addManager(managerPayload: manager): Observable<any> {
    return this.http
      .post<any>(`${this.API.baseUrl}/Manager/add`, managerPayload)
      .pipe(
        map((response) => response.result),
        catchError((error) => {
          console.error('Error adding manager:', error);
          return throwError(() => error);
        }),
      );
  }

  /** PUT api/Manager — generic update (GetManagerDTO in body, includes managerID). */
  updateManager(dto: ManagerDto): Observable<any> {
    return this.http.put(`${this.API.baseUrl}/Manager`, dto).pipe(
      catchError((error: HttpErrorResponse) => throwError(() => error)),
    );
  }

}
