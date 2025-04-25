import { inject, Injectable } from '@angular/core';
import { BackendAspService } from '../../../../ASP.NET/backend-asp.service';
import { map, Observable } from 'rxjs';
import { Teachers } from '../models/teacher.model';

@Injectable({
  providedIn: 'root'
})
export class TeacherService {

  private API =inject(BackendAspService);
  getAllTeacher(): Observable<Teachers[]> {
    return this.API.getRequest<Teachers[]>("Teacher").pipe(
      map((response: Teachers[]) => {
        return response.map((teacher: Teachers) => {
          teacher.fullName = `${teacher.firstName} ${teacher.middleName===null?'':teacher.middleName} ${teacher.lastName}`;
          return teacher;
        });
      })
    );
  }
  getTeacherById(id: number): Observable<any> {
    return this.API.getRequest<any>(`Teacher/${id}`); 
  }
  addTeacher(newTeacher: any): Observable<any> {
    return this.API.postRequest<any>("Teacher", newTeacher);
  }
  updateTeacher(id: number, updatedTeacher: any): Observable<any> {
    return this.API.putRequest<any>(`Teacher/${id}`, updatedTeacher);
  }
  deleteTeacher(id: number): Observable<any> {
    return this.API.deleteRequest(`Teacher/${id}`);
  }
}
