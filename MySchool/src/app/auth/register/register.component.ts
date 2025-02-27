import { Component, Inject, ViewChild, AfterViewInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { NgForm } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { AuthAPIService } from '../authAPI.service';
import { User } from '../../core/models/user.model';
import { ShardModule } from '../../shared/shard.module';
import { DropdownModule } from 'primeng/dropdown';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ShardModule, DropdownModule],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss', '../../shared/styles/style-select.scss']
})
export class RegisterComponent implements AfterViewInit {
  @ViewChild('registerForm') registerForm!: NgForm; // Get the form reference

  registerError: string | null = null;

  user: User = {
    userName: '',
    email: '',
    password: '',
    userType: ''
  };
  userTypes = [
    { label: 'مدير', value: 'Admin' },
    { label: 'معلم', value: 'Teacher' },
    { label: 'طالب', value: 'Student' },
    { label: 'ولي أمر', value: 'Guardian' },
    { label: 'موظف', value: 'Employee' }
  ];
  selectedUserType: string = '';

  constructor(
    private authService: AuthAPIService,
    private toastr: ToastrService,
    public dialogRef: MatDialogRef<RegisterComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {
    // Do not call resetForm() here because registerForm is not yet available.
  }

  ngAfterViewInit(): void {
    // Now the registerForm is available.
    this.resetForm();
  }

  register(registerForm:NgForm) {
    if (registerForm.valid) {
      this.authService.register(this.user).subscribe({
        next: () => {
          this.toastr.success('تم إنشاء الحساب بنجاح.');
          console.log("the form register is ",registerForm.value);
          console.log("the form user is ",this.user);
          this.resetForm(); // Reset form on success
        },
        error: (error) => {
          this.registerError = error.error?.message || 'فشل في التسجيل';
          console.error('Error when registering:', this.registerError);
          this.toastr.error('حدث خطأ أثناء التسجيل.');
        }
      });
    } else {
      this.toastr.error('الرجاء تعبئة جميع الحقول المطلوبة.');
    }
  }

  resetForm() {
    if (this.registerForm) {
      this.registerForm.resetForm(); // Resets the form including validation errors
    }
    this.user = { userName: '', email: '', password: '', userType: '' }; // Reset model
    this.selectedUserType = ''; // Reset dropdown
  }

  closeModal(): void {
    this.dialogRef.close();
  }
}
