import { Component, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { map, Observable, startWith } from 'rxjs';

@Component({
  selector: 'app-fees',
  templateUrl: './fees.component.html',
  styleUrls: ['./fees.component.scss']
})
export class FeesComponent implements OnInit {
  @Input() formGroup!: FormGroup;
  myControl = new FormControl('');
  options: string[] = ['One', 'Two', 'Three'];
  filteredOptions!: Observable<string[]>;
  isOptionSelected = false; // Tracks if an option is selected

  ngOnInit() {
    this.filteredOptions = this.myControl.valueChanges.pipe(
      startWith(''),
      map(value => this._filter(value || '')),
    );
  }

  private _filter(value: string): string[] {
    const filterValue = value.toLowerCase();
    return this.options.filter(option => option.toLowerCase().includes(filterValue));
  }

  onOptionSelected(event: any) {
    this.isOptionSelected = true; // Set to true when an option is selected
  }

  clearSelection() {
    this.myControl.setValue(''); // Clear the input value
    this.isOptionSelected = false; // Reset the selection state
  }
}
