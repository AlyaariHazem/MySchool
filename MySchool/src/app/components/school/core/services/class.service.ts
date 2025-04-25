import { inject, Injectable } from '@angular/core';
import { BackendAspService } from '../../../../ASP.NET/backend-asp.service';
import { Observable } from 'rxjs';
import { CLass, updateClass } from '../models/class.model';

@Injectable({
  providedIn: 'root'
})
export class ClassService {
  private API = inject(BackendAspService); // Dependency injection of BackendAspService

  constructor() { }

  // Get all classes
  GetAll(): Observable<any> {
    return this.API.getRequest<any>("Classes");
  }
  GetAllNames(): Observable<any> {
    return this.API.getRequest<any>("Classes/GetAllNameClasses");
  }

  // Add a new class
  Add(Class: CLass): Observable<any> {
    return this.API.postRequest<CLass>("Classes", Class);
  }

  // Delete a class by ID
  Delete(id: number): Observable<any> {
    return this.API.deleteRequest(`Classes/${id}`);
  }

  // Update a class by ID
  Update(id: number, update: updateClass): Observable<any> {
    return this.API.putRequest<updateClass>(`Classes/${id}`, update);
  }

  // Partially update a class by ID
  partialUpdate(id: number, patchDoc: any): Observable<any> {
    return this.API.patchRequest<any>(`Classes/${id}`, patchDoc);
  }
}
