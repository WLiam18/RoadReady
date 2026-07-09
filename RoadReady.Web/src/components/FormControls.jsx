import { useState } from 'react';
import { EyeIcon, EyeOffIcon, CheckIcon, TimesIcon } from './icons';
import { passwordRules, getPasswordStrength } from '../lib/validators';

export function Field({ label, error, children, hint, required }) {
  return (
    <div className="mb-4">
      {label && (
        <label className="label">
          {label}
          {required && <span className="text-brand-danger ml-0.5">*</span>}
        </label>
      )}
      {children}
      {hint && !error && <p className="text-xs text-brand-muted mt-1">{hint}</p>}
      {error && <p className="text-xs text-brand-danger mt-1">{error}</p>}
    </div>
  );
}

export function TextInput({ className = '', error, ...props }) {
  return (
    <input
      {...props}
      className={`input ${error ? 'border-brand-danger focus:border-brand-danger focus:ring-brand-danger/30' : ''} ${className}`}
    />
  );
}

export function PasswordInput({ value, onChange, placeholder = 'Enter password', error, autoComplete = 'current-password', name = 'password', id }) {
  const [show, setShow] = useState(false);
  return (
    <div className="relative">
      <input
        id={id ?? name}
        name={name}
        type={show ? 'text' : 'password'}
        value={value}
        onChange={onChange}
        placeholder={placeholder}
        autoComplete={autoComplete}
        className={`input pr-10 ${error ? 'border-brand-danger focus:border-brand-danger focus:ring-brand-danger/30' : ''}`}
      />
      <button
        type="button"
        onClick={() => setShow((v) => !v)}
        tabIndex={-1}
        className="absolute right-2 top-1/2 -translate-y-1/2 text-brand-muted hover:text-brand-ink p-1"
        aria-label={show ? 'Hide password' : 'Show password'}
      >
        {show ? <EyeOffIcon className="w-5 h-5" /> : <EyeIcon className="w-5 h-5" />}
      </button>
    </div>
  );
}

export function PasswordStrengthMeter({ password }) {
  if (!password) return null;
  const rules = passwordRules;
  const passed = rules.filter((r) => r.test(password)).length;
  const total = rules.length;
  const pct = (passed / total) * 100;
  const { label, color } = getPasswordStrength(password);
  return (
    <div className="mt-2">
      <div className="h-1.5 w-full bg-gray-200 rounded-full overflow-hidden">
        <div
          className={`h-full transition-all ${color}`}
          style={{ width: `${pct}%` }}
        />
      </div>
      <p className="text-xs text-brand-muted mt-1">Strength: <span className="font-medium text-brand-ink">{label}</span></p>
      <ul className="mt-2 grid grid-cols-1 sm:grid-cols-2 gap-1 text-xs">
        {rules.map((r) => (
          <li key={r.id} className={`flex items-center gap-1.5 ${r.test(password) ? 'text-brand-success' : 'text-brand-muted'}`}>
            {r.test(password) ? <CheckIcon className="w-3.5 h-3.5" /> : <TimesIcon className="w-3.5 h-3.5" />}
            {r.label}
          </li>
        ))}
      </ul>
    </div>
  );
}
