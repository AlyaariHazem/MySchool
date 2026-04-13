import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, map, of, throwError } from 'rxjs';
import { BackendAspService } from '../../../../ASP.NET/backend-asp.service';
import { ApiResponse } from '../../../../core/models/response.model';

export interface ReportTemplateGetDTO {
  id: number;
  name: string;
  code: string;
  schoolId?: number;
  templateHtml: string;
  createdAt: string;
  updatedAt: string;
}

export interface ReportTemplateSaveDTO {
  name: string;
  code: string;
  schoolId?: number;
  templateHtml: string;
}

/** Merge field for #Name# placeholders (API may return PascalCase or camelCase). */
export interface ReportTemplatePlaceholderDto {
  name: string;
  description?: string | null;
}

function normalizePlaceholder(raw: Record<string, unknown>): ReportTemplatePlaceholderDto {
  const name = String(raw['name'] ?? raw['Name'] ?? '');
  const desc = raw['description'] ?? raw['Description'];
  return {
    name,
    description: desc == null ? undefined : String(desc),
  };
}

@Injectable({
  providedIn: 'root',
})
export class ReportTemplateService {
  private API = inject(BackendAspService);
  private http = inject(HttpClient);

  /**
   * Get report template by code
   * @param code Template code (e.g., "STUDENT_MONTH_RESULT")
   * @param schoolId Optional school ID for school-specific templates
   */
  getTemplateByCode(code: string, schoolId?: number): Observable<ReportTemplateGetDTO> {
    let url = `${this.API.baseUrl}/Report/template/${code}`;
    if (schoolId) {
      url += `?schoolId=${schoolId}`;
    }

    return this.http.get<ApiResponse<ReportTemplateGetDTO>>(url).pipe(
      map(response => response.result),
      catchError((error) => {
        console.error('Error fetching template:', error);
        return throwError(() => new Error(error.message || 'Failed to fetch template'));
      })
    );
  }

  /**
   * Save or update report template
   * @param dto Template data
   * @param schoolId Optional school ID (can also be in DTO)
   */
  saveTemplate(dto: ReportTemplateSaveDTO, schoolId?: number): Observable<ReportTemplateGetDTO> {
    let url = `${this.API.baseUrl}/Report/save`;
    if (schoolId) {
      url += `?schoolId=${schoolId}`;
    }

    return this.http.post<ApiResponse<ReportTemplateGetDTO>>(url, dto).pipe(
      map(response => response.result),
      catchError((error) => {
        console.error('Error saving template:', error);
        return throwError(() => new Error(error.message || 'Failed to save template'));
      })
    );
  }

  /**
   * Field names supported for the report template code (shown after typing # in the editor).
   */
  getTemplatePlaceholders(code: string): Observable<ReportTemplatePlaceholderDto[]> {
    const encoded = encodeURIComponent(code);
    const url = `${this.API.baseUrl}/Report/template-placeholders/${encoded}`;

    return this.http.get<ApiResponse<ReportTemplatePlaceholderDto[]>>(url).pipe(
      map((response) =>
        (response.result ?? []).map((row: Record<string, unknown>) => normalizePlaceholder(row))
      ),
      catchError((error) => {
        console.error('Error fetching template placeholders:', error);
        return of([]);
      })
    );
  }
}
