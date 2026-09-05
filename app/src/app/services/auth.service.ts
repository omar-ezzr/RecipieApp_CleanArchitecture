import { Injectable } from '@angular/core';

import { HttpClient } from '@angular/common/http';

import { Observable, of, throwError } from 'rxjs';
import { catchError, finalize, map, shareReplay, tap } from 'rxjs/operators';
import { API_BASE_URL } from '../app-api.config';

export interface TokenResponse {
  accessToken: string;
  refreshToken: string;
}

export interface RegisterResponse {
  message: string;
}

const roleClaim = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';
const nameIdentifierClaim = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier';
const nameClaim = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name';


@Injectable({
  providedIn: 'root'
})

export class AuthService {

  private apiUrl = `${API_BASE_URL}/auth`;
  private activeRefresh$?: Observable<TokenResponse>;


  constructor(private http: HttpClient) {}


  login(data: any): Observable<TokenResponse> {

    return this.http.post<TokenResponse>(
      `${this.apiUrl}/login`,
      data
    );
  }


  register(data: any): Observable<any> {

    return this.http.post<TokenResponse>(
      `${this.apiUrl}/register`,
      data
    );
  }


  refreshToken(refreshToken: string): Observable<TokenResponse> {

    return this.http.post<TokenResponse>(
      `${this.apiUrl}/refresh`,
      {
        refreshToken: refreshToken
      }
    );
  }

  refreshSession(): Observable<TokenResponse> {
    const refreshToken = this.getRefreshToken();

    if (!refreshToken) {
      this.logout();
      return throwError(() => new Error('Refresh token is missing'));
    }

    if (!this.activeRefresh$) {
      this.activeRefresh$ = this.refreshToken(refreshToken).pipe(
        tap((tokens) => this.saveTokens(tokens.accessToken, tokens.refreshToken)),
        finalize(() => {
          this.activeRefresh$ = undefined;
        }),
        shareReplay({ bufferSize: 1, refCount: false })
      );
    }

    return this.activeRefresh$;
  }

  restoreSession(): Observable<boolean> {
    const accessToken = this.getAccessToken();

    if (accessToken && !this.isTokenExpired(accessToken)) {
      return of(true);
    }

    if (!this.getRefreshToken()) {
      this.logout();
      return of(false);
    }

    return this.refreshSession().pipe(
      map(() => true),
      catchError(() => {
        this.logout();
        return of(false);
      })
    );
  }


  saveTokens(
    accessToken: string,
    refreshToken: string
  ) {

    localStorage.setItem(
      'accessToken',
      accessToken
    );

    localStorage.setItem(
      'refreshToken',
      refreshToken
    );
  }


  getAccessToken(): string | null {

    return localStorage.getItem(
      'accessToken'
    );
  }


  getRefreshToken(): string | null {

    return localStorage.getItem(
      'refreshToken'
    );
  }


  isLoggedIn(): boolean {

    const token = this.getAccessToken();

    return !!token && !this.isTokenExpired(token);
  }


  logout(): void {    const refreshToken = this.getRefreshToken();    localStorage.removeItem('accessToken');    localStorage.removeItem('refreshToken');    if (refreshToken) {      this.http.post<void>(`${this.apiUrl}/logout`, { refreshToken }).pipe(catchError(() => of(void 0))).subscribe();    }  }
  isTokenExpired(token: string): boolean {
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


  isAdmin(): boolean {
    return this.hasRole('Admin');
  }

  isOperator(): boolean {
    return this.hasRole('Operator');
  }

  canManageRecipes(): boolean {
    return this.isLoggedIn();
  }

  hasRole(role: string): boolean {
    return this.getCurrentRole() === role;
  }

  getCurrentRole(): string | null {
    const payload = this.getTokenPayload();

    return payload?.[roleClaim] ?? payload?.role ?? null;
  }

  getCurrentUserId(): string | null {
    const payload = this.getTokenPayload();

    return payload?.[nameIdentifierClaim] ?? payload?.sub ?? payload?.nameid ?? null;
  }

  getCurrentDisplayName(): string | null {
    const payload = this.getTokenPayload();
    const candidate =
      payload?.displayName ??
      payload?.display_name ??
      payload?.given_name ??
      payload?.preferred_username ??
      payload?.name;

    return typeof candidate === 'string' && candidate.trim() && !candidate.includes('@')
      ? candidate.trim()
      : null;
  }

  getCurrentEmail(): string | null {
    const payload = this.getTokenPayload();
    const candidate = payload?.[nameClaim] ?? payload?.email ?? payload?.name;

    return typeof candidate === 'string' && candidate.includes('@') ? candidate : null;
  }

  private getTokenPayload(): any | null {
    const token = this.getAccessToken();

    if (!token) {
      return null;
    }

    try {
      return JSON.parse(atob(token.split('.')[1]));
    } catch {
      return null;
    }
  }
}
