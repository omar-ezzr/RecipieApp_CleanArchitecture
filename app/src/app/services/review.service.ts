import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Review {
  id: string;
  recipeId: string;
  userId: string;
  userEmail: string;
  rating: number;
  comment: string;
  createdAt: string;
  updatedAt?: string | null;
}

export interface CreateReview {
  recipeId: string;
  rating: number;
  comment: string;
}

export interface UpdateReview {
  rating: number;
  comment: string;
}

@Injectable({
  providedIn: 'root'
})
export class ReviewService {
  private apiUrl = 'http://localhost:5130/api/reviews';

  constructor(private http: HttpClient) {}

  getByRecipe(recipeId: string): Observable<Review[]> {
    return this.http.get<Review[]>(`${this.apiUrl}/recipe/${recipeId}`);
  }

  create(dto: CreateReview): Observable<void> {
    return this.http.post<void>(this.apiUrl, dto);
  }

  update(reviewId: string, dto: UpdateReview): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${reviewId}`, dto);
  }

  delete(reviewId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${reviewId}`);
  }
}