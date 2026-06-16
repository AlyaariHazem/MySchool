import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { BackendAspService } from '../../../../ASP.NET/backend-asp.service';
import { PagedResultDto } from '../../../../core/models/students.model';
import { Curriculms } from '../models/Curriculms.model';
import { ApiResponse } from '../../../../core/models/response.model';

export interface SimplePageRequestDto {
  pageIndex: number;
  pageSize: number;
}

@Injectable({
  providedIn: 'root'
})
export class CurriculmService {

  private API = inject(BackendAspService);

  getAllCurriculm(): Observable<ApiResponse<Curriculms[]>> {
    return this.API.getRequest<Curriculms[]>("Curriculms");
  }

  getCurriculmPage(body: SimplePageRequestDto): Observable<ApiResponse<PagedResultDto<Curriculms>>> {
    return this.API.postRequest<PagedResultDto<Curriculms>>('Curriculms/page', body);
  }

  getCurriculmById(id: number): Observable<ApiResponse<Curriculms>> {
    return this.API.getRequest<Curriculms>(`Curriculms/${id}`);
  }

  addCurriculm(newCurriculm: Curriculms): Observable<ApiResponse<Curriculms>> {
    return this.API.postRequest<Curriculms>("Curriculms", newCurriculm);
  }

  updateCurriculm(subID: number, classID: number, updatedCurriculm: Curriculms): Observable<ApiResponse<string>> {
    return this.API.putRequestWithToParms<string>(`Curriculms`, subID, classID, updatedCurriculm);
  }

  deleteCurriculm(id1: number, id2: number): Observable<ApiResponse<any>> {
    return this.API.deleteRequest<any>(`Curriculms/${id1}/${id2}`);
  }
}
