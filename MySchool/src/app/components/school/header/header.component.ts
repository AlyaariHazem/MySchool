import { Component, inject, OnInit } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { Store } from '@ngrx/store';

import { AuthAPIService } from '../../../auth/authAPI.service';
import { languageAction } from '../../../core/store/language/language.action';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss'] // use plural "styleUrls" if you have more than one file
})
export class HeaderComponent implements OnInit {
  currentUserName: string = '';
  userName: string = '';
  toggle: boolean = false;

  languageStore = inject(Store);
  private auth = inject(AuthAPIService);
  private toaster = inject(ToastrService);
  private translate = inject(TranslateService);



  langDir!: string;
  langName!: string;
  languageimage!: string;


  dir: string = "ltr";

  open() {
    this.toggle = !this.toggle;
  }

  currentLanguage(): void {
    this.languageStore.select("language").subscribe((res) => {
      this.langDir = res;
      if (this.langDir === 'en') {
        this.languageimage = 'Amarica.jpg';
      } else {
        this.languageimage = 'SudiAribia.jpg';
      }
      this.dir = (res === "en") ? "ltr" : "rtl";
      this.langName = (res === "en") ? "English" : "العربية";
      // Tell the translation service to use the current language:
      this.translate.use(this.langDir);
    });
  }

  changeLang(changeLang: string): void {
    // Dispatch the new language to the store
    this.languageStore.dispatch(languageAction({ lang: changeLang }));
    // Also update the translation service immediately. This helps if you want an instant change.
    this.translate.use(changeLang);
  }

  ngOnInit(): void {
    this.currentLanguage();
    this.userName = localStorage.getItem('userName') || '';
    this.currentUserName = localStorage.getItem('managerName') || '';
    this.toaster.success('مرحبا بك : ' + this.currentUserName, '', {
      positionClass: 'toast-center-center'
    });
  }


  Logout() {
    // For example: console.log('logout');
    this.auth.logout();
  }
}
