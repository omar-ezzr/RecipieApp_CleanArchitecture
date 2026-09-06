import { Injectable } from "@angular/core";
import { HttpClient, HttpParams } from "@angular/common/http";
import { Observable, map, switchMap, of } from "rxjs";
import { CreateRecipe, Difficulty, Recipe, RecipeMedia } from "../models/recipe.model";
import { tap } from 'rxjs/operators';
import { API_BASE_URL } from '../app-api.config';

export interface RecipeQuery {
  search?: string;
  categoryId?: string;
  cuisineId?: string;
  regionId?: string;
  isTraditional?: boolean;
  difficulty?: Difficulty;
  sortBy?: string;
  page?: number;
  pageSize?: number;
}

export interface PagedRecipes {
    items: Recipe[];
    total: number;
    page: number;
    pageSize: number;
    totalPages: number;
}
@Injectable({
    providedIn: "root",

})

export class RecipeService {
    private apiUrl = `${API_BASE_URL}/recipes`;
    // List responses are not cached because they contain user-specific social state.

    constructor(private http: HttpClient){}

    getAll(): Observable<PagedRecipes>{
        return this.http.get<PagedRecipes>(this.apiUrl);
    }

    getById(id: string): Observable<Recipe>{
        return this.http.get<Recipe>(`${this.apiUrl}/${id}`);
    }

    create(recipe: CreateRecipe): Observable<Recipe> {
        return this.http.post<Recipe>(this.apiUrl , recipe);
    }
        update(id: string, data: CreateRecipe): Observable<Recipe> {
return this.http.put<Recipe>(
  `${this.apiUrl}/${id}`,
  data
);    
}

addMedia(id: string, file: File): Observable<RecipeMedia> { const data = new FormData(); data.append('file', file); return this.http.post<RecipeMedia>(`${this.apiUrl}/${id}/media`, data); }
removeMedia(id: string, mediaId: string): Observable<void> { return this.http.delete<void>(`${this.apiUrl}/${id}/media/${mediaId}`); }
setMainMedia(id: string, mediaId: string): Observable<void> { return this.http.put<void>(`${this.apiUrl}/${id}/media/${mediaId}/main`, {}); }
reorderMedia(id: string, mediaIds: string[]): Observable<void> { return this.http.put<void>(`${this.apiUrl}/${id}/media/order`, { mediaIds }); }

// Legacy UI compatibility: these delegate to the canonical media endpoints.
uploadImage(id: string, file: File): Observable<{ imageUrl: string }> { return this.addMedia(id, file).pipe(map(media => ({ imageUrl: media.url }))); }
removeImage(id: string): Observable<void> { return this.getById(id).pipe(switchMap(recipe => { const main = recipe.media?.find(media => media.isMain) ?? recipe.media?.[0]; return main ? this.removeMedia(id, main.id) : of(void 0); })); }

    delete(id: string) {
return this.http.delete(
  `${this.apiUrl}/${id}`
);
    }
    getMine(query: RecipeQuery): Observable<PagedRecipes> {
      let params = this.buildParams(query);

      return this.http.get<PagedRecipes>(`${this.apiUrl}/me`, { params });
    }

 getPaged(
  query: RecipeQuery
): Observable<PagedRecipes> {

  // BUILD PARAMS
  let params = this.buildParams(query);

  // API REQUEST
  return this.http.get<PagedRecipes>(

    `${this.apiUrl}/paged`,

    { params }

  ) ;
}





private buildParams(query: RecipeQuery): HttpParams {
  let params = new HttpParams();

  if (query.search) {
    params = params.set('search', query.search);
  }

  if (query.categoryId) {
    params = params.set('categoryId', query.categoryId);
  }

  if (query.cuisineId) {
    params = params.set('cuisineId', query.cuisineId);
  }

  if (query.regionId) {
    params = params.set('regionId', query.regionId);
  }

  if (query.isTraditional !== undefined && query.isTraditional !== null) {
    params = params.set('isTraditional', query.isTraditional);
  }

  if (query.difficulty) {
    params = params.set('difficulty', query.difficulty);
  }

  if (query.sortBy) {
    params = params.set('sortBy', query.sortBy);
  }

  if (query.page) {
    params = params.set('page', query.page);
  }

  if (query.pageSize) {
    params = params.set('pageSize', query.pageSize);
  }

  return params;
}

}
