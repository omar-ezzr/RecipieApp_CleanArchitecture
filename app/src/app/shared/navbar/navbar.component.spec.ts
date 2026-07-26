import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { NavbarComponent } from './navbar.component';
import { AuthService } from '../../services/auth.service';

describe('NavbarComponent', () => {
  let component: NavbarComponent;
  let fixture: ComponentFixture<NavbarComponent>;
  let auth: jasmine.SpyObj<AuthService>;

  beforeEach(async () => {
    auth = jasmine.createSpyObj<AuthService>('AuthService', ['isLoggedIn', 'logout', 'isAdmin']);

    await TestBed.configureTestingModule({
      imports: [NavbarComponent],
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: auth }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(NavbarComponent);
    component = fixture.componentInstance;
  });

  it('reports login state from AuthService', () => {
    auth.isLoggedIn.and.returnValue(true);

    expect(component.isLoggedIn()).toBeTrue();
  });

  it('removes only auth tokens through AuthService on logout', () => {
    const router = TestBed.inject(Router);
    spyOn(router, 'navigate');

    component.logout();

    expect(auth.logout).toHaveBeenCalled();
    expect(router.navigate).toHaveBeenCalledWith(['/login']);
  });

  it('shows account-management navigation only for admins', () => {
    auth.isLoggedIn.and.returnValue(true);
    auth.isAdmin.and.returnValue(true);
    fixture.detectChanges();

    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Accounts');

    auth.isAdmin.and.returnValue(false);
    fixture.detectChanges();

    expect((fixture.nativeElement as HTMLElement).textContent).not.toContain('Accounts');
  });

  it('shows create and my recipes links for authenticated users', () => {
    auth.isLoggedIn.and.returnValue(true);
    auth.isAdmin.and.returnValue(false);
    fixture.detectChanges();

    const text = (fixture.nativeElement as HTMLElement).textContent ?? '';

    expect(text).toContain('Create Recipe');
    expect(text).toContain('My Recipes');
  });
});
