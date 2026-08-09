import { DifficultyLevel, RecipeAuthor } from './recipe.model';

export interface NamedSummary {
  id: string;
  name: string;
}

export interface FeedRecipe {
  id: string;
  title: string;
  description: string;
  imageUrl?: string | null;
  preparationTimeMinutes: number;
  difficulty: DifficultyLevel;
  createdAt: string;
  author: RecipeAuthor;
  cuisine: NamedSummary;
  region?: NamedSummary | null;
  isTraditional: boolean;
  likeCount: number;
  commentCount: number;
  isLikedByCurrentUser: boolean;
  isFavoriteByCurrentUser: boolean;
}
