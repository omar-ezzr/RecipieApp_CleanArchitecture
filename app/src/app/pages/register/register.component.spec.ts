import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { RegisterComponent } from './register.component';
import { AuthService } from '../../services/auth.service';

describe('RegisterComponent', () => {
  let component: RegisterComponent;
  let fixture: ComponentFixture<RegisterComponent>;
  let auth: jasmine.SpyObj<AuthService>;
  let router: Router;

  beforeEach(async () => {
    auth = jasmine.createSpyObj<AuthService>('AuthService', ['register']);

    await TestBed.configureTestingModule({
      imports: [RegisterComponent],
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: auth },
      ]
    }).compileComponents();

    router = TestBed.inject(Router);
    spyOn(router, 'navigate').and.resolveTo(true);

    fixture = TestBed.createComponent(RegisterComponent);
    component = fixture.componentInstance;
  });

  it('handles successful registration', fakeAsync(() => {
    auth.register.and.returnValue(of({ message: 'Account created and waiting for administrator approval.' }));

    component.email = 'new@example.com';
    component.password = 'password';
    component.register();
    tick(1500);

    expect(component.message).toBe('Account created and waiting for administrator approval.');
    expect(router.navigate).toHaveBeenCalledWith(['/login']);
  }));

  it('shows backend errors', () => {
    auth.register.and.returnValue(throwError(() => ({ error: { error: 'User already exists' } })));

    component.register();

    expect(component.error).toBe('User already exists');
  });
});
