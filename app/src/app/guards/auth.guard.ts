import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';

export const authGuard: CanActivateFn = () => {

  const router = inject(Router);
  const token = localStorage.getItem('accessToken');

  if (!token) {
    router.navigate(['/login']);
    return false;
  }

  //  check expiration
  const payload = JSON.parse(atob(token.split('.')[1]));
  const exp = payload.exp * 1000;

  if (Date.now() > exp) {
    localStorage.clear();
    router.navigate(['/login']);
    return false;
  }

  return true;
};