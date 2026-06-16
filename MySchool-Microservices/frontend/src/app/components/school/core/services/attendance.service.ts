import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BackendAspService } from '../../../../ASP.NET/backend-asp.service';
import { ApiResponse } from '../../../../core/models/response.model';
import { AttendanceDto, BulkAttendanceRequest } from '../models/attendance.model';

@Injectable({
  providedIn: 'root'
})
export class AttendanceService {
  private readonly API = inject(BackendAspService);

  getByClassAndDate(classId: number, dateIso: string): Observable<ApiResponse<AttendanceDto[]>> {
    const q = encodeURIComponent(dateIso);
    return this.API.getRequest<AttendanceDto[]>(`Attendance/class/${classId}?date=${q}`);
  }

  bulkUpsert(body: BulkAttendanceRequest): Observable<ApiResponse<{ updatedCount: number }>> {
    return this.API.postRequest<{ updatedCount: number }>('Attendance/bulk', body);
  }

  /** Guardian: attendance rows for all students linked to this account (optional date range; server defaults last 30 days). */
  getGuardianMy(fromIso?: string, toIso?: string): Observable<ApiResponse<AttendanceDto[]>> {
    const parts: string[] = [];
    if (fromIso) {
      parts.push(`from=${encodeURIComponent(fromIso)}`);
    }
    if (toIso) {
      parts.push(`to=${encodeURIComponent(toIso)}`);
    }
    const qs = parts.length > 0 ? `?${parts.join('&')}` : '';
    return this.API.getRequest<AttendanceDto[]>(`Attendance/guardian/my${qs}`);
  }
}
