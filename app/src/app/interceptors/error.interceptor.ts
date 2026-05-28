import {
  HttpErrorResponse,
  HttpEvent,
  HttpHandler,
  HttpInterceptor,
  HttpRequest
} from '@angular/common/http';

import { Injectable } from '@angular/core';

import { Router } from '@angular/router';

import { Observable, throwError } from 'rxjs';

import { catchError, switchMap } from 'rxjs/operators';

import { ToastrService } from 'ngx-toastr';

import { AuthService } from '../services/auth.service';


@Injectable()

export class ErrorInterceptor implements HttpInterceptor {

  constructor(
    private auth: AuthService,
    private router: Router,
    private toastr: ToastrService
  ) {}


  intercept(
    req: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {

    return next.handle(req).pipe(

      catchError((error: HttpErrorResponse) => {

        if (
          error.status === 401 &&
          !req.url.includes('/auth/login') &&
          !req.url.includes('/auth/register') &&
          !req.url.includes('/auth/refresh')
        ) {

          const refreshToken =
            this.auth.getRefreshToken();

          if (!refreshToken) {

            this.forceLogout();

            return throwError(() => error);
          }

          return this.auth.refreshToken(refreshToken).pipe(

            switchMap((res: any) => {

              this.auth.saveTokens(
                res.accessToken,
                res.refreshToken
              );

              const clonedRequest = req.clone({
                setHeaders: {
                  Authorization: `Bearer ${res.accessToken}`
                }
              });

              return next.handle(clonedRequest);
            }),

            catchError((refreshError) => {

              this.forceLogout();

              return throwError(() => refreshError);
            })
          );
        }


        if (error.status === 403) {

          this.toastr.error(
            'Access denied'
          );
        }


        if (error.status === 500) {

          this.toastr.error(
            'Server error occurred'
          );
        }


        if (error.status === 0) {

          this.toastr.error(
            'Network error'
          );
        }

        return throwError(() => error);
      })
    );
  }


  private forceLogout() {

    this.auth.logout();

    this.toastr.error(
      'Session expired'
    );

    this.router.navigate([
      '/login'
    ]);
  }
}