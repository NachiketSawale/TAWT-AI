import { HttpClient } from '@angular/common/http';
import { effect, inject, Injectable, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { AuthSession, PatLoginRequest, PatLoginResponse } from '../models/auth.model';

const STORAGE_KEY = 'app-auth.session';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);

  private readonly sessionSignal = signal<AuthSession | null>(this.loadSession());

  readonly session = this.sessionSignal.asReadonly();

  constructor() {
    effect((onCleanup) => {
      const session = this.sessionSignal();
      if (!session) {
        return;
      }
      const ms = new Date(session.expiresAtUtc).getTime() - Date.now();
      const timer = setTimeout(() => this.logout(), Math.max(ms, 0));
      onCleanup(() => clearTimeout(timer));
    });
  }

  login(request: PatLoginRequest): Observable<PatLoginResponse> {
    // Ensure API path includes the '/api/v1' prefix used by the backend controllers
    return this.http.post<PatLoginResponse>(`${environment.apiUrl}/api/v1/Auth/login`, request).pipe(
      tap((response) => {
        if (response.success && response.accessToken && response.expiresAtUtc) {
          const session: AuthSession = {
            accessToken: response.accessToken,
            expiresAtUtc: response.expiresAtUtc,
            displayName: response.displayName ?? '',
            emailAddress: response.emailAddress ?? '',
            organization: response.organization ?? ''
          };
          this.setSession(session);
        }
      })
    );
  }

  logout(): void {
    localStorage.removeItem(STORAGE_KEY);
    this.sessionSignal.set(null);
  }

  isAuthenticated(): boolean {
    const session = this.sessionSignal();
    if (!session) {
      return false;
    }
    return new Date(session.expiresAtUtc).getTime() > Date.now();
  }

  private setSession(session: AuthSession): void {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(session));
    this.sessionSignal.set(session);
  }

  private loadSession(): AuthSession | null {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) {
      return null;
    }

    try {
      const session = JSON.parse(raw) as AuthSession;
      if (new Date(session.expiresAtUtc).getTime() <= Date.now()) {
        localStorage.removeItem(STORAGE_KEY);
        return null;
      }
      return session;
    } catch {
      localStorage.removeItem(STORAGE_KEY);
      return null;
    }
  }
}
