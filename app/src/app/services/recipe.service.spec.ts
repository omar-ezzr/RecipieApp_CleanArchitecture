import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { CreateRecipe, DifficultyLevel } from '../models/recipe.model';
import { RecipeService } from './recipe.service';

describe('RecipeService', () => {
  let service: RecipeService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        RecipeService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });

    service = TestBed.inject(RecipeService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    http.verify();
  });

  it('posts create recipe requests to the recipe endpoint', () => {
    const request = newRecipe();

    service.create(request).subscribe();

    const req = http.expectOne('/api/recipes');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(request);
    req.flush({ id: 'recipe-1' });
  });

  it('loads my recipes from the user-specific endpoint', () => {
    service.getMine({ page: 2, pageSize: 5 }).subscribe();

    const req = http.expectOne('/api/recipes/me?page=2&pageSize=5');
    expect(req.request.method).toBe('GET');
    req.flush({ items: [], total: 0, page: 2, pageSize: 5, totalPages: 0 });
  });

  it('sends cultural filter query parameters', () => {
    service.getPaged({
      page: 1,
      pageSize: 10,
      cuisineId: 'cuisine-1',
      regionId: 'region-1',
      isTraditional: true
    }).subscribe();

    const req = http.expectOne('/api/recipes/paged?cuisineId=cuisine-1&regionId=region-1&isTraditional=true&page=1&pageSize=10');
    expect(req.request.method).toBe('GET');
    req.flush({ items: [], total: 0, page: 1, pageSize: 10, totalPages: 0 });
  });

  it('uses correct update and delete endpoints', () => {
    const request = newRecipe();

    service.update('recipe-1', request).subscribe();
    http.expectOne('/api/recipes/recipe-1').flush({ id: 'recipe-1' });

    service.delete('recipe-1').subscribe();
    const deleteReq = http.expectOne('/api/recipes/recipe-1');
    expect(deleteReq.request.method).toBe('DELETE');
    deleteReq.flush(null);
  });

  function newRecipe(): CreateRecipe {
    return {
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
