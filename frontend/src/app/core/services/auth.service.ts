import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, firstValueFrom, from } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  username: string;
  displayName: string;
  role: string;
  expiresAt: string;
}

const TOKEN_KEY = 'tracelens.token';
const USER_KEY = 'tracelens.user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);
  private tokenEndpoint = `${environment.apiUrl.replace('/api', '')}/connect/token`;

  private currentUserSignal = signal<LoginResponse | null>(this.loadStoredUser());

  readonly currentUser = this.currentUserSignal.asReadonly();
  readonly isAuthenticated = computed(() => this.currentUserSignal() !== null);

  login(request: LoginRequest): Observable<LoginResponse> {
    return from(this.doLogin(request));
  }

  private async doLogin(request: LoginRequest): Promise<LoginResponse> {
    const form = new URLSearchParams();
    form.set('grant_type', 'password');
    form.set('username', request.username);
    form.set('password', request.password);
    form.set('client_id', 'tracelens-spa');
    form.set('client_secret', 'tracelens-secret');
    form.set('scope', 'api');

    const res = await firstValueFrom(
      this.http.post<any>(this.tokenEndpoint, form.toString(), {
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' }
      })
    );

    const user: LoginResponse = {
      token: res.access_token,
      username: res.username ?? request.username,
      displayName: res.display_name ?? request.username,
      role: res.role ?? 'Admin',
      expiresAt: new Date(Date.now() + (res.expires_in ?? 3600) * 1000).toISOString()
    };

    localStorage.setItem(TOKEN_KEY, user.token);
    localStorage.setItem(USER_KEY, JSON.stringify(user));
    this.currentUserSignal.set(user);

    return user;
  }

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    this.currentUserSignal.set(null);
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  getUserInitials(): string {
    const user = this.currentUserSignal();
    if (!user) return '?';
    const parts = (user.displayName || user.username).split(' ').filter(Boolean);
    if (parts.length >= 2) return (parts[0][0] + parts[1][0]).toUpperCase();
    return (parts[0]?.[0] ?? '?').toUpperCase();
  }

  private loadStoredUser(): LoginResponse | null {
    const raw = localStorage.getItem(USER_KEY);
    if (!raw) return null;
    try {
      const user = JSON.parse(raw) as LoginResponse;
      if (new Date(user.expiresAt).getTime() < Date.now()) {
        localStorage.removeItem(TOKEN_KEY);
        localStorage.removeItem(USER_KEY);
        return null;
      }
      return user;
    } catch {
      return null;
    }
  }
}
