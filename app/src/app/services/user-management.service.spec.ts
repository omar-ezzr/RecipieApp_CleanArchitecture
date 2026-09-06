import { TestBed } from '@angular/core/testing';
import { provideHttpClient, withXhr } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { UserManagementService } from './user-management.service';

describe('UserManagementService', () => {
  let service: UserManagementService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        UserManagementService,
        provideHttpClient(withXhr()),
        provideHttpClientTesting()
      ]
    });

    service = TestBed.inject(UserManagementService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('queries paged accounts with filters', () => {
    service.getPaged({
      page: 2,
      pageSize: 20,
      search: 'admin',
      role: 'Admin',
      isActive: true
    }).subscribe();

    const req = http.expectOne(request =>
      request.url === '/api/admin/users' &&
      request.params.get('page') === '2' &&
      request.params.get('role') === 'Admin' &&
      request.params.get('isActive') === 'true');

    expect(req.request.method).toBe('GET');
    req.flush({ items: [], total: 0, page: 2, pageSize: 20 });
  });

  it('calls account mutation endpoints', () => {
    service.create({ email: 'operator@example.com', password: 'StrongPass123', role: 'Operator' }).subscribe();
    const create = http.expectOne('/api/admin/users');
    expect(create.request.method).toBe('POST');
    create.flush({ id: 'user-1', email: 'operator@example.com', role: 'Operator', isActive: true });

    service.updateRole('user-1', { role: 'User' }).subscribe();
    const role = http.expectOne('/api/admin/users/user-1/role');
    expect(role.request.method).toBe('PUT');
    role.flush({ id: 'user-1', email: 'operator@example.com', role: 'User', isActive: true });

    service.updateStatus('user-1', { isActive: false }).subscribe();
    const status = http.expectOne('/api/admin/users/user-1/status');
    expect(status.request.method).toBe('PUT');
    status.flush({ id: 'user-1', email: 'operator@example.com', role: 'User', isActive: false });

    service.delete('user-1').subscribe();
    const deleteRequest = http.expectOne('/api/admin/users/user-1');
    expect(deleteRequest.request.method).toBe('DELETE');
    deleteRequest.flush(null);
  });
});
