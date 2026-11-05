/**
 * Format a numeric decimal value with a unit.
 */
export function formatDecimal(
  value: number | null | undefined,
  unit?: string,
  decimals = 2,
): string {
  if (value == null || Number.isNaN(value)) return "—";
  const formatted = Number(value).toFixed(decimals);
  // Strip trailing zeros if clean integer (e.g., 98.00 -> 98, 12.40 -> 12.4)
  const clean = formatted.replace(/\.?0+$/, "");
  return unit ? `${clean} ${unit}` : clean;
}

/**
 * Format bytes to readable size string.
 */
export function formatFileSize(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(2)} MB`;
}

/**
 * Format an ISO date/time string to human-readable format.
 */
export function formatDateTime(dateStr: string | null | undefined): string {
  if (!dateStr) return "—";
  try {
    const d = new Date(dateStr);
    if (Number.isNaN(d.getTime())) return dateStr;
    return d.toLocaleString("en-GB", {
      year: "numeric",
      month: "short",
      day: "2-digit",
      hour: "2-digit",
      minute: "2-digit",
      hour12: false,
    });
  } catch {
    return dateStr;
  }
}
