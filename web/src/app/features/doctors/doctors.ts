import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../core/auth.service';
import { ClinicApi } from '../../core/clinic-api.service';
import { Doctor } from '../../core/models';
import { problemMessage } from '../../core/problem';

@Component({
  selector: 'app-doctors',
  imports: [ReactiveFormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="page-head"><h1>Doctors</h1></div>

    @if (auth.isAdmin()) {
      <section class="card">
        <h2>Add a doctor</h2>
        <form class="row" [formGroup]="form" (ngSubmit)="save()">
          <label>Full name <input formControlName="fullName" placeholder="Dr. Jane Doe" /></label>
          <label>Specialty <input formControlName="specialty" placeholder="Dermatology" /></label>
          <button type="submit" class="primary" [disabled]="form.invalid">Add</button>
        </form>
      </section>
    }

    @if (error()) {
      <p class="error" role="alert">{{ error() }}</p>
    }

    <section class="card">
      <ul class="list">
        @for (d of doctors(); track d.id) {
          <li><strong>{{ d.fullName }}</strong><span class="muted">{{ d.specialty }}</span></li>
        } @empty {
          <li class="muted">No doctors yet.</li>
        }
      </ul>
      @if (!auth.isAdmin()) {
        <p class="muted">Only clinic admins can add doctors.</p>
      }
    </section>
  `,
})
export class Doctors {
  private readonly api = inject(ClinicApi);
  protected readonly auth = inject(AuthService);
  protected readonly doctors = signal<Doctor[]>([]);
  protected readonly error = signal<string | null>(null);

  protected readonly form = inject(NonNullableFormBuilder).group({
    fullName: ['', [Validators.required, Validators.maxLength(150)]],
    specialty: ['', [Validators.required, Validators.maxLength(100)]],
  });

  constructor() {
    this.load();
  }

  private load(): void {
    this.api.doctors().subscribe({
      next: (d) => this.doctors.set(d),
      error: (err: unknown) => this.error.set(problemMessage(err)),
    });
  }

  protected save(): void {
    if (this.form.invalid) return;
    this.api.createDoctor(this.form.getRawValue()).subscribe({
      next: () => {
        this.form.reset();
        this.load();
      },
      error: (err: unknown) => this.error.set(problemMessage(err)),
    });
  }
}
