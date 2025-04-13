import { inject, Injectable } from '@angular/core';
import { BackendAspService } from '../../../../environments/ASP.NET/backend-asp.service';
import { Observable } from 'rxjs';
import { Terms } from '../models/term.model';

@Injectable({
  providedIn: 'root'
})
export class TermService {

  private API =inject(BackendAspService);
  getAllTerm(): Observable<Terms[]> {
    return this.API.getRequest<Terms[]>("Term");
  }
  getTermById(id: number): Observable<any> {
    return this.API.getRequest<any>(`Term/${id}`);
  }
  addTerm(newTerm: any): Observable<any> {
    return this.API.postRequest<any>("Term", newTerm);
  }
  
  deleteTerm(id: number): Observable<string> {
    return this.API.deleteRequest<string>(`Term/${id}`);
  }
}
