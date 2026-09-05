import { Routes } from '@angular/router';
import { authGuard } from './guards/auth.guard';
import { adminGuard } from './guards/admin.guard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'feed',
    pathMatch: 'full'
  },
  {
    path: 'feed',
    loadComponent: () => import('./pages/feed/feed.component').then(m => m.FeedComponent),
    canActivate: [authGuard]
  },
  {
    path: 'recipes',
    loadComponent: () => import('./pages/recipes/recipes.component').then(m => m.RecipesComponent),
    canActivate: [authGuard]
  },
  {
    path: 'login',
    loadComponent: () => import('./login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'register',
    loadComponent: () => import('./pages/register/register.component').then(m => m.RegisterComponent)
  },
  {
    path: 'recipes/:id',
    loadComponent: () => import('./recipe-details/recipe-details.component').then(m => m.RecipeDetailsComponent),
    canActivate: [authGuard]
  },
  {
    path: 'create-recipe',
    loadComponent: () => import('./pages/create-recipe/create-recipe.component').then(m => m.CreateRecipeComponent),
    canActivate: [authGuard]
  },
  {
    path: 'my-recipes',
    loadComponent: () => import('./pages/my-recipes/my-recipes.component').then(m => m.MyRecipesComponent),
    canActivate: [authGuard]
  },
  {
    path: 'saved',
    loadComponent: () => import('./pages/saved-recipes/saved-recipes.component').then(m => m.SavedRecipesComponent),
    canActivate: [authGuard]
  },
  {
    path: 'users/:id',
    loadComponent: () => import('./pages/profile/profile.component').then(m => m.ProfileComponent)
  },
  {
    path: 'profile/edit',
    loadComponent: () => import('./pages/profile-edit/profile-edit.component').then(m => m.ProfileEditComponent),
    canActivate: [authGuard]
  },
  {
    path: 'notifications',
    loadComponent: () => import('./pages/notifications/notifications.component').then(m => m.NotificationsComponent),
    canActivate: [authGuard]
  },
  {
    path: 'admin/accounts',
    loadComponent: () => import('./pages/admin/accounts/accounts.component').then(m => m.AccountsComponent),
    canActivate: [authGuard, adminGuard]
  },
  { path: 'admin', redirectTo: 'admin/accounts', pathMatch: 'full' },
  { path: 'admin/recipes', loadComponent: () => import('./pages/admin/moderation/moderation.component').then(m => m.ModerationComponent), canActivate: [authGuard, adminGuard], data: { kind: 'recipes' } },
  { path: 'admin/comments', loadComponent: () => import('./pages/admin/moderation/moderation.component').then(m => m.ModerationComponent), canActivate: [authGuard, adminGuard], data: { kind: 'comments' } },
  { path: 'admin/reviews', loadComponent: () => import('./pages/admin/moderation/moderation.component').then(m => m.ModerationComponent), canActivate: [authGuard, adminGuard], data: { kind: 'reviews' } },
  { path: 'admin/categories', loadComponent: () => import('./pages/admin/reference-data/reference-data.component').then(m => m.ReferenceDataComponent), canActivate: [authGuard, adminGuard], data: { kind: 'categories' } },
  { path: 'admin/cuisines', loadComponent: () => import('./pages/admin/reference-data/reference-data.component').then(m => m.ReferenceDataComponent), canActivate: [authGuard, adminGuard], data: { kind: 'cuisines' } },
  { path: 'admin/regions', loadComponent: () => import('./pages/admin/reference-data/reference-data.component').then(m => m.ReferenceDataComponent), canActivate: [authGuard, adminGuard], data: { kind: 'regions' } },
  {
    path: '**',
    redirectTo: 'recipes'
  }
];
