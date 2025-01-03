import { ApplicationConfig} from '@angular/core';
import { provideRouter, withComponentInputBinding } from '@angular/router';

import { routes } from './app.routes';
// Removed provideClientHydration since SSR is not being used
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { HTTP_INTERCEPTORS, provideHttpClient, withFetch, withInterceptorsFromDi } from '@angular/common/http';
import { provideToastr } from 'ngx-toastr';
import { provideAnimations } from '@angular/platform-browser/animations';
import { TokenInterceptor } from './auth/interceptors/token.interceptor';
import { counterReducer } from './core/store/counter/counter.reducer';
import { provideStore } from '@ngrx/store';
import { languageReduser } from './core/store/language/language.reducer';
import { provideEffects } from '@ngrx/effects';
import { LanguageEffect } from './core/store/language/language.effect';
// import { IntercepterService } from './core/services/intercepter.service';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes, withComponentInputBinding()),
    provideAnimationsAsync(),
    provideHttpClient(withInterceptorsFromDi()),
    provideHttpClient(withFetch()),
    provideAnimations(), // required animations providers
    // Toastr providers
    provideStore({ counter: counterReducer }),
    provideStore({ language: languageReduser }),
    provideEffects([LanguageEffect]),

    provideToastr({
      timeOut: 5000,
    }), 
    {
      provide: HTTP_INTERCEPTORS,
      useClass: TokenInterceptor,
      multi: true,  // Allow multiple interceptors if needed
    }
    // {provide: HTTP_INTERCEPTORS, useClass: IntercepterService, multi: true}
  ]
};
