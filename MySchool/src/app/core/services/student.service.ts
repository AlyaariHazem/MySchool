import { inject, Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { v4 as uuidv4 } from 'uuid';

import { FirebaseService } from '../../firebase/firebase.service';
import { Students } from '../models/students.model';
import { firebaseUrl } from '../../firebase/firebase-config';

@Injectable({
  providedIn: 'root'
})
export class StudentsServicesService {

  firebase = inject(FirebaseService);

  // Get all users from Firebase
  getStudents(): Observable<Array<Students>> {
    return this.firebase.getRequest<{ [key: string]: Students }>('students').pipe(
      map(studentsObj => {
        const studentsArray: Students[] = [];
        for (const key in studentsObj) {
          if (studentsObj.hasOwnProperty(key)) {
            studentsArray.push({ ...studentsObj[key], id: key });
          }
        }
        return studentsArray;
      })
    );
  }
  addStudent(student: Students): Observable<any> {
    const newStudent = { ...student, id: uuidv4() };
    return this.firebase.postRequest(`${firebaseUrl}students.json`, newStudent, { 'content-type': 'application/json' });
  }

  editStudent(student: Students): Observable<any> {
    return this.firebase.patchRequest(`${firebaseUrl}students/${student.id}.json`, student, { 'content-type': 'application/json' });
  }

  deleteStudent(id: string): Observable<any> {
    return this.firebase.deleteRequest(`${firebaseUrl}students/${id}.json`, { 'content-type': 'application/json' });
  }

}
