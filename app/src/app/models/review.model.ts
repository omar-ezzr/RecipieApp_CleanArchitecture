import { RecipeAuthor } from './recipe.model';

export interface Review {
  id: string;
  recipeId: string;
  userId: string;
  author: RecipeAuthor;
  rating: number;
  comment: string;
  createdAt: string;
  updatedAt?: string | null;
}

export interface CreateReview {
  recipeId: string;
  rating: number;
  comment: string;
}

export interface UpdateReview {
  rating: number;
  comment: string;
}
