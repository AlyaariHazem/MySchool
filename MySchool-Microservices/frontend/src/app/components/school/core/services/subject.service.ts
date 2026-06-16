import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BackendAspService } from '../../../../ASP.NET/backend-asp.service';
import { Subjects } from '../models/subjects.model';
import { Paginates } from '../models/Pagination.model';
import { ApiResponse } from '../../../../core/models/response.model';

@Injectable({
  providedIn: 'root'
})
export class SubjectService {
  private API = inject(BackendAspService);

  getPaginatedSubjects(pageNumber: number, pageSize: number): Observable<ApiResponse<Paginates>> {
    return this.API.getRequest<Paginates>(`Subject/paginated?pageNumber=${pageNumber}&pageSize=${pageSize}`);
  }

  getAllSubjects(): Observable<ApiResponse<Subjects[]>> {
    return this.API.getRequest<Subjects[]>("Subject/AllSubjects");
  }

  addSubject(newSubject: Subjects): Observable<ApiResponse<Subjects>> {
    return this.API.postRequest<Subjects>("Subject", newSubject);
  }

  deleteSubject(id: number): Observable<ApiResponse<any>> {
    return this.API.deleteRequest<any>(`Subject/${id}`);
  }

  updateSubject(id: number, subject: Subjects): Observable<ApiResponse<Subjects>> {
    return this.API.putRequest<Subjects>(`Subject/${id}`, subject);
  }
}
