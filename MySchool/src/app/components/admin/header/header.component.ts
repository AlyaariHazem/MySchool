import { Component, inject, OnInit } from '@angular/core';
import { Store } from '@ngrx/store';

import { AuthAPIService } from '../../../auth/authAPI.service';
import { languageAction } from '../../../core/store/language/language.action';
// Import the translate service (assuming you are using ngx-translate)
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss'] // use plural "styleUrls" if you have more than one file
})
export class HeaderComponent implements OnInit {

  toggle: boolean = false;
  open() {
    this.toggle = !this.toggle;
  }
  private auth = inject(AuthAPIService);
  
  langDir!: string;
  langName!: string;
  languageimage!: string;
  languageStore = inject(Store);
  // Add the translateService injection:
  private translate = inject(TranslateService);

  dir: string = "ltr";

  currentLanguage(): void {
    this.languageStore.select("language").subscribe((res) => {
      this.langDir = res;
      if(this.langDir === 'en'){
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
  }
  
  Logout(){
    // For example: console.log('logout');
    this.auth.logout();
  }
}
