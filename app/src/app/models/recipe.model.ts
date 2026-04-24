export interface Recipe {
  id: string;
  title: string;
  description: string; // ✅ keep it
  preparationTimeMinutes: number;
  categoryId: string;
  imageUrl?: string;
}

export interface CreateRecipe {
  title: string;
  description: string;
  preparationTimeMinutes: number;
  categoryId: string;
  imageUrl?: string;
}