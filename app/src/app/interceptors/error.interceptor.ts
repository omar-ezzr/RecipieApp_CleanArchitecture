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
  private logoutInProgress = false;

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

        if (error.status === 401 && !this.isAuthEndpoint(req)) {
          return this.auth.refreshSession().pipe(
            switchMap((tokens) => {
              const clonedRequest = req.clone({
                setHeaders: {
                  Authorization: `Bearer ${tokens.accessToken}`
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
    if (this.logoutInProgress) {
      return;
    }

    this.logoutInProgress = true;

    this.auth.logout();

    this.toastr.error(
      'Session expired'
    );

    this.router.navigate([
      '/login'
    ]);
  }

  private isAuthEndpoint(req: HttpRequest<any>): boolean {
    const url = req.url.toLowerCase();

    return url.includes('/api/auth/login') ||
      url.includes('/api/auth/register') ||
      url.includes('/api/auth/refresh') ||
      url.includes('/auth/login') ||
      url.includes('/auth/register') ||
      url.includes('/auth/refresh');
  }
}
