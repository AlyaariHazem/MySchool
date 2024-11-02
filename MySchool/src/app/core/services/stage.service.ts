import { inject, Injectable } from '@angular/core';
import { FirebaseService } from '../../firebase/firebase.service';
import { map, Observable,switchMap } from 'rxjs';
import { Stages } from '../models/stages-grades.modul';
import { firebaseUrl } from '../../firebase/firebase-config';

@Injectable({
  providedIn: 'root'
})
export class StageService {
  firebaseService = inject(FirebaseService);

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
  constructor() { }
}
