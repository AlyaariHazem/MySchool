import { inject, Injectable } from '@angular/core';
import { FirebaseService } from '../../firebase/firebase.service';
import { map, Observable, switchMap, of } from 'rxjs';
import { Grades } from '../models/stages-grades.modul';
import { firebaseUrl } from '../../firebase/firebase-config';

@Injectable({
  providedIn: 'root'
})
export class GradeService {
  firebaseService = inject(FirebaseService);

  // Get all grades from Firebase
  getGrades(): Observable<Array<Grades>> {
    return this.firebaseService.getRequest<{ [key: string]: Grades }>('Grades').pipe(
      map(gradeObj => {
        const gradeArray: Grades[] = [];
        for (const key in gradeObj) {
          if (gradeObj.hasOwnProperty(key)) {
            gradeArray.push({ ...gradeObj[key] });
          }
        }
        return gradeArray;
      })
    );
  }

  // Add a new grade to Firebase
  addGrade(grade: Grades): Observable<any> {
    return this.getGrades().pipe(
      map(grades => {
        const numericIds = grades.map(g => +g.id).filter(id => !isNaN(id));
        const maxId = numericIds.length > 0 ? Math.max(...numericIds) : 0;
        grade.id = (maxId + 1).toString();
        return grade;
      }),
      switchMap(newGrade => 
        this.firebaseService.postRequest(`${firebaseUrl}Grades.json`, newGrade, { 'content-type': 'application/json' })
      )
    );
  }

  // Edit an existing grade in Firebase
  editGrade(grade: Grades): Observable<any> {
    const gradeId = grade.id;
    const { id, ...gradeWithoutId } = grade; // Destructure the grade to exclude the id
    return this.firebaseService.patchRequest(`${firebaseUrl}Grades/${gradeId}.json`, gradeWithoutId, { 'content-type': 'application/json' });
  }

  // Delete a grade from Firebase
  deleteGrade(gradeId: string): Observable<any> {
    return this.firebaseService.deleteRequest(`${firebaseUrl}Grades/${gradeId}.json`, { 'content-type': 'application/json' });
  }

  constructor() { }
}
