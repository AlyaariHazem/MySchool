import { Component, Input } from '@angular/core';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';

import { LoginComponent } from "./auth/login/login.component";
import { AdminModule } from "./components/admin/admin.module";

@Component({
    selector: 'app-root',
    standalone: true,
    templateUrl: './app.component.html',
    styleUrl: './app.component.scss',
    imports: [RouterOutlet, LoginComponent, AdminModule]
})
export class AppComponent {
  title = 'MySchool';
  showOutlet: boolean = true;

  @Input() userIsAdmin=true;

  constructor(private router: Router) {}

  ngOnInit(): void {
    this.router.events.subscribe(event => {
      if (event instanceof NavigationEnd) {
        // Check the current route and update the visibility of the router-outlet
        this.showOutlet = !event.url.includes('/login');
      }
    });
  }
}
