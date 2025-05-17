import {  createEffect } from "@ngrx/effects";

import { tap } from "rxjs";
import { Injectable } from "@angular/core";
import { Store } from "@ngrx/store";
import { TranslateService } from "@ngx-translate/core";
import { selectLanguage } from "./language.selectors";

@Injectable()
export class LanguageEffects {
  syncTranslate$ = createEffect(
    () => this.store.select(selectLanguage).pipe(
      tap(lang => {
        this.translate.use(lang);
        document.documentElement.dir =               // <html dir="rtl|ltr">
          lang === 'ar' ? 'rtl' : 'ltr';
      }),
    ),
    { dispatch: false },
  );

  constructor(
    private store: Store,
    private translate: TranslateService,
  ) {}
}