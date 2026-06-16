import { Injectable, inject } from '@angular/core';

import { BackendAspService } from '../../ASP.NET/backend-asp.service';
import {
  ApproveRegistrationPayload,
  PendingRegistrationRequest,
  PendingRegistrationRequestsFilter,
  PublicSchoolOption,
  RequestRegistrationPayload,
} from '../models/registration-request.model';

@Injectable({
  providedIn: 'root',
})
export class RegistrationRequestService {
  private readonly api = inject(BackendAspService);

  getPublicSchools() {
    return this.api.http.get<PublicSchoolOption[]>(`${this.api.baseUrl}/auth/PublicSchools`);
  }

  requestRegistration(payload: RequestRegistrationPayload) {
    const fd = new FormData();
    fd.append('TenantId', String(payload.tenantId));
    fd.append('UserName', payload.userName);
    fd.append('PhoneNumber', payload.phoneNumber);
    fd.append('Password', payload.password);
    fd.append('ConfirmPassword', payload.confirmPassword);
    fd.append('RequestedRole', payload.requestedRole);
    fd.append('Gender', payload.gender);
    if (payload.fullName) {
      fd.append('FullName', payload.fullName);
    }
    if (payload.dateOfBirth) {
      fd.append('DateOfBirth', payload.dateOfBirth);
    }
    for (const f of payload.files) {
      fd.append('attachments', f, f.name);
    }
    return this.api.http.post<{ message: string }>(
      `${this.api.baseUrl}/auth/RequestRegistration`,
      fd,
    );
  }

  /** POST with optional filters (date range, gender, phone, school). */
  searchPendingRequests(filter: PendingRegistrationRequestsFilter = {}) {
    return this.api.http.post<PendingRegistrationRequest[]>(
      `${this.api.baseUrl}/auth/PendingRequests`,
      filter,
    );
  }

  approveRequest(id: number, body?: ApproveRegistrationPayload) {
    return this.api.http.post<{ message: string }>(
      `${this.api.baseUrl}/auth/ApproveRequest/${id}`,
      body ?? {},
    );
  }

  rejectRequest(id: number, reason?: string | null) {
    return this.api.http.post<{ message: string }>(
      `${this.api.baseUrl}/auth/RejectRequest/${id}`,
      { reason: reason ?? null },
    );
  }
}
