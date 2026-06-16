import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { BackendAspService } from '../../ASP.NET/backend-asp.service';
import { ApiResponse } from '../models/response.model';
import { TeacherWorkspaceResult } from '../models/teacher-workspace.model';

@Injectable({
  providedIn: 'root',
})
export class TeacherWorkspaceService {
  private readonly api = inject(BackendAspService);

  getWorkspace(): Observable<ApiResponse<TeacherWorkspaceResult>> {
    return this.api.getRequest<TeacherWorkspaceResult>('TeacherWorkspace');
  }
}
