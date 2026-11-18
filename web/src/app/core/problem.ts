import { HttpErrorResponse } from '@angular/common/http';

/** Turns an RFC 9457 problem-details response (or a validation error) into one readable sentence. */
export function problemMessage(error: unknown, fallback = 'Something went wrong. Please try again.'): string {
  if (!(error instanceof HttpErrorResponse)) return fallback;
  if (error.status === 0) return 'Cannot reach the server. Is the API running?';

  const body = error.error as { title?: string; detail?: string; errors?: Record<string, string[]> } | null;
  if (body?.errors) {
    const first = Object.values(body.errors).flat()[0];
    if (first) return first;
  }
  return body?.detail ?? body?.title ?? fallback;
}
