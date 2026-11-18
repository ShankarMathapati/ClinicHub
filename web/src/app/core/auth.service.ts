import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { LoginResponse } from './models';

const STORAGE_KEY = 'clinichub.session';

function readStoredSession(): LoginResponse | null {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) return null;
    const session = JSON.parse(raw) as LoginResponse;
    return new Date(session.expiresAtUtc) > new Date() ? session : null;
  } catch {
    return null;
  }
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly session = signal<LoginResponse | null>(readStoredSession());

  readonly user = this.session.asReadonly();
  readonly isLoggedIn = computed(() => this.session() !== null);
  readonly isAdmin = computed(() => this.session()?.role === 'Admin');
  readonly token = computed(() => this.session()?.accessToken ?? null);

  login(email: string, password: string): Observable<LoginResponse> {
    return this.http.post<LoginResponse>('/api/auth/login', { email, password }).pipe(
      tap((session) => {
        this.session.set(session);
        try {
          localStorage.setItem(STORAGE_KEY, JSON.stringify(session));
        } catch {
          /* storage unavailable: session lives for this tab only */
        }
      }),
    );
  }

  logout(): void {
    this.session.set(null);
    try {
      localStorage.removeItem(STORAGE_KEY);
    } catch {
      /* ignore */
    }
    void this.router.navigate(['/login']);
  }
}
