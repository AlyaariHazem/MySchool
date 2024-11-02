import { Component } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';

@Component({
  selector: 'app-primary-data',
  templateUrl: './primary-data.component.html',
  styleUrl: './primary-data.component.scss'
})
export class PrimaryDataComponent {
  dataForm: FormGroup;

  constructor(private formBuilder: FormBuilder) {
    this.dataForm = this.formBuilder.group({
      firstNameAr: '',
      secondNameAr: '',
      thirdNameAr: '',
      lastNameAr: '',
      firstNameEn: '',
      secondNameEn: '',
      thirdNameEn: '',
      lastNameEn: '',
      fullNameAr: '',
      fullNameEn: '',
      dob: '',
      grade: '',
      division: '',
      sex: ''
    });

    this.dataForm.get('firstNameAr')?.valueChanges.subscribe(() => this.updateFullName());
    this.dataForm.get('secondNameAr')?.valueChanges.subscribe(() => this.updateFullName());
    this.dataForm.get('thirdNameAr')?.valueChanges.subscribe(() => this.updateFullName());
    this.dataForm.get('lastNameAr')?.valueChanges.subscribe(() => this.updateFullName());
    this.dataForm.get('firstNameEn')?.valueChanges.subscribe(() => this.updateFullName());
    this.dataForm.get('secondNameEn')?.valueChanges.subscribe(() => this.updateFullName());
    this.dataForm.get('thirdNameEn')?.valueChanges.subscribe(() => this.updateFullName());
    this.dataForm.get('lastNameEn')?.valueChanges.subscribe(() => this.updateFullName());
  }

  updateFullName() {
    const firstNameAr = this.dataForm.get('firstNameAr')?.value;
    const secondNameAr = this.dataForm.get('secondNameAr')?.value;
    const thirdNameAr = this.dataForm.get('thirdNameAr')?.value;
    const lastNameAr = this.dataForm.get('lastNameAr')?.value;
    const firstNameEn = this.dataForm.get('firstNameEn')?.value;
    const secondNameEn = this.dataForm.get('secondNameEn')?.value;
    const thirdNameEn = this.dataForm.get('thirdNameEn')?.value;
    const lastNameEn = this.dataForm.get('lastNameEn')?.value;

    const fullNameAr = `${firstNameAr} ${secondNameAr} ${thirdNameAr} ${lastNameAr}`;
    const fullNameEn = `${firstNameEn} ${secondNameEn} ${thirdNameEn} ${lastNameEn}`;

    this.dataForm.get('fullNameAr')?.setValue(fullNameAr);
    this.dataForm.get('fullNameEn')?.setValue(fullNameEn);
  }
}
