import { Injectable} from "@angular/core";
import { HttpClient, HttpParams } from "@angular/common/http";
import { Observable } from "rxjs";
import { CreateRecipe, Recipe } from "../models/recipe.model";

export interface RecipeQuery {
    search?: string;
    categoryId?: string;
    difficulty?: string;
    page?: number;
    pageSize?: number;
}

export interface PagedRecipes {
    items: Recipe[];
    total: number;
    page: number;
    pageSize: number;
}
@Injectable({
    providedIn: "root",

})

export class RecipeService {
    private apiUrl = 'http://localhost:5130/api/recipes';


    constructor(private http: HttpClient){}

    getAll(): Observable<Recipe[]>{
        return this.http.get<Recipe[]>(this.apiUrl);
    }

    getFiltered(query: RecipeQuery): Observable<PagedRecipes> {
        let params = new HttpParams()
            .set('page', query.page?.toString() ?? '1')
            .set('pageSize', query.pageSize?.toString() ?? '50');

        if (query.search?.trim()) {
            params = params.set('search', query.search.trim());
        }

        if (query.categoryId) {
            params = params.set('categoryId', query.categoryId);
        }

        if (query.difficulty) {
            params = params.set('difficulty', query.difficulty);
        }

        return this.http.get<PagedRecipes>(`${this.apiUrl}/paged`, { params });
    }

    getById(id: string): Observable<Recipe>{
        return this.http.get<Recipe>(`${this.apiUrl}/${id}`);
    }

    create(recipe: CreateRecipe) {
        return this.http.post(this.apiUrl , recipe);
    }
        update(id: string, data: any) {
    return this.http.put(`http://localhost:5130/api/recipes/${id}`, data);
    }

    delete(id: string) {
    return this.http.delete(`http://localhost:5130/api/recipes/${id}`);
    }
    
}
