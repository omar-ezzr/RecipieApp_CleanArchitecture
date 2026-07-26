import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';

import { RecipeService } from '../services/recipe.service';
import { DifficultyLevel, Recipe } from '../models/recipe.model';
import { Review, ReviewService } from '../services/review.service';
import { API_BASE_URL } from '../app-api.config';
import { resolveAssetUrl } from '../core/utils/asset-url.util';
import { FavoriteService } from '../services/favorite.service';
import { AuthService } from '../services/auth.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-recipe-details',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './recipe-details.component.html',
  styleUrl: './recipe-details.component.css'
})
export class RecipeDetailsComponent implements OnInit {
  recipe?: Recipe;
  isLoading = true;
  isFavorite = false;
  isDeleting = false;

  reviews: Review[] = [];

  newReview = {
    rating: 5,
    comment: ''
  };

  reviewError = '';
  reviewSuccess = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private recipeService: RecipeService,
    private reviewService: ReviewService,
    private favoriteService: FavoriteService,
    public auth: AuthService,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');

    if (!id) {
      this.isLoading = false;
      return;
    }

    this.recipeService.getById(id).subscribe({
      next: (data) => {
        this.recipe = data;
        this.isLoading = false;
        this.loadReviews(data.id);
        this.loadFavorite(data.id);
      },
      error: () => {
        this.reviewError = 'Failed to load recipe.';
        this.isLoading = false;
      }
    });
  }

  loadFavorite(recipeId: string): void {
    if (!this.auth.isLoggedIn()) {
      return;
    }

    this.favoriteService.check(recipeId).subscribe({
      next: result => this.isFavorite = result.isFavorite,
      error: () => this.isFavorite = false
    });
  }

  loadReviews(recipeId: string): void {
    this.reviewService.getByRecipe(recipeId).subscribe({
      next: reviews => {
        this.reviews = reviews;
      },
      error: () => {
        this.reviewError = 'Failed to load reviews.';
      }
    });
  }

  submitReview(): void {
    if (!this.recipe) {
      return;
    }

    this.reviewError = '';
    this.reviewSuccess = '';

    const rating = Number(this.newReview.rating);

    if (rating < 1 || rating > 5) {
      this.reviewError = 'Rating must be between 1 and 5.';
      return;
    }

    this.reviewService.create({
      recipeId: this.recipe.id,
      rating,
      comment: this.newReview.comment
    }).subscribe({
      next: () => {
        this.reviewSuccess = 'Review added successfully.';
        this.newReview = {
          rating: 5,
          comment: ''
        };
        this.loadReviews(this.recipe!.id);
      },
      error: error => {
        this.reviewError = error.error?.title || error.error || 'Failed to add review.';
      }
    });
  }

  resolveImageUrl(path: string | null | undefined): string {
    return resolveAssetUrl(path, API_BASE_URL);
  }

  canManageRecipe(): boolean {
    const currentUserId = this.auth.getCurrentUserId();

    return !!this.recipe && (this.auth.isAdmin() || (!!currentUserId && this.recipe.author?.id === currentUserId));
  }

  editRecipe(): void {
    if (!this.recipe) {
      return;
    }

    this.router.navigate(['/recipes'], { queryParams: { edit: this.recipe.id } });
  }

  deleteRecipe(): void {
    if (!this.recipe || !confirm(`Delete ${this.recipe.title}?`)) {
      return;
    }

    this.isDeleting = true;
    this.recipeService.delete(this.recipe.id).subscribe({
      next: () => {
        this.toastr.success('Recipe deleted successfully');
        this.router.navigate(['/recipes']);
      },
      error: () => {
        this.toastr.error('Failed to delete recipe');
        this.isDeleting = false;
      }
    });
  }

  toggleFavorite(): void {
    if (!this.recipe || !this.auth.isLoggedIn()) {
      this.router.navigate(['/login']);
      return;
    }

    const recipeId = this.recipe.id;
    const wasFavorite = this.isFavorite;
    this.isFavorite = !wasFavorite;

    const request = wasFavorite
      ? this.favoriteService.remove(recipeId)
      : this.favoriteService.add(recipeId);

    request.subscribe({
      error: () => {
        this.isFavorite = wasFavorite;
        this.toastr.error('Failed to update favorite');
      }
    });
  }

  difficultyLabel(): string {
    if (!this.recipe) {
      return '';
    }

    return this.recipe.difficulty === DifficultyLevel.Easy
      ? 'Easy'
      : this.recipe.difficulty === DifficultyLevel.Medium
        ? 'Medium'
        : 'Hard';
  }

  displayReviewAuthor(review: Review): string {
    return this.maskEmail(review.userEmail);
  }

  trackIngredient(index: number): number {
    return index;
  }

  trackStep(index: number): number {
    return index;
  }

  trackReview(_index: number, review: Review): string {
    return review.id;
  }

  private maskEmail(email: string): string {
    const [name, domain] = email.split('@');

    if (!name || !domain) {
      return 'Community cook';
    }

    return `${name.charAt(0)}***@${domain}`;
  }
}
