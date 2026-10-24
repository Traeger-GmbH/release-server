/**
 * Formats a release date of the API (ISO date, e.g. "2021-05-07T00:00:00") for display.
 */
export function formatDate (value) {
  if (!value) {
    return '';
  }
  // Dates without time zone are calendar dates; show them unchanged in every time zone
  const date = /Z|[+-]\d{2}:\d{2}$/.test(value) ? new Date(value) : new Date(`${value.substring(0, 10)}T00:00:00`);
  return Number.isNaN(date.getTime()) ? value : date.toLocaleDateString();
}
