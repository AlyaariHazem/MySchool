import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { map } from 'rxjs/operators';

import { environment } from '../../../../environments/environment';
import { PermissionService } from '../../../core/services/permission.service';
import type {
  AiChatResponse,
  SchoolAiSupportChatRequest,
  SchoolAiSupportChatResponse,
  SchoolAiSupportUserRole,
} from './ai-assistant.models';

/**
 * Calls minimal-agent POST {schoolAiSupportUrl}/api/chat (OpenAI + agent on that service only).
 */
@Injectable({ providedIn: 'root' })
export class AiAssistantService {
  private readonly http = inject(HttpClient);
  private readonly permissions = inject(PermissionService);

  chat(message: string, conversationId?: string | null): Observable<AiChatResponse> {
    const base = environment.schoolAiSupportUrl?.trim();
    if (!base) {
      return throwError(
        () =>
          new Error(
            'School AI Support is not configured. Set environment.schoolAiSupportUrl to your minimal-agent base URL (no /api suffix).',
          ),
      );
    }

    const body: SchoolAiSupportChatRequest = {
      message,
      userRole: mapSchoolRoleToSupportUserRole(this.permissions.getSchoolRole()),
      conversationId: conversationId?.trim() || undefined,
      userName: readDisplayName(),
    };

    const url = `${base.replace(/\/+$/, '')}/api/chat`;

    return this.http.post<SchoolAiSupportChatResponse>(url, body).pipe(
      map((res) => ({
        reply: res.response ?? '',
        toolSteps: [] as AiChatResponse['toolSteps'],
        conversationId: res.conversationId,
      })),
    );
  }
}

function readDisplayName(): string | undefined {
  if (typeof localStorage === 'undefined') return undefined;
  const n = localStorage.getItem('userName')?.trim();
  return n || undefined;
}

function mapSchoolRoleToSupportUserRole(schoolRole: string | null): SchoolAiSupportUserRole {
  const key = (schoolRole ?? '').trim().toLowerCase();
  if (key === 'student') return 'student';
  if (key === 'teacher') return 'teacher';
  if (key === 'guardian') return 'parent';
  return 'admin';
}
