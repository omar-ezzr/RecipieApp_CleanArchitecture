import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private api = 'http://localhost:5130/api/auth';

  constructor(private http: HttpClient, private router: Router) {}

  login(data: any) {
    return this.http.post<any>(`${this.api}/login`, data);
  }

  saveTokens(accessToken: string, refreshToken: string) {
    localStorage.setItem('accessToken', accessToken);
    localStorage.setItem('refreshToken', refreshToken);
  }
  register(data: any) {
  return this.http.post(`${this.api}/register`, data);
}

  logout() {
    localStorage.clear();
    this.router.navigate(['/login']);
  }

  isLoggedIn(): boolean {
    return !!localStorage.getItem('accessToken');
  }
  refresh() {
  const refreshToken = localStorage.getItem('refreshToken');

  return this.http.post<any>('http://localhost:5130/api/auth/refresh', {
    refreshToken
  });
}
getRole(): string | null {
  const token = localStorage.getItem('accessToken');
  if (!token) return null;

  const payload = JSON.parse(atob(token.split('.')[1]));

  return payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];
}
isAdmin(): boolean {
  return this.getRole() === 'Admin';
}

}