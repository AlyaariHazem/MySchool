// fee.component.ts
import { Component, EventEmitter, inject, Input, OnInit, Output, ChangeDetectorRef, OnChanges, SimpleChanges } from '@angular/core';
import { FormControl, FormGroup, FormArray } from '@angular/forms';
import { Observable } from 'rxjs';

import { FeeClassService } from '../../../../../core/services/fee-class.service';
import { FeeClasses } from '../../../../../core/models/Fee.model';
import { ClassService } from '../../../../../core/services/class.service';
import { ClassDTO } from '../../../../../core/models/class.model';

@Component({
  selector: 'app-fee',
  templateUrl: './fee.component.html',
  styleUrls: ['./fee.component.scss']
})
export class FeeComponent implements OnInit, OnChanges {
  @Input() formGroup!: FormGroup;
  @Output() feeClassesChanged = new EventEmitter<FeeClasses[]>(); // Output for notifying parent
  @Output() requiredFeesChanged = new EventEmitter<number>(); // Output for notifying parent about required fees
  @Input() selectedClassID!: number | string;
  @Input('feeClasses') set feeClassesInput(value: FeeClasses[]) {
    this.feeClasses = value;
    this.updateFormGroup();
  }
  myControl = new FormControl('');
  classes: ClassDTO[] = [];
  feeClasses: FeeClasses[] = [];
  filteredOptions!: Observable<string[]>;
  isOptionSelected = false; // Tracks if an option is selected

  feeClassService = inject(FeeClassService);
  changeDetectorRef = inject(ChangeDetectorRef);
  classService = inject(ClassService);

  ngOnInit() {
    this.GetAllClasses();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['selectedClassID'] && this.selectedClassID) {
      // Ensure it's a string before setting it in the autocomplete control
      const classIDValue = typeof this.selectedClassID === 'number'
        ? this.selectedClassID.toString()
        : this.selectedClassID;
  
      this.myControl.setValue(classIDValue);
  
      // Call the existing method
      this.onOptionSelected({ option: { value: classIDValue } });
    }
  }

  GetAllClasses(): void {
    this.classService.GetAll().subscribe((res) => (this.classes = res));
  }

  onOptionSelected(event: any) {
    const feeClassID = event.option.value;
    this.isOptionSelected = true;

    this.feeClassService.GetAllByID(feeClassID).subscribe((res: any) => {
      if (res.isSuccess) {
        this.feeClasses = res.result;
        this.feeClassesChanged.emit(this.feeClasses); // Notify parent
        this.updateFormGroup();
        this.changeDetectorRef.detectChanges();
      }
    });
  }

  updateFeeClassField<K extends keyof FeeClasses>(fieldName: K, value: FeeClasses[K], feeClass: FeeClasses): void {
    feeClass[fieldName] = value;
    this.feeClassesChanged.emit(this.feeClasses); // Emit the updated array
    this.updateFormGroup();
  }

  handleNoteChange(value: string, feeClass: FeeClasses) {
    this.updateFeeClassField('noteDiscount', value, feeClass);
  }

  clearSelection() {
    this.myControl.setValue(''); // Clear the input value
    this.isOptionSelected = false; // Reset the selection state
  }

  getTotalFees(): number {
    return this.feeClasses
      .filter(fee => fee.mandatory) // Filter only the fees where mandatory is true
      .reduce((sum, fee) => sum + (fee.amount || 0), 0); // Sum the amounts of the filtered fees
  }  

  getTotalDiscounts(): number {
    return this.feeClasses.reduce((sum, fee) => sum + (fee.amountDiscount || 0), 0);
  }

  changeMandatory(event: any, feeClass: FeeClasses) {
    const isChecked = event.target.checked;
    this.updateFeeClassField('mandatory', isChecked, feeClass);
    this.getRequiredFees(); // Trigger recalculation and emit
  }

  handleDiscountChange(value: number, feeClass: FeeClasses) {
    if (value < 0 || value > (feeClass.amount || 0)) {
      console.error('Invalid discount value');
      return;
    }
    this.updateFeeClassField('amountDiscount', value, feeClass);
    this.getRequiredFees(); // Trigger recalculation and emit
  }

  getRequiredFees(): number {
    const requiredFees = this.getTotalFees() - this.getTotalDiscounts();
    this.requiredFeesChanged.emit(requiredFees); // Emit the required fees to parent
    return requiredFees;
  }  

  private updateFormGroup(): void {
    const discountsArray = this.formGroup.get('discounts') as FormArray;
    discountsArray.clear();
    this.feeClasses.forEach(fee => {
      discountsArray.push(
        this.feeClassService.buildFeeClassFormGroup(fee)
      );
    });
  }
}
