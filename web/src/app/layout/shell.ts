import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../core/auth.service';

@Component({
  selector: 'app-shell',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <header class="topbar">
      <div class="brand">
        <strong>ClinicHub</strong>
        <span class="tenant">{{ auth.user()?.tenantName }}</span>
      </div>
      <nav>
        <a routerLink="/appointments" routerLinkActive="active">Appointments</a>
        <a routerLink="/patients" routerLinkActive="active">Patients</a>
        <a routerLink="/doctors" routerLinkActive="active">Doctors</a>
      </nav>
      <div class="user">
        <span class="muted">{{ auth.user()?.email }} · {{ auth.user()?.role }}</span>
        <button type="button" (click)="auth.logout()">Sign out</button>
      </div>
    </header>
    <main class="page">
      <router-outlet />
    </main>
  `,
  styles: `
    .topbar { display: flex; flex-wrap: wrap; gap: 12px 24px; align-items: center; justify-content: space-between;
      padding: 12px 16px; background: var(--surface); border-bottom: 1px solid var(--border); }
    .brand { display: flex; gap: 10px; align-items: baseline; }
    .brand strong { color: var(--accent); font-size: 1.15rem; }
    .tenant { padding: 2px 8px; border-radius: 999px; background: var(--accent-soft); font-size: .85rem; }
    nav { display: flex; gap: 4px; }
    nav a { padding: 6px 12px; border-radius: 6px; color: var(--text); text-decoration: none; }
    nav a.active { background: var(--accent-soft); color: var(--accent); font-weight: 600; }
    .user { display: flex; gap: 12px; align-items: center; }
    .page { max-width: 1100px; margin: 0 auto; padding: 24px 16px; }
  `,
})
export class Shell {
  protected readonly auth = inject(AuthService);
}
