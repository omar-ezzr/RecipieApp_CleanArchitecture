import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { of } from 'rxjs';
import { authGuard } from './auth.guard';
import { AuthService } from '../services/auth.service';

describe('authGuard', () => {
  let auth: jasmine.SpyObj<AuthService>;

  beforeEach(() => {
    auth = jasmine.createSpyObj<AuthService>('AuthService', [
      'getAccessToken',
      'isTokenExpired',
      'restoreSession'
    ]);

    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: auth }
      ]
    });
  });

  it('allows navigation with a valid access token', () => {
    auth.getAccessToken.and.returnValue('token');
    auth.isTokenExpired.and.returnValue(false);

    const result = TestBed.runInInjectionContext(() => authGuard({} as any, {} as any));

    expect(result).toBeTrue();
  });

  it('restores navigation when access token is expired but refresh succeeds', (done) => {
    auth.getAccessToken.and.returnValue('expired');
    auth.isTokenExpired.and.returnValue(true);
    auth.restoreSession.and.returnValue(of(true));

    const result = TestBed.runInInjectionContext(() => authGuard({} as any, {} as any)) as any;

    result.subscribe((value: boolean) => {
      expect(value).toBeTrue();
      done();
    });
  });

  it('redirects when restoration fails', (done) => {
    auth.getAccessToken.and.returnValue(null);
    auth.restoreSession.and.returnValue(of(false));

    const result = TestBed.runInInjectionContext(() => authGuard({} as any, {} as any)) as any;
    const router = TestBed.inject(Router);

    result.subscribe((value: unknown) => {
      expect(value).toEqual(router.createUrlTree(['/login']));
      done();
    });
  });
});
