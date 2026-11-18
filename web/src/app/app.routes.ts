import { Routes } from '@angular/router';
import { authGuard } from './core/auth.guard';

export const routes: Routes = [
  { path: 'login', loadComponent: () => import('./features/login/login').then((m) => m.Login), title: 'Sign in · ClinicHub' },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () => import('./layout/shell').then((m) => m.Shell),
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'appointments' },
      {
        path: 'appointments',
        loadComponent: () => import('./features/appointments/appointments').then((m) => m.Appointments),
        title: 'Appointments · ClinicHub',
      },
      {
        path: 'patients',
        loadComponent: () => import('./features/patients/patients').then((m) => m.Patients),
        title: 'Patients · ClinicHub',
      },
      {
        path: 'doctors',
        loadComponent: () => import('./features/doctors/doctors').then((m) => m.Doctors),
        title: 'Doctors · ClinicHub',
      },
    ],
  },
  { path: '**', redirectTo: '' },
];
