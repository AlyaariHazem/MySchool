import { Component, inject, OnInit} from '@angular/core';
import { PaginatorState } from 'primeng/paginator';

import { StudentDetailsDTO } from '../../../core/models/students.model';
import { StudentService } from '../../../core/services/student.service';

@Component({
    selector: 'app-dashboard',
    templateUrl: './dashboard.component.html',
    styleUrl: './dashboard.component.scss',
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

first: number = 0;
rows: number = 4;
onPageChange(event: PaginatorState) {
  this.first = event.first || 0; // Default to 0 if undefined
  this.rows = event.rows!;
}

}
