import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { BackendAspService } from '../../../../ASP.NET/backend-asp.service';
import { CLass, updateClass } from '../models/class.model';
import { ApiResponse } from '../../../../core/models/response.model';

@Injectable({
  providedIn: 'root'
})
export class ClassService {
  private API = inject(BackendAspService);

  constructor() { }

  // ✅ Get all classes
  GetAll(): Observable<ApiResponse<CLass[]>> {
    return this.API.getRequest<CLass[]>("Classes");
  }

  // ✅ Get class names only
  GetAllNames(): Observable<ApiResponse<CLass[]>> {
    return this.API.getRequest<CLass[]>("Classes/GetAllNameClasses");
  }

  // ✅ Add new class
  Add(Class: CLass): Observable<ApiResponse<CLass>> {
    return this.API.postRequest<CLass>("Classes", Class);
  }

  // ✅ Delete class
  Delete(id: number): Observable<ApiResponse<any>> {
    return this.API.deleteRequest<any>(`Classes/${id}`);
  }

  // ✅ Full update
  Update(id: number, update: updateClass): Observable<ApiResponse<updateClass>> {
    return this.API.putRequest<updateClass>(`Classes/${id}`, update);
  }

  // ✅ Partial update
  partialUpdate(id: number, patchDoc: any): Observable<ApiResponse<any>> {
    return this.API.patchRequest<any>(`Classes/${id}`, patchDoc);
  }
}
