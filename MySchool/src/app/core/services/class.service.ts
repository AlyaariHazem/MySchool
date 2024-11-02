import { inject, Injectable } from '@angular/core';
import { FirebaseService } from '../../firebase/firebase.service';
import { map, Observable } from 'rxjs';
import { Class } from '../models/class.model';
import { firebaseUrl } from '../../firebase/firebase-config';

@Injectable({
  providedIn: 'root'
})
export class ClassService {
  firebaseService = inject(FirebaseService);

  // Get all class from Firebase
  getClass(): Observable<Array<Class>> {
    return this.firebaseService.getRequest<{ [key: string]: Class }>('classes').pipe(
      map(classObj => {
        const classArray: Class[] = [];
        for (const key in classObj) {
          if (classObj.hasOwnProperty(key)) {
            classArray.push({ ...classObj[key], id: key });
          }
        }
        return classArray;
      })
    );
  }

  // Add a new class to Firebase
  addClass(clas: Class): Observable<any> {
    return this.firebaseService.postRequest(`${firebaseUrl}classes.json`, clas, { 'content-type': 'application/json' });
  }

  // Edit an existing class in Firebase
  editClass(clas: Class): Observable<any> {
    const classId = clas.id;
    const { id, ...classWithoutId } = clas; // Destructure the class to exclude the id
    return this.firebaseService.patchRequest(`${firebaseUrl}classes/${classId}.json`, classWithoutId, { 'content-type': 'application/json' });
  }

  // Delete a class from Firebase
  deleteClass(classId: string): Observable<any> {
    return this.firebaseService.deleteRequest(`${firebaseUrl}classes/${classId}.json`, { 'content-type': 'application/json' });
  }
  constructor() { }
}
