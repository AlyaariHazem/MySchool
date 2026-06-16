import { Component, OnInit } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { StudentFormStoreService } from '../../../core/store/student-form-store.service'; // تأكد من المسار

@Component({
  selector: 'app-option-data',
  templateUrl: './option-data.component.html',
  styleUrls: ['./option-data.component.scss']
})
export class OptionDataComponent implements OnInit {
  formGroup!: FormGroup;

  constructor(private formStore: StudentFormStoreService) { }

  ngOnInit(): void {
    const fullForm = this.formStore.getForm();
    this.formGroup = fullForm.get('optionData') as FormGroup;
  }
}
