import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
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
  /** Root student form — holds `existingGuardianId` (not nested under `guardian`). */
  mainForm!: FormGroup;
  formGroup!: FormGroup;
  guardians: GuardianExist[] = [];

  guardianStore = inject(StudentFormStoreService);
  guardianService = inject(GuardianService);
  private readonly destroyRef = inject(DestroyRef);

  ngOnInit(): void {
    const mainForm = this.guardianStore.getForm();
    this.mainForm = mainForm;
    this.formGroup = mainForm.get('guardian') as FormGroup;

    const dobValue = this.formGroup.get('guardianDOB')?.value;
    if (dobValue) {
      this.formGroup.get('guardianDOB')?.setValue(this.formatDateForInput(dobValue));
    }

    mainForm.get('existingGuardianId')?.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((v) => {
        if ((v === null || v === undefined || v === '') && this.guardians.length === 1) {
          const id = this.guardians[0]?.guardianID;
          if (id != null) {
            mainForm.get('existingGuardianId')?.setValue(id, { emitEvent: false });
          }
        }
      });

    this.guardianService.getAllGuardiansExist().subscribe({
      next: (res) => {
        if(!res.isSuccess) {
          console.error('Error fetching guardians:', res.errorMasseges[0]);
          this.guardians = [];
          return;
        } else {
          this.guardians = (res.result ?? []).map((g: GuardianExist) => ({
            ...g,
            guardianID: Number(g.guardianID),
          }));
          this.normalizeExistingGuardianId();
          this.syncExistingGuardianIfSingle();
        }
      },
      error: (err) => console.error('Error fetching guardians:', err)
    });
  }

  /** Align form value with option values (API/JSON often uses string IDs; p-dropdown compares strictly). */
  private normalizeExistingGuardianId(): void {
    const ctrl = this.mainForm.get('existingGuardianId');
    if (!ctrl) {
      return;
    }
    const v = ctrl.value;
    if (v === null || v === undefined || v === '') {
      return;
    }
    const n = Number(v);
    if (!Number.isNaN(n) && n !== v) {
      ctrl.setValue(n, { emitEvent: false });
    }
  }

  /**
   * When there is only one guardian, the browser shows it selected but does not fire `change`
   * if the user does not switch options — so we align the control with that row.
   */
  private syncExistingGuardianIfSingle(): void {
    const ctrl = this.mainForm.get('existingGuardianId');
    if (!ctrl || this.guardians.length !== 1) {
      return;
    }
    const v = ctrl.value;
    if (v !== null && v !== undefined && v !== '') {
      return;
    }
    ctrl.setValue(this.guardians[0].guardianID);
  }

  formatDateForInput(isoDate: string): string {
    const date = new Date(isoDate);
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }
}
