import { Component, EventEmitter, inject, Input, OnInit, Output } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
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
export class FeeComponent implements OnInit {
  @Input() formGroup!: FormGroup;
  @Output() feeClassesChanged = new EventEmitter<FeeClasses[]>(); // Output for notifying parent
  @Output() requiredFeesChanged = new EventEmitter<number>(); // Output for notifying parent about required fees


  myControl = new FormControl('');
  classes: ClassDTO[] = [];
  feeClasses: FeeClasses[] = [];
  filteredOptions!: Observable<string[]>;
  isOptionSelected = false; // Tracks if an option is selected

  feeClassService = inject(FeeClassService);
  classService = inject(ClassService);

  ngOnInit() {
    this.GetAllClasses();
  }

  GetAllClasses(): void {
    this.classService.GetAll().subscribe((res) => (this.classes = res));
  }

  onOptionSelected(event: any) {
    this.isOptionSelected = true;
    const feeClassID = event.option.value;

    this.feeClassService.GetAllByID(feeClassID).subscribe((res: any) => {
      if (res.success) {
        this.feeClasses = res.data;
        this.feeClassesChanged.emit(this.feeClasses); // Notify parent
      } else {
        console.error('Error fetching fee classes', res);
      }
    });
  }

  updateFeeClassField<K extends keyof FeeClasses>(fieldName: K, value: FeeClasses[K], feeClass: FeeClasses): void {
  feeClass[fieldName] = value;
  this.feeClassesChanged.emit(this.feeClasses); // Emit the updated array
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
    this.updateFeeClassField('mandatory', !isChecked, feeClass);
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
  
}
