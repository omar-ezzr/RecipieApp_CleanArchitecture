import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ToastrService } from '@openng/ngx-toastr';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { AccountsComponent } from './accounts.component';
import { AuthService } from '../../../services/auth.service';
import { UserManagementService } from '../../../services/user-management.service';

describe('AccountsComponent', () => {
  let component: AccountsComponent;
  let fixture: ComponentFixture<AccountsComponent>;
  let users: jasmine.SpyObj<UserManagementService>;
  let auth: jasmine.SpyObj<AuthService>;
  let toastr: jasmine.SpyObj<ToastrService>;

  beforeEach(async () => {
    users = jasmine.createSpyObj<UserManagementService>('UserManagementService', [
      'getPaged',
      'create',
      'updateRole',
      'updateStatus',
      'delete'
    ]);
    auth = jasmine.createSpyObj<AuthService>('AuthService', ['getCurrentUserId']);
    toastr = jasmine.createSpyObj<ToastrService>('ToastrService', ['success', 'error']);

    users.getPaged.and.returnValue(of({
      items: [
        { id: 'admin-1', email: 'admin@example.com', role: 'Admin', isActive: true },
        { id: 'user-1', email: 'user@example.com', role: 'User', isActive: true }
      ],
      total: 2,
      page: 1,
      pageSize: 20
    }));
    auth.getCurrentUserId.and.returnValue('admin-1');

    await TestBed.configureTestingModule({
      imports: [AccountsComponent],
      providers: [
        provideRouter([]),
        { provide: UserManagementService, useValue: users },
        { provide: AuthService, useValue: auth },
        { provide: ToastrService, useValue: toastr }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(AccountsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('loads and renders accounts', () => {
    expect(users.getPaged).toHaveBeenCalled();
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Account management');
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('user@example.com');
  });

  it('disables dangerous controls for the current admin account', () => {
    const row = (fixture.nativeElement as HTMLElement).querySelector('tbody tr');

    expect(row?.querySelector('select')?.hasAttribute('disabled')).toBeTrue();
    expect(row?.querySelector('button')?.hasAttribute('disabled')).toBeTrue();
  });

  it('creates an account and clears password state after success', () => {
    users.create.and.returnValue(of({ id: 'new-1', email: 'new@example.com', role: 'Operator', isActive: true }));
    component.createRequest = {
      email: 'new@example.com',
      password: 'StrongPass123',
      role: 'Operator'
    };

    component.createAccount();

    expect(users.create).toHaveBeenCalled();
    expect(component.createRequest.password).toBe('');
    expect(toastr.success).toHaveBeenCalledWith('Account created.');
  });

  it('shows backend errors', () => {
    users.create.and.returnValue(throwError(() => ({ error: { message: 'Duplicate account.' } })));

    component.createAccount();

    expect(toastr.error).toHaveBeenCalledWith('Duplicate account.');
  });
});
