import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { CreateRecipe, Difficulty, Recipe } from '../../models/recipe.model';
import { Category } from '../../models/category.model';
import { ActivatedRoute, Router } from '@angular/router';
import { RecipeService } from '../../services/recipe.service';
import { CategoryService } from '../../services/category.service';
import { AuthService } from '../../services/auth.service';
import { ToastrService } from 'ngx-toastr';
import { FavoriteService } from '../../services/favorite.service';
@Component({
  selector: 'app-recipes',
  standalone: true,
imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './recipes.component.html',
  styleUrls: ['./recipes.component.css']
})
export class RecipesComponent implements OnInit {

  // ========================
  //  DATA
  // ========================
  recipes: Recipe[] = [];
  filteredRecipes: Recipe[] = [];
  categories: Category[] = [];
  isLoading: boolean = false;
  favoriteRecipeIds = new Set<string>();
  skeletonCards = Array(12).fill(0);

  // ========================
  //  FILTER STATE
  // ========================
  searchTerm: string = '';
  selectedCategory: string = '';
  selectedDifficulty: Difficulty | '' = '';
  private searchSubject = new Subject<string>();

  // ========================
  //  FORM STATE
  // ========================
  editingId: string | null = null;
  
  // ========================
  // pagination
  // ========================

  currentPage: number = 1;
pageSize: number = 10;
totalItems: number = 0;
totalPages: number = 0;
sortBy: string = '';
visiblePages: (number | string)[] = [];
  newRecipe: CreateRecipe = {
    title: '',
    description: '',
    preparationTimeMinutes: 0,
    categoryId: '',
    difficulty: 'Easy',
    imageUrl: ''
  };

  
constructor(
  private recipeService: RecipeService,
  private categoryService: CategoryService,
  public auth: AuthService,
  private route: ActivatedRoute,
  private router: Router,
  private toastr: ToastrService,
    private favoriteService: FavoriteService

) {}

  // ========================
  // INIT
  // ========================
ngOnInit() {

  this.loadCategories();

  this.route.queryParams.subscribe(params => {

    this.currentPage = +params['page'] || 1;

    this.searchTerm = params['search'] || '';

    this.selectedCategory = params['categoryId'] || '';

    this.selectedDifficulty = this.toDifficulty(params['difficulty']);

    this.sortBy = params['sortBy'] || '';

    this.pageSize = +params['pageSize'] || 10;

    this.loadRecipes();
    this.loadFavorites();
  });

  this.searchSubject
    .pipe(
      debounceTime(400),
      distinctUntilChanged()
    )
    .subscribe(() => {

      this.currentPage = 1;

      this.updateQueryParams();
    });
}

loadRecipes() {

  this.isLoading = true;

const params = {
  page: this.currentPage,
  pageSize: this.pageSize,
  search: this.searchTerm || undefined,
  difficulty: this.selectedDifficulty || undefined,
  categoryId: this.selectedCategory || undefined,
  sortBy: this.sortBy || undefined
};
  this.recipeService.getPaged(params).subscribe({

    next: (res) => {

      this.recipes = res.items;
      this.filteredRecipes = res.items;

      this.totalItems = res.total;
      this.currentPage = res.page;
      this.pageSize = res.pageSize;
      this.totalPages = res.totalPages;

      this.generateVisiblePages();

      this.isLoading = false;
    },

    error: (error) => {
  this.toastr.error(this.getApiError(error, 'Failed to load recipes'));
  this.isLoading = false;
}
  });
}


updateQueryParams() {

  this.router.navigate([], {
    relativeTo: this.route,
    queryParams: {
      page: this.currentPage,
      search: this.searchTerm || null,
      categoryId: this.selectedCategory || null,
      difficulty: this.selectedDifficulty || null,
      sortBy: this.sortBy || null,
      pageSize: this.pageSize
    },
    queryParamsHandling: 'merge'
  });
}


generateVisiblePages() {

  const pages: (number | string)[] = [];

  if (this.totalPages <= 7) {
    for (let i = 1; i <= this.totalPages; i++) {
      pages.push(i);
    }
  }
  else {

    // START
    if (this.currentPage <= 4) {
      pages.push(1, 2, 3, 4, 5, '...', this.totalPages);
    }

    // END
    else if (this.currentPage >= this.totalPages - 3) {
      pages.push(
        1,
        '...',
        this.totalPages - 4,
        this.totalPages - 3,
        this.totalPages - 2,
        this.totalPages - 1,
        this.totalPages
      );
    }

    // MIDDLE
    else {
      pages.push(
        1,
        '...',
        this.currentPage - 1,
        this.currentPage,
        this.currentPage + 1,
        '...',
        this.totalPages
      );
    }
  }

  this.visiblePages = pages;
}
  loadCategories() {
    this.categoryService.getAll().subscribe({
      next: (data) => this.categories = data,
error: () => {
  this.toastr.error('Failed to load categories');
}    });
  }

  // ========================
  //  FILTER LOGIC (FIXED)
  // ========================


applyFilters() {
  this.currentPage = 1;
  this.updateQueryParams();
}


goToPage(page: number) {
  if (page < 1 || page > this.totalPages) return;

  this.currentPage = page;
this.updateQueryParams();
}


onSearchChange() {
  this.searchSubject.next(this.searchTerm);
}

// ========================
// CREATE
// ========================

createRecipe() {

  if (!this.newRecipe.title || !this.newRecipe.categoryId || !this.newRecipe.difficulty) {

    this.toastr.error('Title, category, and difficulty are required');

    return;
  }

  this.recipeService.create(this.newRecipe).subscribe({

    next: () => {

      this.toastr.success('Recipe created successfully');

      this.recipeService.clearCache();

      this.loadRecipes();

      this.resetForm();
    },

    error: (error) => {

      this.toastr.error(this.getApiError(error, 'Failed to create recipe'));
    }
  });
}


// ========================
// EDIT
// ========================

startEdit(recipe: Recipe) {

  this.editingId = recipe.id;

  this.newRecipe = {
    title: recipe.title,
    description: recipe.description,
    preparationTimeMinutes: recipe.preparationTimeMinutes,
    categoryId: recipe.categoryId,
    difficulty: recipe.difficulty,
    imageUrl: recipe.imageUrl || ''
  };
}


updateRecipe() {

  if (!this.editingId) return;

  this.recipeService.update(this.editingId, this.newRecipe).subscribe({

    next: () => {

      this.toastr.success('Recipe updated successfully');

      this.recipeService.clearCache();

      this.editingId = null;

      this.loadRecipes();

      this.resetForm();
    },

    error: (error) => {

      this.toastr.error(this.getApiError(error, 'Failed to update recipe'));
    }
  });
}


// ========================
// DELETE (OPTIMISTIC)
// ========================

deleteRecipe(id: string) {

  if (!confirm('Delete this recipe?')) return;

  // BACKUP
  const previousRecipes = [...this.filteredRecipes];

  // OPTIMISTIC UI UPDATE
  this.filteredRecipes =
    this.filteredRecipes.filter(r => r.id !== id);

  this.recipes =
    this.recipes.filter(r => r.id !== id);

  this.totalItems--;

  this.totalPages = Math.ceil(this.totalItems / this.pageSize);

  this.generateVisiblePages();

  // API CALL
  this.recipeService.delete(id).subscribe({

    next: () => {

      this.toastr.success('Recipe deleted successfully');

      this.recipeService.clearCache();
    },

    error: (error) => {

      // ROLLBACK
      this.filteredRecipes = previousRecipes;
      this.recipes = previousRecipes;

      this.totalItems++;

      this.totalPages = Math.ceil(this.totalItems / this.pageSize);

      this.generateVisiblePages();

      this.toastr.error(this.getApiError(error, 'Failed to delete recipe'));
    }
  });
}



// ========================
// RESET
// ========================

resetForm() {

  this.newRecipe = {
    title: '',
    description: '',
    preparationTimeMinutes: 0,
    categoryId: '',
    difficulty: 'Easy',
    imageUrl: ''
  };

  this.editingId = null;
}

loadFavorites(): void {
  this.favoriteService.getMine().subscribe({
    next: favorites => {
      this.favoriteRecipeIds = new Set(
        favorites.map(favorite => favorite.recipeId)
      );
    },
    error: error => {
      this.toastr.error(this.getApiError(error, 'Failed to load favorites'));
    }
  });
}

isFavorite(recipeId: string): boolean {
  return this.favoriteRecipeIds.has(recipeId);
}

toggleFavorite(recipeId: string, event?: Event): void {
  event?.stopPropagation();

  const wasFavorite = this.isFavorite(recipeId);
  const updatedFavorites = new Set(this.favoriteRecipeIds);

  if (wasFavorite) {
    updatedFavorites.delete(recipeId);
    this.favoriteRecipeIds = updatedFavorites;

    this.favoriteService.remove(recipeId).subscribe({
      error: error => {
        updatedFavorites.add(recipeId);
        this.favoriteRecipeIds = new Set(updatedFavorites);
        this.toastr.error(this.getApiError(error, 'Failed to update favorite'));
      }
    });

    return;
  }

  updatedFavorites.add(recipeId);
  this.favoriteRecipeIds = updatedFavorites;

  this.favoriteService.add(recipeId).subscribe({
    error: error => {
      updatedFavorites.delete(recipeId);
      this.favoriteRecipeIds = new Set(updatedFavorites);
      this.toastr.error(this.getApiError(error, 'Failed to update favorite'));
    }
  });
}

private getApiError(error: any, fallback: string): string {
  const validationErrors = error?.error?.errors;

  if (validationErrors) {
    const firstError = Object.values(validationErrors)
      .flat()
      .find((message): message is string => typeof message === 'string');

    if (firstError) {
      return firstError;
    }
  }

  if (typeof error?.error === 'string') {
    return error.error;
  }

  if (typeof error?.error?.title === 'string') {
    return error.error.title;
  }

  if (typeof error?.error?.message === 'string') {
    return error.error.message;
  }

  return fallback;
}

private toDifficulty(value: string | undefined): Difficulty | '' {
  return value === 'Easy' || value === 'Medium' || value === 'Hard'
    ? value
    : '';
}


}
