import { useState, useEffect } from 'react';
import { Link, useSearchParams, useNavigate } from 'react-router-dom';
import { toast } from 'react-hot-toast';
import AuthLayout from '../../components/AuthLayout';
import { Field, PasswordInput, PasswordStrengthMeter } from '../../components/FormControls';
import { validatePasswordRegister } from '../../lib/validators';
import { api } from '../../lib/api';

export default function ResetPasswordPage() {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const token = searchParams.get('token');
  const email = searchParams.get('email');

  const [password, setPassword] = useState('');
  const [confirm, setConfirm] = useState('');
  const [errors, setErrors] = useState({});
  const [busy, setBusy] = useState(false);
  const [success, setSuccess] = useState(false);

  useEffect(() => {
    if (!token) {
      toast.error('Invalid reset link. Please request a new password reset.');
      navigate('/forgot-password', { replace: true });
    }
  }, [token, navigate]);

  const validate = () => {
    const e = {};
    e.password = validatePasswordRegister(password);
    e.confirm = !confirm ? 'Please confirm your password.' : confirm !== password ? 'Passwords do not match.' : '';
    setErrors(e);
    return !e.password && !e.confirm;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!validate() || !token) return;
    setBusy(true);
    try {
      const res = await api.post('/api/v1/auth/reset-password', { token, newPassword: password });
      if (res.data.success) {
        toast.success('Password reset successful! You can now log in.');
        setSuccess(true);
      } else {
        toast.error(res.data.message || 'Reset failed. Please request a new link.');
      }
    } catch {
      toast.error('Something went wrong. Try again.');
    } finally {
      setBusy(false);
    }
  };

  if (!token) return null;

  return (
    <AuthLayout title="Choose a new password" subtitle="Pick a strong password and you're all set.">
      <div className="flex flex-col gap-6">
        <h2 className="text-2xl font-bold text-brand-ink">New password</h2>

        {success ? (
          <div className="text-center">
            <div className="w-16 h-16 mx-auto mb-4 rounded-full bg-green-100 flex items-center justify-center">
              <svg className="w-8 h-8 text-green-600" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
                <polyline points="20 6 9 17 4 12" />
              </svg>
            </div>
            <p className="text-brand-ink font-medium mb-2">Password changed!</p>
            <p className="text-sm text-brand-muted mb-6">
              Your new password is set for {email || 'your account'}.
            </p>
            <Link to="/login" className="btn btn-primary">
              Log in now
            </Link>
          </div>
        ) : (
          <form onSubmit={handleSubmit} noValidate>
            <Field label="New password" error={errors.password} required>
              <PasswordInput
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                placeholder="Create a strong password"
                autoComplete="new-password"
                name="reset-password"
                error={errors.password}
              />
              <PasswordStrengthMeter password={password} />
            </Field>

            <Field label="Confirm new password" error={errors.confirm} required>
              <PasswordInput
                value={confirm}
                onChange={(e) => setConfirm(e.target.value)}
                placeholder="Re-enter your password"
                autoComplete="new-password"
                name="reset-confirm-password"
                error={errors.confirm}
              />
            </Field>

            <button
              type="submit"
              className="btn btn-primary w-full mt-2"
              disabled={busy}
            >
              {busy ? 'Resetting…' : 'Reset password'}
            </button>

            <p className="text-center text-sm text-brand-muted mt-4">
              <Link to="/login" className="hover:text-brand-ink">&larr; Back to log in</Link>
            </p>
          </form>
        )}
      </div>
    </AuthLayout>
  );
}