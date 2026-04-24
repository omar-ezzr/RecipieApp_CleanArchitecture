import { Routes } from '@angular/router';
import { RecipesComponent } from './pages/recipes/recipes.component';
import { LoginComponent } from './login/login.component';
import { RegisterComponent } from './pages/register/register.component';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
{
  path: '',
  canActivate: [authGuard],
  children: [
    { path: 'recipes', component: RecipesComponent }
  ]
},
  { path: 'login', component: LoginComponent },
  { path: 'recipes', component: RecipesComponent },
  { path: 'register', component: RegisterComponent },
    {
    path: 'recipes',
    component: RecipesComponent,
    canActivate: [authGuard]   
  },
];
