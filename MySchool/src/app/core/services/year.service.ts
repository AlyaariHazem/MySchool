import { inject, Injectable } from '@angular/core';
import { BackendAspService } from '../../ASP.NET/backend-asp.service';
import { catchError, map, Observable } from 'rxjs';
import { Year } from '../models/year.model';

@Injectable({
  providedIn: 'root'
})
export class YearService {
  private API = inject(BackendAspService);

  addYear(year: Year) {
    return this.API.http.post(`${this.API.baseUrl}/Year`, year).pipe(
      catchError(error => {
        console.error("Error adding Year:", error);
        throw error; // Optionally handle the error or rethrow
      })
    );
  }

  updateYear(year: Year, id: number): Observable<Year> {
    // Send data directly like POST request
    return this.API.http.put<any>(`${this.API.baseUrl}/Year/${id}`, year).pipe(
      map(response => {
        // Handle both wrapped and direct responses
        return response?.result || response;
      }),
      catchError(error => {
        console.error("Error updating Year:", error);
        throw error;
      })
    );
  }

  getAllYears(): Observable<Year[]> {
    return this.API.http.get<{ result: Year[] }>(`${this.API.baseUrl}/Year`).pipe(
      map(response => response.result),
      catchError(error => {
        console.error("Error fetching Year Details:", error);
        throw error; // Optionally handle the error or rethrow
      })
    );
  }

  getYearById(id: number) {
    return this.API.http.get(`${this.API.baseUrl}/Year/${id}`).pipe(
      catchError(error => {
        console.error("Error fetching Year Details:", error);
        throw error; // Optionally handle the error or rethrow
      })
    );
  }

  deleteYear(id: number) {
   return this.API.http.delete(`${this.API.baseUrl}/Year/${id}`).pipe(
      catchError(error => {
        console.error("Error deleting Year:", error);
        throw error; // Optionally handle the error or rethrow
      })
    );
  }

  partialUpdate(id: number, patchDoc: any): Observable<any> {
    return this.API.http.patch<any>(`${this.API.baseUrl}/year/${id}`, patchDoc).pipe(
      map(response => response.result),
      catchError(error => {
        console.error("Error with partial update:", error);
        throw error;
      })
    );
  }

  getYearsPage(pageNumber: number = 1, pageSize: number = 8, filters: Record<string, string> = {}): Observable<any> {
    // Transform filters from Record<string, string> to backend FilterRequest format
    const filtersDict: Record<string, { value: string }> = {};
    Object.entries(filters).forEach(([key, value]) => {
      if (value && value.trim() !== '') {
        filtersDict[key] = { value: value };
      }
    });

    const requestBody = {
      pageNumber: pageNumber,
      pageSize: pageSize,
      filters: filtersDict
    };

    return this.API.http.post<any>(`${this.API.baseUrl}/Year/page`, requestBody).pipe(
      map(response => {
        // POST /api/Year/page returns PagedResult directly (not wrapped in APIResponse)
        // Handle both PascalCase (C# default) and camelCase (JSON serialization)
        const data = response;
        
        // Extract data array - handle both cases
        let items: any[] = [];
        if (Array.isArray(data)) {
          items = data;
        } else if (data?.Data) {
          items = Array.isArray(data.Data) ? data.Data : [];
        } else if (data?.data) {
          items = Array.isArray(data.data) ? data.data : [];
        } else if (data?.result?.Data) {
          items = Array.isArray(data.result.Data) ? data.result.Data : [];
        } else if (data?.result?.data) {
          items = Array.isArray(data.result.data) ? data.result.data : [];
        }
        
        return {
          data: items,
          pageNumber: data?.PageNumber ?? data?.pageNumber ?? pageNumber,
          pageSize: data?.PageSize ?? data?.pageSize ?? pageSize,
          totalCount: data?.TotalCount ?? data?.totalCount ?? 0,
          totalPages: data?.TotalPages ?? data?.totalPages ?? 0
        };
      }),
      catchError(error => {
        console.error("Error fetching paginated Year Details:", error);
        throw error;
      })
    );
  }
}
