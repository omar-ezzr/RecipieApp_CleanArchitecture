import { Routes } from '@angular/router';

import { RecipesComponent } from './pages/recipes/recipes.component';
import { LoginComponent } from './login/login.component';
import { RegisterComponent } from './pages/register/register.component';

import { authGuard } from './guards/auth.guard';
import { RecipeDetailsComponent } from './recipe-details/recipe-details.component';



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
  },{
  path: 'recipes/:id',
  component: RecipeDetailsComponent,
  canActivate: [authGuard]
},

  {
    path: '**',
    redirectTo: 'recipes'
  }

];