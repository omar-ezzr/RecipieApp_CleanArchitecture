import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../app-api.config';
import { CreateRegion, Region } from '../models/region.model';

@Injectable({
  providedIn: 'root'
})
export class RegionService {
  private apiUrl = `${API_BASE_URL}/regions`;

  constructor(private http: HttpClient) {}

  getById(id: string): Observable<Region> {
    return this.http.get<Region>(`${this.apiUrl}/${id}`);
  }

  create(dto: CreateRegion): Observable<Region> {
    return this.http.post<Region>(this.apiUrl, dto);
  }

  update(id: string, dto: CreateRegion): Observable<Region> {
    return this.http.put<Region>(`${this.apiUrl}/${id}`, dto);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
