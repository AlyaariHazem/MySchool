import { inject, Injectable } from '@angular/core';
import { BackendAspService } from '../../../../ASP.NET/backend-asp.service';
import { catchError, Observable } from 'rxjs';
import { Employee } from '../models/employee.model';
import { HttpParams } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class EmployeeService {

  API = inject(BackendAspService);
  getAllEmployees(): Observable<Employee[]> {
    return this.API.getRequest<Employee[]>('Employee').pipe(
      catchError(err => {
        console.error('Error fetching employees:', err);
        return [];
      })
    );
  }
  addEmployee(newEmployee: Employee): Observable<Employee> {
    return this.API.postRequest<Employee>('Employee', newEmployee);
  }
  updateEmployee(id:number,employee: Employee): Observable<Employee> {
    return this.API.putRequest<Employee>(`Employee/${id}`, employee);
  }

  deleteEmployee(id: number, jobType: string) {
    const params = new HttpParams().set('jobType', jobType);
  
    return this.API.http.delete(`${this.API.baseUrl}/Employee/${id}`, { params });
  }
  
}
