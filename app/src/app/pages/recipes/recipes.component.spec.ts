import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { of, throwError } from 'rxjs';
import { RecipesComponent } from './recipes.component';
import { RecipeService } from '../../services/recipe.service';
import { CategoryService } from '../../services/category.service';
import { AuthService } from '../../services/auth.service';
import { FavoriteService } from '../../services/favorite.service';
import { CuisineService } from '../../services/cuisine.service';
import { LikeService } from '../../services/like.service';
import { DifficultyLevel, Recipe } from '../../models/recipe.model';

describe('RecipesComponent', () => {
  let component: RecipesComponent;
  let fixture: ComponentFixture<RecipesComponent>;
  let recipeService: jasmine.SpyObj<RecipeService>;
  let auth: jasmine.SpyObj<AuthService>;
  let favoriteService: jasmine.SpyObj<FavoriteService>;
  let router: jasmine.SpyObj<Router>;

  const recipe: Recipe = {
    id: 'recipe-1',
    title: 'Soup',
    description: 'Warm',
    preparationTimeMinutes: 20,
    categoryId: 'cat-1',
    category: 'Dinner',
    cuisineId: 'cuisine-1',
    cuisineName: 'Moroccan',
    cuisineSlug: 'moroccan',
    regionId: 'region-1',
    regionName: 'Souss-Massa',
    regionSlug: 'souss-massa',
    difficulty: DifficultyLevel.Medium,
    author: { id: 'user-1', displayName: 'User' },
    imageUrl: '',
    isTraditional: true,
    ingredients: [{ name: 'Salt', quantity: '1 tsp' }],
    steps: [{ stepNumber: 1, instruction: 'Cook' }]
  };

  beforeEach(async () => {
    recipeService = jasmine.createSpyObj<RecipeService>('RecipeService', [
      'getPaged',
      'delete',
    ]);
    auth = jasmine.createSpyObj<AuthService>('AuthService', [
      'isAdmin',
      'isLoggedIn',
      'canManageRecipes',
      'getCurrentUserId'
    ]);
    favoriteService = jasmine.createSpyObj<FavoriteService>('FavoriteService', ['getMine', 'add', 'remove']);
    router = jasmine.createSpyObj<Router>('Router', ['navigate']);

    recipeService.getPaged.and.returnValue(of({
      items: [recipe],
      total: 1,
      page: 1,
      pageSize: 10,
      totalPages: 1
    }));
    favoriteService.getMine.and.returnValue(of([]));
    auth.isAdmin.and.returnValue(false);
    auth.isLoggedIn.and.returnValue(true);
    auth.canManageRecipes.and.returnValue(true);
    auth.getCurrentUserId.and.returnValue('user-1');

    await TestBed.configureTestingModule({
      imports: [RecipesComponent],
      providers: [
        { provide: RecipeService, useValue: recipeService },
        { provide: CategoryService, useValue: { getAll: () => of([]) } },
        { provide: CuisineService, useValue: { getAll: () => of([{ id: 'cuisine-1', name: 'Moroccan', slug: 'moroccan', countryCode: 'MA', isActive: true }]), getRegions: () => of([{ id: 'region-1', name: 'Souss-Massa', slug: 'souss-massa', cuisineId: 'cuisine-1', cuisineName: 'Moroccan', isActive: true }]) } },
        { provide: AuthService, useValue: auth },
        { provide: FavoriteService, useValue: favoriteService },
        { provide: LikeService, useValue: { like: () => of(void 0), unlike: () => of(void 0) } },
        { provide: ActivatedRoute, useValue: { queryParams: of({}) } },
        { provide: Router, useValue: router },
        { provide: ToastrService, useValue: jasmine.createSpyObj('ToastrService', ['success', 'error']) }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(RecipesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('sends difficulty filter as a string', () => {
    component.selectedDifficulty = 'Hard';
    component.loadRecipes();

    expect(recipeService.getPaged).toHaveBeenCalledWith(jasmine.objectContaining({ difficulty: 'Hard' }));
  });

  it('sends each supported difficulty filter value unchanged', () => {
    (['Easy', 'Medium', 'Hard'] as const).forEach(difficulty => {
      component.selectedDifficulty = difficulty;
      component.loadRecipes();

      expect(recipeService.getPaged).toHaveBeenCalledWith(jasmine.objectContaining({ difficulty }));
    });
  });

  it('shows validation details returned by the API', () => {
    const toastr = TestBed.inject(ToastrService) as jasmine.SpyObj<ToastrService>;
    recipeService.getPaged.and.returnValue(throwError(() => ({
      error: {
        errors: {
          difficulty: ['Difficulty must be Easy, Medium, or Hard.']
        }
      }
    })));

    component.loadRecipes();

    expect(toastr.error).toHaveBeenCalledWith('Difficulty must be Easy, Medium, or Hard.');
  });

  it('does not render the old inline owner edit form', () => {
    const compiled = fixture.nativeElement as HTMLElement;

    expect(compiled.textContent).not.toContain('Owner edit');
    expect(compiled.textContent).not.toContain('Edit recipe story');
  });

  it('navigates owner card edit to recipe details edit mode', () => {
    component.editRecipe(recipe);

    expect(router.navigate).toHaveBeenCalledWith(
      ['/recipes', 'recipe-1'],
      { queryParams: { edit: 'true' } }
    );
  });

  it('resets region when cuisine filter changes', () => {
    component.selectedCuisine = 'cuisine-1';
    component.selectedRegion = 'region-1';

    component.onCuisineFilterChange();

    expect(component.selectedRegion).toBe('');
  });

  it('shows recipe creation controls for authenticated normal users', () => {
    auth.isLoggedIn.and.returnValue(true);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;

    expect(compiled.textContent).toContain('Create Recipe');
  });

  it('allows owners to manage their recipes', () => {
    auth.getCurrentUserId.and.returnValue('user-1');
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;

    expect(compiled.textContent).toContain('Edit');
    expect(component.canManageRecipe(recipe)).toBeTrue();
  });

  it('hides edit and delete controls from other normal users', () => {
    auth.getCurrentUserId.and.returnValue('other-user');
    auth.isAdmin.and.returnValue(false);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;

    expect(component.canManageRecipe(recipe)).toBeFalse();
    expect(compiled.textContent).not.toContain('Edit');
  });

  it('allows admins to manage any recipe', () => {
    auth.getCurrentUserId.and.returnValue('other-user');
    auth.isAdmin.and.returnValue(true);

    expect(component.canManageRecipe(recipe)).toBeTrue();
  });

  it('rolls back favorite toggle after API failure', () => {
    favoriteService.add.and.returnValue(throwError(() => new Error('failed')));

    component.toggleFavorite('recipe-1');

    expect(component.isFavorite('recipe-1')).toBeFalse();
  });
});
