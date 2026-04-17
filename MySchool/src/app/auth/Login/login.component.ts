import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { ToastrService } from 'ngx-toastr';

import { User } from '../../core/models/user.model';
import { AuthAPIService } from '../authAPI.service';
import { PublicRecruitmentTopbarComponent } from '../../shared/components/public-recruitment-topbar/public-recruitment-topbar.component';
import { ShardModule } from '../../shared/shard.module';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    ShardModule,
    PublicRecruitmentTopbarComponent,
    RouterLink,
    MatButtonModule,
    MatCardModule,
    MatDividerModule,
    MatIconModule,
  ],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
})
export class LoginComponent {
  private authService = inject(AuthAPIService);
  private toastr = inject(ToastrService);

  /** True while login HTTP request is in flight. */
  isLoading = false;

  // Define user types for the dropdown
  userTypes = [
    { label: 'Admin', value: 'ADMIN' },
    { label: 'طالب', value: 'STUDENT' },
    { label: 'معلم', value: 'TEACHER' },
    { label: 'ولي أمر', value: 'GUARDIAN' },
    { label: 'مدير', value: 'MANAGER' }
  ];

  login(user: User): void {
    if (this.isLoading) {
      return;
    }
    if (!user.userType) {
      this.toastr.error('يرجى اختيار نوع المستخدم');
      return;
    }

    this.isLoading = true;
    this.authService.login(user).pipe(
      finalize(() => {
        this.isLoading = false;
      }),
    ).subscribe({
      next: (response: any) => {
        if (response && response.token) {
          if (user.userType === 'ADMIN') {
            this.authService.router.navigateByUrl('admin');
          } else if (user.userType === 'TEACHER') {
            this.authService.router.navigateByUrl('/teacher');
          } else if (user.userType === 'STUDENT') {
            this.authService.router.navigateByUrl('/students/home');
            const name = response.managerName || response.userName || '';
            if (name) {
              this.toastr.success('مرحبا بك : ' + name, '', { positionClass: 'toast-center-center' });
            }
          } else if (user.userType === 'GUARDIAN') {
            this.authService.router.navigateByUrl('/guardian/home');
            const name = response.managerName || response.userName || '';
            if (name) {
              this.toastr.success('مرحبا بك : ' + name, '', { positionClass: 'toast-center-center' });
            }
          } else {
            this.authService.router.navigateByUrl('school');
            console.log(response);
            this.toastr.success('مرحبا بك : ' + response.managerName, '', {
              positionClass: 'toast-center-center'
            });
          }
        }
      },
      error: () => {
        this.toastr.error('فشل تسجيل الدخول');
      },
    });
  }

  /** Placeholder until a real password-reset flow exists. */
  openForgotPasswordHint(): void {
    if (this.isLoading) {
      return;
    }
    this.toastr.info('تواصل مع إدارة المدرسة لاستعادة كلمة السر.', '', {
      timeOut: 5000,
    });
  }
}
