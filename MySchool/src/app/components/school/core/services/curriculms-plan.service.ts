import { inject, Injectable } from '@angular/core';
import { BackendAspService } from '../../../../environments/ASP.NET/backend-asp.service';
import { Observable } from 'rxjs';
import { CurriculmsPlans, CurriculmsPlanSubject } from '../models/curriculmsPlans.model';

@Injectable({
  providedIn: 'root'
})
export class CurriculmsPlanService {

  private API=inject(BackendAspService);
  getAllCurriculmPlan():Observable<CurriculmsPlans[]> {
    return this.API.getRequest<CurriculmsPlans[]>("CoursePlans");
  }
  getAllCurriculmPlanSubjects():Observable<CurriculmsPlanSubject[]> {
    return this.API.getRequest<CurriculmsPlanSubject[]>("CoursePlans/subjects");
  }
  getCurriculmPlanById(subID: number,ClassID:number): Observable<any> {
    return this.API.getRequestByID<any>("CoursePlans",subID,ClassID);
  }
  addCurriculmPlan(newCurriculm: any): Observable<any> {
    return this.API.postRequest<any>("CoursePlans", newCurriculm);
  }
  updateCurriculmPlan(subID: number,classID: number, updatedCurriculm: any): Observable<string> {
    return this.API.putRequestWithToParms<string>(`CoursePlans`,subID,classID, updatedCurriculm);
  }
}
