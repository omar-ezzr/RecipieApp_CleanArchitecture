import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  CreateUserAccountRequest,
  PagedUserAccounts,
  UpdateUserRoleRequest,
  UpdateUserStatusRequest,
  UserAccount,
  UserAccountQuery
} from '../models/user-account.model';

@Injectable({
  providedIn: 'root'
})
export class UserManagementService {
  private apiUrl = 'http://localhost:5130/api/admin/users';

  constructor(private http: HttpClient) {}

  getPaged(query: UserAccountQuery): Observable<PagedUserAccounts> {
    let params = new HttpParams();

    if (query.page) params = params.set('page', query.page);
    if (query.pageSize) params = params.set('pageSize', query.pageSize);
    if (query.search) params = params.set('search', query.search);
    if (query.role) params = params.set('role', query.role);
    if (query.isActive !== null && query.isActive !== undefined) {
      params = params.set('isActive', query.isActive);
    }

    return this.http.get<PagedUserAccounts>(this.apiUrl, { params });
  }

  getById(id: string): Observable<UserAccount> {
    return this.http.get<UserAccount>(`${this.apiUrl}/${id}`);
  }

  create(request: CreateUserAccountRequest): Observable<UserAccount> {
    return this.http.post<UserAccount>(this.apiUrl, request);
  }

  updateRole(id: string, request: UpdateUserRoleRequest): Observable<UserAccount> {
    return this.http.put<UserAccount>(`${this.apiUrl}/${id}/role`, request);
  }

  updateStatus(id: string, request: UpdateUserStatusRequest): Observable<UserAccount> {
    return this.http.put<UserAccount>(`${this.apiUrl}/${id}/status`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
