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

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'recipes',
    pathMatch: 'full'
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
    path: 'admin/accounts',
    component: AccountsComponent,
    canActivate: [authGuard, adminGuard]
  },
  {
    path: '**',
    redirectTo: 'recipes'
  }
];
