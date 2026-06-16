/* header.component.ts */
import { Component, inject, signal } from '@angular/core';
import { Store } from '@ngrx/store';
import { TranslateService } from '@ngx-translate/core';
import { AuthAPIService } from '../../../auth/authAPI.service';
import { languageAction } from '../../../core/store/language/language.action';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss'],
})
export class HeaderComponent {
  /* one signal drives everything */
  isSidebarOpen = signal(false);

  /* ------------ language / user -------------------- */
  dir = 'ltr';
  langName = 'English';
  languageImage = 'Amarica.jpg';
  userName = localStorage.getItem('userName') ?? '';

  /* ------------ injected services ------------------ */
  private store = inject(Store);
  private translate = inject(TranslateService);
  private auth = inject(AuthAPIService);

  constructor() {
    /* keep language reactive */
    this.store.select('language').subscribe(lang => {
      this.dir = lang === 'en' ? 'ltr' : 'rtl';
      this.langName = lang === 'en' ? 'English' : 'العربية';
      this.languageImage = lang === 'en' ? 'Amarica.jpg' : 'SudiAribia.jpg';
      this.translate.use(lang);
    });
  }

  /* ------------ UI actions ------------------------- */
  toggleSidebar() { this.isSidebarOpen.update(v => !v); }
  onSidebarClosed() { this.isSidebarOpen.set(false); }
  changeLang(lang: string) {
    this.store.dispatch(languageAction({ lang }));
    localStorage.setItem('lang', lang);
    this.translate.use(lang);
  }
  logout() { this.auth.logout().subscribe(); }
}
