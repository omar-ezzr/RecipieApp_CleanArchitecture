import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { DifficultyLevel, Recipe } from '../../models/recipe.model';
import { RecipeService } from '../../services/recipe.service';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { RecipeCardComponent } from '../../shared/components/recipe-card/recipe-card.component';
import { RecipeCardSkeletonComponent } from '../../shared/components/recipe-card-skeleton/recipe-card-skeleton.component';

@Component({
  selector: 'app-my-recipes',
  standalone: true,
  imports: [CommonModule, RouterModule, EmptyStateComponent, RecipeCardComponent, RecipeCardSkeletonComponent],
  templateUrl: './my-recipes.component.html',
  styleUrl: './my-recipes.component.css'
})
export class MyRecipesComponent implements OnInit {
  recipes: Recipe[] = [];
  isLoading = false;
  error = '';
  currentPage = 1;
  pageSize = 10;
  totalItems = 0;
  totalPages = 0;
  skeletonCards = Array(6).fill(0);

  constructor(
    private recipeService: RecipeService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadRecipes();
  }

  loadRecipes(): void {
    this.isLoading = true;
    this.error = '';

    this.recipeService.getMine({
      page: this.currentPage,
      pageSize: this.pageSize
    }).subscribe({
      next: result => {
        this.recipes = result.items;
        this.totalItems = result.total;
        this.currentPage = result.page;
        this.pageSize = result.pageSize;
        this.totalPages = result.totalPages;
        this.isLoading = false;
      },
      error: error => {
        this.error = this.getApiError(error, 'Failed to load your recipes.');
        this.isLoading = false;
      }
    });
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages) {
      return;
    }

    this.currentPage = page;
    this.loadRecipes();
  }

  deleteRecipe(recipe: Recipe): void {
    if (!confirm(`Delete ${recipe.title}?`)) {
      return;
    }

    this.recipeService.delete(recipe.id).subscribe({
      next: () => {
        this.recipes = this.recipes.filter(item => item.id !== recipe.id);
        this.totalItems = Math.max(0, this.totalItems - 1);
      },
      error: error => {
        this.error = this.getApiError(error, 'Failed to delete recipe.');
      }
    });
  }

  editRecipe(recipe: Recipe): void {
    this.router.navigate(['/recipes', recipe.id], { queryParams: { edit: 'true' } });
  }

  trackRecipe(_index: number, recipe: Recipe): string {
    return recipe.id;
  }

  difficultyLabel(value: DifficultyLevel): string {
    return value === DifficultyLevel.Easy
      ? 'Easy'
      : value === DifficultyLevel.Medium
        ? 'Medium'
        : 'Hard';
  }

  private getApiError(error: any, fallback: string): string {
    if (error?.status === 403) {
      return 'You are not allowed to manage this recipe.';
    }

    if (error?.status === 404) {
      return 'Recipe was not found.';
    }

    return error?.error?.message || error?.error?.title || error?.error || fallback;
  }
}
