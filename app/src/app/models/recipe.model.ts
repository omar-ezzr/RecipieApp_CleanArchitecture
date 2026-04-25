export interface Recipe {
  id: string;
  title: string;
  description: string;
  preparationTimeMinutes: number;
  categoryId: string;
  category: string;
  difficulty?: string;
  imageUrl?: string;
}

export interface CreateRecipe {
  title: string;
  description: string;
  preparationTimeMinutes: number;
  categoryId: string;
  difficulty?: string;
  imageUrl?: string;
}
