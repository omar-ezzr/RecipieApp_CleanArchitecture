export enum DifficultyLevel {
  Easy = 1,
  Medium = 2,
  Hard = 3
}

export type Difficulty = 'Easy' | 'Medium' | 'Hard';

export interface RecipeAuthor {
  id: string;
  displayName: string;
}

export interface RecipeIngredientInput {
  name: string;
  quantity: string;
}

export interface RecipeStepInput {
  stepNumber: number;
  instruction: string;
}

export interface Recipe {
  id: string;
  title: string;
  description: string;
  preparationTimeMinutes: number;
  categoryId: string;
  category: string;
  cuisineId: string;
  cuisineName: string;
  cuisineSlug: string;
  regionId?: string | null;
  regionName?: string | null;
  regionSlug?: string | null;
  difficulty: DifficultyLevel;
  author: RecipeAuthor;
  imageUrl?: string;
  traditionalName?: string | null;
  originDescription?: string | null;
  isTraditional: boolean;
  servingOccasion?: string | null;
  ingredients: RecipeIngredientInput[];
  steps: RecipeStepInput[];
}

export interface CreateRecipe {
  title: string;
  description: string;
  preparationTimeMinutes: number;
  categoryId: string;
  cuisineId: string;
  regionId?: string | null;
  difficulty: DifficultyLevel;
  imageUrl?: string | null;
  traditionalName?: string | null;
  originDescription?: string | null;
  isTraditional: boolean;
  servingOccasion?: string | null;
  ingredients: RecipeIngredientInput[];
  steps: RecipeStepInput[];
}
