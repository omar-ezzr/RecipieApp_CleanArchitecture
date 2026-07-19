import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';

import { RecipeService } from '../services/recipe.service';
import { Recipe } from '../models/recipe.model';
import { Review, ReviewService } from '../services/review.service';

@Component({
  selector: 'app-recipe-details',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './recipe-details.component.html',
  styleUrl: './recipe-details.component.css'
})
export class RecipeDetailsComponent implements OnInit {
  recipe?: Recipe;
  isLoading = true;

  reviews: Review[] = [];

  newReview = {
    rating: 5,
    comment: ''
  };

  reviewError = '';
  reviewSuccess = '';

  constructor(
    private route: ActivatedRoute,
    private recipeService: RecipeService,
    private reviewService: ReviewService
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
      },
      error: () => {
        this.reviewError = 'Failed to load recipe.';
        this.isLoading = false;
      }
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
}
