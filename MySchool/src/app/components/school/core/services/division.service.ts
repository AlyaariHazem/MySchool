import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BackendAspService } from '../../../../ASP.NET/backend-asp.service';
import { Division, divisions } from '../models/division.model';
import { ApiResponse } from '../../../../core/models/response.model';

@Injectable({
  providedIn: 'root'
})
export class DivisionService {
  private API = inject(BackendAspService);

  constructor() { }

  GetAll(): Observable<ApiResponse<divisions[]>> {
    return this.API.getRequest<divisions[]>('Divisions');
  }

  Add(division: Division): Observable<ApiResponse<Division>> {
    return this.API.postRequest<Division>('Divisions', division);
  }

  Delete(id: number): Observable<ApiResponse<any>> {
    return this.API.deleteRequest<any>(`Divisions/${id}`);
  }

  partialUpdate(id: number, patchDoc: any): Observable<ApiResponse<any>> {
    return this.API.patchRequest<any>(`Divisions/${id}`, patchDoc);
  }

  UpdateDivision(id: number, division: Division): Observable<ApiResponse<Division>> {
    return this.API.putRequest<Division>(`Divisions/${id}`, division);
  }
}
