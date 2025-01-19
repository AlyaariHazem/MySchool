import { inject, Injectable } from '@angular/core';
import { Store } from '@ngrx/store';

@Injectable({
  providedIn: 'root'
})
export class LanguageService {

    langDir!:string;
    languageStore=inject(Store);
    dir:string="ltr";
    currentLanguage():void{
      this.languageStore.select("language").subscribe((lang: string) => {
        this.langDir=lang;
        this.dir=(lang=="en")?"ltr":"rtl";
      });
    }
}
