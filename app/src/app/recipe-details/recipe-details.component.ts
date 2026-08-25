import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';

import { RecipeService } from '../services/recipe.service';
import { CreateRecipe, DifficultyLevel, Recipe } from '../models/recipe.model';
import { Review, ReviewService } from '../services/review.service';
import { API_BASE_URL } from '../app-api.config';
import { resolveAssetUrl } from '../core/utils/asset-url.util';
import { FavoriteService } from '../services/favorite.service';
import { AuthService } from '../services/auth.service';
import { ToastrService } from 'ngx-toastr';
import { CommentService } from '../services/comment.service';
import { RecipeComment } from '../models/recipe-comment.model';
import { LikeService } from '../services/like.service';
import { Category } from '../models/category.model';
import { Cuisine } from '../models/cuisine.model';
import { Region } from '../models/region.model';
import { CategoryService } from '../services/category.service';
import { CuisineService } from '../services/cuisine.service';

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
  isLiked = false;
  likeCount = 0;
  isLikeBusy = false;
  isDeleting = false;
  isEditMode = false;
  isSaving = false;
  editError = '';
  editRecipeModel: CreateRecipe | null = null;
  categories: Category[] = [];
  cuisines: Cuisine[] = [];
  regions: Region[] = [];
  readonly DifficultyLevel = DifficultyLevel;

  reviews: Review[] = [];
  comments: RecipeComment[] = [];
  newComment = '';
  editingCommentId: string | null = null;
  editingCommentContent = '';

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
    private categoryService: CategoryService,
    private cuisineService: CuisineService,
    private reviewService: ReviewService,
    private favoriteService: FavoriteService,
    private likeService: LikeService,
    private commentService: CommentService,
    public auth: AuthService,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.loadEditLookups();

    const id = this.route.snapshot.paramMap.get('id');

    if (!id) {
      this.isLoading = false;
      return;
    }

    this.recipeService.getById(id).subscribe({
      next: (data) => {
        this.recipe = data;
        this.isLiked = !!data.isLikedByCurrentUser;
        this.likeCount = Math.max(0, data.likeCount || 0);
        this.isLoading = false;
        this.loadReviews(data.id);
        this.loadComments(data.id);
        this.loadFavorite(data.id);
        this.loadLike(data.id);

        if (this.route.snapshot.queryParamMap.get('edit') === 'true' && this.canManageRecipe()) {
          this.enterEditMode(false);
        }
      },
      error: () => {
        this.reviewError = 'Failed to load recipe.';
        this.isLoading = false;
      }
    });
  }

  loadLike(recipeId: string): void {
    if (!this.auth.isLoggedIn()) {
      return;
    }

    this.likeService.getStatus(recipeId).subscribe({
      next: result => {
        this.isLiked = result.isLiked;
        this.likeCount = Math.max(0, result.likeCount || 0);
      },
      error: () => {
        this.isLiked = false;
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

  loadComments(recipeId: string): void {
    this.commentService.getByRecipe(recipeId).subscribe({
      next: result => this.comments = result.items,
      error: () => this.toastr.error('Failed to load comments')
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
    this.enterEditMode(true);
  }

  enterEditMode(updateUrl = true): void {
    if (!this.recipe || !this.canManageRecipe()) {
      return;
    }

    this.editRecipeModel = this.createEditModel(this.recipe);
    this.editError = '';
    this.isEditMode = true;
    this.loadEditRegions(this.editRecipeModel.cuisineId);

    if (updateUrl) {
      this.router.navigate([], {
        relativeTo: this.route,
        queryParams: { edit: 'true' },
        queryParamsHandling: 'merge'
      });
    }
  }

  cancelEdit(): void {
    this.isEditMode = false;
    this.editRecipeModel = null;
    this.editError = '';
    this.removeEditQueryParam();
  }

  onEditCuisineChange(): void {
    if (!this.editRecipeModel) {
      return;
    }

    this.editRecipeModel.regionId = null;
    this.regions = [];
    this.loadEditRegions(this.editRecipeModel.cuisineId);
  }

  addIngredient(): void {
    if (this.editRecipeModel && this.editRecipeModel.ingredients.length < 100) {
      this.editRecipeModel.ingredients.push({ name: '', quantity: '' });
    }
  }

  removeIngredient(index: number): void {
    if (this.editRecipeModel && this.editRecipeModel.ingredients.length > 1) {
      this.editRecipeModel.ingredients.splice(index, 1);
    }
  }

  addStep(): void {
    if (!this.editRecipeModel || this.editRecipeModel.steps.length >= 100) {
      return;
    }

    this.editRecipeModel.steps.push({
      stepNumber: this.editRecipeModel.steps.length + 1,
      instruction: ''
    });
  }

  removeStep(index: number): void {
    if (this.editRecipeModel && this.editRecipeModel.steps.length > 1) {
      this.editRecipeModel.steps.splice(index, 1);
      this.renumberEditSteps();
    }
  }

  saveRecipe(): void {
    if (!this.recipe || !this.editRecipeModel) {
      return;
    }

    const payload = this.buildUpdatePayload();

    if (!payload) {
      return;
    }

    this.isSaving = true;
    this.recipeService.update(this.recipe.id, payload).subscribe({
      next: updated => {
        this.recipe = updated;
        this.isLiked = !!updated.isLikedByCurrentUser;
        this.likeCount = Math.max(0, updated.likeCount || 0);
        this.isSaving = false;
        this.isEditMode = false;
        this.editRecipeModel = null;
        this.recipeService.clearCache();
        this.toastr.success('Recipe updated successfully');
        this.removeEditQueryParam();
      },
      error: error => {
        this.editError = this.getApiError(error, 'Failed to update recipe.');
        this.isSaving = false;
      }
    });
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

  toggleLike(): void {
    if (this.isLikeBusy) {
      return;
    }

    if (!this.recipe || !this.auth.isLoggedIn()) {
      this.router.navigate(['/login']);
      return;
    }

    this.isLikeBusy = true;
    const wasLiked = this.isLiked;
    const previousCount = Math.max(0, this.likeCount);
    this.isLiked = !wasLiked;
    this.likeCount = Math.max(0, previousCount + (wasLiked ? -1 : 1));
    const request = wasLiked ? this.likeService.unlike(this.recipe.id) : this.likeService.like(this.recipe.id);

    request.subscribe({
      next: () => {
        this.isLikeBusy = false;
      },
      error: () => {
        this.isLiked = wasLiked;
        this.likeCount = previousCount;
        this.toastr.error('Failed to update like');
        this.isLikeBusy = false;
      }
    });
  }

  submitComment(): void {
    if (!this.recipe) {
      return;
    }

    const content = this.newComment.trim();
    if (!content) {
      this.toastr.error('Comment is required');
      return;
    }

    this.commentService.create(this.recipe.id, { content }).subscribe({
      next: comment => {
        this.comments = [comment, ...this.comments];
        this.newComment = '';
      },
      error: error => this.toastr.error(error?.error?.message || 'Failed to add comment')
    });
  }

  startEditComment(comment: RecipeComment): void {
    this.editingCommentId = comment.id;
    this.editingCommentContent = comment.content;
  }

  saveComment(comment: RecipeComment): void {
    const content = this.editingCommentContent.trim();
    if (!content) {
      this.toastr.error('Comment is required');
      return;
    }

    this.commentService.update(comment.id, { content }).subscribe({
      next: updated => {
        this.comments = this.comments.map(item => item.id === updated.id ? updated : item);
        this.editingCommentId = null;
        this.editingCommentContent = '';
      },
      error: () => this.toastr.error('Failed to update comment')
    });
  }

  deleteComment(comment: RecipeComment): void {
    this.commentService.delete(comment.id).subscribe({
      next: () => this.comments = this.comments.filter(item => item.id !== comment.id),
      error: () => this.toastr.error('Failed to delete comment')
    });
  }


  previewEditImageUrl(): string {
    return resolveAssetUrl(this.editRecipeModel?.imageUrl, API_BASE_URL);
  }

  private loadEditLookups(): void {
    this.categoryService.getAll().subscribe({
      next: categories => this.categories = categories,
      error: () => this.toastr.error('Failed to load categories')
    });

    this.cuisineService.getAll().subscribe({
      next: cuisines => this.cuisines = cuisines,
      error: () => this.toastr.error('Failed to load cuisines')
    });
  }

  private loadEditRegions(cuisineId: string): void {
    if (!cuisineId) {
      this.regions = [];
      return;
    }

    this.cuisineService.getRegions(cuisineId).subscribe({
      next: regions => this.regions = regions,
      error: () => this.toastr.error('Failed to load regions')
    });
  }

  private createEditModel(recipe: Recipe): CreateRecipe {
    return {
      title: recipe.title,
      description: recipe.description,
      preparationTimeMinutes: recipe.preparationTimeMinutes,
      categoryId: recipe.categoryId,
      cuisineId: recipe.cuisineId,
      regionId: recipe.regionId || null,
      difficulty: recipe.difficulty,
      imageUrl: recipe.imageUrl || null,
      traditionalName: recipe.traditionalName || null,
      originDescription: recipe.originDescription || null,
      isTraditional: recipe.isTraditional,
      servingOccasion: recipe.servingOccasion || null,
      ingredients: recipe.ingredients?.length
        ? recipe.ingredients.map(ingredient => ({
          name: ingredient.name,
          quantity: ingredient.quantity
        }))
        : [{ name: '', quantity: '' }],
      steps: recipe.steps?.length
        ? recipe.steps.map(step => ({
          stepNumber: step.stepNumber,
          instruction: step.instruction
        }))
        : [{ stepNumber: 1, instruction: '' }]
    };
  }

  private buildUpdatePayload(): CreateRecipe | null {
    if (!this.editRecipeModel) {
      return null;
    }

    this.editError = '';
    this.renumberEditSteps();

    const ingredients = this.editRecipeModel.ingredients.map(ingredient => ({
      name: ingredient.name.trim(),
      quantity: ingredient.quantity.trim()
    }));

    if (!ingredients.length || ingredients.some(ingredient => !ingredient.name)) {
      this.editError = 'Please enter a name for every ingredient.';
      return null;
    }

    const steps = this.editRecipeModel.steps.map((step, index) => ({
      stepNumber: index + 1,
      instruction: step.instruction.trim()
    }));

    if (!steps.length || steps.some(step => !step.instruction)) {
      this.editError = 'Please add an instruction for every preparation step.';
      return null;
    }

    const title = this.editRecipeModel.title.trim();
    const description = this.editRecipeModel.description.trim();

    if (!title) {
      this.editError = 'Please enter a recipe title.';
      return null;
    }

    if (!description) {
      this.editError = 'Please add a description.';
      return null;
    }

    if (!this.editRecipeModel.categoryId) {
      this.editError = 'Please select a category.';
      return null;
    }

    if (!this.editRecipeModel.cuisineId) {
      this.editError = 'Please select a cuisine.';
      return null;
    }

    if (!Number.isFinite(Number(this.editRecipeModel.preparationTimeMinutes)) || Number(this.editRecipeModel.preparationTimeMinutes) < 1) {
      this.editError = 'Preparation time must be greater than 0 minutes.';
      return null;
    }

    return {
      title,
      description,
      preparationTimeMinutes: Number(this.editRecipeModel.preparationTimeMinutes),
      categoryId: this.editRecipeModel.categoryId,
      cuisineId: this.editRecipeModel.cuisineId,
      regionId: this.editRecipeModel.regionId || null,
      difficulty: this.editRecipeModel.difficulty,
      imageUrl: this.toOptionalString(this.editRecipeModel.imageUrl),
      traditionalName: this.toOptionalString(this.editRecipeModel.traditionalName),
      originDescription: this.toOptionalString(this.editRecipeModel.originDescription),
      isTraditional: this.editRecipeModel.isTraditional,
      servingOccasion: this.toOptionalString(this.editRecipeModel.servingOccasion),
      ingredients,
      steps
    };
  }

  private renumberEditSteps(): void {
    if (!this.editRecipeModel) {
      return;
    }

    this.editRecipeModel.steps = this.editRecipeModel.steps.map((step, index) => ({
      ...step,
      stepNumber: index + 1
    }));
  }

  private removeEditQueryParam(): void {
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { edit: null },
      queryParamsHandling: 'merge',
      replaceUrl: true
    });
  }

  private toOptionalString(value: string | null | undefined): string | null {
    const trimmed = value?.trim();
    return trimmed ? trimmed : null;
  }

  private getApiError(error: any, fallback: string): string {
    const validationErrors = error?.error?.errors;

    if (validationErrors) {
      const firstError = Object.entries(validationErrors)
        .flatMap(([field, values]) => Array.isArray(values)
          ? values.map(value => ({ field, value }))
          : [])
        .find(item => typeof item.value === 'string' && !this.isTechnicalValidationMessage(item.field, item.value)) as { value: string } | undefined;

      if (firstError) {
        return firstError.value;
      }
    }

    if (typeof error?.error === 'string' && !this.isTechnicalValidationMessage('', error.error)) {
      return error.error;
    }

    if (typeof error?.error?.title === 'string' && !this.isTechnicalValidationMessage('', error.error.title)) {
      return error.error.title;
    }

    if (typeof error?.error?.message === 'string' && !this.isTechnicalValidationMessage('', error.error.message)) {
      return error.error.message;
    }

    return error?.status === 400 ? 'Please check the recipe information and try again.' : fallback;
  }

  private isTechnicalValidationMessage(field: string, message: string): boolean {
    const normalizedField = field.toLowerCase();
    const normalizedMessage = message.toLowerCase();

    return normalizedField === 'dto'
      || normalizedField === '$'
      || normalizedMessage === 'validation failed.'
      || normalizedMessage.includes('dto')
      || normalizedMessage.includes('json')
      || normalizedMessage.includes('json path')
      || normalizedMessage.includes('could not be converted')
      || normalizedMessage.includes('the input was not valid')
      || normalizedMessage.includes('system.');
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
    return review.author?.displayName || 'Community cook';
  }

  canManageComment(comment: RecipeComment): boolean {
    const currentUserId = this.auth.getCurrentUserId();
    return this.auth.isAdmin() || (!!currentUserId && comment.author.id === currentUserId);
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

  trackComment(_index: number, comment: RecipeComment): string {
    return comment.id;
  }
}
