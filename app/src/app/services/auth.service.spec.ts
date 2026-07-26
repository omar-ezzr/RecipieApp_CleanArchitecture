import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { AuthService } from './auth.service';

describe('AuthService', () => {
  let service: AuthService;
  let http: HttpTestingController;

  beforeEach(() => {
    clearAuthStorage();

    TestBed.configureTestingModule({
      providers: [
        AuthService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });

    service = TestBed.inject(AuthService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    http.verify();
    clearAuthStorage();
  });

  it('shares one refresh request for concurrent callers', () => {
    localStorage.setItem('refreshToken', 'refresh');
    const results: string[] = [];

    service.refreshSession().subscribe(tokens => results.push(tokens.accessToken));
    service.refreshSession().subscribe(tokens => results.push(tokens.accessToken));

    const req = http.expectOne('http://localhost:5130/api/auth/refresh');
    expect(req.request.method).toBe('POST');
    req.flush({ accessToken: 'new-access', refreshToken: 'new-refresh' });

    expect(results).toEqual(['new-access', 'new-access']);
    expect(localStorage.getItem('accessToken')).toBe('new-access');
    expect(localStorage.getItem('refreshToken')).toBe('new-refresh');
  });

  it('resets the shared refresh request after success', () => {
    localStorage.setItem('refreshToken', 'refresh-one');

    service.refreshSession().subscribe();
    http.expectOne('http://localhost:5130/api/auth/refresh')
      .flush({ accessToken: 'access-one', refreshToken: 'refresh-two' });

    service.refreshSession().subscribe();
    http.expectOne('http://localhost:5130/api/auth/refresh')
      .flush({ accessToken: 'access-two', refreshToken: 'refresh-three' });

    expect(localStorage.getItem('accessToken')).toBe('access-two');
    expect(localStorage.getItem('refreshToken')).toBe('refresh-three');
  });

  it('resets the shared refresh request after failure', () => {
    localStorage.setItem('refreshToken', 'refresh-one');

    service.refreshSession().subscribe({ error: () => undefined });
    http.expectOne('http://localhost:5130/api/auth/refresh').flush(
      { title: 'Invalid refresh token' },
      { status: 401, statusText: 'Unauthorized' }
    );

    service.refreshSession().subscribe();
    http.expectOne('http://localhost:5130/api/auth/refresh')
      .flush({ accessToken: 'access-two', refreshToken: 'refresh-two' });

    expect(localStorage.getItem('accessToken')).toBe('access-two');
  });

  it('logs out when refresh fails', () => {
    localStorage.setItem('accessToken', 'old-access');
    localStorage.setItem('refreshToken', 'refresh');

    service.restoreSession().subscribe(restored => expect(restored).toBeFalse());

    http.expectOne('http://localhost:5130/api/auth/refresh').flush(
      { title: 'Invalid refresh token' },
      { status: 401, statusText: 'Unauthorized' }
    );

    expect(localStorage.getItem('accessToken')).toBeNull();
    expect(localStorage.getItem('refreshToken')).toBeNull();
  });

  it('reads user id and role claims and allows recipe management for authenticated users', () => {
    localStorage.setItem('accessToken', createToken({
      'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier': 'user-1',
      'http://schemas.microsoft.com/ws/2008/06/identity/claims/role': 'User',
      exp: Math.floor(Date.now() / 1000) + 300
    }));

    expect(service.getCurrentUserId()).toBe('user-1');
    expect(service.getCurrentRole()).toBe('User');
    expect(service.canManageRecipes()).toBeTrue();
  });
});

function clearAuthStorage(): void {
  localStorage.removeItem('accessToken');
  localStorage.removeItem('refreshToken');
}

function createToken(payload: object): string {
  return [
    btoa(JSON.stringify({ alg: 'none' })),
    btoa(JSON.stringify(payload)),
    'signature'
  ].join('.');
}
