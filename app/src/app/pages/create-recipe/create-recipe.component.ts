import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
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

@Component({
  selector: 'app-create-recipe',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './create-recipe.component.html',
  styleUrl: './create-recipe.component.css'
})
export class CreateRecipeComponent implements OnInit {
  readonly DifficultyLevel = DifficultyLevel;
  categories: Category[] = [];
  cuisines: Cuisine[] = [];
  regions: Region[] = [];
  isSubmitting = false;
  error = '';

  recipe: CreateRecipe = this.newRecipe();

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
      this.renumberSteps();
    }
  }

  previewImageUrl(): string {
    return resolveAssetUrl(this.recipe.imageUrl, API_BASE_URL);
  }

  trackIngredient(index: number): number {
    return index;
  }

  trackStep(index: number): number {
    return index;
  }

  submit(): void {
    this.error = '';
    this.renumberSteps();

    const payload = this.buildPayload();

    if (!payload) {
      return;
    }

    this.isSubmitting = true;

    this.recipeService.create(payload).subscribe({
      next: recipe => this.router.navigate(['/recipes', recipe.id]),
      error: error => {
        this.error = this.getApiError(error);
        this.isSubmitting = false;
      }
    });
  }

  private buildPayload(): CreateRecipe | null {
    const title = this.recipe.title.trim();
    if (!title) {
      this.error = 'Please enter a recipe title.';
      return null;
    }

    const description = this.recipe.description.trim();
    if (!description) {
      this.error = 'Please add a description.';
      return null;
    }

    if (!this.recipe.categoryId) {
      this.error = 'Please select a category.';
      return null;
    }

    if (!this.recipe.cuisineId) {
      this.error = 'Please select a cuisine.';
      return null;
    }

    const preparationTimeMinutes = Number(this.recipe.preparationTimeMinutes);
    if (!Number.isFinite(preparationTimeMinutes) || preparationTimeMinutes <= 0) {
      this.error = 'Preparation time must be greater than 0 minutes.';
      return null;
    }

    const ingredients = this.recipe.ingredients.map(ingredient => ({
      name: ingredient.name.trim(),
      quantity: ingredient.quantity.trim()
    }));

    if (!ingredients.length || ingredients.some(ingredient => !ingredient.name)) {
      this.error = 'Please enter a name for every ingredient.';
      return null;
    }

    const steps = this.recipe.steps.map((step, index) => ({
      stepNumber: index + 1,
      instruction: step.instruction.trim()
    }));

    if (!steps.length || steps.some(step => !step.instruction)) {
      this.error = 'Please add an instruction for every preparation step.';
      return null;
    }

    return {
      title,
      description,
      preparationTimeMinutes,
      categoryId: this.recipe.categoryId,
      cuisineId: this.recipe.cuisineId,
      regionId: this.recipe.regionId || null,
      difficulty: this.recipe.difficulty,
      imageUrl: this.toOptionalString(this.recipe.imageUrl),
      traditionalName: this.toOptionalString(this.recipe.traditionalName),
      originDescription: this.toOptionalString(this.recipe.originDescription),
      isTraditional: this.recipe.isTraditional,
      servingOccasion: this.toOptionalString(this.recipe.servingOccasion),
      ingredients,
      steps
    };
  }

  private renumberSteps(): void {
    this.recipe.steps = this.recipe.steps.map((step, index) => ({
      ...step,
      stepNumber: index + 1
    }));
  }

  private newRecipe(): CreateRecipe {
    return {
      title: '',
      description: '',
      preparationTimeMinutes: 30,
      categoryId: '',
      cuisineId: '',
      regionId: null,
      difficulty: DifficultyLevel.Easy,
      imageUrl: null,
      traditionalName: null,
      originDescription: null,
      isTraditional: false,
      servingOccasion: null,
      ingredients: [{ name: '', quantity: '' }],
      steps: [{ stepNumber: 1, instruction: '' }]
    };
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
