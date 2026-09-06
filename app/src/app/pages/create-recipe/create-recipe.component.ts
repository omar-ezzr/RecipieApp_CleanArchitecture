
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, OnDestroy, ChangeDetectionStrategy } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { Category } from '../../models/category.model';
import { CreateRecipe, DifficultyLevel, RecipeMedia } from '../../models/recipe.model';
import { Cuisine } from '../../models/cuisine.model';
import { Region } from '../../models/region.model';
import { CategoryService } from '../../services/category.service';
import { CuisineService } from '../../services/cuisine.service';
import { RecipeService } from '../../services/recipe.service';
import { API_BASE_URL } from '../../app-api.config';
import { resolveAssetUrl } from '../../core/utils/asset-url.util';
import { RecipeFormMapper } from '../../core/recipes/recipe-form.mapper';
import { catchError, concatMap, from, of, toArray } from 'rxjs';

interface SelectedMedia { localId: string; file: File; previewUrl: string; kind: 'image' | 'video'; isMain: boolean; }

@Component({
    selector: 'app-create-recipe',
    imports: [FormsModule, RouterModule],
    templateUrl: './create-recipe.component.html',
    changeDetection: ChangeDetectionStrategy.Eager,
    styleUrl: './create-recipe.component.css'
})
export class CreateRecipeComponent implements OnInit, OnDestroy {
  readonly DifficultyLevel = DifficultyLevel;
  categories: Category[] = [];
  cuisines: Cuisine[] = [];
  regions: Region[] = [];
  isSubmitting = false;
  error = '';
  selectedMedia: SelectedMedia[] = [];
  createdRecipeId: string | null = null;

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
    const cover = this.selectedMedia.find(item => item.isMain);
    return cover?.kind === 'image' ? cover.previewUrl : resolveAssetUrl(this.recipe.imageUrl, API_BASE_URL);
  }

  onMediaSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const files = Array.from(input.files ?? []);
    input.value = '';
    if (!files.length) return;
    const capacity = 9 - this.selectedMedia.length;
    if (files.length > capacity) this.error = `You can add only ${capacity} more media item${capacity === 1 ? '' : 's'} (maximum 9).`;
    for (const file of files.slice(0, Math.max(0, capacity))) {
      const kind = this.validateMedia(file);
      if (!kind) continue;
      this.selectedMedia.push({ localId: crypto.randomUUID(), file, previewUrl: URL.createObjectURL(file), kind, isMain: this.selectedMedia.length === 0 });
    }
  }

  removeSelectedMedia(localId: string): void {
    const index = this.selectedMedia.findIndex(item => item.localId === localId);
    if (index < 0) return;
    const [removed] = this.selectedMedia.splice(index, 1);
    URL.revokeObjectURL(removed.previewUrl);
    if (removed.isMain && this.selectedMedia.length) this.selectedMedia[0].isMain = true;
  }

  setSelectedCover(localId: string): void { this.selectedMedia.forEach(item => item.isMain = item.localId === localId); }
  trackMedia(_index: number, item: SelectedMedia): string { return item.localId; }
  ngOnDestroy(): void { this.clearSelectedMedia(); }
  private clearSelectedMedia(): void { this.selectedMedia.forEach(item => URL.revokeObjectURL(item.previewUrl)); this.selectedMedia = []; }
  private validateMedia(file: File): 'image' | 'video' | null {
    const images = ['image/jpeg', 'image/png', 'image/webp']; const videos = ['video/mp4', 'video/webm'];
    if (images.includes(file.type)) { if (file.size > 5 * 1024 * 1024) this.error = 'Images must be 5 MB or smaller.'; else return 'image'; }
    else if (videos.includes(file.type)) { if (file.size > 50 * 1024 * 1024) this.error = 'Videos must be 50 MB or smaller.'; else return 'video'; }
    else this.error = 'Choose JPEG, PNG, WEBP, MP4, or WebM media.';
    return null;
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
    if (!result.payload) { this.error = result.error || 'Please check the recipe information.'; return; }
    if (!this.selectedMedia.length) { this.error = 'Add at least one photo or video.'; return; }
    this.isSubmitting = true;
    const selected = [...this.selectedMedia];
    this.recipeService.create(result.payload).pipe(concatMap(recipe => {
      this.createdRecipeId = recipe.id;
      return from(selected).pipe(concatMap(item => this.recipeService.addMedia(recipe.id, item.file).pipe(catchError(() => of(null)))), toArray(), concatMap(uploaded => {
        const successes = uploaded.filter((media): media is RecipeMedia => media !== null);
        const failed = uploaded.length - successes.length;
        if (!successes.length) { this.error = `Recipe created, but ${failed} media item${failed === 1 ? '' : 's'} failed to upload.`; this.isSubmitting = false; return of(null); }
        const coverIndex = selected.findIndex(item => item.isMain);
        const cover = successes[coverIndex];
        const order = successes.map(media => media.id);
        const finish = cover && !cover.isMain ? this.recipeService.setMainMedia(recipe.id, cover.id).pipe(concatMap(() => this.recipeService.reorderMedia(recipe.id, order))) : this.recipeService.reorderMedia(recipe.id, order);
        return finish.pipe(concatMap(() => of({ recipe, failed })));
      }));
    })).subscribe({ next: value => { if (!value) return; if (value.failed) { this.error = `Recipe created, but ${value.failed} media item${value.failed === 1 ? '' : 's'} failed to upload.`; this.isSubmitting = false; return; } this.clearSelectedMedia(); this.router.navigate(['/recipes', value.recipe.id]); }, error: error => { this.error = this.getApiError(error); this.isSubmitting = false; } });
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
