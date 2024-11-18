import { Component, Input, OnInit } from '@angular/core';
import { FormGroup } from '@angular/forms';

@Component({
  selector: 'app-primary-data',
  templateUrl: './primary-data.component.html',
  styleUrls: ['./primary-data.component.scss']
})
export class PrimaryDataComponent implements OnInit {
  @Input() formGroup!: FormGroup;

  ngOnInit(): void {
    // Optionally check or initialize anything here
  }
}
