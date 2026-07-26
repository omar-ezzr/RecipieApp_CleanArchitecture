import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../app-api.config';

export interface FavoriteRecipe {
  id: string;
  recipeId: string;
  recipeTitle: string;
  recipeImageUrl?: string | null;
  createdAt: string;
}

@Injectable({
  providedIn: 'root'
})
export class FavoriteService {
  private apiUrl = `${API_BASE_URL}/favorites`;

  constructor(private http: HttpClient) {}

  add(recipeId: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${recipeId}`, {});
  }

  remove(recipeId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${recipeId}`);
  }

  getMine(): Observable<FavoriteRecipe[]> {
    return this.http.get<FavoriteRecipe[]>(`${this.apiUrl}/me`);
  }

  check(recipeId: string): Observable<{ isFavorite: boolean }> {
    return this.http.get<{ isFavorite: boolean }>(
      `${this.apiUrl}/check/${recipeId}`
    );
  }
}
