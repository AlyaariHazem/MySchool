import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { BackendAspService } from '../../../../environments/ASP.NET/backend-asp.service';
import { Subjects } from '../models/subjects.model';
import { Paginates } from '../models/Pagination.model';

@Injectable({
  providedIn: 'root'
})
export class SubjectService {
  private API = inject(BackendAspService);

  getPaginatedSubjects(pageNumber: number, pageSize: number): Observable<Paginates> {
    return this.API.getRequest<Paginates>(`Subject/paginated?pageNumber=${pageNumber}&pageSize=${pageSize}`);
  }
  getAllSubjects(): Observable<Subjects[]> {
    return this.API.getRequest<Subjects[]>("Subject/AllSubjects");
  }
  addSubject(newSubject: Subjects): Observable<Subjects> {
    return this.API.postRequest<Subjects>("Subject", newSubject);
  }
  deleteSubject(id: number): Observable<void> {
    return this.API.deleteRequest<void>(`Subject/${id}`);//call it here?
  }
  updateSubject(id: number, subject: Subjects): Observable<Subjects> {
    return this.API.putRequest<Subjects>(`Subject/${id}`, subject);
  }

}
