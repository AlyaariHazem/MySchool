import { inject, Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';

import { BackendAspService } from '../../../../ASP.NET/backend-asp.service';
import { PagedResultDto } from '../../../../core/models/students.model';
import { CLass, updateClass } from '../models/class.model';
import { ApiResponse } from '../../../../core/models/response.model';

/** Row for POST Classes/GetAllNameClasses/page (camelCase JSON). */
export interface ClassNameLookupRow {
  classID: number;
  className: string;
}

export interface ClassNamesPageRequest {
  pageIndex: number;
  pageSize: number;
  search?: string | null;
}

@Injectable({
  providedIn: 'root'
})
export class ClassService {
  private API = inject(BackendAspService);

  constructor() { }

  // ✅ Get all classes
  GetAll(): Observable<ApiResponse<CLass[]>> {
    return this.API.getRequest<CLass[]>("Classes");
  }

  // ✅ Get class names only
  GetAllNames(): Observable<ApiResponse<CLass[]>> {
    return this.API.getRequest<CLass[]>("Classes/GetAllNameClasses");
  }

  /**
   * POST Classes/GetAllNameClasses/page — paged names for active year (same filter as GET GetAllNameClasses).
   * Unwraps APIResponse.result to PagedResultDto.
   */
  getClassNamesPage(body: ClassNamesPageRequest): Observable<PagedResultDto<ClassNameLookupRow>> {
    const payload = {
      pageIndex: body.pageIndex ?? 0,
      pageSize: body.pageSize ?? 20,
      search: body.search != null && String(body.search).trim() !== '' ? String(body.search).trim() : null
    };
    return this.API.postRequest<PagedResultDto<ClassNameLookupRow>>('Classes/GetAllNameClasses/page', payload).pipe(
      map((res) => {
        const empty: PagedResultDto<ClassNameLookupRow> = {
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
        const data: ClassNameLookupRow[] = Array.isArray(rawList)
          ? rawList.map((x) => ({
              classID: Number(x.classID ?? x.ClassID),
              className: String(x.className ?? x.ClassName ?? '').trim()
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

  /** GET Classes/{id} — minimal fields for dropdown label hydration. */
  getClassById(id: number): Observable<ApiResponse<ClassNameLookupRow>> {
    return this.API.getRequest<ClassNameLookupRow>(`Classes/${id}`).pipe(
      map((res) => ({
        ...res,
        result:
          res.result != null
            ? {
                classID: Number((res.result as any).classID ?? (res.result as any).ClassID),
                className: String((res.result as any).className ?? (res.result as any).ClassName ?? '').trim()
              }
            : null
      }))
    );
  }

  // ✅ Add new class
  Add(Class: CLass): Observable<ApiResponse<CLass>> {
    return this.API.postRequest<CLass>("Classes", Class);
  }

  // ✅ Delete class
  Delete(id: number): Observable<ApiResponse<any>> {
    return this.API.deleteRequest<any>(`Classes/${id}`);
  }

  // ✅ Full update
  Update(id: number, update: updateClass): Observable<ApiResponse<updateClass>> {
    return this.API.putRequest<updateClass>(`Classes/${id}`, update);
  }

  // ✅ Partial update
  partialUpdate(id: number, patchDoc: any): Observable<ApiResponse<any>> {
    return this.API.patchRequest<any>(`Classes/${id}`, patchDoc);
  }
}
