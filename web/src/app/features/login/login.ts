import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth.service';
import { problemMessage } from '../../core/problem';

const DEMO_PASSWORD = 'Demo@12345';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <main class="login">
      <section class="card login-card">
        <h1>ClinicHub</h1>
        <p class="muted">Multi-tenant clinic management. Sign in to your clinic.</p>

        <form [formGroup]="form" (ngSubmit)="submit()">
          <label>
            Email
            <input type="email" formControlName="email" autocomplete="username" />
          </label>
          <label>
            Password
            <input type="password" formControlName="password" autocomplete="current-password" />
          </label>
          @if (error()) {
            <p class="error" role="alert">{{ error() }}</p>
          }
          <button type="submit" class="primary" [disabled]="form.invalid || busy()">
            {{ busy() ? 'Signing in…' : 'Sign in' }}
          </button>
        </form>

        <div class="demo">
          <p class="muted">Demo accounts (two separate clinics):</p>
          <div class="row">
            <button type="button" (click)="useDemo('admin@sunrise.demo')">Sunrise admin</button>
            <button type="button" (click)="useDemo('staff@sunrise.demo')">Sunrise staff</button>
            <button type="button" (click)="useDemo('admin@lakeside.demo')">Lakeside admin</button>
          </div>
        </div>
      </section>
    </main>
  `,
  styles: `
    .login { min-height: 100dvh; display: grid; place-items: center; padding: 16px; }
    .login-card { width: min(420px, 100%); }
    h1 { margin: 0 0 4px; color: var(--accent); }
    form { display: grid; gap: 12px; margin-top: 20px; }
    .demo { margin-top: 24px; border-top: 1px solid var(--border); padding-top: 16px; }
  `,
})
export class Login {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly busy = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly form = inject(NonNullableFormBuilder).group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required],
  });

  protected useDemo(email: string): void {
    this.form.setValue({ email, password: DEMO_PASSWORD });
  }

  protected submit(): void {
    if (this.form.invalid) return;
    const { email, password } = this.form.getRawValue();
    this.busy.set(true);
    this.error.set(null);
    this.auth.login(email, password).subscribe({
      next: () => void this.router.navigate(['/appointments']),
      error: (err: unknown) => {
        this.error.set(problemMessage(err, 'Invalid email or password.'));
        this.busy.set(false);
      },
    });
  }
}
