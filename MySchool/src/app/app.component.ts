import { Component, Input } from '@angular/core';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';

import { AdminModule } from "./components/admin/admin.module";
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';

@Component({
    selector: 'app-root',
    standalone: true,
    templateUrl: './app.component.html',
    styleUrl: './app.component.scss',
    imports: [RouterOutlet, AdminModule]
})
export class AppComponent {
  title = 'MySchool';
  showOutlet: boolean = true;

  @Input() userIsAdmin=true;
  lang$:Observable<string>
  constructor(private router: Router,private store:Store<{language:string}>) {
    this.lang$=this.store.select("language");
  }

  ngOnInit(): void {
    this.router.events.subscribe(event => {
      if (event instanceof NavigationEnd) {
        // Check the current route and update the visibility of the router-outlet
        this.showOutlet = !event.url.includes('/login');
      }
    });
  }
}
