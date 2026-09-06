import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { ToastrService } from '@openng/ngx-toastr';
import { of, throwError } from 'rxjs';
import { LoginComponent } from './login.component';
import { AuthService } from '../services/auth.service';

describe('LoginComponent', () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;
  let auth: jasmine.SpyObj<AuthService>;
  let router: Router;

  beforeEach(async () => {
    auth = jasmine.createSpyObj<AuthService>('AuthService', ['login', 'saveTokens']);

    await TestBed.configureTestingModule({
      imports: [LoginComponent],
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: auth },
        { provide: ToastrService, useValue: jasmine.createSpyObj('ToastrService', ['success', 'error', 'warning', 'info']) }
      ]
    }).compileComponents();

    router = TestBed.inject(Router);
    spyOn(router, 'navigate').and.resolveTo(true);

    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
  });

  it('stores tokens and navigates after successful login', () => {
    auth.login.and.returnValue(of({ accessToken: 'access', refreshToken: 'refresh' }));

    component.email = 'user@example.com';
    component.password = 'password';
    component.login();

    expect(auth.login).toHaveBeenCalledWith({ email: 'user@example.com', password: 'password' });
    expect(auth.saveTokens).toHaveBeenCalledWith('access', 'refresh');
    expect(router.navigate).toHaveBeenCalledWith(['/recipes']);
  });

  it('shows an error when login fails', () => {
    const toastr = TestBed.inject(ToastrService) as jasmine.SpyObj<ToastrService>;
    auth.login.and.returnValue(throwError(() => new Error('bad credentials')));

    component.login();

    expect(toastr.error).toHaveBeenCalledWith('Invalid email or password');
  });
});
