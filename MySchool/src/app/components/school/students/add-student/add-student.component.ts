import { Component, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';


@Component({
  selector: 'app-add-student',
  templateUrl: './add-student.component.html',
  styleUrl: './add-student.component.scss'
})
export class AddStudentComponent {
  form: FormGroup;
  name = "info";
  private toastService = inject(ToastrService);
  
  sendNewStudent(): void {
    if (this.form.valid) {
      // this.addStudent();
    } else {
      console.log('Form is invalid:', this.form);
      // this.toastService.error('ادخل بيانات الطالب بالكامل');
    }
  }
  constructor( private formBuilder: FormBuilder ,
    ){

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
    
  onSubmit(): void {
   
  }
  fileImage!:File;
  validateImageFile(event: any) {
    const file = event.target.files[0];
    const input=event.target as HTMLElement;
    console.log('input',input);
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
