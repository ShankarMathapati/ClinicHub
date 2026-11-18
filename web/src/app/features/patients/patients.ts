import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { EMPTY, catchError, debounceTime, switchMap, tap } from 'rxjs';
import { ClinicApi } from '../../core/clinic-api.service';
import { PagedResult, Patient } from '../../core/models';
import { problemMessage } from '../../core/problem';

@Component({
  selector: 'app-patients',
  imports: [ReactiveFormsModule, DatePipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './patients.html',
})
export class Patients {
  private readonly api = inject(ClinicApi);

  protected readonly search = signal('');
  protected readonly page = signal(1);
  protected readonly result = signal<PagedResult<Patient> | null>(null);
  protected readonly error = signal<string | null>(null);
  protected readonly showForm = signal(false);
  private readonly reload = signal(0);
  private readonly query = computed(() => ({ search: this.search(), page: this.page(), reload: this.reload() }));

  protected readonly form = inject(NonNullableFormBuilder).group({
    firstName: ['', [Validators.required, Validators.maxLength(100)]],
    lastName: ['', [Validators.required, Validators.maxLength(100)]],
    dateOfBirth: ['', Validators.required],
    phone: [''],
    email: ['', Validators.email],
  });

  constructor() {
    toObservable(this.query)
      .pipe(
        debounceTime(250),
        tap(() => this.error.set(null)),
        // Errors are handled per request so one failed search does not end the stream.
        switchMap((q) =>
          this.api.searchPatients(q.search, q.page).pipe(
            catchError((err: unknown) => {
              this.error.set(problemMessage(err));
              return EMPTY;
            }),
          ),
        ),
        takeUntilDestroyed(),
      )
      .subscribe((r) => this.result.set(r));
  }

  protected onSearch(value: string): void {
    this.search.set(value);
    this.page.set(1);
  }

  protected save(): void {
    if (this.form.invalid) return;
    const v = this.form.getRawValue();
    this.api.createPatient({ ...v, phone: v.phone || null, email: v.email || null }).subscribe({
      next: () => {
        this.form.reset();
        this.showForm.set(false);
        this.reload.update((n) => n + 1);
      },
      error: (err: unknown) => this.error.set(problemMessage(err)),
    });
  }
}
