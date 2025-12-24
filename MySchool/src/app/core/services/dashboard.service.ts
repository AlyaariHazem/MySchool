import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BackendAspService } from '../../ASP.NET/backend-asp.service';
import { ApiResponse } from '../models/response.model';
import { DashboardResult } from '../models/dashboard.model';

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private API = inject(BackendAspService);

  getDashboardData(): Observable<ApiResponse<DashboardResult>> {
    return this.API.getRequest<DashboardResult>('Dashboard');
  }
}

