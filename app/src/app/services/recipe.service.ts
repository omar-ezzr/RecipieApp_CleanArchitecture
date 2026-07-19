import { Injectable} from "@angular/core";
import { HttpClient, HttpParams } from "@angular/common/http";
import { Observable, of } from "rxjs";
import { CreateRecipe, Difficulty, Recipe } from "../models/recipe.model";
import { tap } from 'rxjs/operators';

export interface RecipeQuery {
  search?: string;
  categoryId?: string;
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
    private apiUrl = 'http://localhost:5130/api/recipes';
    private cache = new Map<string, any>();

    constructor(private http: HttpClient){}

    getAll(): Observable<PagedRecipes>{
        return this.http.get<PagedRecipes>(this.apiUrl);
    }

    // getFiltered(query: RecipeQuery): Observable<PagedRecipes> {
    //     let params = new HttpParams()
    //         .set('page', query.page?.toString() ?? '1')
    //         .set('pageSize', query.pageSize?.toString() ?? '50');

    //     if (query.search?.trim()) {
    //         params = params.set('search', query.search.trim());
    //     }

    //     if (query.categoryId) {
    //         params = params.set('categoryId', query.categoryId);
    //     }

    //     if (query.difficulty) {
    //         params = params.set('difficulty', query.difficulty);
    //     }

    //     return this.http.get<PagedRecipes>(`${this.apiUrl}/paged`, { params });
    // }

    getById(id: string): Observable<Recipe>{
        return this.http.get<Recipe>(`${this.apiUrl}/${id}`);
    }

    create(recipe: CreateRecipe) {
        return this.http.post(this.apiUrl , recipe);
    }
        update(id: string, data: any) {
return this.http.put(
  `${this.apiUrl}/${id}`,
  data
);    
}

    delete(id: string) {
return this.http.delete(
  `${this.apiUrl}/${id}`
);
    }
 getPaged(
  query: RecipeQuery
): Observable<PagedRecipes> {

  const cacheKey = JSON.stringify(query);

  // CACHE HIT
  if (this.cache.has(cacheKey)) {

    return of(this.cache.get(cacheKey));
  }

  // BUILD PARAMS
  let params = new HttpParams();

if (query.search) {
  params = params.set('search', query.search);
}

if (query.categoryId) {
  params = params.set('categoryId', query.categoryId);
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

  // API REQUEST
  return this.http.get<PagedRecipes>(

    `${this.apiUrl}/paged`,

    { params }

  ).pipe(

    tap(response => {

      this.cache.set(
        cacheKey,
        response
      );
    })
  );
}



clearCache() {
  this.cache.clear();
}

}
