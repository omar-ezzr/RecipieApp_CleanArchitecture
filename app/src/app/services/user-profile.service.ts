import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../app-api.config';
import { PagedResult } from '../models/paged-result.model';
import { Recipe } from '../models/recipe.model';
import { PublicUserProfile, UpdatePublicUserProfile } from '../models/user-profile.model';

@Injectable({ providedIn: 'root' })
export class UserProfileService {
  private apiUrl = `${API_BASE_URL}/users`;

  constructor(private http: HttpClient) {}

  getProfile(id: string): Observable<PublicUserProfile> {
    return this.http.get<PublicUserProfile>(`${this.apiUrl}/${id}`);
  }

  getCurrentProfile(): Observable<PublicUserProfile> {
    return this.http.get<PublicUserProfile>(`${this.apiUrl}/me/profile`);
  }

  updateCurrentProfile(dto: UpdatePublicUserProfile): Observable<PublicUserProfile> {
    return this.http.put<PublicUserProfile>(`${this.apiUrl}/me/profile`, dto);
  }

  getRecipes(id: string, page = 1, pageSize = 10): Observable<PagedResult<Recipe>> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<PagedResult<Recipe>>(`${this.apiUrl}/${id}/recipes`, { params });
  }
}
