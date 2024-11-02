import { Component, inject, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import {  ToastrService } from 'ngx-toastr';


import { ShardModule } from '../../shared/shard.module';
import { StudentsServicesService } from '../../core/services/student.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ShardModule],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss'
})
export class RegisterComponent {
  form: FormGroup;
  name = "info";

  private toastService = inject(ToastrService);
  private studentService=inject(StudentsServicesService);
  
  sendNewStudent(): void {
    if (this.form.valid) {
      // this.addStudent();
    } else {
      console.log('Form is invalid:', this.form);
      // this.toastService.error('ادخل بيانات الطالب بالكامل');
    }
  }
  constructor( public dialogRef: MatDialogRef<RegisterComponent>,
    
    private formBuilder: FormBuilder){

      this.form = this.formBuilder.group({
        PlacePD: ['', Validators.required],
        StudentName: ['', Validators.required],
        PirthDate: '',
        LnameE: ["", [Validators.required]],
        TnameE: ["", [Validators.required]],
        SnameE: ["", [Validators.required]],
        fnameE: ["", [Validators.required]],
        Lname: ["", [Validators.required]],
        Tname: ["", [Validators.required]],
        Sname: ["", [Validators.required]],
        fname: ["", [Validators.required]],
        six: "male",
        city: "",
        section: "",
        phone: ["", [Validators.minLength(9)]],
        country: "ye",
        lSchool: "",
        class: "",
        discriptionJob: "",
        typeJob: "",
        parantJob: "",
        parantType: "father",
        parantEmail: "",
        parantPhone: ['', [Validators.required, Validators.minLength(8)]],
        ParantName: '',
        ParnatContryNum: "",
      });
    }
  
  onNoClick(): void {
      this.dialogRef.close();
      this.toastService.error('ادخل بيانات الطالب بالكامل');
  }
  onSubmit(): void {
    if (this.form.valid) {
      this.studentService.addStudent(this.form.value).subscribe(()=>{
        console.log('you sent the form',this.form.value);
      });
      this.dialogRef.close(this.form.value);
      this.toastService.success('تم إضافة الطالب بنجاح');
    } else {
      this.form.markAllAsTouched();      
      this.toastService.error('ادخل بيانات الطالب بالكامل');
    }
  }
}
