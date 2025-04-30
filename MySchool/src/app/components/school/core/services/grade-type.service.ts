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
}
