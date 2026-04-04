import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BackendAspService } from '../../../../ASP.NET/backend-asp.service';
import { GradeType } from '../models/gradeType.model';
import { ApiResponse } from '../../../../core/models/response.model';

@Injectable({
  providedIn: 'root'
})
export class GradeTypeService {

  private API = inject(BackendAspService);
  
  getAllGradeType(): Observable<ApiResponse<GradeType[]>> {
    return this.API.getRequest<GradeType[]>('GradeTypes');
  }

  createGradeType(payload: Partial<GradeType>): Observable<ApiResponse<GradeType>> {
    return this.API.postRequest<GradeType>('GradeTypes', payload);
  }

  updateGradeType(id: number, payload: Partial<GradeType>): Observable<ApiResponse<unknown>> {
    return this.API.putRequest<unknown>(`GradeTypes/${id}`, payload);
  }

  deleteGradeType(id: number): Observable<ApiResponse<unknown>> {
    return this.API.deleteRequest<unknown>(`GradeTypes/${id}`);
  }
}
