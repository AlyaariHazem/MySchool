import { inject, Injectable } from '@angular/core';
import { BackendAspService } from '../../../../ASP.NET/backend-asp.service';
import { map } from 'rxjs';
import { updateMonthlyGrades } from '../models/MonthlyGrade.model';

@Injectable({
  providedIn: 'root'
})
export class MonthlyGradesService {

  API=inject(BackendAspService);
  getAllMonthlyGrades(term:number,monthId:number,classId:number,subjectId:number){
    return this.API.http.get(`${this.API.baseUrl}/MonthlyGrades/${term}/${monthId}/${classId}/${subjectId}`).pipe(
      map((res:any)=>{
        return res.result;
      }
      )
    );
  }

  updateMonthlyGrades(monthlyGrades: updateMonthlyGrades[]){
    return this.API.http.put(`${this.API.baseUrl}/MonthlyGrades/UpdateMany`, monthlyGrades).pipe(
      map((res:any)=>{
        return res.result;
      }
      )
    );
  }
  
}
