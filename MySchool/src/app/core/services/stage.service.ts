import { inject, Injectable } from '@angular/core';
import { FirebaseService } from '../../firebase/firebase.service';
import { catchError, map, Observable,switchMap } from 'rxjs';
import { AddStage, Stages, updateStage } from '../models/stages-grades.modul';
import { firebaseUrl } from '../../firebase/firebase-config';
import { BackendAspService } from '../../environments/ASP.NET/backend-asp.service';
import { error } from 'console';

@Injectable({
  providedIn: 'root'
})
export class StageService {
  firebaseService = inject(FirebaseService);
  APIService = inject(FirebaseService);
  private API = inject(BackendAspService);  

 // I want this to display my stages 
  getAllStages(): Observable<any> {
    return this.API.http.get(`${this.API.baseUrl}/stages`).pipe(
      map(response => response), // Process or map the response here if needed
      catchError(error => {
        console.error("Error fetching stages:", error);
        throw error; // Optionally handle the error or rethrow
      })
    );
  }

  AddStage(stage:AddStage):Observable<any>{
    return this.API.http.post(`${this.API.baseUrl}/stages`,stage).pipe(
      catchError(error => {
        console.error("Error adding stage:", error);
        throw error; // Optionally rethrow or handle the error here
      })
    )
  }

  DeleteStage(id: number): Observable<any> {
    return this.API.http.delete(`${this.API.baseUrl}/stages/${id}`).pipe(
      catchError(error => {
        console.error("Error deleting stage:", error);
        throw error; // Optionally rethrow or handle the error here
      })
    );
  }
    
  DeleteClass(id:number):Observable<any>{
    return this.API.http.delete(`${this.API.baseUrl}/classes/${id}`);
  }

  Update(id:number,update:updateStage):Observable<any>{
    return this.API.http.put(`${this.API.baseUrl}/stages/${id}`,update).pipe(
      catchError(error=>{
        console.log("some error accured");
        throw error;
      })
    );
  }

  // Get all stage from Firebase
  getStages(): Observable<Array<Stages>> {
    return this.firebaseService.getRequest<{ [key: string]: Stages }>('stages').pipe(
      map(stageObj => {
        const stageArray: Stages[] = [];
        for (const key in stageObj) {
          if (stageObj.hasOwnProperty(key)) {
            stageArray.push({ ...stageObj[key]});
          }
        }
        return stageArray;
      })
    );
  }

  // Add a new stage to Firebase
  addStage(stage: Stages): Observable<any> {
    return this.getStages().pipe(
      map(stages => {
        const maxId = Math.max(...stages.map(s => +s.id), 0);
        stage.id = (maxId + 1).toString();
        return stage;
      }),
      switchMap(newStage => 
        this.firebaseService.postRequest(`${firebaseUrl}stages.json`, newStage, { 'content-type': 'application/json' })
      )
    );
  }

  // Edit an existing stage in Firebase
  editStage(clas: Stages): Observable<any> {
    const stageId = clas.id;
    const { id, ...stageWithoutId } = clas; // Destructure the stage to exclude the id
    return this.firebaseService.patchRequest(`${firebaseUrl}stages/${stageId}.json`, stageWithoutId, { 'content-type': 'application/json' });
  }

  // Delete a stage from Firebase
  deleteStage(stageId: string): Observable<any> {
    return this.firebaseService.deleteRequest(`${firebaseUrl}stages/${stageId}.json`, { 'content-type': 'application/json' });
  }
  
}
