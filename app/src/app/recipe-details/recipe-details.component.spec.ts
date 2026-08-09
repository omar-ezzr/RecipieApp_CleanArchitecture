import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute } from '@angular/router';
import { of } from 'rxjs';
import { RecipeDetailsComponent } from './recipe-details.component';
import { RecipeService } from '../services/recipe.service';
import { ReviewService } from '../services/review.service';
import { DifficultyLevel } from '../models/recipe.model';
import { FavoriteService } from '../services/favorite.service';
import { LikeService } from '../services/like.service';
import { CommentService } from '../services/comment.service';
import { AuthService } from '../services/auth.service';
import { ToastrService } from 'ngx-toastr';
import { provideRouter } from '@angular/router';

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
        provideRouter([]),
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
              cuisineId: 'cuisine-1',
              cuisineName: 'Moroccan',
              cuisineSlug: 'moroccan',
              regionId: 'region-1',
              regionName: 'Souss-Massa',
              regionSlug: 'souss-massa',
              difficulty: DifficultyLevel.Easy,
              author: { id: 'user-1', displayName: 'User' },
              imageUrl: '',
              isTraditional: true,
              ingredients: [],
              steps: [],
              likeCount: 2,
              isLikedByCurrentUser: true
            })
          }
        },
        { provide: ReviewService, useValue: reviewService },
        { provide: FavoriteService, useValue: { check: () => of({ isFavorite: false }) } },
        { provide: LikeService, useValue: { getStatus: () => of({ isLiked: true, likeCount: 2 }), like: () => of(void 0), unlike: () => of(void 0) } },
        { provide: CommentService, useValue: { getByRecipe: () => of({ items: [], total: 0, page: 1, pageSize: 20, totalPages: 0 }) } },
        { provide: AuthService, useValue: { isLoggedIn: () => true, isAdmin: () => false, getCurrentUserId: () => 'user-2' } },
        { provide: ToastrService, useValue: { error: jasmine.createSpy('error'), success: jasmine.createSpy('success') } }
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
