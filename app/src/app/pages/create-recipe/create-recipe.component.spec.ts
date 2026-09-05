import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpErrorResponse } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { DifficultyLevel } from '../../models/recipe.model';
import { CategoryService } from '../../services/category.service';
import { CuisineService } from '../../services/cuisine.service';
import { RecipeService } from '../../services/recipe.service';
import { CreateRecipeComponent } from './create-recipe.component';

describe('CreateRecipeComponent', () => {
  let component: CreateRecipeComponent;
  let fixture: ComponentFixture<CreateRecipeComponent>;
  let recipeService: jasmine.SpyObj<RecipeService>;

  beforeEach(async () => {
    recipeService = jasmine.createSpyObj<RecipeService>('RecipeService', ['create']);

    await TestBed.configureTestingModule({
      imports: [CreateRecipeComponent],
      providers: [
        provideRouter([]),
        { provide: RecipeService, useValue: recipeService },
        { provide: CategoryService, useValue: { getAll: () => of([{ id: 'cat-1', name: 'Dinner' }]) } },
        { provide: CuisineService, useValue: { getAll: () => of([{ id: 'cuisine-1', name: 'Moroccan', slug: 'moroccan', countryCode: 'MA', isActive: true }]), getRegions: () => of([{ id: 'region-1', name: 'Souss-Massa', slug: 'souss-massa', cuisineId: 'cuisine-1', cuisineName: 'Moroccan', isActive: true }]) } }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(CreateRecipeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('builds', () => {
    expect(component).toBeTruthy();
  });

  it('adds and removes ingredient rows while keeping one row', () => {
    component.addIngredient();
    expect(component.recipe.ingredients.length).toBe(2);

    component.removeIngredient(1);
    component.removeIngredient(0);

    expect(component.recipe.ingredients.length).toBe(1);
  });

  it('adds and removes steps while keeping numbers sequential', () => {
    component.addStep();
    component.addStep();
    component.removeStep(1);

    expect(component.recipe.steps.map(step => step.stepNumber)).toEqual([1, 2]);
  });

  it('submits the expected payload shape', () => {
    recipeService.create.and.returnValue(of({
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
      difficulty: DifficultyLevel.Easy,
      author: { id: 'user-1', displayName: 'User' },
      isTraditional: true,
      ingredients: [{ name: 'Salt', quantity: '1 tsp' }],
      steps: [{ stepNumber: 1, instruction: 'Cook' }]
    }));
    component.recipe = {
      title: 'Soup',
      description: 'Warm',
      preparationTimeMinutes: 20,
      categoryId: 'cat-1',
      cuisineId: 'cuisine-1',
      regionId: 'region-1',
      difficulty: DifficultyLevel.Easy,
      imageUrl: null,
      isTraditional: true,
      ingredients: [{ name: 'Salt', quantity: '1 tsp' }],
      steps: [{ stepNumber: 1, instruction: 'Cook' }]
    };

    component.submit();

    expect(recipeService.create).toHaveBeenCalledWith(jasmine.objectContaining({
      title: 'Soup',
      difficulty: DifficultyLevel.Easy,
      ingredients: [{ name: 'Salt', quantity: '1 tsp' }],
      steps: [{ stepNumber: 1, instruction: 'Cook' }]
    }));
  });

  it('prefers readable API validation errors over technical dto errors', () => {
    setValidRecipe();
    recipeService.create.and.returnValue(throwError(() => new HttpErrorResponse({
      status: 400,
      error: {
        message: 'Validation failed.',
        errors: {
          dto: ['The dto field is required.'],
          categoryId: ['Please select a category.']
        }
      }
    })));

    component.submit();

    expect(component.error).toBe('Please select a category.');
    expect(component.error).not.toContain('dto');
    expect(component.isSubmitting).toBeFalse();
  });

  it('uses a generic message instead of technical dto validation text', () => {
    setValidRecipe();
    recipeService.create.and.returnValue(throwError(() => new HttpErrorResponse({
      status: 400,
      error: {
        message: 'Validation failed.',
        errors: {
          dto: ['The dto field is required.']
        }
      }
    })));

    component.submit();

    expect(component.error).toBe('Please check the recipe information and try again.');
    expect(component.error).not.toContain('dto');
  });


  it('does not call the API when category is empty', () => {
    setValidRecipe();
    component.recipe.categoryId = '';

    component.submit();

    expect(recipeService.create).not.toHaveBeenCalled();
    expect(component.error).toBe('Please select a category.');
  });

  it('does not call the API when cuisine is empty', () => {
    setValidRecipe();
    component.recipe.cuisineId = '';

    component.submit();

    expect(recipeService.create).not.toHaveBeenCalled();
    expect(component.error).toBe('Please select a cuisine.');
  });

  it('allows empty ingredient quantity', () => {
    setValidRecipe();
    component.recipe.ingredients = [{ name: 'Salt', quantity: '' }];
    recipeService.create.and.returnValue(of({
      id: 'recipe-1',
      title: 'Soup',
      description: 'Warm',
      preparationTimeMinutes: 20,
      categoryId: 'cat-1',
      category: 'Dinner',
      cuisineId: 'cuisine-1',
      cuisineName: 'Moroccan',
      cuisineSlug: 'moroccan',
      difficulty: DifficultyLevel.Easy,
      author: { id: 'user-1', displayName: 'User' },
      isTraditional: true,
      ingredients: [{ name: 'Salt', quantity: '' }],
      steps: [{ stepNumber: 1, instruction: 'Cook' }]
    }));

    component.submit();

    expect(recipeService.create).toHaveBeenCalledWith(jasmine.objectContaining({
      ingredients: [{ name: 'Salt', quantity: '' }]
    }));
  });

  it('rejects empty ingredient names before calling the API', () => {
    setValidRecipe();
    component.recipe.ingredients = [{ name: '', quantity: '' }];

    component.submit();

    expect(recipeService.create).not.toHaveBeenCalled();
    expect(component.error).toBe('Please enter a name for every ingredient.');
  });

  it('rejects empty preparation step instructions before calling the API', () => {
    setValidRecipe();
    component.recipe.steps = [{ stepNumber: 1, instruction: ' ' }];

    component.submit();

    expect(recipeService.create).not.toHaveBeenCalled();
    expect(component.error).toBe('Please add an instruction for every preparation step.');
  });

  it('does not expose raw server error text in the page', () => {
    setValidRecipe();
    recipeService.create.and.returnValue(throwError(() => new HttpErrorResponse({
      status: 500,
      error: 'System.ArgumentException: Authorization: Bearer secret.jwt.token'
    })));

    component.submit();

    expect(component.error).toBe('Something went wrong while publishing the recipe. Please try again.');
    expect(component.error).not.toContain('System.ArgumentException');
    expect(component.error).not.toContain('Bearer');
    expect(component.isSubmitting).toBeFalse();
  });

  function setValidRecipe(): void {
    component.recipe = {
      title: 'Soup',
      description: 'Warm',
      preparationTimeMinutes: 20,
      categoryId: 'cat-1',
      cuisineId: 'cuisine-1',
      regionId: null,
      difficulty: DifficultyLevel.Easy,
      imageUrl: null,
      isTraditional: false,
      ingredients: [{ name: 'Salt', quantity: '1 tsp' }],
      steps: [{ stepNumber: 1, instruction: 'Cook' }]
    };
  }

});
