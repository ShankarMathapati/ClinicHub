import { HttpErrorResponse } from '@angular/common/http';
import { describe, expect, it } from 'vitest';
import { problemMessage } from './problem';

describe('problemMessage', () => {
  it('uses the problem detail when present', () => {
    const err = new HttpErrorResponse({ status: 409, error: { title: 'Conflict', detail: 'Dr. Rao is busy.' } });
    expect(problemMessage(err)).toBe('Dr. Rao is busy.');
  });

  it('uses the first validation error', () => {
    const err = new HttpErrorResponse({ status: 400, error: { title: 'Invalid', errors: { Email: ['Email is invalid.'] } } });
    expect(problemMessage(err)).toBe('Email is invalid.');
  });

  it('explains a network failure', () => {
    expect(problemMessage(new HttpErrorResponse({ status: 0 }))).toContain('Cannot reach the server');
  });

  it('falls back for unknown errors', () => {
    expect(problemMessage(new Error('boom'), 'fallback')).toBe('fallback');
  });
});
