import { Component, inject, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';

import { DialogData } from '../all-students/all-students.component';
import { StudentsServicesService } from '../../../../core/services/student.service';
import { UploadImageService } from '../../../../core/services/upload-image.service';
import { Students } from '../../../../core/models/students.model';

@Component({
  selector: 'app-edit-student',
  templateUrl: './edit-student.component.html',
  styleUrls: ['./edit-student.component.scss']
})
export class EditStudentComponent implements OnInit {
  form: FormGroup;

  private toastService = inject(ToastrService);
  private studentService = inject(StudentsServicesService);
  private uploadImageService = inject(UploadImageService);

  constructor(private formBuilder: FormBuilder,
              public dialogRef: MatDialogRef<EditStudentComponent>,
              @Inject(MAT_DIALOG_DATA) public data: DialogData) {

    this.form = this.formBuilder.group({
      PlacePD: ['', Validators.required],
      ContryNum: ['', Validators.required],
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
      image: "",
    });
  }

  ngOnInit(): void {
    this.patchFormValue();
  }

  patchFormValue(): void {
    this.form.patchValue({
      PlacePD: this.data.student.PlacePD,
      ContryNum: this.data.student.ContryNum,
      PirthDate: this.data.student.PirthDate,
      LnameE: this.data.student.LnameE,
      TnameE: this.data.student.TnameE,
      SnameE: this.data.student.SnameE,
      fnameE: this.data.student.fnameE,
      Lname: this.data.student.Lname,
      Tname: this.data.student.Tname,
      Sname: this.data.student.Sname,
      fname: this.data.student.fname,
      six: this.data.student.six,
      city: this.data.student.city,
      section: this.data.student.section,
      phone: this.data.student.phone,
      country: this.data.student.country,
      lSchool: this.data.student.lSchool,
      class: this.data.student.class,
      discriptionJob: this.data.student.discriptionJob,
      typeJob: this.data.student.typeJob,
      parantJob: this.data.student.parantJob,
      parantType: this.data.student.parantType,
      parantEmail: this.data.student.parantEmail,
      parantPhone: this.data.student.parantPhone,
      ParantName: this.data.student.ParantName,
      ParnatContryNum: this.data.student.ParnatContryNum,
      image: this.data.student.image
    });
  }

  closeEdit(): void {
    this.dialogRef.close(null);
  }

  onSubmit(): void {
    if (this.form.valid) {
      this.editStudent();
    } else {
      this.form.markAllAsTouched();
      this.toastService.error('ادخل بيانات الطالب بالكامل');
    }
  }

  editStudent(): void {
    const student: Students = {
      ...this.form.value,
      id: this.data.student.id
    };
    const imageFile = this.form.get('image')?.value;
    if (imageFile) {
      this.uploadImageService.uploadImage(imageFile).subscribe(() => {
        // handle the image upload if necessary
      });
    }
    this.studentService.editStudent(student).subscribe((result) => {
      this.dialogRef.close(result);
      this.toastService.success('تم تعديل الطالب بنجاح');
    });
  }

  validateImageFile(event: any): void {
    const file = event.target.files[0];
    const allowedExtensions = ['jpg', 'jpeg', 'png', 'gif'];

    if (file) {
      const fileExtension = file.name.split('.').pop().toLowerCase();

      if (!allowedExtensions.includes(fileExtension)) {
        this.toastService.error('(JPG, JPEG, PNG or GIF) يجب أن يكون أمتداد الصورة ');
        // Reset the file input
        event.target.value = '';
      }
    }
  }
}
