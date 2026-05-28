import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

function isTokenExpired(token: string): boolean {
  try {
    const payloadBase64 = token.split('.')[1];

    if (!payloadBase64) {
      return true;
    }

    const payload = JSON.parse(atob(payloadBase64));
    const expiry = payload.exp;

    if (!expiry) {
      return true;
    }

    return Date.now() >= expiry * 1000;
  } catch {
    return true;
  }
}

export const authGuard: CanActivateFn = () => {
  const router = inject(Router);
  const token = localStorage.getItem('accessToken');

  if (!token || isTokenExpired(token)) {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    router.navigate(['/login']);
    return false;
  }

  return true;
};