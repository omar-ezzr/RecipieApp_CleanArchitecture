import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { Difficulty, DifficultyLevel, Recipe } from '../../models/recipe.model';
import { Category } from '../../models/category.model';
import { Cuisine } from '../../models/cuisine.model';
import { Region } from '../../models/region.model';
import { ActivatedRoute, Router } from '@angular/router';
import { RecipeService } from '../../services/recipe.service';
import { CategoryService } from '../../services/category.service';
import { CuisineService } from '../../services/cuisine.service';
import { AuthService } from '../../services/auth.service';
import { ToastrService } from 'ngx-toastr';
import { FavoriteService } from '../../services/favorite.service';
import { LikeService } from '../../services/like.service';
import { API_BASE_URL } from '../../app-api.config';
import { resolveAssetUrl } from '../../core/utils/asset-url.util';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { RecipeCardComponent } from '../../shared/components/recipe-card/recipe-card.component';
import { RecipeCardSkeletonComponent } from '../../shared/components/recipe-card-skeleton/recipe-card-skeleton.component';
@Component({
  selector: 'app-recipes',
  standalone: true,
imports: [CommonModule, FormsModule, RouterModule, EmptyStateComponent, RecipeCardComponent, RecipeCardSkeletonComponent],
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
  cuisines: Cuisine[] = [];
  regions: Region[] = [];
  isLoading: boolean = false;
  favoriteRecipeIds = new Set<string>();
  busyLikes = new Set<string>();
  skeletonCards = Array(12).fill(0);
  filtersOpen = false;

  // ========================
  //  FILTER STATE
  // ========================
  searchTerm: string = '';
  selectedCategory: string = '';
  selectedCuisine: string = '';
  selectedRegion: string = '';
  isTraditionalOnly = false;
  selectedDifficulty: Difficulty | '' = '';
  private searchSubject = new Subject<string>();


  // ========================
  // pagination
  // ========================

  currentPage: number = 1;
pageSize: number = 10;
totalItems: number = 0;
totalPages: number = 0;
sortBy: string = '';
visiblePages: (number | string)[] = [];

  
constructor(
  private recipeService: RecipeService,
  private categoryService: CategoryService,
  private cuisineService: CuisineService,
  public auth: AuthService,
  private route: ActivatedRoute,
  private router: Router,
  private toastr: ToastrService,
    private favoriteService: FavoriteService,
    private likeService: LikeService

) {}

  // ========================
  // INIT
  // ========================
ngOnInit() {

  this.loadCategories();
  this.loadCuisines();

  this.route.queryParams.subscribe(params => {

    this.currentPage = +params['page'] || 1;

    this.searchTerm = params['search'] || '';

    this.selectedCategory = params['categoryId'] || '';
    this.selectedCuisine = params['cuisineId'] || '';
    this.selectedRegion = params['regionId'] || '';
    this.isTraditionalOnly = params['isTraditional'] === 'true';

    this.selectedDifficulty = this.toDifficulty(params['difficulty']);

    this.sortBy = params['sortBy'] || '';

    this.pageSize = +params['pageSize'] || 10;

    if (this.selectedCuisine) {
      this.loadRegions(this.selectedCuisine);
    } else {
      this.regions = [];
    }

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
  cuisineId: this.selectedCuisine || undefined,
  regionId: this.selectedRegion || undefined,
  isTraditional: this.isTraditionalOnly ? true : undefined,
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
      cuisineId: this.selectedCuisine || null,
      regionId: this.selectedRegion || null,
      isTraditional: this.isTraditionalOnly ? true : null,
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

  loadCuisines() {
    this.cuisineService.getAll().subscribe({
      next: (data) => this.cuisines = data,
      error: () => {
        this.toastr.error('Failed to load cuisines');
      }
    });
  }

  loadRegions(cuisineId: string) {
    this.cuisineService.getRegions(cuisineId).subscribe({
      next: (data) => this.regions = data,
      error: () => {
        this.toastr.error('Failed to load regions');
      }
    });
  }

  // ========================
  //  FILTER LOGIC (FIXED)
  // ========================


applyFilters() {
  this.currentPage = 1;
  this.updateQueryParams();
}

onCuisineFilterChange() {
  this.selectedRegion = '';
  this.currentPage = 1;

  if (this.selectedCuisine) {
    this.loadRegions(this.selectedCuisine);
  } else {
    this.regions = [];
  }

  this.updateQueryParams();
}

selectCuisine(cuisineId: string) {
  this.selectedCuisine = cuisineId;
  this.onCuisineFilterChange();
}

clearCuisineFilter() {
  this.selectedCuisine = '';
  this.onCuisineFilterChange();
}

clearAllFilters() {
  this.searchTerm = '';
  this.selectedCategory = '';
  this.selectedCuisine = '';
  this.selectedRegion = '';
  this.selectedDifficulty = '';
  this.isTraditionalOnly = false;
  this.sortBy = '';
  this.currentPage = 1;
  this.regions = [];
  this.updateQueryParams();
}

clearSearch() {
  this.searchTerm = '';
  this.onSearchChange();
}

toggleFilters() {
  this.filtersOpen = !this.filtersOpen;
}

onRegionFilterChange() {
  this.currentPage = 1;
  this.updateQueryParams();
}

onTraditionalFilterChange() {
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
// EDIT
// ========================

editRecipe(recipe: Recipe): void {
  if (!this.canManageRecipe(recipe)) {
    this.toastr.error('You can only edit recipes you created');

    return;
  }

  this.router.navigate(
    ['/recipes', recipe.id],
    { queryParams: { edit: 'true' } }
  );
}


// ========================
// DELETE (OPTIMISTIC)
// ========================

deleteRecipe(recipeOrId: Recipe | string) {
  const id = typeof recipeOrId === 'string' ? recipeOrId : recipeOrId.id;
  const recipe = this.recipes.find(r => r.id === id);

  if (recipe && !this.canManageRecipe(recipe)) {
    this.toastr.error('You can only delete recipes you created');

    return;
  }

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

canManageRecipe(recipe: Recipe): boolean {
  const currentUserId = this.auth.getCurrentUserId();

  return this.auth.isAdmin() || (!!currentUserId && recipe.author?.id === currentUserId);
}

difficultyLabel(value: DifficultyLevel): Difficulty {
  switch (value) {
    case DifficultyLevel.Easy:
      return 'Easy';
    case DifficultyLevel.Medium:
      return 'Medium';
    case DifficultyLevel.Hard:
      return 'Hard';
    default:
      return 'Easy';
  }
}

difficultyValue(value: Difficulty): DifficultyLevel {
  return DifficultyLevel[value];
}

resolveImageUrl(path: string | null | undefined): string {
  return resolveAssetUrl(path, API_BASE_URL);
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

toggleFavoriteRecipe(recipe: Recipe): void {
  this.toggleFavorite(recipe.id);
}

toggleLikeRecipe(recipe: Recipe): void {
  if (!this.auth.isLoggedIn()) {
    this.router.navigate(['/login']);
    return;
  }

  if (this.busyLikes.has(recipe.id)) {
    return;
  }

  this.busyLikes.add(recipe.id);
  const wasLiked = !!recipe.isLikedByCurrentUser;
  const previousCount = Math.max(0, recipe.likeCount || 0);

  recipe.isLikedByCurrentUser = !wasLiked;
  recipe.likeCount = Math.max(0, previousCount + (wasLiked ? -1 : 1));

  const request = wasLiked
    ? this.likeService.unlike(recipe.id)
    : this.likeService.like(recipe.id);

  request.subscribe({
    next: () => {
      this.busyLikes.delete(recipe.id);
      this.recipeService.clearCache();
    },
    error: error => {
      recipe.isLikedByCurrentUser = wasLiked;
      recipe.likeCount = previousCount;
      this.busyLikes.delete(recipe.id);
      this.toastr.error(this.getApiError(error, 'Failed to update like'));
    }
  });
}

trackRecipe(_index: number, recipe: Recipe): string {
  return recipe.id;
}

trackCuisine(_index: number, cuisine: Cuisine): string {
  return cuisine.id;
}

trackRegion(_index: number, region: Region): string {
  return region.id;
}

trackPage(index: number, page: number | string): number | string {
  return page === '...' ? `ellipsis-${index}` : page;
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
