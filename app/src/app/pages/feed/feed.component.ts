
import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { RouterModule } from '@angular/router';
import { ToastrService } from '@openng/ngx-toastr';
import { FeedRecipe } from '../../models/feed.model';
import { DifficultyLevel, Recipe } from '../../models/recipe.model';
import { FavoriteService } from '../../services/favorite.service';
import { FeedService } from '../../services/feed.service';
import { LikeService } from '../../services/like.service';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { RecipeCardComponent } from '../../shared/components/recipe-card/recipe-card.component';
import { RecipeCardSkeletonComponent } from '../../shared/components/recipe-card-skeleton/recipe-card-skeleton.component';

@Component({
    selector: 'app-feed',
    imports: [RouterModule, EmptyStateComponent, RecipeCardComponent, RecipeCardSkeletonComponent],
    templateUrl: './feed.component.html',
    changeDetection: ChangeDetectionStrategy.Eager,
    styleUrl: './feed.component.css'
})
export class FeedComponent implements OnInit {
  recipes: Recipe[] = [];
  isLoading = false;
  currentPage = 1;
  pageSize = 10;
  totalPages = 0;
  totalItems = 0;
  busyLikes = new Set<string>();
  skeletonCards = Array(6).fill(0);

  constructor(
    private feedService: FeedService,
    private likeService: LikeService,
    private favoriteService: FavoriteService,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.loadFeed();
  }

  loadFeed(): void {
    this.isLoading = true;
    this.feedService.getFeed(this.currentPage, this.pageSize).subscribe({
      next: result => {
        this.recipes = result.items.map(item => this.toRecipe(item));
        this.totalItems = result.total;
        this.totalPages = result.totalPages;
        this.currentPage = result.page;
        this.pageSize = result.pageSize;
        this.isLoading = false;
      },
      error: () => {
        this.toastr.error('Failed to load feed');
        this.isLoading = false;
      }
    });
  }

  toggleLike(recipe: Recipe): void {
    if (this.busyLikes.has(recipe.id)) {
      return;
    }

    this.busyLikes.add(recipe.id);
    const wasLiked = !!recipe.isLikedByCurrentUser;
    const previousCount = Math.max(0, recipe.likeCount || 0);
    recipe.isLikedByCurrentUser = !wasLiked;
    recipe.likeCount = Math.max(0, previousCount + (wasLiked ? -1 : 1));

    const request = wasLiked ? this.likeService.unlike(recipe.id) : this.likeService.like(recipe.id);

    request.subscribe({
      next: () => {
        this.busyLikes.delete(recipe.id);
      },
      error: () => {
        recipe.isLikedByCurrentUser = wasLiked;
        recipe.likeCount = previousCount;
        this.toastr.error('Failed to update like');
        this.busyLikes.delete(recipe.id);
      }
    });
  }

  toggleFavorite(recipe: Recipe): void {
    const wasFavorite = !!recipe.isFavoriteByCurrentUser;
    const request = wasFavorite ? this.favoriteService.remove(recipe.id) : this.favoriteService.add(recipe.id);

    request.subscribe({
      next: () => {
        recipe.isFavoriteByCurrentUser = !wasFavorite;
      },
      error: () => this.toastr.error('Failed to update saved recipe')
    });
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages) {
      return;
    }

    this.currentPage = page;
    this.loadFeed();
  }

  trackRecipe(_index: number, recipe: Recipe): string {
    return recipe.id;
  }

  private toRecipe(item: FeedRecipe): Recipe {
    return {
      id: item.id,
      title: item.title,
      description: item.description,
      preparationTimeMinutes: item.preparationTimeMinutes,
      categoryId: '',
      category: 'Community',
      cuisineId: item.cuisine.id,
      cuisineName: item.cuisine.name,
      cuisineSlug: item.cuisine.name.toLowerCase(),
      regionId: item.region?.id ?? null,
      regionName: item.region?.name ?? null,
      regionSlug: item.region?.name?.toLowerCase() ?? null,
      difficulty: item.difficulty || DifficultyLevel.Easy,
      author: item.author,
      imageUrl: item.imageUrl ?? undefined,
      isTraditional: item.isTraditional,
      ingredients: [],
      steps: [],
      likeCount: item.likeCount,
      commentCount: item.commentCount,
      isLikedByCurrentUser: item.isLikedByCurrentUser,
      isFavoriteByCurrentUser: item.isFavoriteByCurrentUser
    };
  }
}
