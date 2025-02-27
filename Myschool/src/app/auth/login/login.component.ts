import { Component, inject } from '@angular/core';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { RegisterComponent } from '../register/register.component';
import { ToastrService } from 'ngx-toastr';
import { ShardModule } from '../../shared/shard.module';
import { AuthAPIService } from '../authAPI.service';
import { User } from '../../core/models/user.model';
import { MatSelectModule } from '@angular/material/select';

@Component({
  selector: 'app-login',
  standalone: true,
  // Import MatSelectModule to provide a value accessor for mat-select.
  imports: [ShardModule, MatDialogModule, MatSelectModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
})
export class LoginComponent {
  private authService = inject(AuthAPIService);
  private toastr = inject(ToastrService);
  private dialog = inject(MatDialog);

  // Define user types for the dropdown
  userTypes = [
    { label: 'طالب', value: 'STUDENT' },
    { label: 'معلم', value: 'TEACHER' },
    { label: 'ولي أمر', value: 'GUARDIAN' },
    { label: 'مدير', value: 'MANAGER' }
  ];

  login(user: User): void {
    if (!user.userType) {
      this.toastr.error('يرجى اختيار نوع المستخدم');
      return;
    }

    this.authService.login(user).subscribe({
      next: (response: any) => {
        if (response && response.token) {
          if (user.userType === 'MANAGER') {
            this.authService.router.navigateByUrl('admin');
          } else {
            this.authService.router.navigateByUrl('school');
          }
          this.toastr.success('تم تسجيل الدخول بنجاح');
        }
      },
      error: () => {
        this.toastr.error('فشل تسجيل الدخول');
      },
    });
  }

  openRegisterDialog(): void {
    this.dialog.open(RegisterComponent, {
      width: '400px',
    });
  }
}
