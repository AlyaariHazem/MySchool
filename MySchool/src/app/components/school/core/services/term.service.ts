import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BackendAspService } from '../../../../ASP.NET/backend-asp.service';
import { Terms } from '../models/term.model';
import { ApiResponse } from '../../../../core/models/response.model';

@Injectable({
  providedIn: 'root'
})
export class TermService {

  private API = inject(BackendAspService);

  getAllTerm(): Observable<ApiResponse<Terms[]>> {
    return this.API.getRequest<Terms[]>('Term');
  }

  getTermById(id: number): Observable<ApiResponse<Terms>> {
    return this.API.getRequest<Terms>(`Term/${id}`);
  }

  addTerm(newTerm: Terms): Observable<ApiResponse<Terms>> {
    return this.API.postRequest<Terms>('Term', newTerm);
  }

  deleteTerm(id: number): Observable<ApiResponse<any>> {
    return this.API.deleteRequest<any>(`Term/${id}`);
  }
}