import { Component, inject, OnInit } from '@angular/core';

import { ToastrService } from 'ngx-toastr';
import { MatDialog } from '@angular/material/dialog';

export interface DialogData {
  }

@Component({
    selector: 'app-dashboard',
    templateUrl: './dashboard.component.html',
    styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
    
  toastr = inject(ToastrService);
  constructor(public dialog: MatDialog) {}

  

  ngOnInit(): void {
  }


  // this is for firebase upload 
}
