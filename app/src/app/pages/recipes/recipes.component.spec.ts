import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { of, throwError } from 'rxjs';
import { RecipesComponent } from './recipes.component';
import { RecipeService } from '../../services/recipe.service';
import { CategoryService } from '../../services/category.service';
import { AuthService } from '../../services/auth.service';
import { FavoriteService } from '../../services/favorite.service';
import { Recipe } from '../../models/recipe.model';

describe('RecipesComponent', () => {
  let component: RecipesComponent;
  let fixture: ComponentFixture<RecipesComponent>;
  let recipeService: jasmine.SpyObj<RecipeService>;
  let auth: jasmine.SpyObj<AuthService>;
  let favoriteService: jasmine.SpyObj<FavoriteService>;

  const recipe: Recipe = {
    id: 'recipe-1',
    title: 'Soup',
    description: 'Warm',
    preparationTimeMinutes: 20,
    categoryId: 'cat-1',
    category: 'Dinner',
    difficulty: 'Medium',
    imageUrl: '',
    ingredients: [],
    steps: []
  };

  beforeEach(async () => {
    recipeService = jasmine.createSpyObj<RecipeService>('RecipeService', [
      'getPaged',
      'create',
      'update',
      'delete',
      'clearCache'
    ]);
    auth = jasmine.createSpyObj<AuthService>('AuthService', ['isAdmin', 'canManageRecipes']);
    favoriteService = jasmine.createSpyObj<FavoriteService>('FavoriteService', ['getMine', 'add', 'remove']);

    recipeService.getPaged.and.returnValue(of({
      items: [recipe],
      total: 1,
      page: 1,
      pageSize: 10,
      totalPages: 1
    }));
    favoriteService.getMine.and.returnValue(of([]));
    auth.isAdmin.and.returnValue(false);
    auth.canManageRecipes.and.returnValue(false);

    await TestBed.configureTestingModule({
      imports: [RecipesComponent],
      providers: [
        { provide: RecipeService, useValue: recipeService },
        { provide: CategoryService, useValue: { getAll: () => of([]) } },
        { provide: AuthService, useValue: auth },
        { provide: FavoriteService, useValue: favoriteService },
        { provide: ActivatedRoute, useValue: { queryParams: of({}) } },
        { provide: Router, useValue: jasmine.createSpyObj<Router>('Router', ['navigate']) },
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

  it('initializes edit form with existing difficulty', () => {
    component.startEdit(recipe);

    expect(component.newRecipe.difficulty).toBe('Medium');
  });

  it('preserves edit difficulty when updating other fields', () => {
    component.startEdit(recipe);
    component.newRecipe.title = 'Updated soup';
    recipeService.update.and.returnValue(of({}));

    component.updateRecipe();

    expect(recipeService.update).toHaveBeenCalledWith(
      'recipe-1',
      jasmine.objectContaining({
        title: 'Updated soup',
        difficulty: 'Medium'
      })
    );
  });

  it('requires difficulty when creating', () => {
    const toastr = TestBed.inject(ToastrService) as jasmine.SpyObj<ToastrService>;
    component.newRecipe = {
      title: 'Soup',
      description: 'Warm',
      preparationTimeMinutes: 20,
      categoryId: 'cat-1',
      difficulty: '' as any,
      imageUrl: ''
    };

    component.createRecipe();

    expect(recipeService.create).not.toHaveBeenCalled();
    expect(toastr.error).toHaveBeenCalledWith('Title, category, and difficulty are required');
  });

  it('hides recipe-management controls for normal users', () => {
    auth.canManageRecipes.and.returnValue(false);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;

    expect(compiled.textContent).not.toContain('Edit Recipe');
  });

  it('shows recipe-management controls for operators', () => {
    auth.canManageRecipes.and.returnValue(true);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;

    expect(compiled.textContent).toContain('Create Recipe');
    expect(compiled.textContent).toContain('Edit');
  });

  it('rolls back favorite toggle after API failure', () => {
    favoriteService.add.and.returnValue(throwError(() => new Error('failed')));

    component.toggleFavorite('recipe-1');

    expect(component.isFavorite('recipe-1')).toBeFalse();
  });
});
