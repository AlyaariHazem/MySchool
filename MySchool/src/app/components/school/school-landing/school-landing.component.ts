import { Component, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';

/**
 * Default child under /school: managers and staff land on the admin dashboard (students use /students/home).
 */
@Component({
  selector: 'app-school-landing',
  template: '',
})
export class SchoolLandingComponent implements OnInit {
  private router = inject(Router);

  ngOnInit(): void {
    const ut = typeof localStorage !== 'undefined' ? localStorage.getItem('userType') : '';
    if (ut === 'STUDENT') {
      void this.router.navigateByUrl('/students/home', { replaceUrl: true });
    } else if (ut === 'GUARDIAN') {
      void this.router.navigateByUrl('/guardian/home', { replaceUrl: true });
    } else {
      void this.router.navigateByUrl('/school/dashboard', { replaceUrl: true });
    }
  }
}
