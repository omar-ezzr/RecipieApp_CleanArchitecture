import { Routes } from '@angular/router';
import { RecipesComponent } from './pages/recipes/recipes.component';
import { LoginComponent } from './login/login.component';
import { RegisterComponent } from './pages/register/register.component';
import { authGuard } from './guards/auth.guard';
import { adminGuard } from './guards/admin.guard';
import { RecipeDetailsComponent } from './recipe-details/recipe-details.component';
import { AccountsComponent } from './pages/admin/accounts/accounts.component';
import { CreateRecipeComponent } from './pages/create-recipe/create-recipe.component';
import { MyRecipesComponent } from './pages/my-recipes/my-recipes.component';
import { FeedComponent } from './pages/feed/feed.component';
import { ProfileComponent } from './pages/profile/profile.component';
import { ProfileEditComponent } from './pages/profile-edit/profile-edit.component';
import { NotificationsComponent } from './pages/notifications/notifications.component';
import { SavedRecipesComponent } from './pages/saved-recipes/saved-recipes.component';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'feed',
    pathMatch: 'full'
  },
  {
    path: 'feed',
    component: FeedComponent,
    canActivate: [authGuard]
  },
  {
    path: 'recipes',
    component: RecipesComponent,
    canActivate: [authGuard]
  },
  {
    path: 'login',
    component: LoginComponent
  },
  {
    path: 'register',
    component: RegisterComponent
  },
  {
    path: 'recipes/:id',
    component: RecipeDetailsComponent,
    canActivate: [authGuard]
  },
  {
    path: 'create-recipe',
    component: CreateRecipeComponent,
    canActivate: [authGuard]
  },
  {
    path: 'my-recipes',
    component: MyRecipesComponent,
    canActivate: [authGuard]
  },
  {
    path: 'saved',
    component: SavedRecipesComponent,
    canActivate: [authGuard]
  },
  {
    path: 'users/:id',
    component: ProfileComponent
  },
  {
    path: 'profile/edit',
    component: ProfileEditComponent,
    canActivate: [authGuard]
  },
  {
    path: 'notifications',
    component: NotificationsComponent,
    canActivate: [authGuard]
  },
  {
    path: 'admin/accounts',
    component: AccountsComponent,
    canActivate: [authGuard, adminGuard]
  },
  {
    path: '**',
    redirectTo: 'recipes'
  }
];
