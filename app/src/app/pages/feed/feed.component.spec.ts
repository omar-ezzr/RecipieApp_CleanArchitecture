import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute } from '@angular/router';
import { activatedRouteStub } from '../../testing/route.stub';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { FeedComponent } from './feed.component';
import { FavoriteService } from '../../services/favorite.service';
import { FeedService } from '../../services/feed.service';
import { LikeService } from '../../services/like.service';
import { ToastrService } from '@openng/ngx-toastr';
import { DifficultyLevel } from '../../models/recipe.model';

describe('FeedComponent', () => {
  let fixture: ComponentFixture<FeedComponent>;
  let likeService: jasmine.SpyObj<LikeService>;

  const feedRecipe = {
    id: 'recipe-1',
    title: 'Soup',
    description: 'Warm',
    imageUrl: null,
    preparationTimeMinutes: 20,
    difficulty: DifficultyLevel.Easy,
    createdAt: '',
    author: { id: 'user-2', displayName: 'Cook' },
    cuisine: { id: 'cuisine-1', name: 'Moroccan' },
    region: null,
    isTraditional: true,
    likeCount: 1,
    commentCount: 0,
    isLikedByCurrentUser: false,
    isFavoriteByCurrentUser: false
  };

  beforeEach(async () => {
    likeService = jasmine.createSpyObj<LikeService>('LikeService', ['like', 'unlike']);
    likeService.like.and.returnValue(of(void 0));
    likeService.unlike.and.returnValue(of(void 0));

    await TestBed.configureTestingModule({
      imports: [FeedComponent],
      providers: [
        { provide: ActivatedRoute, useValue: activatedRouteStub },
        provideRouter([]),
        { provide: FeedService, useValue: { getFeed: () => of({ items: [feedRecipe], total: 1, page: 1, pageSize: 10, totalPages: 1 }) } },
        { provide: LikeService, useValue: likeService },
        { provide: FavoriteService, useValue: { add: () => of(void 0), remove: () => of(void 0) } },
        { provide: ToastrService, useValue: { error: jasmine.createSpy('error') } }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(FeedComponent);
  });

  it('loads a following feed with like state', () => {
    fixture.detectChanges();
    expect(fixture.componentInstance.recipes[0].likeCount).toBe(1);
    expect(fixture.componentInstance.recipes[0].isLikedByCurrentUser).toBeFalse();
  });

  it('increments count when liking an unliked recipe', () => {
    fixture.detectChanges();
    const recipe = fixture.componentInstance.recipes[0];

    fixture.componentInstance.toggleLike(recipe);

    expect(likeService.like).toHaveBeenCalledWith('recipe-1');
    expect(recipe.isLikedByCurrentUser).toBeTrue();
    expect(recipe.likeCount).toBe(2);
  });

  it('decrements count without going negative when unliking', () => {
    fixture.detectChanges();
    const recipe = fixture.componentInstance.recipes[0];
    recipe.isLikedByCurrentUser = true;
    recipe.likeCount = 0;

    fixture.componentInstance.toggleLike(recipe);

    expect(likeService.unlike).toHaveBeenCalledWith('recipe-1');
    expect(recipe.isLikedByCurrentUser).toBeFalse();
    expect(recipe.likeCount).toBe(0);
  });

  it('restores previous state when like request fails', () => {
    likeService.like.and.returnValue(throwError(() => new Error('fail')));
    fixture.detectChanges();
    const recipe = fixture.componentInstance.recipes[0];

    fixture.componentInstance.toggleLike(recipe);

    expect(recipe.isLikedByCurrentUser).toBeFalse();
    expect(recipe.likeCount).toBe(1);
  });

  it('ignores repeated clicks while a like request is in flight', () => {
    fixture.detectChanges();
    const recipe = fixture.componentInstance.recipes[0];
    fixture.componentInstance.busyLikes.add(recipe.id);

    fixture.componentInstance.toggleLike(recipe);

    expect(likeService.like).not.toHaveBeenCalled();
  });
});
