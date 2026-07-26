import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../app-api.config';
import { CreateCuisine, Cuisine } from '../models/cuisine.model';
import { Region } from '../models/region.model';

@Injectable({
  providedIn: 'root'
})
export class CuisineService {
  private apiUrl = `${API_BASE_URL}/cuisines`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<Cuisine[]> {
    return this.http.get<Cuisine[]>(this.apiUrl);
  }

  getById(id: string): Observable<Cuisine> {
    return this.http.get<Cuisine>(`${this.apiUrl}/${id}`);
  }

  getRegions(id: string): Observable<Region[]> {
    return this.http.get<Region[]>(`${this.apiUrl}/${id}/regions`);
  }

  create(dto: CreateCuisine): Observable<Cuisine> {
    return this.http.post<Cuisine>(this.apiUrl, dto);
  }

  update(id: string, dto: CreateCuisine): Observable<Cuisine> {
    return this.http.put<Cuisine>(`${this.apiUrl}/${id}`, dto);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
