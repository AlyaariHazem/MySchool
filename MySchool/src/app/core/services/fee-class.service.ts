import { inject, Injectable } from '@angular/core';
import { catchError, map, Observable, throwError } from 'rxjs';

import { BackendAspService } from '../../environments/ASP.NET/backend-asp.service';
import { FeeClass, FeeClasses } from '../models/Fee.model';
import { FormBuilder, FormGroup } from '@angular/forms';

@Injectable({
  providedIn: 'root'
})
export class FeeClassService {
  private API = inject(BackendAspService);

  constructor(private fb: FormBuilder) {}

  getAllFeeClass(): Observable<any> {
    return this.API.http.get<any>(`${this.API.baseUrl}/FeeClass`).pipe(
      map(response=> response.result),
      catchError(err => this.handleError(err, "Failed to fetch fee classes"))
    );
  }

  AddFeeClass(feeClass: FeeClass): Observable<any> {
    return this.API.http.post(`${this.API.baseUrl}/FeeClass`, feeClass).pipe(
      catchError(err => this.handleError(err, "Failed to add fee class"))
    );
  }

  UpdateFeeClass(FeeClassID: number, feeClass: FeeClass): Observable<any> {
    return this.API.http.put(`${this.API.baseUrl}/FeeClass/${FeeClassID}`, feeClass).pipe(
      catchError(err => this.handleError(err, "Failed to update fee class"))
    );
  }

  DeleteFeeClass(FeeClassID: number): Observable<any> {
    return this.API.http.delete(`${this.API.baseUrl}/FeeClass/${FeeClassID}`).pipe(
      catchError(err => this.handleError(err, "Failed to delete fee class"))
    );
  }

  GetByID(FeeClassID: number): Observable<any> {
    return this.API.http.get(`${this.API.baseUrl}/FeeClass/${FeeClassID}`).pipe(
      catchError(err => this.handleError(err, "Failed to fetch fee class by ID"))
    );
  }

  GetAllByID(ClassID: number): Observable<FeeClasses[]> {
    return this.API.http.get<FeeClasses[]>(`${this.API.baseUrl}/FeeClass/Class/${ClassID}`).pipe(
      catchError(err => this.handleError(err, "Failed to fetch fee classes by class ID"))
    );
  }
  
  buildFeeClassFormGroup(feeClass: FeeClasses): FormGroup {
    return this.fb.group({
      feeName: [feeClass.feeName || ''],
      amount: [feeClass.amount || 0],
      amountDiscount: [feeClass.amountDiscount || 0],
      noteDiscount: [feeClass.noteDiscount || ''],
      feeClassID: [feeClass.feeClassID || null],
      className: [feeClass.className || ''],
      mandatory: [feeClass.mandatory || false],
    });
  }
  private handleError(error: any, message: string): Observable<never> {
    console.error(message, error);
    return throwError(() => new Error(message));
  }
}
