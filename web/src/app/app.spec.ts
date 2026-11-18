import { describe, expect, it } from 'vitest';
import { groupByDay } from './features/appointments/appointments';
import { Appointment } from './core/models';

function appt(id: string, startLocal: Date): Appointment {
  return {
    id,
    patientId: 'p',
    patientName: 'Patient',
    doctorId: 'd',
    doctorName: 'Doctor',
    startUtc: startLocal.toISOString(),
    endUtc: new Date(startLocal.getTime() + 30 * 60_000).toISOString(),
    reason: null,
    status: 'Scheduled',
  };
}

describe('groupByDay', () => {
  it('groups by local calendar day in chronological order', () => {
    const groups = groupByDay([
      appt('c', new Date(2030, 0, 2, 9, 0)),
      appt('b', new Date(2030, 0, 1, 15, 0)),
      appt('a', new Date(2030, 0, 1, 9, 0)),
    ]);

    expect(groups.map((g) => g.day)).toEqual(['2030-01-01', '2030-01-02']);
    expect(groups[0].items.map((a) => a.id)).toEqual(['a', 'b']);
  });

  it('returns no groups for no appointments', () => {
    expect(groupByDay([])).toEqual([]);
  });
});
