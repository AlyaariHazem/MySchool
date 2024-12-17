import { Component, inject } from '@angular/core';
import { AuthAPIService } from '../../../auth/authAPI.service';

@Component({
    selector: 'app-header',
    templateUrl: './header.component.html',
    styleUrl: './header.component.scss'
})
export class HeaderComponent {
  toggle:boolean=false;
  open(){
    this.toggle=!this.toggle;
  }
  private auth = inject(AuthAPIService);

  Logout(){
    // console.log('logout');
    this.auth.logout();
  }
}
