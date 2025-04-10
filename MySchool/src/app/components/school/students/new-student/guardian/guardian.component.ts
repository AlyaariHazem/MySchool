import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormGroup } from '@angular/forms';

import { GuardianService } from '../../../core/services/guardian.service';
import { Guardians } from '../../../core/models/guardian.model';

@Component({
  selector: 'app-guardian',
  templateUrl: './guardian.component.html',
  styleUrl: './guardian.component.scss'
})
export class GuardianComponent implements OnInit {
  @Input() formGroup!: FormGroup;
  @Output() existingGuardianId = new EventEmitter<number>();

  guardians: Guardians[] = [];

  constructor(private guardianService: GuardianService) { }


  ngOnInit(): void {
    const dobValue = this.formGroup.get('guardianDOB')?.value;
    if (dobValue) {
      this.formGroup.get('guardianDOB')?.setValue(this.formatDateForInput(dobValue));
    }

    this.guardianService.getAllGuardians().subscribe({
      next: (res) => {
        this.guardians = res;
        console.log('Guardians fetched successfully:', this.guardians);
      },
      error: (err) => console.error('Error occurred while fetching guardians:', err)
    });
    console.log('the guardian data is?', this.formGroup.value);
  }

  onGuardianChange(event: Event): void {
    const selectedValue = +(event.target as HTMLSelectElement).value;
    this.existingGuardianId.emit(selectedValue);
    console.log('Selected Guardian ID:', selectedValue);
  }
  formatDateForInput(isoDate: string): string {
    const date = new Date(isoDate);
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0'); // Ensure 2 digits
    const day = String(date.getDate()).padStart(2, '0'); // Ensure 2 digits
    return `${year}-${month}-${day}`;
  }

}
