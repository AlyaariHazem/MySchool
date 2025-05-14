import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

import { AdminModule } from "./components/admin/admin.module";
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';
import { PrimeNG } from 'primeng/config';
import { ShardModule } from './shared/shard.module';

@Component({
    selector: 'app-root',
    standalone: true,
    templateUrl: './app.component.html',
    styleUrl: './app.component.scss',
    imports: [RouterOutlet, AdminModule,ShardModule]
})
export class AppComponent {
  title = 'MySchool';

  lang$:Observable<string>
  constructor(private store:Store<{language:string}>,private primeng: PrimeNG) {
    this.lang$=this.store.select("language");
  }

  ngOnInit(): void {
    
    this.primeng.zIndex = {
      modal: 1100,    // dialog, sidebar
      overlay: 1000,  // dropdown, overlaypanel
      menu: 1000,     // overlay menus
      tooltip: 1100   // tooltip
  };
  }
}
