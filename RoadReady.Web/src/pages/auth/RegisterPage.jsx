import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { toast } from 'react-hot-toast';
import AuthLayout from '../../components/AuthLayout';
import { Field, TextInput, PasswordInput, PasswordStrengthMeter } from '../../components/FormControls';
import { validateEmail, validatePasswordRegister, validateName, validatePhone } from '../../lib/validators';
import { useAuth } from '../../context/AuthContext';

export default function RegisterPage() {
  const { register } = useAuth();
  const navigate = useNavigate();

  const [form, setForm] = useState({ firstName: '', lastName: '', email: '', phone: '', password: '', confirm: '' });
  const [errors, setErrors] = useState({});
  const [busy, setBusy] = useState(false);

  const set = (field) => (e) => setForm((v) => ({ ...v, [field]: e.target.value }));

  const validate = () => {
    const e = {};
    e.firstName = validateName(form.firstName, 'First name');
    e.lastName = validateName(form.lastName, 'Last name');
    e.email = validateEmail(form.email);
    e.phone = validatePhone(form.phone);
    e.password = validatePasswordRegister(form.password);
    e.confirm = !form.confirm ? 'Please confirm your password.' : form.confirm !== form.password ? 'Passwords do not match.' : '';
    setErrors(e);
    return !e.firstName && !e.lastName && !e.email && !e.phone && !e.password && !e.confirm;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!validate()) return;
    setBusy(true);
    try {
      const { ok, message } = await register({
        firstName: form.firstName.trim(),
        lastName: form.lastName.trim(),
        email: form.email.trim().toLowerCase(),
        phoneNumber: form.phone.trim(),
        password: form.password,
      });
      if (ok) {
        toast.success('Account created! Welcome to RoadReady.');
        const stored = localStorage.getItem('rr_user');
        if (stored) {
          try {
            const u = JSON.parse(stored);
            const role = String(u?.role || '').toLowerCase();
            if (role === 'admin' || role === '1') { navigate('/admin', { replace: true }); return; }
            if (role === 'rentalagent' || role === '2') { navigate('/agent', { replace: true }); return; }
          } catch {}
        }
        navigate('/', { replace: true });
      } else {
        toast.error(message || 'Registration failed');
      }
    } finally {
      setBusy(false);
    }
  };

  return (
    <AuthLayout title="Create your account" subtitle="Join RoadReady and start booking premium rental cars in seconds.">
      <div className="flex flex-col gap-6">
        <h2 className="text-2xl font-bold text-brand-ink">Sign up</h2>

        <form onSubmit={handleSubmit} noValidate className="space-y-1">
          <div className="grid grid-cols-2 gap-4">
            <Field label="First name" error={errors.firstName} required>
              <TextInput
                value={form.firstName}
                onChange={set('firstName')}
                placeholder="John"
                autoComplete="given-name"
                error={errors.firstName}
              />
            </Field>
            <Field label="Last name" error={errors.lastName} required>
              <TextInput
                value={form.lastName}
                onChange={set('lastName')}
                placeholder="Doe"
                autoComplete="family-name"
                error={errors.lastName}
              />
            </Field>
          </div>

          <Field label="Email" error={errors.email} required>
            <TextInput
              type="email"
              value={form.email}
              onChange={set('email')}
              placeholder="you@example.com"
              autoComplete="email"
              error={errors.email}
            />
          </Field>

          <Field label="Phone number" error={errors.phone} required>
            <TextInput
              type="tel"
              value={form.phone}
              onChange={set('phone')}
              placeholder="+1 415 555 0199"
              autoComplete="tel"
              error={errors.phone}
            />
          </Field>

          <Field label="Password" error={errors.password} required>
            <PasswordInput
              value={form.password}
              onChange={set('password')}
              placeholder="Create a strong password"
              autoComplete="new-password"
              name="new-password"
              error={errors.password}
            />
            <PasswordStrengthMeter password={form.password} />
          </Field>

          <Field label="Confirm password" error={errors.confirm} required>
            <PasswordInput
              value={form.confirm}
              onChange={set('confirm')}
              placeholder="Re-enter your password"
              autoComplete="new-password"
              name="confirm-password"
              error={errors.confirm}
            />
          </Field>

          <button
            type="submit"
            className="btn btn-primary w-full mt-2"
            disabled={busy}
          >
            {busy ? 'Creating account…' : 'Create account'}
          </button>
        </form>

        <p className="text-center text-sm text-brand-muted mt-4">
          Already have an account?{' '}
          <Link to="/login" className="text-brand-ink font-semibold underline underline-offset-2">
            Log in
          </Link>
        </p>
      </div>
    </AuthLayout>
  );
}