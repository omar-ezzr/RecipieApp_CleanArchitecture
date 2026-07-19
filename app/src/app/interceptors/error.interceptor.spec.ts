import { HttpErrorResponse, HttpHandler, HttpRequest } from '@angular/common/http';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { of, throwError } from 'rxjs';
import { ErrorInterceptor } from './error.interceptor';
import { AuthService } from '../services/auth.service';

describe('ErrorInterceptor', () => {
  let auth: jasmine.SpyObj<AuthService>;
  let router: jasmine.SpyObj<Router>;
  let toastr: jasmine.SpyObj<ToastrService>;

  beforeEach(() => {
    auth = jasmine.createSpyObj<AuthService>('AuthService', ['refreshSession', 'logout']);
    router = jasmine.createSpyObj<Router>('Router', ['navigate']);
    toastr = jasmine.createSpyObj<ToastrService>('ToastrService', ['error']);
  });

  it('retries a 401 request after successful refresh', (done) => {
    auth.refreshSession.and.returnValue(of({ accessToken: 'new-access', refreshToken: 'new-refresh' }));
    const interceptor = new ErrorInterceptor(auth, router, toastr);
    const next = jasmine.createSpyObj<HttpHandler>('HttpHandler', ['handle']);
    next.handle.and.returnValues(
      throwError(() => new HttpErrorResponse({ status: 401 })),
      of({ ok: true } as any)
    );

    interceptor.intercept(new HttpRequest('GET', '/api/Recipes'), next).subscribe(() => {
      expect(next.handle).toHaveBeenCalledTimes(2);
      const retry = next.handle.calls.mostRecent().args[0] as HttpRequest<unknown>;
      expect(retry.headers.get('Authorization')).toBe('Bearer new-access');
      done();
    });
  });

  it('does not recursively refresh the refresh endpoint', (done) => {
    const interceptor = new ErrorInterceptor(auth, router, toastr);
    const next = jasmine.createSpyObj<HttpHandler>('HttpHandler', ['handle']);
    next.handle.and.returnValue(throwError(() => new HttpErrorResponse({ status: 401 })));

    interceptor.intercept(new HttpRequest('POST', '/api/Auth/refresh', {}), next).subscribe({
      error: () => {
        expect(auth.refreshSession).not.toHaveBeenCalled();
        done();
      }
    });
  });

  it('logs out once when refresh fails', (done) => {
    auth.refreshSession.and.returnValue(throwError(() => new Error('refresh failed')));
    const interceptor = new ErrorInterceptor(auth, router, toastr);
    const next = jasmine.createSpyObj<HttpHandler>('HttpHandler', ['handle']);
    next.handle.and.returnValue(throwError(() => new HttpErrorResponse({ status: 401 })));

    interceptor.intercept(new HttpRequest('GET', '/api/Recipes'), next).subscribe({
      error: () => {
        expect(auth.logout).toHaveBeenCalledTimes(1);
        expect(router.navigate).toHaveBeenCalledWith(['/login']);
        done();
      }
    });
  });

  it('logs out once for concurrent failed refresh retries', (done) => {
    auth.refreshSession.and.returnValue(throwError(() => new Error('refresh failed')));
    const interceptor = new ErrorInterceptor(auth, router, toastr);
    const next = jasmine.createSpyObj<HttpHandler>('HttpHandler', ['handle']);
    next.handle.and.returnValue(throwError(() => new HttpErrorResponse({ status: 401 })));
    let failures = 0;

    const assertDone = () => {
      failures++;

      if (failures === 2) {
        expect(auth.logout).toHaveBeenCalledTimes(1);
        expect(toastr.error).toHaveBeenCalledTimes(1);
        expect(router.navigate).toHaveBeenCalledTimes(1);
        done();
      }
    };

    interceptor.intercept(new HttpRequest('GET', '/api/Recipes'), next).subscribe({ error: assertDone });
    interceptor.intercept(new HttpRequest('GET', '/api/Categories'), next).subscribe({ error: assertDone });
  });
});
