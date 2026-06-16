import { Component, inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { FormGroup, FormArray, FormBuilder } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';

import { FeeClassService } from '../../../core/services/fee-class.service';
import { FeeClasses } from '../../../core/models/Fee.model';
import { StudentFormStoreService } from '../../../core/store/student-form-store.service';

@Component({
  selector: 'app-fee',
  templateUrl: './fee.component.html',
  styleUrls: ['./fee.component.scss']
})
export class FeeComponent implements OnInit {
  formGroup: FormGroup;
  feesFormGroup!: FormGroup;

  feeClassService = inject(FeeClassService);
  changeDetectorRef = inject(ChangeDetectorRef);
  toastr = inject(ToastrService);
  formStore = inject(StudentFormStoreService);
  formBuilder = inject(FormBuilder);

  constructor() {
    this.formGroup = this.formStore.getForm();
  }

  ngOnInit(): void {
    const feesGroup = this.formGroup.get('fees');
    if (feesGroup instanceof FormGroup) {
      this.feesFormGroup = feesGroup;
    } else {
      throw new Error('fees is not a FormGroup');
    }

    // Watch for class changes
    this.formGroup.get('primaryData.classID')?.valueChanges.subscribe(classID => {
      if (classID) {
        this.loadFeeClasses(classID);
      } else {
        this.clearFeeDiscounts();
      }
    });

    // Initial load
    const initialClassID = this.formGroup.get('primaryData.classID')?.value;
    if (initialClassID) {
      this.loadFeeClasses(initialClassID);
    }
  }


  loadFeeClasses(classID: number): void {
    this.feeClassService.GetAllByID(classID).subscribe((res: any) => {
      if (res.isSuccess) {
        const discountsArray = this.formGroup.get('fees.discounts') as FormArray;
  
        // Store existing values before clearing
        const existingValues: { [key: number]: any } = {};
        this.feeFormArray.controls.forEach(ctrl => {
          const id = ctrl.get('feeClassID')?.value;
          if (id != null) {
            existingValues[id] = ctrl.value;
          }
        });
  
        discountsArray.clear();
  
        res.result.forEach((fee: FeeClasses) => {
          const existing = existingValues[fee.feeClassID];
  
          const formGroup = this.feeClassService.buildFeeClassFormGroup(fee);
  
          if (existing) {
            formGroup.patchValue({
              amountDiscount: existing.amountDiscount,
              noteDiscount: existing.noteDiscount,
              mandatory: existing.mandatory
            });
          }
  
          discountsArray.push(formGroup);
        });
  
        this.changeDetectorRef.detectChanges();
      }
    });
  }
  
  clearFeeDiscounts(): void {
    const discountsArray = this.formGroup.get('fees.discounts') as FormArray;
    discountsArray.clear();
  }

  get feeFormArray(): FormArray {
    return this.formStore.getForm().get('fees.discounts') as FormArray;
  }

  getTotalFees(): number {
    return this.feeFormArray.controls
      .filter(ctrl => ctrl.get('mandatory')?.value)
      .reduce((sum, ctrl) => sum + (+ctrl.get('amount')?.value || 0), 0);
  }

  getTotalDiscounts(): number {
    return this.feeFormArray.controls
      .reduce((sum, ctrl) => sum + (+ctrl.get('amountDiscount')?.value || 0), 0);
  }

  getRequiredFees(): number {
    return this.getTotalFees() - this.getTotalDiscounts();
  }
}
