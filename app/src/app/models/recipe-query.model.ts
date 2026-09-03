import { Difficulty } from './recipe.model';

export interface RecipeQuery {
  search?: string;
  categoryId?: string;
  cuisineId?: string;
  regionId?: string;
  isTraditional?: boolean;
  difficulty?: Difficulty;
  sortBy?: string;
  page?: number;
  pageSize?: number;
}
