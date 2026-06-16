import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { BackendAspService } from '../../../../ASP.NET/backend-asp.service';
import { Guardians } from '../models/guardian.model';
import { ApiResponse } from '../../../../core/models/response.model';

@Injectable({
  providedIn: 'root'
})
export class GuardianService {
  private API = inject(BackendAspService);
  constructor() { }

  getAllGuardians(): Observable<ApiResponse<Guardians[]>> {
    return this.API.getRequest<Guardians[]>('Guardian');
  }
  getAllGuardiansExist(): Observable<ApiResponse<Guardians[]>> {
    return this.API.getRequest<Guardians[]>('Guardian/GuardianExists');
  }

  getGuardianById(id: number): Observable<ApiResponse<Guardians>> {
    return this.API.getRequest<Guardians>(`Guardian/${id}`);
  }

 getGuardiansInfo(): Observable<ApiResponse<Guardians[]>> {
    return this.API.getRequest<Guardians[]>('Guardian/GuardianInfo');
  }

  updateGuardian(id: number, guardian: any): Observable<ApiResponse<Guardians>> {
    return this.API.putRequest<Guardians>(`Guardian/${id}`, guardian);
  }


}
