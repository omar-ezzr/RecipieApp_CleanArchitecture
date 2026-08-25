import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
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
        { provide: RecipeService, useValue: recipeService },
        { provide: CategoryService, useValue: { getAll: () => of([{ id: 'cat-1', name: 'Dinner' }]) } },
        { provide: CuisineService, useValue: { getAll: () => of([{ id: 'cuisine-1', name: 'Moroccan', slug: 'moroccan', countryCode: 'MA', isActive: true }]), getRegions: () => of([{ id: 'region-1', name: 'Souss-Massa', slug: 'souss-massa', cuisineId: 'cuisine-1', cuisineName: 'Moroccan', isActive: true }]) } },
        { provide: Router, useValue: jasmine.createSpyObj<Router>('Router', ['navigate']) }
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

  it('shows concise validation errors from the API response', () => {
    recipeService.create.and.returnValue(throwError(() => new HttpErrorResponse({
      status: 400,
      error: {
        message: 'Validation failed.',
        errors: {
          quantity: ['Ingredient quantity is required']
        }
      }
    })));

    component.submit();

    expect(component.error).toBe('Ingredient quantity is required');
    expect(component.isSubmitting).toBeFalse();
  });

  it('does not expose raw server error text in the page', () => {
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
});
