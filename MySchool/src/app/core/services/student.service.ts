import { inject, Injectable } from '@angular/core';
import { catchError, Observable, throwError } from 'rxjs';

import { UpdateStudent } from '../models/update-student.model';
import { AddStudent, StudentDetailsDTO } from '../models/students.model';
import { BackendAspService } from '../../environments/ASP.NET/backend-asp.service';

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
        return this.API.http.get(`${this.API.baseUrl}/Students/id?studentId=${id}`).pipe(
            catchError(error => {
                console.error("Error fetched Student:", error);
                throw error; // Rethrow error to propagate it to the caller
            })
        );
    }
    
    // Update an existing student
    updateStudent(student: UpdateStudent): Observable<any> {
        return this.API.http.put(`${this.API.baseUrl}/Students/${student.studentID}`, student).pipe(
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
    uploadAttachments(files: File[], studentId: number): Observable<any> {
        const formData = new FormData();
        files.forEach(file => formData.append('files', file));
        formData.append('studentId', studentId.toString());

        return this.API.http.post(`${this.API.baseUrl}/Students/uploadAttachments`, formData).pipe(
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
