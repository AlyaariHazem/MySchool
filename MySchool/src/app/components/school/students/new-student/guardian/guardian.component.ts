import { Component, OnInit, inject } from '@angular/core';
import { FormGroup } from '@angular/forms';

import { GuardianService } from '../../../core/services/guardian.service';
import { GuardianExist } from '../../../core/models/guardian.model';
import { StudentFormStoreService } from '../../../core/store/student-form-store.service';

@Component({
  selector: 'app-guardian',
  templateUrl: './guardian.component.html',
  styleUrls: ['./guardian.component.scss']
})
export class GuardianComponent implements OnInit {
  formGroup!: FormGroup;
  guardians: GuardianExist[] = [];

  guardianStore = inject(StudentFormStoreService);
  guardianService = inject(GuardianService);

  ngOnInit(): void {
    const mainForm = this.guardianStore.getForm();
    this.formGroup = mainForm.get('guardian') as FormGroup;

    const dobValue = this.formGroup.get('guardianDOB')?.value;
    if (dobValue) {
      this.formGroup.get('guardianDOB')?.setValue(this.formatDateForInput(dobValue));
    }

    this.guardianService.getAllGuardiansExist().subscribe({
      next: (res) => {
        if(!res.isSuccess) {
          console.error('Error fetching guardians:', res.errorMasseges[0]);
          this.guardians = [];
          return;
        } else {
          this.guardians = res.result;
        }
      },
      error: (err) => console.error('Error fetching guardians:', err)
    });
  }

  onGuardianChange(event: Event): void {
    const selectedValue = +(event.target as HTMLSelectElement).value;
    this.guardianStore.getForm().get('existingGuardianId')?.setValue(selectedValue);
  }

  formatDateForInput(isoDate: string): string {
    const date = new Date(isoDate);
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }
}
