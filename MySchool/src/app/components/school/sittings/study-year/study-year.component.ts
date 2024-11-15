import { Component, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { ClassDTO } from '../../../../core/models/class.model';


@Component({
  selector: 'app-study-year',
  templateUrl: './study-year.component.html',
  styleUrl: './study-year.component.scss'
})
export class StudyYearComponent {
  title = 'السنوات الدراسية';
  checkTOEdit = false;
  class:ClassDTO[]=[];
  
  toastr = inject(ToastrService);
  formBuilder = inject(FormBuilder);

  form: FormGroup = this.formBuilder.group({
    id: [''],
    name: ['', Validators.required],
    state: [''],
    level: '',
    totalStudents:0,
    type: [],
    semester:''
  });

  ngOnInit(): void {
    // this.refreshClass();
  }

  // refreshClass(): void {
  //   this.classService.getClass().subscribe(clas => {
  //     this.class = clas;
  //     console.log('Classes:', this.class);
  //   });
  // }

  close(): void {
    const modal = document.getElementById('id01');
    if (modal) {
      modal.style.display = 'none';
    }
  }
//this fucntion for display the edit form 
  patchClass(editClass: ClassDTO): void {
    const modal = document.getElementById('id01');
    if (modal) {
      this.checkTOEdit = true;
      modal.style.display = 'block';
      this.form.patchValue(editClass);
      console.log('Editing class:', editClass);
    }
  }

}
