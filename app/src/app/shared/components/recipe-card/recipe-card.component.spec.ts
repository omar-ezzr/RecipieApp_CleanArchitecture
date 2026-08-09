import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { RecipeCardComponent } from './recipe-card.component';
import { DifficultyLevel, Recipe } from '../../../models/recipe.model';

describe('RecipeCardComponent', () => {
  let fixture: ComponentFixture<RecipeCardComponent>;
  let component: RecipeCardComponent;

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
    difficulty: DifficultyLevel.Easy,
    author: { id: 'user-1', displayName: 'Cook' },
    isTraditional: false,
    ingredients: [],
    steps: [],
    likeCount: 3
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RecipeCardComponent],
      providers: [provideRouter([])]
    }).compileComponents();

    fixture = TestBed.createComponent(RecipeCardComponent);
    component = fixture.componentInstance;
    component.recipe = recipe;
  });

  it('renders like count and liked state', () => {
    component.isLiked = true;
    fixture.detectChanges();

    const button = (fixture.nativeElement as HTMLElement).querySelector('button.btn-icon-text')!;
    expect(button.textContent).toContain('3');
    expect(button.classList).toContain('is-active');
  });

  it('emits like event and stops click navigation side effects', () => {
    spyOn(component.likeToggled, 'emit');
    const event = jasmine.createSpyObj<MouseEvent>('MouseEvent', ['preventDefault', 'stopPropagation']);

    component.onLikeClick(event);

    expect(event.preventDefault).toHaveBeenCalled();
    expect(event.stopPropagation).toHaveBeenCalled();
    expect(component.likeToggled.emit).toHaveBeenCalledWith(recipe);
  });

  it('disables reaction button while processing', () => {
    component.isLikeBusy = true;
    fixture.detectChanges();

    const button = (fixture.nativeElement as HTMLElement).querySelector('button.btn-icon-text') as HTMLButtonElement;
    expect(button.disabled).toBeTrue();
  });

  it('keeps favorite action independent from like action', () => {
    spyOn(component.favoriteToggled, 'emit');
    const event = jasmine.createSpyObj<MouseEvent>('MouseEvent', ['preventDefault', 'stopPropagation']);

    component.onFavoriteClick(event);

    expect(component.favoriteToggled.emit).toHaveBeenCalledWith(recipe);
  });
});
