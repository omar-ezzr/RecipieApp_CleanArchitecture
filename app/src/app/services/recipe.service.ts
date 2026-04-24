import { Injectable} from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { CreateRecipe, Recipe } from "../models/recipe.model";
@Injectable({
    providedIn: "root",

})

export class RecipeService {
    private apiUrl = 'http://localhost:5130/api/recipes';


    constructor(private http: HttpClient){}

    getAll(): Observable<Recipe[]>{
        return this.http.get<Recipe[]>(this.apiUrl);
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
