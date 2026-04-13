import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { BackendAspService } from '../../../ASP.NET/backend-asp.service';
import type { AiChatRequest, AiChatResponse } from './ai-assistant.models';

/**
 * Calls POST api/Ai/chat (OpenAI + server-side tools only — no API key in browser).
 */
@Injectable({ providedIn: 'root' })
export class AiAssistantService {
  private readonly api = inject(BackendAspService);
  private readonly http = inject(HttpClient);

  chat(request: AiChatRequest): Observable<AiChatResponse> {
    return this.http.post<AiChatResponse>(`${this.api.baseUrl}/Ai/chat`, request);
  }
}
