import { Component } from '@angular/core';
import { AuthAPIService } from '../authAPI.service';  // Make sure this points to your service
import { Router } from '@angular/router';
import { ShardModule } from '../../shared/shard.module';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ShardModule, MatDialogModule],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss']
})
export class RegisterComponent {
  registerError: string | null = null;

  constructor(private authService: AuthAPIService, private router: Router) {}

  register(formValue: { userName: string; email: string; password: string }) {
    this.authService.register(formValue).subscribe({
      next: () => {
        this.router.navigate(['/login']); // Navigate to login or any other page on success
      },
      error: (error) => {
        this.registerError = error.error.message || 'Registration failed'; // Handle errors
      }
    });
  }
}
