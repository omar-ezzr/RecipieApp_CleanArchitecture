import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute } from '@angular/router';
import { of } from 'rxjs';
import { RecipeDetailsComponent } from './recipe-details.component';
import { RecipeService } from '../services/recipe.service';
import { ReviewService } from '../services/review.service';

describe('RecipeDetailsComponent', () => {
  let component: RecipeDetailsComponent;
  let fixture: ComponentFixture<RecipeDetailsComponent>;
  let reviewService: jasmine.SpyObj<ReviewService>;

  beforeEach(async () => {
    reviewService = jasmine.createSpyObj<ReviewService>('ReviewService', ['getByRecipe', 'create']);
    reviewService.getByRecipe.and.returnValue(of([]));

    await TestBed.configureTestingModule({
      imports: [RecipeDetailsComponent],
      providers: [
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { paramMap: { get: () => 'recipe-1' } } }
        },
        {
          provide: RecipeService,
          useValue: {
            getById: () => of({
              id: 'recipe-1',
              title: 'Soup',
              description: 'Warm',
              preparationTimeMinutes: 20,
              categoryId: 'cat-1',
              category: 'Dinner',
              difficulty: 'Easy',
              imageUrl: '',
              ingredients: [],
              steps: []
            })
          }
        },
        { provide: ReviewService, useValue: reviewService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(RecipeDetailsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('loads the recipe and reviews', () => {
    expect(component.recipe?.id).toBe('recipe-1');
    expect(reviewService.getByRecipe).toHaveBeenCalledWith('recipe-1');
  });

  it('validates review rating', () => {
    component.newReview.rating = 6;

    component.submitReview();

    expect(component.reviewError).toBe('Rating must be between 1 and 5.');
    expect(reviewService.create).not.toHaveBeenCalled();
  });
});
