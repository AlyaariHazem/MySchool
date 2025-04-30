import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BackendAspService } from '../../../../ASP.NET/backend-asp.service';
import { CurriculmsPlan, CurriculmsPlans, CurriculmsPlanSubject } from '../models/curriculmsPlans.model';
import { ApiResponse } from '../../../../core/models/response.model';

@Injectable({
  providedIn: 'root'
})
export class CurriculmsPlanService {

  private API = inject(BackendAspService);

  getAllCurriculmPlan(): Observable<ApiResponse<CurriculmsPlans[]>> {
    return this.API.getRequest<CurriculmsPlans[]>("CoursePlans");
  }

  getAllCurriculmPlanSubjects(): Observable<ApiResponse<CurriculmsPlanSubject[]>> {
    return this.API.getRequest<CurriculmsPlanSubject[]>("CoursePlans/subjects");
  }

  getCurriculmPlanById(subID: number, classID: number): Observable<ApiResponse<CurriculmsPlans>> {
    return this.API.getRequestByID<CurriculmsPlans>("CoursePlans", subID, classID);
  }

  addCurriculmPlan(newCurriculm: CurriculmsPlan): Observable<ApiResponse<string>> {
    return this.API.postRequest<string>("CoursePlans", newCurriculm);
  }

  updateCurriculmPlan(subID: number, classID: number, updatedCurriculm: CurriculmsPlans): Observable<ApiResponse<string>> {
    return this.API.putRequestWithToParms<string>("CoursePlans", subID, classID, updatedCurriculm);
  }
}
