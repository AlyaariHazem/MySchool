import { inject, Injectable } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';

@Injectable({
  providedIn: 'root'
})
export class TranslationService {

  translateService = inject(TranslateService);

  changeLanguage(lang: string): void {
    // This will immediately update your HTML translation pipes
    this.translateService.use(lang);
  }

}
