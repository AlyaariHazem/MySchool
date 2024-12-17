import { Component, inject, OnInit} from '@angular/core';
import { StudentDetailsDTO } from '../../../core/models/students.model';
import { StudentService } from '../../../core/services/student.service';


export interface DialogData {
  }

@Component({
    selector: 'app-dashboard',
    templateUrl: './dashboard.component.html',
    styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit  {

 students:StudentDetailsDTO[]=[];
 studentService=inject(StudentService);

 ngOnInit(): void {
this.getAllStudent();
}

getAllStudent():void{
 this.studentService.getAllStudents().subscribe(res=>this.students=res);
}


}
