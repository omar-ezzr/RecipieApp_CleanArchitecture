import { RecipeAuthor } from './recipe.model';

export interface RecipeComment {
  id: string;
  recipeId: string;
  content: string;
  createdAt: string;
  updatedAt?: string | null;
  author: RecipeAuthor;
}

export interface CreateRecipeComment {
  content: string;
}

export interface UpdateRecipeComment {
  content: string;
}
