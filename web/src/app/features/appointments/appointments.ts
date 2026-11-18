import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { ClinicApi } from '../../core/clinic-api.service';
import { Appointment, Doctor, Patient } from '../../core/models';
import { problemMessage } from '../../core/problem';

export interface DayGroup {
  day: string;
  items: Appointment[];
}

/** Groups appointments by the viewer's local calendar day, keeping chronological order. */
export function groupByDay(appointments: Appointment[]): DayGroup[] {
  const groups = new Map<string, Appointment[]>();
  for (const a of [...appointments].sort((x, y) => x.startUtc.localeCompare(y.startUtc))) {
    const start = new Date(a.startUtc);
    const key = `${start.getFullYear()}-${String(start.getMonth() + 1).padStart(2, '0')}-${String(start.getDate()).padStart(2, '0')}`;
    groups.set(key, [...(groups.get(key) ?? []), a]);
  }
  return [...groups].map(([day, items]) => ({ day, items }));
}

function toDateInput(d: Date): string {
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
}

@Component({
  selector: 'app-appointments',
  imports: [ReactiveFormsModule, DatePipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './appointments.html',
})
export class Appointments {
  private readonly api = inject(ClinicApi);

  protected readonly appointments = signal<Appointment[]>([]);
  protected readonly doctors = signal<Doctor[]>([]);
  protected readonly patients = signal<Patient[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly notice = signal<string | null>(null);
  protected readonly days = computed(() => groupByDay(this.appointments()));

  private readonly tomorrow = new Date(Date.now() + 24 * 60 * 60 * 1000);

  protected readonly form = inject(NonNullableFormBuilder).group({
    patientId: ['', Validators.required],
    doctorId: ['', Validators.required],
    date: [toDateInput(this.tomorrow), Validators.required],
    time: ['10:00', Validators.required],
    duration: [30, Validators.required],
    reason: ['', Validators.maxLength(500)],
  });

  constructor() {
    forkJoin({ doctors: this.api.doctors(), patients: this.api.searchPatients('', 1, 100) }).subscribe({
      next: ({ doctors, patients }) => {
        this.doctors.set(doctors);
        this.patients.set(patients.items);
      },
      error: (err: unknown) => this.error.set(problemMessage(err)),
    });
    this.refresh();
  }

  protected refresh(): void {
    const from = new Date();
    from.setHours(0, 0, 0, 0);
    const to = new Date(from.getTime() + 14 * 24 * 60 * 60 * 1000);
    this.loading.set(true);
    this.api.appointments(from, to).subscribe({
      next: (list) => {
        this.appointments.set(list);
        this.loading.set(false);
      },
      error: (err: unknown) => {
        this.error.set(problemMessage(err));
        this.loading.set(false);
      },
    });
  }

  protected book(): void {
    if (this.form.invalid) return;
    const v = this.form.getRawValue();
    const start = new Date(`${v.date}T${v.time}`);
    const end = new Date(start.getTime() + Number(v.duration) * 60 * 1000);

    this.error.set(null);
    this.notice.set(null);
    this.api
      .book({
        patientId: v.patientId,
        doctorId: v.doctorId,
        startUtc: start.toISOString(),
        endUtc: end.toISOString(),
        reason: v.reason || null,
      })
      .subscribe({
        next: (a) => {
          this.notice.set(`Booked ${a.patientName} with ${a.doctorName}.`);
          this.refresh();
        },
        error: (err: unknown) => this.error.set(problemMessage(err)),
      });
  }

  protected cancel(a: Appointment): void {
    this.api.cancel(a.id).subscribe({ next: () => this.refresh(), error: (e: unknown) => this.error.set(problemMessage(e)) });
  }

  protected complete(a: Appointment): void {
    this.api.complete(a.id).subscribe({ next: () => this.refresh(), error: (e: unknown) => this.error.set(problemMessage(e)) });
  }
}
