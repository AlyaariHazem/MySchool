import { inject, Injectable } from '@angular/core';
import { BackendAspService } from '../../../../environments/ASP.NET/backend-asp.service';
import { Curriculms } from '../models/Curriculms.model';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class CurriculmService {

  private API = inject(BackendAspService); // Dependency injection of BackendAspService
  getAllCurriculm(): Observable<Curriculms[]> {
    return this.API.getRequest<Curriculms[]>("Curriculms");
  }
  getCurriculmById(id: number): Observable<any> {
    return this.API.getRequest<any>(`Curriculms/${id}`);
  }
  addCurriculm(newCurriculm: any): Observable<any> {
    return this.API.postRequest<any>("Curriculms", newCurriculm);
  }
  updateCurriculm(subID: number,classID: number, updatedCurriculm: any): Observable<string> {
    return this.API.putRequestWithToParms<string>(`Curriculms`,subID,classID, updatedCurriculm);
  }
  deleteCurriculm(id1: number,id2:number): Observable<string> {
    return this.API.deleteRequest<string>(`Curriculms/${id1}/${id2}`);
  }
}
