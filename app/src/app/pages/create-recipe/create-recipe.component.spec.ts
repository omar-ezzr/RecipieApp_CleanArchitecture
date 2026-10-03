import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpErrorResponse } from '@angular/common/http';
import { Router, provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { DifficultyLevel, Recipe, RecipeMedia, RecipeMediaType } from '../../models/recipe.model';
import { CategoryService } from '../../services/category.service';
import { CuisineService } from '../../services/cuisine.service';
import { RecipeService } from '../../services/recipe.service';
import { CreateRecipeComponent } from './create-recipe.component';

describe('CreateRecipeComponent', () => {
  let component: CreateRecipeComponent;
  let fixture: ComponentFixture<CreateRecipeComponent>;
  let recipeService: jasmine.SpyObj<RecipeService>;
  let router: Router;
  let objectUrlCounter = 0;
  let createObjectUrlSpy: jasmine.Spy;
  let revokeObjectUrlSpy: jasmine.Spy;
  let randomUuidSpy: jasmine.Spy;

  beforeEach(async () => {
    objectUrlCounter = 0;
    createObjectUrlSpy = spyOn(URL, 'createObjectURL').and.callFake(() => `blob:test-${++objectUrlCounter}`);
    revokeObjectUrlSpy = spyOn(URL, 'revokeObjectURL').and.stub();
    randomUuidSpy = spyOn(crypto, 'randomUUID').and.callFake(() => `local-${++objectUrlCounter}` as `${string}-${string}-${string}-${string}-${string}`);
    recipeService = jasmine.createSpyObj<RecipeService>('RecipeService', [
      'create',
      'addMedia',
      'setMainMedia',
      'reorderMedia'
    ]);
    recipeService.create.and.returnValue(of(recipeResponse()));
    recipeService.addMedia.and.callFake((_id: string, file: File) => of(mediaResponse(file.name)));
    recipeService.setMainMedia.and.returnValue(of(void 0));
    recipeService.reorderMedia.and.returnValue(of(void 0));

    await TestBed.configureTestingModule({
      imports: [CreateRecipeComponent],
      providers: [
        provideRouter([]),
        { provide: RecipeService, useValue: recipeService },
        { provide: CategoryService, useValue: { getAll: () => of([{ id: 'cat-1', name: 'Dinner' }]) } },
        { provide: CuisineService, useValue: { getAll: () => of([{ id: 'cuisine-1', name: 'Moroccan', slug: 'moroccan', countryCode: 'MA', isActive: true }]), getRegions: () => of([{ id: 'region-1', name: 'Souss-Massa', slug: 'souss-massa', cuisineId: 'cuisine-1', cuisineName: 'Moroccan', isActive: true }]) } }
      ]
    }).compileComponents();

    router = TestBed.inject(Router);
    spyOn(router, 'navigate').and.returnValue(Promise.resolve(true));
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

  it('cannot submit with zero photos', () => {
    setValidRecipe();

    component.submit();

    expect(recipeService.create).not.toHaveBeenCalled();
    expect(component.error).toBe('Add at least one photo.');
  });

  it('submits the expected payload shape with a single image', () => {
    setValidRecipe();
    selectMedia([imageFile('cover.jpg')]);

    component.submit();

    expect(recipeService.create).toHaveBeenCalledOnceWith(jasmine.objectContaining({
      title: 'Soup',
      difficulty: DifficultyLevel.Easy,
      ingredients: [{ name: 'Salt', quantity: '1 tsp' }],
      steps: [{ stepNumber: 1, instruction: 'Cook' }]
    }));
    expect(recipeService.addMedia).toHaveBeenCalledOnceWith('recipe-1', jasmine.any(File));
    expect(recipeService.reorderMedia).toHaveBeenCalledOnceWith('recipe-1', ['media-cover-jpg']);
    expect(router.navigate).toHaveBeenCalledOnceWith(['/recipes', 'recipe-1']);
  });

  it('uploads multiple photos in selection order and creates only once', () => {
    setValidRecipe();
    selectMedia([imageFile('first.jpg'), imageFile('second.png'), imageFile('third.webp', 'image/webp')]);

    component.submit();

    expect(recipeService.create).toHaveBeenCalledTimes(1);
    expect(recipeService.addMedia.calls.allArgs().map(args => args[1].name)).toEqual(['first.jpg', 'second.png', 'third.webp']);
    expect(recipeService.reorderMedia).toHaveBeenCalledOnceWith('recipe-1', ['media-first-jpg', 'media-second-png', 'media-third-webp']);
  });

  it('rejects MP4 and WebM files', () => {
    selectMedia([file('clip.mp4', 'video/mp4'), file('clip.webm', 'video/webm')]);

    expect(component.selectedMedia).toEqual([]);
    expect(component.error).toBe('Choose a JPEG, PNG, or WebP image.');
  });

  it('preserves the chosen cover after uploads', () => {
    setValidRecipe();
    selectMedia([imageFile('first.jpg'), imageFile('second.jpg')]);
    component.setSelectedCover(component.selectedMedia[1].localId);
    recipeService.addMedia.and.callFake((_id: string, file: File) => of(mediaResponse(file.name, false)));

    component.submit();

    expect(recipeService.setMainMedia).toHaveBeenCalledOnceWith('recipe-1', 'media-second-jpg');
    expect(recipeService.reorderMedia).toHaveBeenCalledOnceWith('recipe-1', ['media-first-jpg', 'media-second-jpg']);
  });

  it('falls back to the first successful upload when the selected cover fails', () => {
    setValidRecipe();
    selectMedia([imageFile('cover.jpg'), imageFile('fallback.jpg')]);
    component.setSelectedCover(component.selectedMedia[0].localId);
    recipeService.addMedia.and.callFake((_id: string, file: File) => file.name === 'cover.jpg'
      ? throwError(() => new Error('upload failed'))
      : of(mediaResponse(file.name, true)));

    component.submit();

    expect(recipeService.setMainMedia).not.toHaveBeenCalled();
    expect(recipeService.reorderMedia).toHaveBeenCalledOnceWith('recipe-1', ['media-fallback-jpg']);
    expect(router.navigate).toHaveBeenCalledOnceWith(['/recipes', 'recipe-1'], { queryParams: { edit: true } });
    expect(component.error).toBe('Recipe created, but 1 photo failed to upload. Continue in edit mode to retry.');
  });

  it('navigates to edit mode after a partial upload failure', () => {
    setValidRecipe();
    selectMedia([imageFile('first.jpg'), imageFile('second.jpg')]);
    recipeService.addMedia.and.callFake((_id: string, file: File) => file.name === 'second.jpg'
      ? throwError(() => new Error('upload failed'))
      : of(mediaResponse(file.name, true)));

    component.submit();

    expect(recipeService.create).toHaveBeenCalledTimes(1);
    expect(recipeService.reorderMedia).toHaveBeenCalledOnceWith('recipe-1', ['media-first-jpg']);
    expect(router.navigate).toHaveBeenCalledOnceWith(['/recipes', 'recipe-1'], { queryParams: { edit: true } });
    expect(component.isSubmitting).toBeFalse();
  });

  it('rejects unsupported MIME types', () => {
    selectMedia([file('notes.txt', 'text/plain')]);

    expect(component.selectedMedia).toEqual([]);
    expect(component.error).toBe('Choose a JPEG, PNG, or WebP image.');
    expect(createObjectUrlSpy).not.toHaveBeenCalled();
  });

  it('rejects oversized images', () => {
    selectMedia([file('large.jpg', 'image/jpeg', 5 * 1024 * 1024 + 1)]);

    expect(component.selectedMedia).toEqual([]);
    expect(component.error).toBe('Images must be 5 MB or smaller.');
  });

  it('rejects MP4 regardless of size', () => {
    selectMedia([file('large.mp4', 'video/mp4', 1)]);

    expect(component.selectedMedia).toEqual([]);
    expect(component.error).toBe('Choose a JPEG, PNG, or WebP image.');
  });

  it('limits selections to 9 photos', () => {
    selectMedia(Array.from({ length: 10 }, (_, index) => imageFile(`image-${index}.jpg`)));

    expect(component.selectedMedia.length).toBe(9);
    expect(component.error).toBe('You can add only 9 more photos (maximum 9).');
  });

  it('revokes object URLs when media is removed and when the component is destroyed', () => {
    selectMedia([imageFile('first.jpg'), imageFile('second.jpg')]);
    const removedUrl = component.selectedMedia[0].previewUrl;
    const remainingUrl = component.selectedMedia[1].previewUrl;

    component.removeSelectedMedia(component.selectedMedia[0].localId);
    component.ngOnDestroy();

    expect(revokeObjectUrlSpy).toHaveBeenCalledWith(removedUrl);
    expect(revokeObjectUrlSpy).toHaveBeenCalledWith(remainingUrl);
  });

  it('allows empty ingredient quantity', () => {
    setValidRecipe();
    component.recipe.ingredients = [{ name: 'Salt', quantity: '' }];
    selectMedia([imageFile('cover.jpg')]);

    component.submit();

    expect(recipeService.create).toHaveBeenCalledWith(jasmine.objectContaining({
      ingredients: [{ name: 'Salt', quantity: '' }]
    }));
  });

  it('rejects empty ingredient names before calling the API', () => {
    setValidRecipe();
    component.recipe.ingredients = [{ name: '', quantity: '' }];
    selectMedia([imageFile('cover.jpg')]);

    component.submit();

    expect(recipeService.create).not.toHaveBeenCalled();
    expect(component.error).toBe('Please enter a name for every ingredient.');
  });

  it('rejects empty preparation step instructions before calling the API', () => {
    setValidRecipe();
    component.recipe.steps = [{ stepNumber: 1, instruction: ' ' }];
    selectMedia([imageFile('cover.jpg')]);

    component.submit();

    expect(recipeService.create).not.toHaveBeenCalled();
    expect(component.error).toBe('Please add an instruction for every preparation step.');
  });

  it('prefers readable API validation errors over technical dto errors', () => {
    setValidRecipe();
    selectMedia([imageFile('cover.jpg')]);
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

  it('hides technical DTO validation errors from users', () => {
    setValidRecipe();
    selectMedia([imageFile('cover.jpg')]);
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

  it('does not expose raw server error text in the page', () => {
    setValidRecipe();
    selectMedia([imageFile('cover.jpg')]);
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

  it('does not call the API when category is empty', () => {
    setValidRecipe();
    component.recipe.categoryId = '';
    selectMedia([imageFile('cover.jpg')]);

    component.submit();

    expect(recipeService.create).not.toHaveBeenCalled();
    expect(component.error).toBe('Please select a category.');
  });

  it('does not call the API when cuisine is empty', () => {
    setValidRecipe();
    component.recipe.cuisineId = '';
    selectMedia([imageFile('cover.jpg')]);

    component.submit();

    expect(recipeService.create).not.toHaveBeenCalled();
    expect(component.error).toBe('Please select a cuisine.');
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

  function selectMedia(files: File[]): void {
    component.onMediaSelected({
      target: {
        files,
        value: 'selected'
      }
    } as unknown as Event);
  }

  function file(name: string, type: string, size = 16): File {
    return new File([new Uint8Array(size)], name, { type });
  }

  function imageFile(name: string, type = 'image/jpeg'): File {
    return file(name, type);
  }

  function recipeResponse(id = 'recipe-1'): Recipe {
    return {
      id,
      title: 'Soup',
      description: 'Warm',
      preparationTimeMinutes: 20,
      categoryId: 'cat-1',
      category: 'Dinner',
      cuisineId: 'cuisine-1',
      cuisineName: 'Moroccan',
      cuisineSlug: 'moroccan',
      regionId: null,
      difficulty: DifficultyLevel.Easy,
      author: { id: 'user-1', displayName: 'User' },
      isTraditional: false,
      ingredients: [{ name: 'Salt', quantity: '1 tsp' }],
      steps: [{ stepNumber: 1, instruction: 'Cook' }],
      media: []
    };
  }

  function mediaResponse(fileName: string, isMain = fileName.includes('cover') || fileName.includes('first')): RecipeMedia {
    const normalized = fileName.replace(/[^a-z0-9]+/gi, '-').replace(/-$/, '').toLowerCase();
    return {
      id: `media-${normalized}`,
      url: `/images/recipes/${fileName}`,
      mediaType: RecipeMediaType.Image,
      contentType: fileName.endsWith('.png') ? 'image/png' : fileName.endsWith('.webp') ? 'image/webp' : 'image/jpeg',
      isMain,
      sortOrder: 0
    };
  }
});
