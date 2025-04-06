import { Component, inject } from '@angular/core';
import { ToastrService } from 'ngx-toastr';

import { User } from '../../core/models/user.model';
import { AuthAPIService } from '../authAPI.service';
import { ShardModule } from '../../shared/shard.module';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    ShardModule,
  ],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
})
export class LoginComponent {
  private authService = inject(AuthAPIService);
  private toastr = inject(ToastrService);

  // Track the loading state
  isLoading = false;
  visible: boolean = false;

    showDialog() {
        this.visible = true;
    }

    hideDialog() {
        this.visible = false;
        this.isLoading = false;
    }

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

    this.isLoading = true; // Start loading
    this.showDialog();
    this.authService.login(user).subscribe({
      next: (response: any) => {
        if (response && response.token) {
          if (user.userType === 'MANAGER') {
            this.authService.router.navigateByUrl('admin');
          } else {
            this.authService.router.navigateByUrl('school');
          }
        }
      },
      error: () => {
        this.isLoading = false; // Stop loading on error
        this.hideDialog();
        this.toastr.error('فشل تسجيل الدخول');
      },
    });
  }

  openRegisterDialog(): void {
    this.toastr.info('فتح نافذة التسجيل');
  }
}
