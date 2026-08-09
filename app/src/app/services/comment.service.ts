import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../app-api.config';
import { PagedResult } from '../models/paged-result.model';
import { CreateRecipeComment, RecipeComment, UpdateRecipeComment } from '../models/recipe-comment.model';

@Injectable({ providedIn: 'root' })
export class CommentService {
  private apiUrl = `${API_BASE_URL}`;

  constructor(private http: HttpClient) {}

  getByRecipe(recipeId: string, page = 1, pageSize = 20): Observable<PagedResult<RecipeComment>> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<PagedResult<RecipeComment>>(`${this.apiUrl}/recipes/${recipeId}/comments`, { params });
  }

  create(recipeId: string, dto: CreateRecipeComment): Observable<RecipeComment> {
    return this.http.post<RecipeComment>(`${this.apiUrl}/recipes/${recipeId}/comments`, dto);
  }

  update(commentId: string, dto: UpdateRecipeComment): Observable<RecipeComment> {
    return this.http.put<RecipeComment>(`${this.apiUrl}/comments/${commentId}`, dto);
  }

  delete(commentId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/comments/${commentId}`);
  }
}
