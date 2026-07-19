import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { map } from 'rxjs/operators';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = () => {
  const router = inject(Router);
  const auth = inject(AuthService);
  const token = auth.getAccessToken();

  if (token && !auth.isTokenExpired(token)) {
    return true;
  }

  return auth.restoreSession().pipe(
    map((restored) => restored ? true : router.createUrlTree(['/login']))
  );
};
