import { ComponentFixture, TestBed } from '@angular/core/testing';
import { activatedRouteStub } from '../testing/route.stub';
import { ActivatedRoute, provideRouter, Router } from '@angular/router';
import { of } from 'rxjs';
import { RecipeDetailsComponent } from './recipe-details.component';
import { RecipeService } from '../services/recipe.service';
import { ReviewService } from '../services/review.service';
import { DifficultyLevel, Recipe, RecipeMediaType } from '../models/recipe.model';
import { FavoriteService } from '../services/favorite.service';
import { LikeService } from '../services/like.service';
import { CommentService } from '../services/comment.service';
import { AuthService } from '../services/auth.service';
import { ToastrService } from '@openng/ngx-toastr';
import { CategoryService } from '../services/category.service';
import { CuisineService } from '../services/cuisine.service';

describe('RecipeDetailsComponent', () => {
  let component: RecipeDetailsComponent;
  let fixture: ComponentFixture<RecipeDetailsComponent>;
  let recipeService: jasmine.SpyObj<RecipeService>;
  let reviewService: jasmine.SpyObj<ReviewService>;
  let auth: jasmine.SpyObj<AuthService>;
  let router: Router;

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
    difficulty: DifficultyLevel.Easy,
    author: { id: 'user-1', displayName: 'User' },
    imageUrl: '/images/recipes/soup.webp',
    traditionalName: 'Harira',
    originDescription: 'A Ramadan soup.',
    servingOccasion: 'Ramadan',
    isTraditional: true,
    ingredients: [
      { name: 'Tomato', quantity: '2 cups' },
      { name: 'Lentils', quantity: '1 cup' }
    ],
    steps: [
      { stepNumber: 1, instruction: 'Simmer the base.' },
      { stepNumber: 2, instruction: 'Add lentils.' }
    ],
    likeCount: 2,
    isLikedByCurrentUser: true
  };

  beforeEach(async () => {
    recipeService = jasmine.createSpyObj<RecipeService>('RecipeService', ['getById', 'update', 'delete', 'addMedia', 'removeMedia', 'setMainMedia', 'reorderMedia']);
    reviewService = jasmine.createSpyObj<ReviewService>('ReviewService', ['getByRecipe', 'create']);
    auth = jasmine.createSpyObj<AuthService>('AuthService', ['isLoggedIn', 'isAdmin', 'getCurrentUserId']);

    reviewService.getByRecipe.and.returnValue(of([]));
    recipeService.getById.and.returnValue(of({ ...recipe, ingredients: [...recipe.ingredients], steps: [...recipe.steps] }));
    recipeService.update.and.returnValue(of({ ...recipe, title: 'Updated soup' }));
    recipeService.addMedia.and.returnValue(of(photo('three', false, 2)));
    recipeService.removeMedia.and.returnValue(of(void 0));
    recipeService.setMainMedia.and.returnValue(of(void 0));
    recipeService.reorderMedia.and.returnValue(of(void 0));
    auth.isLoggedIn.and.returnValue(true);
    auth.isAdmin.and.returnValue(false);
    auth.getCurrentUserId.and.returnValue('user-1');

    await TestBed.configureTestingModule({
      imports: [RecipeDetailsComponent],
      providers: [
        provideRouter([]),
        { provide: ActivatedRoute, useValue: activatedRouteStub },
        { provide: RecipeService, useValue: recipeService },
        { provide: CategoryService, useValue: { getAll: () => of([{ id: 'cat-1', name: 'Dinner' }]) } },
        { provide: CuisineService, useValue: { getAll: () => of([{ id: 'cuisine-1', name: 'Moroccan', slug: 'moroccan', countryCode: 'MA', isActive: true }]), getRegions: () => of([{ id: 'region-1', name: 'Souss-Massa', slug: 'souss-massa', cuisineId: 'cuisine-1', cuisineName: 'Moroccan', isActive: true }]) } },
        { provide: ReviewService, useValue: reviewService },
        { provide: FavoriteService, useValue: { check: () => of({ isFavorite: false }) } },
        { provide: LikeService, useValue: { getStatus: () => of({ isLiked: true, likeCount: 2 }), like: () => of(void 0), unlike: () => of(void 0) } },
        { provide: CommentService, useValue: { getByRecipe: () => of({ items: [], total: 0, page: 1, pageSize: 20, totalPages: 0 }), create: () => of({}), update: () => of({}), delete: () => of(void 0) } },
        { provide: AuthService, useValue: auth },
        { provide: ToastrService, useValue: jasmine.createSpyObj<ToastrService>('ToastrService', ['success', 'error', 'warning', 'info']) }
      ]
    }).compileComponents();

  });

  function createComponent(queryEdit = false): void {
    TestBed.overrideProvider(ActivatedRoute, {
      useValue: {
        snapshot: {
          paramMap: { get: () => 'recipe-1' },
          queryParamMap: { get: (key: string) => queryEdit && key === 'edit' ? 'true' : null }
        }
      }
    });

    router = TestBed.inject(Router);
    spyOn(router, 'navigate').and.resolveTo(true);

    fixture = TestBed.createComponent(RecipeDetailsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }

  it('loads the recipe and reviews', () => {
    createComponent();

    expect(component.recipe?.id).toBe('recipe-1');
    expect(reviewService.getByRecipe).toHaveBeenCalledWith('recipe-1');
  });

  it('selects the main photo first and renders image-only gallery markup', () => {
    recipeService.getById.and.returnValue(of({ ...recipe, media: [photo('two', true, 1), photo('one', false, 0)] }));
    createComponent();

    expect(component.activeMedia?.id).toBe('two');
    expect(fixture.nativeElement.querySelector('video')).toBeNull();
    expect(fixture.nativeElement.querySelector('.hero-image')?.tagName).toBe('IMG');
  });

  it('falls back to the first ordered photo when no cover is set', () => {
    recipeService.getById.and.returnValue(of({ ...recipe, media: [photo('two', false, 1), photo('one', false, 0)] }));
    createComponent();

    expect(component.activeMedia?.id).toBe('one');
  });

  it('rejects MP4 and WebM photos in edit mode', () => {
    createComponent(true);
    component.onEditMediaSelected({ target: { files: [new File([new Uint8Array([1])], 'clip.mp4', { type: 'video/mp4' })], value: 'selected' } } as unknown as Event);
    expect(component.editError).toBe('Choose a JPEG, PNG, or WebP image.');

    component.onEditMediaSelected({ target: { files: [new File([new Uint8Array([1])], 'clip.webm', { type: 'video/webm' })], value: 'selected' } } as unknown as Event);
    expect(component.editError).toBe('Choose a JPEG, PNG, or WebP image.');
    expect(recipeService.addMedia).not.toHaveBeenCalled();
  });

  it('sets cover, reorders, and prevents deleting the final photo', () => {
    const photos = [photo('one', true, 0), photo('two', false, 1)];
    recipeService.getById.and.returnValue(of({ ...recipe, media: photos }));
    createComponent(true);

    component.setMainMedia(component.orderedMedia[1]);
    component.moveMedia(component.orderedMedia[1], -1);
    component.removeMedia(component.orderedMedia[0]);

    expect(recipeService.setMainMedia).toHaveBeenCalledWith('recipe-1', 'two');
    expect(recipeService.reorderMedia).toHaveBeenCalledWith('recipe-1', ['two', 'one']);
    expect(recipeService.removeMedia).toHaveBeenCalledWith('recipe-1', 'one');

    component.recipe = { ...recipe, media: [photo('one', true, 0)] };
    component.removeMedia(component.orderedMedia[0]);
    expect(recipeService.removeMedia).toHaveBeenCalledTimes(1);
  });

  it('validates review rating', () => {
    createComponent();
    component.newReview.rating = 6;

    component.submitReview();

    expect(component.reviewError).toBe('Rating must be between 1 and 5.');
    expect(reviewService.create).not.toHaveBeenCalled();
  });

  it('enters edit mode from edit query for an owner', () => {
    createComponent(true);

    expect(component.isEditMode).toBeTrue();
  });

  it('prefills existing recipe values', () => {
    createComponent(true);

    expect(component.editRecipeModel).toEqual(jasmine.objectContaining({
      title: 'Soup',
      description: 'Warm',
      imageUrl: '/images/recipes/soup.webp',
      categoryId: 'cat-1',
      cuisineId: 'cuisine-1',
      regionId: 'region-1',
      traditionalName: 'Harira',
      originDescription: 'A Ramadan soup.',
      servingOccasion: 'Ramadan',
      isTraditional: true
    }));
  });

  it('prefills existing ingredients', () => {
    createComponent(true);

    expect(component.editRecipeModel?.ingredients).toEqual([
      { name: 'Tomato', quantity: '2 cups' },
      { name: 'Lentils', quantity: '1 cup' }
    ]);
  });

  it('prefills existing steps and instructions', () => {
    createComponent(true);

    expect(component.editRecipeModel?.steps).toEqual([
      { stepNumber: 1, instruction: 'Simmer the base.' },
      { stepNumber: 2, instruction: 'Add lentils.' }
    ]);
  });

  it('adds and removes ingredients while keeping at least one', () => {
    createComponent(true);

    component.addIngredient();
    expect(component.editRecipeModel?.ingredients.length).toBe(3);

    component.removeIngredient(2);
    expect(component.editRecipeModel?.ingredients.length).toBe(2);

    component.removeIngredient(1);
    component.removeIngredient(0);
    expect(component.editRecipeModel?.ingredients.length).toBe(1);
  });

  it('adds and removes steps while keeping sequential numbers', () => {
    createComponent(true);

    component.addStep();
    expect(component.editRecipeModel?.steps.length).toBe(3);

    component.removeStep(0);

    expect(component.editRecipeModel?.steps.map(step => step.stepNumber)).toEqual([1, 2]);
  });

  it('renumbers step numbers sequentially before update', () => {
    createComponent(true);
    component.editRecipeModel!.steps = [
      { stepNumber: 4, instruction: 'First' },
      { stepNumber: 9, instruction: 'Second' }
    ];

    component.saveRecipe();

    expect(recipeService.update).toHaveBeenCalledWith(
      'recipe-1',
      jasmine.objectContaining({
        steps: [
          { stepNumber: 1, instruction: 'First' },
          { stepNumber: 2, instruction: 'Second' }
        ]
      })
    );
  });

  it('prevents API submission for an empty step instruction', () => {
    createComponent(true);
    component.editRecipeModel!.steps[0].instruction = ' ';

    component.saveRecipe();

    expect(recipeService.update).not.toHaveBeenCalled();
    expect(component.editError).toBe('Please add an instruction for every preparation step.');
  });



  it('saves blank ingredient quantity as blank', () => {
    createComponent(true);
    component.editRecipeModel!.ingredients = [{ name: 'Salt', quantity: '' }];

    component.saveRecipe();

    expect(recipeService.update).toHaveBeenCalledWith(
      'recipe-1',
      jasmine.objectContaining({
        ingredients: [{ name: 'Salt', quantity: '' }]
      })
    );
  });

  it('trims whitespace quantity to blank instead of zero', () => {
    createComponent(true);
    component.editRecipeModel!.ingredients = [{ name: 'Salt', quantity: '   ' }];

    component.saveRecipe();

    expect(recipeService.update).toHaveBeenCalledWith(
      'recipe-1',
      jasmine.objectContaining({
        ingredients: [{ name: 'Salt', quantity: '' }]
      })
    );
  });

  it('shows readable category validation before update', () => {
    createComponent(true);
    component.editRecipeModel!.categoryId = '';

    component.saveRecipe();

    expect(recipeService.update).not.toHaveBeenCalled();
    expect(component.editError).toBe('Please select a category.');
  });

  it('update payload contains the complete recipe aggregate', () => {
    createComponent(true);

    component.saveRecipe();

    expect(recipeService.update).toHaveBeenCalledWith('recipe-1', jasmine.objectContaining({
      title: 'Soup',
      description: 'Warm',
      preparationTimeMinutes: 20,
      difficulty: DifficultyLevel.Easy,
      categoryId: 'cat-1',
      cuisineId: 'cuisine-1',
      regionId: 'region-1',
      imageUrl: '/images/recipes/soup.webp',
      traditionalName: 'Harira',
      originDescription: 'A Ramadan soup.',
      isTraditional: true,
      servingOccasion: 'Ramadan',
      ingredients: [
        { name: 'Tomato', quantity: '2 cups' },
        { name: 'Lentils', quantity: '1 cup' }
      ],
      steps: [
        { stepNumber: 1, instruction: 'Simmer the base.' },
        { stepNumber: 2, instruction: 'Add lentils.' }
      ]
    }));
  });

  it('successful update returns to normal details mode', () => {
    createComponent(true);

    component.saveRecipe();

    expect(component.isEditMode).toBeFalse();
    expect(component.recipe?.title).toBe('Updated soup');
    expect(router.navigate).toHaveBeenCalledWith([], jasmine.objectContaining({
      queryParams: { edit: null },
      replaceUrl: true
    }));
  });

  it('cancel discards edits without submitting', () => {
    createComponent(true);
    component.editRecipeModel!.title = 'Draft title';

    component.cancelEdit();

    expect(recipeService.update).not.toHaveBeenCalled();
    expect(component.isEditMode).toBeFalse();
    expect(component.recipe?.title).toBe('Soup');
  });

  it('does not enter edit mode for a non-owner', () => {
    auth.getCurrentUserId.and.returnValue('other-user');

    createComponent(true);

    expect(component.isEditMode).toBeFalse();
    expect(component.editRecipeModel).toBeNull();
  });

  function photo(id: string, isMain: boolean, sortOrder: number) {
    return {
      id,
      url: `/images/recipes/${id}.jpg`,
      mediaType: RecipeMediaType.Image,
      contentType: 'image/jpeg',
      isMain,
      sortOrder
    };
  }
});
