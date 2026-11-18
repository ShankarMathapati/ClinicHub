import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Appointment, BookAppointment, Doctor, PagedResult, Patient, SavePatient } from './models';

@Injectable({ providedIn: 'root' })
export class ClinicApi {
  private readonly http = inject(HttpClient);

  searchPatients(search: string, page: number, pageSize = 10) {
    const params = new HttpParams().set('search', search).set('page', page).set('pageSize', pageSize);
    return this.http.get<PagedResult<Patient>>('/api/patients', { params });
  }

  createPatient(patient: SavePatient) {
    return this.http.post<Patient>('/api/patients', patient);
  }

  doctors() {
    return this.http.get<Doctor[]>('/api/doctors');
  }

  createDoctor(doctor: Omit<Doctor, 'id'>) {
    return this.http.post<Doctor>('/api/doctors', doctor);
  }

  appointments(from: Date, to: Date) {
    const params = new HttpParams().set('fromUtc', from.toISOString()).set('toUtc', to.toISOString());
    return this.http.get<Appointment[]>('/api/appointments', { params });
  }

  book(appointment: BookAppointment) {
    return this.http.post<Appointment>('/api/appointments', appointment);
  }

  cancel(id: string) {
    return this.http.post<void>(`/api/appointments/${id}/cancel`, null);
  }

  complete(id: string) {
    return this.http.post<void>(`/api/appointments/${id}/complete`, null);
  }
}
