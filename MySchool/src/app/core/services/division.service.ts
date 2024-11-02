import { inject, Injectable } from '@angular/core';
import { FirebaseService } from '../../firebase/firebase.service';
import { map, Observable, switchMap, of } from 'rxjs';
import { Division, Grades } from '../models/stages-grades.modul';
import { firebaseUrl } from '../../firebase/firebase-config';

@Injectable({
  providedIn: 'root'
})
export class DivisionService {
  firebaseService = inject(FirebaseService);
    
  // Get all Division from Firebase
  getDivision(): Observable<Array<Division>> {
    return this.firebaseService.getRequest<{ [key: string]: Division }>('Division').pipe(
      map(divisionObj => {
        const divisionArray: Division[] = [];
        for (const key in divisionObj) {
          if (divisionObj.hasOwnProperty(key)) {
            divisionArray.push({ ...divisionObj[key] });
          }
        }
        return divisionArray;
      })
    );
  }

  // Add a new division to Firebase
  addDivision(division: Division): Observable<any> {
    return this.getDivision().pipe(
      map(Division => {
        const numericIds = Division.map(d => +d.id).filter(id => !isNaN(id));
        const maxId = numericIds.length > 0 ? Math.max(...numericIds) : 0;
        division.id = (maxId + 1).toString();
        return division;
      }),
      switchMap(newdivision => 
        this.firebaseService.postRequest(`${firebaseUrl}Division.json`, newdivision, { 'content-type': 'application/json' })
      )
    );
  }

  // Edit an existing division in Firebase
  editDivision(division: Division): Observable<any> {
    const divisionId = division.id;
    const { id, ...divisionWithoutId } = division; // Destructure the division to exclude the id
    console.log(`Editing division with id: ${divisionId}`); // Debug log
    console.log(`Data being sent:`, divisionWithoutId); // Debug log
    return this.firebaseService.patchRequest(`${firebaseUrl}Division/${divisionId}.json`, divisionWithoutId, { 'content-type': 'application/json' });
  }
  
  // Delete a division from Firebase
  deletedivision(divisionId: string): Observable<any> {
    return this.firebaseService.deleteRequest(`${firebaseUrl}Division/${divisionId}.json`, { 'content-type': 'application/json' });
  }

  constructor() { }
}
