/**
 * Temporary simple bank validation 
 * Will be replaced with ibankit once tested
 */

export interface IbanValidationResult {
  valid: boolean;
  formatted?: string;
  electronic?: string;
  error?: string;
}

export interface SwiftValidationResult {
  valid: boolean;
  error?: string;
}

export const validateIBAN = (iban: string): IbanValidationResult => {
  if (!iban || iban.trim().length === 0) {
    return { valid: false, error: 'IBAN is required' };
  }

  // Simple format check (2 letters + 2 digits + alphanumeric)
  const ibanRegex = /^[A-Z]{2}[0-9]{2}[A-Z0-9]+$/;
  const cleanIban = iban.replace(/\s/g, '').toUpperCase();

  if (ibanRegex.test(cleanIban)) {
    return {
      valid: true,
      formatted: cleanIban.match(/.{1,4}/g)?.join(' ') || cleanIban,
      electronic: cleanIban
    };
  }

  return { valid: false, error: 'Invalid IBAN format' };
};

export const validateSWIFT = (swift: string): SwiftValidationResult => {
  if (!swift || swift.trim().length === 0) {
    return { valid: false, error: 'SWIFT/BIC code is required' };
  }

  // SWIFT format: 8 or 11 characters (4 bank + 2 country + 2 location + optional 3 branch)
  const swiftRegex = /^[A-Z]{4}[A-Z]{2}[A-Z0-9]{2}([A-Z0-9]{3})?$/;
  const cleanSwift = swift.replace(/\s/g, '').toUpperCase();

  if (swiftRegex.test(cleanSwift)) {
    return { valid: true };
  }

  return { valid: false, error: 'Invalid SWIFT/BIC format (8 or 11 characters)' };
};

export const formatIBAN = (iban: string): string => {
  const clean = iban.replace(/\s/g, '').toUpperCase();
  return clean.match(/.{1,4}/g)?.join(' ') || iban;
};

export const toElectronicIBAN = (iban: string): string => {
  return iban.replace(/\s/g, '').toUpperCase();
};
