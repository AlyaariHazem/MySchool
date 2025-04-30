import { inject, Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { BackendAspService } from '../../../../ASP.NET/backend-asp.service';
import { Teachers } from '../models/teacher.model';
import { ApiResponse } from '../../../../core/models/response.model';

@Injectable({
  providedIn: 'root'
})
export class TeacherService {

  private API = inject(BackendAspService);

  getAllTeacher(): Observable<ApiResponse<Teachers[]>> {
    return this.API.getRequest<Teachers[]>("Teacher").pipe(
      map((res: ApiResponse<Teachers[]>) => {
        if (res.isSuccess && res.result) {
          res.result = res.result.map((teacher: Teachers) => {
            teacher.fullName = `${teacher.firstName} ${teacher.middleName ?? ''} ${teacher.lastName}`.trim();
            return teacher;
          });
        }
        return res;
      })
    );
  }

  getTeacherById(id: number): Observable<ApiResponse<Teachers>> {
    return this.API.getRequest<Teachers>(`Teacher/${id}`);
  }

  addTeacher(newTeacher: Teachers): Observable<ApiResponse<Teachers>> {
    return this.API.postRequest<Teachers>('Teacher', newTeacher);
  }

  updateTeacher(id: number, updatedTeacher: Teachers): Observable<ApiResponse<Teachers>> {
    return this.API.putRequest<Teachers>(`Teacher/${id}`, updatedTeacher);
  }

  deleteTeacher(id: number): Observable<ApiResponse<any>> {
    return this.API.deleteRequest<any>(`Teacher/${id}`);
  }
}
