// Real-time validation helpers (lightweight, no extra deps)
export const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]{2,}$/;
export const phoneRegex = /^\+?\d[\d\s().-]{6,}\d$/;

export const passwordRules = [
  { id: 'length', test: (p) => p.length >= 8, label: 'At least 8 characters' },
  { id: 'upper',  test: (p) => /[A-Z]/.test(p),            label: 'One uppercase letter' },
  { id: 'lower',  test: (p) => /[a-z]/.test(p),            label: 'One lowercase letter' },
  { id: 'digit',  test: (p) => /\d/.test(p),                label: 'One number' },
  { id: 'special',test: (p) => /[^A-Za-z0-9]/.test(p),       label: 'One special character' },
];

export function getPasswordStrength(p) {
  const passed = passwordRules.filter((r) => r.test(p)).length;
  if (!p) return { score: 0, label: '—', color: 'bg-gray-200' };
  if (passed <= 1) return { score: 1, label: 'Weak', color: 'bg-red-500' };
  if (passed === 2) return { score: 2, label: 'Fair', color: 'bg-orange-500' };
  if (passed === 3) return { score: 3, label: 'Good', color: 'bg-amber-500' };
  if (passed === 4) return { score: 4, label: 'Strong', color: 'bg-blue-500' };
  return { score: 5, label: 'Excellent', color: 'bg-green-500' };
}

export function validateEmail(value) {
  if (!value) return 'Email is required.';
  if (!emailRegex.test(value)) return 'Please enter a valid email address.';
  return '';
}

export function validatePhone(value) {
  if (!value) return 'Phone number is required.';
  if (!phoneRegex.test(value)) return 'Please enter a valid phone number.';
  return '';
}

export function validatePasswordRegister(value) {
  if (!value) return 'Password is required.';
  if (value.length < 8) return 'Password must be at least 8 characters.';
  if (!/[A-Z]/.test(value)) return 'Add at least one uppercase letter.';
  if (!/[a-z]/.test(value)) return 'Add at least one lowercase letter.';
  if (!/\d/.test(value)) return 'Add at least one number.';
  if (!/[^A-Za-z0-9]/.test(value)) return 'Add at least one special character.';
  return '';
}

export function validatePasswordLogin(value) {
  if (!value) return 'Password is required.';
  return '';
}

export function validateName(value, field) {
  if (!value) return `${field} is required.`;
  if (value.length > 50) return `${field} must be at most 50 characters.`;
  return '';
}

export function validateConfirmPassword(value, original) {
  if (!value) return 'Please confirm your password.';
  if (value !== original) return 'Passwords do not match.';
  return '';
}
