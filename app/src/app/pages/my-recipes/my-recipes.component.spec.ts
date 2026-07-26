import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { DifficultyLevel } from '../../models/recipe.model';
import { RecipeService } from '../../services/recipe.service';
import { MyRecipesComponent } from './my-recipes.component';

describe('MyRecipesComponent', () => {
  let component: MyRecipesComponent;
  let fixture: ComponentFixture<MyRecipesComponent>;
  let recipeService: jasmine.SpyObj<RecipeService>;

  beforeEach(async () => {
    recipeService = jasmine.createSpyObj<RecipeService>('RecipeService', ['getMine', 'delete']);
    recipeService.getMine.and.returnValue(of({
      items: [{
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
        isTraditional: false,
        ingredients: [],
        steps: []
      }],
      total: 1,
      page: 1,
      pageSize: 10,
      totalPages: 1
    }));

    await TestBed.configureTestingModule({
      imports: [MyRecipesComponent],
      providers: [
        { provide: RecipeService, useValue: recipeService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(MyRecipesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('loads current user recipes', () => {
    expect(recipeService.getMine).toHaveBeenCalledWith({ page: 1, pageSize: 10 });
    expect(component.recipes.length).toBe(1);
  });
});
