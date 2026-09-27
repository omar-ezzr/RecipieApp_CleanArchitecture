import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Category } from '../models/category.model';
import { API_BASE_URL } from '../app-api.config';
@Injectable({
  providedIn: 'root'
})
export class CategoryService {

  private api = `${API_BASE_URL}/categories`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<Category[]> {
    return this.http.get<Category[]>(this.api);
  }
}
