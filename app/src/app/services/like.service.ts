import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../app-api.config';
import { LikeStatus } from '../models/recipe-like.model';
import { PagedResult } from '../models/paged-result.model';
import { UserSummary } from '../models/user-profile.model';

@Injectable({ providedIn: 'root' })
export class LikeService {
  private apiUrl = `${API_BASE_URL}/recipes`;

  constructor(private http: HttpClient) {}

  like(recipeId: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${recipeId}/likes`, {});
  }

  unlike(recipeId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${recipeId}/likes`);
  }

  getStatus(recipeId: string): Observable<LikeStatus> {
    return this.http.get<LikeStatus>(`${this.apiUrl}/${recipeId}/likes/status`);
  }

  status(recipeId: string): Observable<LikeStatus> {
    return this.getStatus(recipeId);
  }

  likes(recipeId: string, page = 1, pageSize = 20): Observable<PagedResult<UserSummary>> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<PagedResult<UserSummary>>(`${this.apiUrl}/${recipeId}/likes`, { params });
  }
}
