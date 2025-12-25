import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { BackendAspService } from '../../../../ASP.NET/backend-asp.service';
import { Employee } from '../models/employee.model';
import { ApiResponse } from '../../../../core/models/response.model';
import { HttpParams } from '@angular/common/http';

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

  updateEmployee(id: number, employee: Employee): Observable<ApiResponse<Employee>> {
    return this.API.putRequest<Employee>(`Employee/${id}`, employee);
  }

  deleteEmployee(id: number, jobType: string): Observable<ApiResponse<any>> {
    const params = new HttpParams().set('jobType', jobType);
    return this.API.http.delete<ApiResponse<any>>(`${this.API.baseUrl}/Employee/${id}`, { params });
  }

  getEmployeesPage(pageNumber: number = 1, pageSize: number = 10, filters: Record<string, string> = {}): Observable<any> {
    // Transform filters from Record<string, string> to backend FilterRequest format
    const filtersDict: Record<string, { value: string }> = {};
    Object.entries(filters).forEach(([key, value]) => {
      if (value && value.trim() !== '') {
        filtersDict[key] = { value: value };
      }
    });

    const requestBody = {
      pageNumber: pageNumber,
      pageSize: pageSize,
      filters: filtersDict
    };

    return this.API.http.post<any>(`${this.API.baseUrl}/Teacher/page`, requestBody).pipe(
      map(response => {
        // Handle both wrapped (APIResponse) and unwrapped responses
        const data = response.result || response;
        // Map PagedResult properties (C# uses PascalCase, but JSON might be camelCase)
        return {
          data: data.Data || data.data || [],
          pageNumber: data.PageNumber ?? data.pageNumber ?? pageNumber,
          pageSize: data.PageSize ?? data.pageSize ?? pageSize,
          totalCount: data.TotalCount ?? data.totalCount ?? 0,
          totalPages: data.TotalPages ?? data.totalPages ?? 0
        };
      }),
      catchError(error => {
        console.error("Error fetching paginated Employee Details:", error);
        throw error;
      })
    );
  }
}