import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormGroup } from '@angular/forms';

import { GuardianService } from '../../../../../core/services/guardian.service';
import { Guardians } from '../../../../../core/models/guardian.model';

@Component({
  selector: 'app-guardian',
  templateUrl: './guardian.component.html',
  styleUrl: './guardian.component.scss'
})
export class GuardianComponent implements OnInit {
  @Input() formGroup!: FormGroup;
  @Output() existingGuardianId = new EventEmitter<number>();

  guardians:Guardians[]=[];

  constructor(private guardianService: GuardianService) {}

  
 ngOnInit(): void {
  this.guardianService.getAllGuardians().subscribe({
    next: (res) => {
      this.guardians = res.data;
      console.log('Guardians fetched successfully:', this.guardians);
    },
    error: (err) => console.error('Error occurred while fetching guardians:', err)
  });
}
onGuardianChange(event: Event): void {
  const selectedValue = +(event.target as HTMLSelectElement).value;
  this.existingGuardianId.emit(selectedValue);
  console.log('Selected Guardian ID:', selectedValue);
}

  
}
