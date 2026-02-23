import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BackendAspService } from '../../../../ASP.NET/backend-asp.service';
import { ApiResponse } from '../../../../core/models/response.model';
import { WeeklySchedule, AddWeeklySchedule, UpdateWeeklySchedule, WeeklyScheduleGrid } from '../models/weekly-schedule.model';

@Injectable({
  providedIn: 'root'
})
export class WeeklyScheduleService {
  private API = inject(BackendAspService);

  constructor() { }

  // Get all schedules
  GetAll(): Observable<ApiResponse<WeeklySchedule[]>> {
    return this.API.getRequest<WeeklySchedule[]>("WeeklySchedule");
  }

  // Get schedule by ID
  GetById(id: number): Observable<ApiResponse<WeeklySchedule>> {
    return this.API.getRequest<WeeklySchedule>(`WeeklySchedule/${id}`);
  }

  // Get schedule by class and term
  GetByClassAndTerm(classId: number, termId: number, divisionId?: number): Observable<ApiResponse<WeeklySchedule[]>> {
    const divisionParam = divisionId ? `?divisionId=${divisionId}` : '';
    return this.API.getRequest<WeeklySchedule[]>(`WeeklySchedule/class/${classId}/term/${termId}${divisionParam}`);
  }

  // Get schedule grid (formatted for display)
  GetScheduleGrid(classId: number, termId: number, divisionId?: number): Observable<ApiResponse<WeeklyScheduleGrid>> {
    const divisionParam = divisionId ? `?divisionId=${divisionId}` : '';
    return this.API.getRequest<WeeklyScheduleGrid>(`WeeklySchedule/grid/class/${classId}/term/${termId}${divisionParam}`);
  }

  // Add new schedule
  Add(schedule: AddWeeklySchedule): Observable<ApiResponse<any>> {
    return this.API.postRequest<any>("WeeklySchedule", schedule);
  }

  // Update schedule
  Update(id: number, schedule: UpdateWeeklySchedule): Observable<ApiResponse<any>> {
    return this.API.putRequest<any>(`WeeklySchedule/${id}`, schedule);
  }

  // Bulk update schedules (replace all for a class/term)
  BulkUpdate(schedules: AddWeeklySchedule[]): Observable<ApiResponse<any>> {
    return this.API.postRequest<any>("WeeklySchedule/bulk", schedules);
  }

  // Delete schedule
  Delete(id: number): Observable<ApiResponse<any>> {
    return this.API.deleteRequest<any>(`WeeklySchedule/${id}`);
  }
}
