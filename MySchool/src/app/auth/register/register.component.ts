import { Component, Inject, ViewChild } from '@angular/core';
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
export class RegisterComponent {
  @ViewChild('registerForm') registerForm!: NgForm; // Get the form reference

  registerError: string | null = null;
  user: User = {
    userName: '',
    email: '',
    password: '',
    userType: ''
  };
  userTypes = [
    { label: 'مشرف', value: 'Admin' },
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
    this.resetForm(); // ✅ Initialize form
  }

  register() {
    this.user.userType = this.selectedUserType; // Assign userType from dropdown

    console.log('Registering user:', this.user);

    if (this.user.userName && this.user.password && this.user.email && this.user.userType) {
      this.authService.register(this.user).subscribe({
        next: () => {
          this.toastr.success('تم إنشاء الحساب بنجاح.');
          this.resetForm(); // ✅ Reset form on success
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
    this.registerForm.resetForm(); // ✅ Resets the form including validation errors
    this.user = { userName: '', email: '', password: '', userType: '' }; // ✅ Reset model
    this.selectedUserType = ''; // ✅ Reset dropdown
  }

  closeModal(): void {
    this.dialogRef.close();
  }
}
