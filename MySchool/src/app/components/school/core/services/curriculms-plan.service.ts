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

  getCurriculmPlanById(yearID: number, teacherID: number, classID: number, divisionID: number, subjectID: number, termID: number): Observable<ApiResponse<CurriculmsPlans>> {
    return this.API.getRequest<CurriculmsPlans>(`CoursePlans/${yearID}/${teacherID}/${classID}/${divisionID}/${subjectID}/${termID}`);
  }

  addCurriculmPlan(newCurriculm: CurriculmsPlan): Observable<ApiResponse<string>> {
    return this.API.postRequest<string>("CoursePlans", newCurriculm);
  }

  updateCurriculmPlan(oldYearID: number, oldTeacherID: number, oldClassID: number, oldDivisionID: number, oldSubjectID: number, oldTermID: number, updatedCurriculm: CurriculmsPlan): Observable<ApiResponse<string>> {
    return this.API.putRequest<string>(`CoursePlans/${oldYearID}/${oldTeacherID}/${oldClassID}/${oldDivisionID}/${oldSubjectID}/${oldTermID}`, updatedCurriculm);
  }

  deleteCurriculmPlan(yearID: number, teacherID: number, classID: number, divisionID: number, subjectID: number, termID: number): Observable<ApiResponse<string>> {
    // POST avoids 404s from some stacks on DELETE with long paths; same semantics as DELETE api/CoursePlans/...
    return this.API.postRequest<string>('CoursePlans/delete', {
      yearID,
      teacherID,
      classID,
      divisionID,
      subjectID,
      termID
    });
  }
}
