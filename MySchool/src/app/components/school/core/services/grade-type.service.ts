import { inject, Injectable } from '@angular/core';
import { BackendAspService } from '../../../../ASP.NET/backend-asp.service';
import { Observable } from 'rxjs';
import { GradeType } from '../models/gradeType.model';

@Injectable({
  providedIn: 'root'
})
export class GradeTypeService {

  API = inject(BackendAspService);
  getAllGradeType(): Observable<GradeType[]> {
    return this.API.getRequest<GradeType[]>("GradeTypes");
  }
  
}
