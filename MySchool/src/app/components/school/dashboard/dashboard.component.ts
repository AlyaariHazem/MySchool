import { Component, inject, OnInit } from '@angular/core';

import { Students } from '../../../core/models/students.model';

export interface DialogData {
    student: Students;
  }

@Component({
    selector: 'app-dashboard',
    templateUrl: './dashboard.component.html',
    styleUrl: './dashboard.component.scss',
})
export class DashboardComponent  {
 

  // this is for firebase upload 
}
