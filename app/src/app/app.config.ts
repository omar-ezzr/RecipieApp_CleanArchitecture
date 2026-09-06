import {
  ApplicationConfig,
  provideZoneChangeDetection
} from '@angular/core';

import {
  provideHttpClient,
  withInterceptorsFromDi,
  HTTP_INTERCEPTORS,
  withXhr
} from '@angular/common/http';

import { provideRouter } from '@angular/router';

import { provideAnimations } from '@angular/platform-browser/animations';

import { provideToastr } from '@openng/ngx-toastr';

import { routes } from './app.routes';

import { AuthInterceptor } from './interceptors/auth.interceptor';
import { ErrorInterceptor } from './interceptors/error.interceptor';


export const appConfig: ApplicationConfig = {

  providers: [

    provideZoneChangeDetection({
      eventCoalescing: true
    }),

    provideRouter(routes),

    provideHttpClient(withXhr(), 
      withInterceptorsFromDi()
    ),

    provideAnimations(),

    provideToastr({
      timeOut: 3000,
      positionClass: 'toast-top-right',
      preventDuplicates: true
    }),{
      provide: HTTP_INTERCEPTORS,
      useClass: AuthInterceptor,
      multi: true
    },

    {
      provide: HTTP_INTERCEPTORS,
      useClass: ErrorInterceptor,
      multi: true
    }
  ]
};
