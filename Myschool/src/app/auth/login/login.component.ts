import { Component, inject } from '@angular/core';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { RegisterComponent } from '../register/register.component';
import { ToastrService } from 'ngx-toastr';
import { ShardModule } from '../../shared/shard.module';
import { AuthAPIService } from '../authAPI.service';
import { User } from '../../core/models/user.model';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ShardModule, MatDialogModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
})
export class LoginComponent {
  private authService = inject(AuthAPIService);
  private toastr = inject(ToastrService);
  private dialog = inject(MatDialog);

  login(user:User): void {
    this.authService.login(user).subscribe({
      next: (response: any) => {
        if (response && response.token) {
          this.authService.router.navigateByUrl('school');
          this.toastr.success('Login successful');// this message appear but I can't navigate to school?
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
