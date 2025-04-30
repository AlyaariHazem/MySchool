import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
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
}