import { inject, Injectable } from '@angular/core';
import { BackendAspService } from '../../ASP.NET/backend-asp.service';
import { catchError, map, Observable } from 'rxjs';
import { Tenant } from '../../components/admin/core/models/tenant.model';

@Injectable({
  providedIn: 'root'
})
export class TenantService {
  API = inject(BackendAspService);

  addTenant(tenant: Tenant): Observable<any> {
    return this.API.http.post(`${this.API.baseUrl}/Tenant`, tenant).pipe(
      catchError(error => {
        console.error("Error adding tenant:", error);
        throw error;
      })
    );
  }

  getAllTenants(): Observable<any> {
    return this.API.http.get<any>(`${this.API.baseUrl}/Tenant`).pipe(
      map(response => response.result),
      catchError(error => {
        console.error("Error fetching tenants:", error);
        throw error;
      })
    );
  }

  getTenantById(id: number): Observable<any> {
    return this.API.http.get<any>(`${this.API.baseUrl}/Tenant/${id}`).pipe(
      map(response => response.result),
      catchError(error => {
        console.error("Error fetching tenant:", error);
        throw error;
      })
    );
  }

  deleteTenant(id: number): Observable<any> {
    return this.API.http.delete(`${this.API.baseUrl}/Tenant/${id}`).pipe(
      catchError(error => {
        console.error("Error deleting tenant:", error);
        throw error;
      })
    );
  }
}
