import { Component, Input, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';

@Component({
  selector: 'app-option-data',
  templateUrl: './option-data.component.html',
  styleUrls: ['./option-data.component.scss']
})
export class OptionDataComponent implements OnInit {
  @Input() formGroup!: FormGroup;

  constructor() {}

  ngOnInit(): void {
   
  }

}
