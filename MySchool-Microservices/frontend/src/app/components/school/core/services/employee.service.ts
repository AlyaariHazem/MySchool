import { inject, Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { HttpParams } from '@angular/common/http';

import { BackendAspService } from '../../../../ASP.NET/backend-asp.service';
import { Employee } from '../models/employee.model';
import { ApiResponse } from '../../../../core/models/response.model';

@Injectable({
  providedIn: 'root'
})
export class EmployeeService {

  private API = inject(BackendAspService);

  getAllEmployees(): Observable<ApiResponse<Employee[]>> {
    return this.API.getRequest<Employee[]>('Employee');
  }

  addEmployee(newEmployee: Employee): Observable<ApiResponse<Employee>> {
    return this.API.postRequest<Employee>('Employee', newEmployee);
  }

  /**
   * PUT must be exactly /api/Employee (no path id). Identity is in the JSON body as employeeID.
   */
  updateEmployee(payload: Record<string, unknown>): Observable<ApiResponse<Employee>> {
    const url = `${this.API.baseUrl}/Employee`;
    return this.API.http.put<ApiResponse<Employee>>(url, payload).pipe(
      catchError((error) => {
        console.error('PUT Employee (no id in URL):', url, error);
        return throwError(() => error);
      })
    );
  }

  /**
   * Archives (deactivates) the employee for the school year — same as DELETE, prefer this for clarity.
   * POST /api/Employee/{id}/archive
   */
  archiveEmployee(id: number, jobType: string, body?: { yearId?: number; exitReason?: string; notes?: string }): Observable<void> {
    const payload = { jobType, ...body };
    return this.API.http
      .post(`${this.API.baseUrl}/Employee/${id}/archive`, payload, { responseType: 'text' })
      .pipe(map(() => undefined));
  }

  /** POST /api/Employee/rollover/continue — copy selected staff into target year as Active. */
  rolloverContinue(request: {
    sourceYearId: number;
    targetYearId: number;
    teacherIds?: number[];
    managerIds?: number[];
    schoolStaffIds?: number[];
  }): Observable<void> {
    return this.API.http
      .post(`${this.API.baseUrl}/Employee/rollover/continue`, request, { responseType: 'text' })
      .pipe(map(() => undefined));
  }

  /** DELETE /api/Employee/{id}?jobType=... — archives for current year (legacy path; prefer archiveEmployee). */
  deleteEmployee(id: number, jobType: string): Observable<void> {
    const params = new HttpParams().set('jobType', jobType);
    return this.API.http
      .delete(`${this.API.baseUrl}/Employee/${id}`, { params, responseType: 'text' })
      .pipe(map(() => undefined));
  }

  /**
   * POST /api/Employee/page — paged teachers + managers (unified staff list).
   * GenericQueryRequest uses zero-based pageIndex; filters may include yearId for academic year scope.
   */
  getEmployeesPage(pageNumber: number = 1, pageSize: number = 10, filters: Record<string, string> = {}): Observable<any> {
    const filtersFlat: Record<string, string> = {};
    Object.entries(filters).forEach(([key, value]) => {
      if (value && value.trim() !== '') {
        filtersFlat[key] = value;
      }
    });

    const requestBody = {
      pageIndex: Math.max(0, pageNumber - 1),
      pageSize: pageSize,
      filters: filtersFlat,
      orders: {} as Record<string, number>
    };

    return this.API.http.post<any>(`${this.API.baseUrl}/Employee/page`, requestBody).pipe(
      map(response => {
        const data = response.result ?? response;
        const raw = data.data ?? data.Data ?? [];
        const pageIndexZero = data.pageNumber ?? data.PageNumber ?? 0;
        return {
          data: raw,
          pageNumber: pageIndexZero + 1,
          pageSize: data.pageSize ?? data.PageSize ?? pageSize,
          totalCount: data.totalCount ?? data.TotalCount ?? 0,
          totalPages: data.totalPages ?? data.TotalPages ?? 0
        };
      }),
      catchError(error => {
        console.error('Error fetching paginated employees (Employee/page):', error);
        throw error;
      })
    );
  }
}