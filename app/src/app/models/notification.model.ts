import { RecipeAuthor } from './recipe.model';

export type NotificationType = 'Follow' | 'RecipeLike' | 'RecipeComment';

export interface SocialNotification {
  id: string;
  type: NotificationType;
  actor: RecipeAuthor;
  recipeId?: string | null;
  recipeTitle?: string | null;
  commentId?: string | null;
  isRead: boolean;
  createdAt: string;
}

export interface UnreadNotificationCount {
  count: number;
}
