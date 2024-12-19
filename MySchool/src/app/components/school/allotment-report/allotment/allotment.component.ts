import { Component, OnInit } from '@angular/core';
import { FormGroup, FormControl } from '@angular/forms';

@Component({
  selector: 'app-allotment',
  templateUrl: './allotment.component.html',
  styleUrls: ['./allotment.component.scss']
})
export class AllotmentComponent implements OnInit {
  formGroup: FormGroup; // No need for undefined

  constructor() {
    // Initialize the form group in the constructor
    this.formGroup = new FormGroup({
      text: new FormControl('') 
    });
  }

  ngOnInit(): void {
    // You can set additional values in ngOnInit if needed
  }
}
