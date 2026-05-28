import { Injectable } from '@angular/core';

import { HttpClient } from '@angular/common/http';

import { Observable } from 'rxjs';


@Injectable({
  providedIn: 'root'
})

export class AuthService {

  private apiUrl = 'http://localhost:5130/api/auth';


  constructor(private http: HttpClient) {}


  login(data: any): Observable<any> {

    return this.http.post(
      `${this.apiUrl}/login`,
      data
    );
  }


  register(data: any): Observable<any> {

    return this.http.post(
      `${this.apiUrl}/register`,
      data
    );
  }


  refreshToken(refreshToken: string): Observable<any> {

    return this.http.post(
      `${this.apiUrl}/refresh`,
      {
        refreshToken: refreshToken
      }
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

    return !!this.getAccessToken();
  }


  logout() {

    localStorage.removeItem(
      'accessToken'
    );

    localStorage.removeItem(
      'refreshToken'
    );
  }


  isAdmin(): boolean {

    const token = this.getAccessToken();

    if (!token) return false;

    try {

      const payload = JSON.parse(
        atob(token.split('.')[1])
      );

      const role =
        payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];

      return role === 'Admin';

    } catch {

      return false;
    }
  }
}