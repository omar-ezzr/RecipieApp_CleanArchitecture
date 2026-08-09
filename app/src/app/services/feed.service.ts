import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../app-api.config';
import { FeedRecipe } from '../models/feed.model';
import { PagedResult } from '../models/paged-result.model';

@Injectable({ providedIn: 'root' })
export class FeedService {
  private apiUrl = `${API_BASE_URL}/feed`;

  constructor(private http: HttpClient) {}

  getFeed(page = 1, pageSize = 10): Observable<PagedResult<FeedRecipe>> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<PagedResult<FeedRecipe>>(this.apiUrl, { params });
  }
}
