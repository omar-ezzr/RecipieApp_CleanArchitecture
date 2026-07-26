export interface Region {
  id: string;
  name: string;
  slug: string;
  description?: string | null;
  cuisineId: string;
  cuisineName: string;
  imageUrl?: string | null;
  isActive: boolean;
  createdAt?: string;
  recipeCount?: number;
}

export interface CreateRegion {
  name: string;
  slug?: string | null;
  description?: string | null;
  cuisineId: string;
  imageUrl?: string | null;
  isActive: boolean;
}
