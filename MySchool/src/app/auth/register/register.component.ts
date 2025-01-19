import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { MatDialogModule } from '@angular/material/dialog';

import { ShardModule } from '../../shared/shard.module';
import { User } from '../../core/models/user.model';
import { AuthAPIService } from '../authAPI.service';

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

  register(user:User) {
    this.authService.register(user).subscribe({
      next: () => {
        this.router.navigate(['/login']); // Navigate to login or any other page on success
      },
      error: (error) => {
        this.registerError = error.error.message || 'Registration failed'; // Handle errors
        console.log('error when you register',this.registerError);
      }
    });
  }
}
