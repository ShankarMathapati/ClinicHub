export type AppointmentStatus = 'Scheduled' | 'Completed' | 'Cancelled';

export interface LoginResponse {
  accessToken: string;
  expiresAtUtc: string;
  email: string;
  role: 'Admin' | 'Staff';
  tenantName: string;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface Patient {
  id: string;
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  phone: string | null;
  email: string | null;
}

export type SavePatient = Omit<Patient, 'id'>;

export interface Doctor {
  id: string;
  fullName: string;
  specialty: string;
}

export interface Appointment {
  id: string;
  patientId: string;
  patientName: string;
  doctorId: string;
  doctorName: string;
  startUtc: string;
  endUtc: string;
  reason: string | null;
  status: AppointmentStatus;
}

export interface BookAppointment {
  patientId: string;
  doctorId: string;
  startUtc: string;
  endUtc: string;
  reason: string | null;
}
