
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { Category } from '../../models/category.model';
import { CreateRecipe, DifficultyLevel } from '../../models/recipe.model';
import { Cuisine } from '../../models/cuisine.model';
import { Region } from '../../models/region.model';
import { CategoryService } from '../../services/category.service';
import { CuisineService } from '../../services/cuisine.service';
import { RecipeService } from '../../services/recipe.service';
import { API_BASE_URL } from '../../app-api.config';
import { resolveAssetUrl } from '../../core/utils/asset-url.util';
import { RecipeFormMapper } from '../../core/recipes/recipe-form.mapper';

@Component({
    selector: 'app-create-recipe',
    imports: [FormsModule, RouterModule],
    templateUrl: './create-recipe.component.html',
    changeDetection: ChangeDetectionStrategy.Eager,
    styleUrl: './create-recipe.component.css'
})
export class CreateRecipeComponent implements OnInit {
  readonly DifficultyLevel = DifficultyLevel;
  categories: Category[] = [];
  cuisines: Cuisine[] = [];
  regions: Region[] = [];
  isSubmitting = false;
  error = '';
  selectedImage: File | null = null;
  imagePreviewUrl: string | null = null;

  recipe: CreateRecipe = RecipeFormMapper.empty();

  constructor(
    private recipeService: RecipeService,
    private categoryService: CategoryService,
    private cuisineService: CuisineService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.categoryService.getAll().subscribe({
      next: categories => this.categories = categories,
      error: () => this.error = 'Failed to load categories.'
    });

    this.cuisineService.getAll().subscribe({
      next: cuisines => this.cuisines = cuisines,
      error: () => this.error = 'Failed to load cuisines.'
    });
  }

  onCuisineChange(): void {
    this.recipe.regionId = null;
    this.regions = [];

    if (!this.recipe.cuisineId) {
      return;
    }

    this.cuisineService.getRegions(this.recipe.cuisineId).subscribe({
      next: regions => this.regions = regions,
      error: () => this.error = 'Failed to load regions.'
    });
  }

  addIngredient(): void {
    if (this.recipe.ingredients.length < 100) {
      this.recipe.ingredients.push({ name: '', quantity: '' });
    }
  }

  removeIngredient(index: number): void {
    if (this.recipe.ingredients.length > 1) {
      this.recipe.ingredients.splice(index, 1);
    }
  }

  addStep(): void {
    if (this.recipe.steps.length < 100) {
      this.recipe.steps.push({
        stepNumber: this.recipe.steps.length + 1,
        instruction: ''
      });
    }
  }

  removeStep(index: number): void {
    if (this.recipe.steps.length > 1) {
      this.recipe.steps.splice(index, 1);
      RecipeFormMapper.renumberSteps(this.recipe);
    }
  }

  previewImageUrl(): string {
    return this.imagePreviewUrl || resolveAssetUrl(this.recipe.imageUrl, API_BASE_URL);
  }

  onImageSelected(event: Event): void {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;
    if (!["image/jpeg", "image/png", "image/webp"].includes(file.type) || file.size > 5 * 1024 * 1024) {
      this.error = "Choose a JPG, PNG, or WEBP image no larger than 5 MB.";
      return;
    }
    this.removeSelectedImage();
    this.selectedImage = file;
    this.imagePreviewUrl = URL.createObjectURL(file);
  }

  removeSelectedImage(): void {
    if (this.imagePreviewUrl) URL.revokeObjectURL(this.imagePreviewUrl);
    this.selectedImage = null; this.imagePreviewUrl = null;
  }

  trackIngredient(index: number, _ingredient: unknown): number {
    return index;
  }

  trackStep(index: number, _step: unknown): number {
    return index;
  }

  submit(): void {
    this.error = '';
    const result = RecipeFormMapper.toPayload(this.recipe);
    if (!result.payload) {
      this.error = result.error || 'Please check the recipe information.';
      return;
    }
    const payload = result.payload;

    this.isSubmitting = true;

    this.recipeService.create(payload).subscribe({
      next: recipe => {
        if (!this.selectedImage) { this.router.navigate(["/recipes", recipe.id]); return; }
        this.recipeService.uploadImage(recipe.id, this.selectedImage).subscribe({
          next: () => this.router.navigate(["/recipes", recipe.id]),
          error: () => { this.error = "Recipe was created, but the image could not be uploaded."; this.isSubmitting = false; }
        });
      },
      error: error => {
        this.error = this.getApiError(error);
        this.isSubmitting = false;
      }
    });
  }


  private getApiError(error: HttpErrorResponse): string {
    const fallback = 'Something went wrong while publishing the recipe. Please try again.';

    if (error.status >= 500 || error.status === 0) {
      return fallback;
    }

    const response = error.error;
    const errors = response?.errors;

    if (errors) {
      const message = Object.entries(errors)
        .flatMap(([field, values]) => Array.isArray(values)
          ? values.map(value => ({ field, value }))
          : [])
        .find(item => typeof item.value === 'string' && !this.isTechnicalValidationMessage(item.field, item.value)) as { value: string } | undefined;

      if (message) {
        return message.value;
      }
    }

    if (error.status === 400) {
      const message = typeof response?.message === 'string' ? response.message : '';
      const title = typeof response?.title === 'string' ? response.title : '';

      if (message.trim() && !this.isTechnicalValidationMessage('', message)) {
        return message;
      }

      if (title.trim() && !this.isTechnicalValidationMessage('', title)) {
        return title;
      }

      return 'Please check the recipe information and try again.';
    }

    return fallback;
  }

  private toOptionalString(value: string | null | undefined): string | null {
    const trimmed = value?.trim();
    return trimmed ? trimmed : null;
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
}
