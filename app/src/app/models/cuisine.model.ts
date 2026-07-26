export interface Cuisine {
  id: string;
  name: string;
  slug: string;
  description?: string | null;
  countryCode: string;
  imageUrl?: string | null;
  isActive: boolean;
  createdAt?: string;
  regionCount?: number;
  recipeCount?: number;
}

export interface CreateCuisine {
  name: string;
  slug?: string | null;
  description?: string | null;
  countryCode: string;
  imageUrl?: string | null;
  isActive: boolean;
}
