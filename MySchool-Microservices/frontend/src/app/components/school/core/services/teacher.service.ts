import { inject, Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { BackendAspService } from '../../../../ASP.NET/backend-asp.service';
import { Teachers } from '../models/teacher.model';
import { ApiResponse } from '../../../../core/models/response.model';
import { PagedResultDto } from '../../../../core/models/students.model';

/** Row for POST Teacher/names/page (camelCase JSON). */
export interface TeacherNameLookupRow {
  teacherID: number;
  fullName: string;
}

export interface TeacherNamesPageRequest {
  pageIndex: number;
  pageSize: number;
  search?: string | null;
}

@Injectable({
  providedIn: 'root'
})
export class TeacherService {

  private API = inject(BackendAspService);

  getAllTeacher(): Observable<ApiResponse<Teachers[]>> {
    return this.API.getRequest<Teachers[]>("Teacher").pipe(
      map((res: ApiResponse<Teachers[]>) => {
        if (res.isSuccess && res.result) {
          res.result = res.result.map((teacher: Teachers) => {
            teacher.fullName = `${teacher.firstName} ${teacher.middleName ?? ''} ${teacher.lastName}`.trim();
            return teacher;
          });
        }
        return res;
      })
    );
  }

  getTeacherById(id: number): Observable<ApiResponse<Teachers>> {
    return this.API.getRequest<Teachers>(`Teacher/${id}`);
  }

  addTeacher(newTeacher: Teachers): Observable<ApiResponse<Teachers>> {
    return this.API.postRequest<Teachers>('Teacher', newTeacher);
  }

  updateTeacher(id: number, updatedTeacher: Teachers): Observable<ApiResponse<Teachers>> {
    return this.API.putRequest<Teachers>(`Teacher/${id}`, updatedTeacher);
  }

  deleteTeacher(id: number): Observable<ApiResponse<any>> {
    return this.API.deleteRequest<any>(`Teacher/${id}`);
  }

  /**
   * POST Teacher/names/page — paged id + name (active year scope when configured).
   * Unwraps APIResponse.result to PagedResultDto.
   */
  getTeacherNamesPage(body: TeacherNamesPageRequest): Observable<PagedResultDto<TeacherNameLookupRow>> {
    const payload = {
      pageIndex: body.pageIndex ?? 0,
      pageSize: body.pageSize ?? 20,
      search: body.search != null && String(body.search).trim() !== '' ? String(body.search).trim() : null
    };
    return this.API.postRequest<PagedResultDto<TeacherNameLookupRow>>('Teacher/names/page', payload).pipe(
      map((res) => {
        const empty: PagedResultDto<TeacherNameLookupRow> = {
          data: [],
          pageNumber: 1,
          pageSize: payload.pageSize,
          totalCount: 0,
          totalPages: 0
        };
        if (!res.isSuccess || res.result == null) {
          return empty;
        }
        const p = res.result as Record<string, unknown>;
        const rawList = (p['data'] ?? p['Data'] ?? []) as any[];
        const data: TeacherNameLookupRow[] = Array.isArray(rawList)
          ? rawList.map((x) => ({
              teacherID: Number(x.teacherID ?? x.TeacherID),
              fullName: String(x.fullName ?? x.FullName ?? '').trim()
            }))
          : [];
        return {
          data,
          pageNumber: Number(p['pageNumber'] ?? p['PageNumber'] ?? 1),
          pageSize: Number(p['pageSize'] ?? p['PageSize'] ?? payload.pageSize),
          totalCount: Number(p['totalCount'] ?? p['TotalCount'] ?? data.length),
          totalPages: Number(p['totalPages'] ?? p['TotalPages'] ?? 1)
        };
      })
    );
  }

  /** GET Teacher/{id}/name-lookup — label hydration for dropdowns. */
  getTeacherNameLookup(id: number): Observable<ApiResponse<TeacherNameLookupRow>> {
    return this.API.getRequest<TeacherNameLookupRow>(`Teacher/${id}/name-lookup`).pipe(
      map((res) => ({
        ...res,
        result:
          res.result != null
            ? {
                teacherID: Number((res.result as any).teacherID ?? (res.result as any).TeacherID),
                fullName: String((res.result as any).fullName ?? (res.result as any).FullName ?? '').trim()
              }
            : null
      }))
    );
  }
}
