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
}
