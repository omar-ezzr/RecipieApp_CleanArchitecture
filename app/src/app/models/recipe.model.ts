export type Difficulty = 'Easy' | 'Medium' | 'Hard';

export interface Recipe {
  id: string;
  title: string;
  description: string;
  preparationTimeMinutes: number;
  categoryId: string;
  category: string;
  difficulty: Difficulty;
  imageUrl?: string;
  ingredients: string[];

steps: string[];
}

export interface CreateRecipe {
  title: string;
  description: string;
  preparationTimeMinutes: number;
  categoryId: string;
  difficulty: Difficulty;
  imageUrl?: string;
}
