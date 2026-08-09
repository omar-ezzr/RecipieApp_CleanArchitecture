import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../app-api.config';
import { PagedResult } from '../models/paged-result.model';
import { FollowStatus } from '../models/user-follow.model';
import { UserSummary } from '../models/user-profile.model';

@Injectable({ providedIn: 'root' })
export class FollowService {
  private apiUrl = `${API_BASE_URL}/users`;

  constructor(private http: HttpClient) {}

  follow(userId: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${userId}/follow`, {});
  }

  unfollow(userId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${userId}/follow`);
  }

  status(userId: string): Observable<FollowStatus> {
    return this.http.get<FollowStatus>(`${this.apiUrl}/${userId}/follow-status`);
  }

  followers(userId: string, page = 1, pageSize = 20): Observable<PagedResult<UserSummary>> {
    return this.getUserList(`${this.apiUrl}/${userId}/followers`, page, pageSize);
  }

  following(userId: string, page = 1, pageSize = 20): Observable<PagedResult<UserSummary>> {
    return this.getUserList(`${this.apiUrl}/${userId}/following`, page, pageSize);
  }

  private getUserList(url: string, page: number, pageSize: number): Observable<PagedResult<UserSummary>> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<PagedResult<UserSummary>>(url, { params });
  }
}
