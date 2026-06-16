import { Component } from '@angular/core';

@Component({
  selector: 'app-edit-parents',
  templateUrl: './edit-parents.component.html',
  styleUrl: './edit-parents.component.scss'
})
export class EditParentsComponent {
  guardian = {
    guardianFullName: '',
    guardianType: '',
    guardianEmail: '',
    guardianPhone: '',
    guardianDOB: '',
    guardianAddress: ''
  };

  onSubmit(form: any) {
    if (form.valid) {
      console.log('تم الإرسال بنجاح:', this.guardian);
    } else {
      console.warn('النموذج غير صالح');
    }
  }
}
