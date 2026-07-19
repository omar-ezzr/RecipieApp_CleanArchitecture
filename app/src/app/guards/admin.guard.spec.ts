import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { of } from 'rxjs';
import { adminGuard } from './admin.guard';
import { AuthService } from '../services/auth.service';

describe('adminGuard', () => {
  let auth: jasmine.SpyObj<AuthService>;

  beforeEach(() => {
    auth = jasmine.createSpyObj<AuthService>('AuthService', [
      'getAccessToken',
      'isTokenExpired',
      'isAdmin',
      'restoreSession'
    ]);

    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: auth }
      ]
    });
  });

  it('allows valid admins', () => {
    auth.getAccessToken.and.returnValue('token');
    auth.isTokenExpired.and.returnValue(false);
    auth.isAdmin.and.returnValue(true);

    const result = TestBed.runInInjectionContext(() => adminGuard({} as any, {} as any));

    expect(result).toBeTrue();
  });

  it('redirects authenticated non-admins to recipes', () => {
    auth.getAccessToken.and.returnValue('token');
    auth.isTokenExpired.and.returnValue(false);
    auth.isAdmin.and.returnValue(false);
    const router = TestBed.inject(Router);

    const result = TestBed.runInInjectionContext(() => adminGuard({} as any, {} as any));

    expect(result).toEqual(router.createUrlTree(['/recipes']));
  });

  it('redirects unauthenticated users to login after failed restore', (done) => {
    auth.getAccessToken.and.returnValue(null);
    auth.restoreSession.and.returnValue(of(false));
    const router = TestBed.inject(Router);

    const result = TestBed.runInInjectionContext(() => adminGuard({} as any, {} as any)) as any;

    result.subscribe((value: unknown) => {
      expect(value).toEqual(router.createUrlTree(['/login']));
      done();
    });
  });
});
