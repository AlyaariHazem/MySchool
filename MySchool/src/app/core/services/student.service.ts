import { inject, Injectable } from '@angular/core';
import { catchError, map, Observable, throwError } from 'rxjs';

import { AddStudent, StudentDetailsDTO, StudentPayload } from '../models/students.model';
import { BackendAspService } from '../../ASP.NET/backend-asp.service';

@Injectable({
    providedIn: 'root'
})
export class StudentService {
    private API = inject(BackendAspService);

    // Add a new student
    addStudent(student: AddStudent): Observable<any> {
        return this.API.http.post(`${this.API.baseUrl}/Students`, student).pipe(
            catchError(error => {
                console.error("Error adding student:", error);
                throw error;
            })
        );
    }
    getAllStudents(): Observable<StudentDetailsDTO[]> {
        return this.API.http.get<StudentDetailsDTO[]>(`${this.API.baseUrl}/Students`).pipe(
            catchError(error => {
                console.error("Error fetching Student Details:", error);
                throw error; // Optionally handle the error or rethrow
            })
        )
    }

    getAllStudentsPaginated(pageNumber: number = 1, pageSize: number = 8): Observable<any> {
        return this.API.http.get<any>(`${this.API.baseUrl}/Students?pageNumber=${pageNumber}&pageSize=${pageSize}`).pipe(
            map(response => {
                // Handle both wrapped (APIResponse) and unwrapped responses
                const data = response.result || response;
                // Map PagedResult properties (C# uses PascalCase, but JSON might be camelCase)
                return {
                    data: data.Data || data.data || [],
                    pageNumber: data.PageNumber ?? data.pageNumber ?? pageNumber,
                    pageSize: data.PageSize ?? data.pageSize ?? pageSize,
                    totalCount: data.TotalCount ?? data.totalCount ?? 0,
                    totalPages: data.TotalPages ?? data.totalPages ?? 0
                };
            }),
            catchError(error => {
                console.error("Error fetching paginated Student Details:", error);
                throw error;
            })
        )
    }

    getStudentsPage(pageNumber: number = 1, pageSize: number = 8, filters: Record<string, string> = {}): Observable<any> {
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

        return this.API.http.post<any>(`${this.API.baseUrl}/Students/page`, requestBody).pipe(
            map(response => {
                // Handle both wrapped (APIResponse) and unwrapped responses
                const data = response.result || response;
                // Map PagedResult properties (C# uses PascalCase, but JSON might be camelCase)
                return {
                    data: data.Data || data.data || [],
                    pageNumber: data.PageNumber ?? data.pageNumber ?? pageNumber,
                    pageSize: data.PageSize ?? data.pageSize ?? pageSize,
                    totalCount: data.TotalCount ?? data.totalCount ?? 0,
                    totalPages: data.TotalPages ?? data.totalPages ?? 0
                };
            }),
            catchError(error => {
                console.error("Error fetching paginated Student Details:", error);
                throw error;
            })
        )
    }
    DeleteStudent(id: number): Observable<any> {
        return this.API.http.delete(`${this.API.baseUrl}/Students/${id}`).pipe(
            catchError(error => {
                console.error("Error deleting Student:", error);
                throw error; // Rethrow error to propagate it to the caller
            })
        );
    }
    getStudentById(id: number): Observable<any> {
        return this.API.http.get(`${this.API.baseUrl}/Students/${id}`).pipe(
            catchError(error => {
                console.error("Error fetched Student:", error);
                throw error; // Rethrow error to propagate it to the caller
            })
        );
    }
    
    // Update an existing student
    updateStudent(student: StudentPayload): Observable<any> {
        return this.API.http.put<{message:string}>(`${this.API.baseUrl}/Students/updateStudentWithGuardian/${student.studentID}`, student).pipe(
            map(s=>s.message),
            catchError(error => {
                console.error("Error updating student:", error);
                return throwError(() => error);
            })
        );
    }

    // Get a student's image by their ID
    getStudentImage(studentId: number): Observable<Blob> {
        return this.API.http.get(`${this.API.baseUrl}/Students/students/${studentId}/image`, {
            responseType: 'blob'
        }).pipe(
            catchError(error => {
                console.error("Error fetching student image:", error);
                throw error;
            })
        );
    }
    // Upload multiple Attachments
    uploadFiles(files: File[], studentId: number): Observable<any> {
        const formData = new FormData();
        files.forEach(file => formData.append('files', file));
        formData.append('folderName', 'Attachments');
        formData.append('studentId', studentId.toString());

        return this.API.http.post(`${this.API.baseUrl}/Students/uploadFiles`, formData).pipe(
            catchError(error => {
                console.error("Error uploading Attachments:", error);
                return throwError(() => error);
            })
        );
    }
    // Get the maximum student ID
    MaxStudentID(): Observable<any> {
        return this.API.http.get(`${this.API.baseUrl}/Students/MaxValue`).pipe(
            catchError(error => {
                console.error("Error fetching maximum student ID:", error);
                throw error;
            })
        );
    }
    // Upload student images
    uploadStudentImage(file: File, studentId: number): Observable<any> {
        const formData = new FormData();
        formData.append('file', file);
        formData.append('studentId', studentId.toString());

        return this.API.http.post(`${this.API.baseUrl}/Students/uploadImage`, formData).pipe(
            catchError(error => {
                console.error("Error uploading student images:", error);
                return throwError(() => error);
            })
        );
    }

    // Get unregistered students (students not registered in new year)
    getUnregisteredStudents(
        pageNumber: number = 1, 
        pageSize: number = 5, 
        targetYearID?: number,
        studentName?: string,
        stageID?: number
    ): Observable<any> {
        let url = `${this.API.baseUrl}/Students/unregistered?pageNumber=${pageNumber}&pageSize=${pageSize}`;
        if (targetYearID) url += `&targetYearID=${targetYearID}`;
        if (studentName) url += `&studentName=${encodeURIComponent(studentName)}`;
        if (stageID) url += `&stageID=${stageID}`;

        return this.API.http.get<any>(url).pipe(
            map(response => {
                const data = response.result || response;
                return {
                    data: data.Data || data.data || [],
                    pageNumber: data.PageNumber ?? data.pageNumber ?? pageNumber,
                    pageSize: data.PageSize ?? data.pageSize ?? pageSize,
                    totalCount: data.TotalCount ?? data.totalCount ?? 0,
                    totalPages: data.TotalPages ?? data.totalPages ?? 0
                };
            }),
            catchError(error => {
                console.error("Error fetching unregistered students:", error);
                throw error;
            })
        );
    }

    // Promote students to new year/division
    promoteStudents(students: Array<{ studentID: number; newDivisionID: number }>, targetYearID?: number, copyCoursePlansFromCurrentYear: boolean = false): Observable<any> {
        const requestBody = {
            students: students,
            targetYearID: targetYearID,
            copyCoursePlansFromCurrentYear: copyCoursePlansFromCurrentYear
        };

        return this.API.http.post<any>(`${this.API.baseUrl}/Students/promote`, requestBody).pipe(
            map(response => {
                // Handle both wrapped (APIResponse) and unwrapped responses
                const data = response.result || response;
                return {
                    success: data.success || false,
                    message: data.message || '',
                    result: data.result || data
                };
            }),
            catchError(error => {
                console.error("Error promoting students:", error);
                return throwError(() => error);
            })
        );
    }
}
