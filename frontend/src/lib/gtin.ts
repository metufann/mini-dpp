/**
 * Validates a GTIN string using the GS1 Modulo-10 check-digit algorithm.
 * Exactly mirrors backend MiniDpp.Api.Validators.GtinValidator.
 */
export function validateGtin(gtin: string | null | undefined): {
  valid: boolean;
  error?: string;
} {
  const value = (gtin ?? "").trim();

  if (value.length === 0) {
    return { valid: false, error: "GTIN cannot be empty." };
  }

  if (!/^[0-9]{14}$/.test(value)) {
    return {
      valid: false,
      error: "GTIN must contain exactly 14 numeric digits.",
    };
  }

  let sum = 0;
  let multiplier = 3;

  for (let i = value.length - 2; i >= 0; i--) {
    sum += (value.charCodeAt(i) - 48) * multiplier;
    multiplier = multiplier === 3 ? 1 : 3;
  }

  const expectedCheckDigit = (10 - (sum % 10)) % 10;
  const actualCheckDigit = value.charCodeAt(value.length - 1) - 48;

  if (actualCheckDigit !== expectedCheckDigit) {
    return { valid: false, error: "GTIN check digit is invalid." };
  }

  return { valid: true };
}
