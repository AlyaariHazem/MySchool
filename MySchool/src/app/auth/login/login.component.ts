import { Component, inject } from '@angular/core';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { RegisterComponent } from '../register/register.component';
import { ToastrService } from 'ngx-toastr';
import { ShardModule } from '../../shared/shard.module';
import { RouterOutlet } from '@angular/router';
import { AuthAPIService } from '../authAPI.service';
import { response } from 'express';
import { TranslateDirective } from '../../directives/translate.directive';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ShardModule, MatDialogModule,TranslateDirective],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
})
export class LoginComponent {
  private authService = inject(AuthAPIService);
  private toastr = inject(ToastrService);
  private dialog = inject(MatDialog);

  login(credentials: { username: string; password: string }): void {
    this.authService.login(credentials).subscribe({
      next: (response: any) => {
        if (response && response.token) {
          this.authService.router.navigateByUrl('school');
          this.toastr.success('Login successful');
        }
      },
      error: () => {
        this.toastr.error('Login failed');
      },
    });
  }

  openRegisterDialog(): void {
    this.dialog.open(RegisterComponent, {
      width: '400px',
    });
  }
}
