import { CommonModule } from '@angular/common';
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
    this.isSubmitting = true;

    this.recipeService.create(this.recipe).subscribe({
      next: recipe => this.router.navigate(['/recipes', recipe.id]),
      error: error => {
        this.error = this.getApiError(error, 'Failed to create recipe.');
        this.isSubmitting = false;
      }
    });
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

  private getApiError(error: any, fallback: string): string {
    const errors = error?.error?.errors;

    if (errors) {
      const message = Object.values(errors)
        .flat()
        .find((value): value is string => typeof value === 'string');

      if (message) {
        return message;
      }
    }

    return error?.error?.message || error?.error?.title || error?.error || fallback;
  }
}
