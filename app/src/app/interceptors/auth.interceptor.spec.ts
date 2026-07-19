import { HttpHandler, HttpRequest } from '@angular/common/http';
import { of } from 'rxjs';
import { AuthInterceptor } from './auth.interceptor';
import { AuthService } from '../services/auth.service';

describe('AuthInterceptor', () => {
  it('adds the bearer token when one exists', () => {
    const auth = jasmine.createSpyObj<AuthService>('AuthService', ['getAccessToken']);
    auth.getAccessToken.and.returnValue('access');
    const interceptor = new AuthInterceptor(auth);
    const next = jasmine.createSpyObj<HttpHandler>('HttpHandler', ['handle']);
    next.handle.and.returnValue(of({} as any));

    interceptor.intercept(new HttpRequest('GET', '/api/Recipes'), next);

    const handledRequest = next.handle.calls.mostRecent().args[0] as HttpRequest<unknown>;
    expect(handledRequest.headers.get('Authorization')).toBe('Bearer access');
  });
});
