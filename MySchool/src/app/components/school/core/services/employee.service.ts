import { inject, Injectable } from '@angular/core';
import { BackendAspService } from '../../../../ASP.NET/backend-asp.service';
import { catchError, Observable } from 'rxjs';
import { Employee } from '../models/employee.model';

@Injectable({
  providedIn: 'root'
})
export class EmployeeService {

  API=inject(BackendAspService);
  getAllEmployees():Observable<Employee[]> {
    return this.API.getRequest<Employee[]>('Employee').pipe(
      catchError(err => {
        console.error('Error fetching employees:', err);
        return [];
      })
    );
  }
}
