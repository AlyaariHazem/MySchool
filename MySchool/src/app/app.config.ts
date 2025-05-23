import { ApplicationConfig, importProvidersFrom, isDevMode } from '@angular/core';
import { provideRouter, withComponentInputBinding } from '@angular/router';

import { routes } from './app.routes';
// Removed provideClientHydration since SSR is not being used
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { HTTP_INTERCEPTORS, provideHttpClient, withFetch, withInterceptorsFromDi } from '@angular/common/http';
import { provideToastr } from 'ngx-toastr';
import { provideAnimations } from '@angular/platform-browser/animations';
import { TokenInterceptor } from './core/interceptors/token.interceptor';
import { counterReducer } from './core/store/counter/counter.reducer';
import { provideStore } from '@ngrx/store';
import { languageReduser } from './core/store/language/language.reducer';
import { provideEffects } from '@ngrx/effects';
import { LanguageEffects } from './core/store/language/language.effect';
import { provideStoreDevtools } from '@ngrx/store-devtools';
import { AppTranslateModule } from './shared/modules/app-translate.module';
import { providePrimeNG } from 'primeng/config';
import Aura from '@primeng/themes/aura';;

export const appConfig: ApplicationConfig = {
    providers: [
        provideRouter(routes, withComponentInputBinding()),
        importProvidersFrom(AppTranslateModule.forRoot()),
        provideAnimationsAsync(),
        providePrimeNG({
            theme: {
                preset: Aura,
                options: {
                    darkModeSelector: false || 'none'
                }
            }
        }),
        
        provideHttpClient(
            withFetch(),
            withInterceptorsFromDi()
        ),

        provideAnimations(), // required animations providers
        provideStore({
            counter : counterReducer,
            language: languageReduser,
          }),
        provideEffects([LanguageEffects]),
        provideToastr({
            timeOut: 5000,
        }),
        {
            provide: HTTP_INTERCEPTORS,
            useClass: TokenInterceptor,
            multi: true,
        },
        // {provide: HTTP_INTERCEPTORS, useClass: IntercepterService, multi: true},
        provideStoreDevtools({ maxAge: 25, logOnly: !isDevMode() })
    ]
};
