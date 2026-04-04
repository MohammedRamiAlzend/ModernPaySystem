/**
 * Generates a UUID v4 string.
 * Uses crypto.randomUUID if available, otherwise falls back to a manual implementation.
 * This ensures compatibility with non-secure contexts (HTTP) where crypto.randomUUID is disabled.
 */
export const v4 = (): string => {
  // Use crypto.randomUUID if available
  if (typeof crypto !== 'undefined' && typeof crypto.randomUUID === 'function') {
    return crypto.randomUUID();
  }

  // Fallback for non-secure contexts or older browsers
  return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, (c) => {
    const r = (Math.random() * 16) | 0;
    const v = c === 'x' ? r : (r & 0x3) | 0x8;
    return v.toString(16);
  });
};
