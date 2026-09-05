import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { map } from 'rxjs/operators';
import { AuthService } from '../services/auth.service';

export const adminGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const token = auth.getAccessToken();

  if (token && !auth.isTokenExpired(token)) {
    return auth.isAdmin() ? true : router.createUrlTree(['/forbidden']);
  }

  return auth.restoreSession().pipe(
    map(restored => {
      if (!restored) {
        return router.createUrlTree(['/login']);
      }

      return auth.isAdmin() ? true : router.createUrlTree(['/forbidden']);
    })
  );
};
