import { Component, Input } from '@angular/core';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';

import { AdminModule } from "./components/admin/admin.module";
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';
import { PrimeNG } from 'primeng/config';

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
  constructor(private router: Router,private store:Store<{language:string}>,private primeng: PrimeNG) {
    this.lang$=this.store.select("language");
  }

  ngOnInit(): void {
    this.router.events.subscribe(event => {
      if (event instanceof NavigationEnd) {
        // Check the current route and update the visibility of the router-outlet
        this.showOutlet = !event.url.includes('/login');
      }
    });
    this.primeng.zIndex = {
      modal: 1100,    // dialog, sidebar
      overlay: 1000,  // dropdown, overlaypanel
      menu: 1000,     // overlay menus
      tooltip: 1100   // tooltip
  };
  }
}
