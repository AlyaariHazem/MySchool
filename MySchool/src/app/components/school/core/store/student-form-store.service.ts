// shared/student-form-store.service.ts
import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

@Injectable({
  providedIn: 'root'
})
export class StudentFormStoreService {
  private formSubject: BehaviorSubject<FormGroup>;

  private fullNameSubject = new BehaviorSubject<string>('');
  fullName$ = this.fullNameSubject.asObservable(); 

  constructor(private fb: FormBuilder) {
    const initialForm = this.fb.group({
      studentID: [0],
      existingGuardianId: [null],
      primaryData: this.fb.group({
        studentFirstName: ['', Validators.required],
        studentMiddleName: ['', Validators.required],
        studentLastName: ['', Validators.required],
        studentFirstNameEng: [''],
        studentMiddleNameEng: [''],
        studentLastNameEng: [''],
        studentGender: ['Male', Validators.required],
        studentDOB: ['', Validators.required],
        studentPassword: ['Student'],
        classID: [null, Validators.required],
        amount: [0, Validators.required],
        divisionID: [null, Validators.required],
        studentAddress: [''],
      }),
      optionData: this.fb.group({
        placeBirth: [''],
        studentPhone: [776137120],
        studentAddress: [''],
      }),
      guardian: this.fb.group({
        guardianFullName: [''],
        guardianType: [''],
        guardianEmail: [''],
        guardianPassword: ['Guardian'],
        guardianPhone: [''],
        guardianGender: ['Male'],
        guardianDOB: [''],
        guardianAddress: ['']
      }),
      fees: this.fb.group({
        discounts: this.fb.array([])
      }),
      documents: this.fb.group({
        attachments: [[], Validators.required],
        attachmentsURLs: [[]]
      }),
      studentImageURL: [''],
    });
    
    this.formSubject = new BehaviorSubject<FormGroup>(initialForm);
    const primary = initialForm.get('primaryData') as FormGroup;
    this.emitFullName(primary.value);

    // react to every change in the three name controls
    primary.valueChanges.subscribe(() => this.emitFullName(primary.value));
  }

  getForm$() {
    return this.formSubject.asObservable();
  }

  getForm(): FormGroup {
    return this.formSubject.getValue();
  }

  updateForm(patch: Partial<any>) {
    const form = this.getForm();
    form.patchValue(patch);
    this.formSubject.next(form);
  }

  setForm(form: FormGroup) {
    this.formSubject.next(form);
  }

  resetForm() {
    this.getForm().reset({
      studentID: this.formSubject.getValue().get('studentID')?.value+1,
      existingGuardianId: null,
      primaryData: {
        studentFirstName: '',
        studentMiddleName: '',
        studentLastName: '',
        studentFirstNameEng: '',
        studentMiddleNameEng: '',
        studentLastNameEng: '',
        studentGender: 'Male',
        studentDOB: '',
        studentPassword: 'Student',
        classID: null,
        amount: 0,
        divisionID: null,
        studentAddress: '',
      },
      optionData: {
        placeBirth: '',
        studentPhone: 776137120,
        studentAddress: '',
      },
      guardian: {
        guardianFullName: '',
        guardianType: '',
        guardianEmail: '',
        guardianPassword: 'Guardian',
        guardianPhone: '',
        guardianGender: 'Male',
        guardianDOB: '',
        guardianAddress: ''
      },
      fees: {
        discounts: []
      },
      documents: {
        attachments: []
      }
    });
    this.formSubject.next(this.getForm());
  }

  //this is emiting Student Full Name
  private emitFullName(v: any) {
    const full = `${v.studentFirstName} ${v.studentMiddleName} ${v.studentLastName}`.trim();
    this.fullNameSubject.next(full);
  }

}
