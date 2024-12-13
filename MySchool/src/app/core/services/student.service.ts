import { inject, Injectable } from '@angular/core';
import { catchError, Observable } from 'rxjs';

import { AddStudent } from '../models/students.model';
import { BackendAspService } from '../../environments/ASP.NET/backend-asp.service';

@Injectable({
    providedIn: 'root'
})
export class StudentService {
    private API = inject(BackendAspService);

    addStudent(student: AddStudent): Observable<any> {
        return this.API.http.post(`${this.API.baseUrl}/Students`, student).pipe(
            catchError(error => {
                console.error("Error adding student:", error);
                throw error;
            })
        );  
    }
    MaxStudentID():Observable<any>{
        return this.API.http.get(`${this.API.baseUrl}/students/MaxValue`);
    }
}
