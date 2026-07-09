import { useState } from 'react';
import { Link } from 'react-router-dom';
import { toast } from 'react-hot-toast';
import AuthLayout from '../../components/AuthLayout';
import { Field, TextInput } from '../../components/FormControls';
import { validateEmail } from '../../lib/validators';
import { api } from '../../lib/api';

export default function ForgotPasswordPage() {
  const [email, setEmail] = useState('');
  const [error, setError] = useState('');
  const [busy, setBusy] = useState(false);
  const [sent, setSent] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    const err = validateEmail(email);
    setError(err);
    if (err) return;

    setBusy(true);
    try {
      const res = await api.post('/api/v1/auth/forgot-password', { email: email.trim().toLowerCase() });
      // Backend always returns success to prevent email enumeration
      toast.success('If an account with that email exists, a reset link has been sent.');
      setSent(true);
    } catch (err) {
      toast.error('We could not process that request right now. Please try again.');
    } finally {
      setBusy(false);
    }
  };

  return (
    <AuthLayout title="Forgot your password?" subtitle="We'll send a reset link to your inbox. It expires in 1 hour.">
      <div className="flex flex-col gap-6">
        <h2 className="text-2xl font-bold text-brand-ink">Reset password</h2>

        {sent ? (
          <div className="text-center">
            <div className="w-16 h-16 mx-auto mb-4 rounded-full bg-green-100 flex items-center justify-center">
              <svg className="w-8 h-8 text-green-600" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
                <polyline points="20 6 9 17 4 12" />
              </svg>
            </div>
            <p className="text-brand-ink font-medium mb-1">Check your inbox</p>
            <p className="text-sm text-brand-muted mb-6">
              We sent a link to {email}.<br/>
              Did not receive it? Check spam, or{' '}
              <button onClick={() => setSent(false)} className="font-semibold underline underline-offset-2">
                try again
              </button>
              .
            </p>
            <Link to="/login" className="btn btn-outline">
              Back to log in
            </Link>
          </div>
        ) : (
          <form onSubmit={handleSubmit} noValidate>
            <Field label="Email address" error={error} required>
              <TextInput
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                placeholder="you@example.com"
                autoComplete="email"
                error={error}
              />
            </Field>

            <button
              type="submit"
              className="btn btn-primary w-full mt-2"
              disabled={busy}
            >
              {busy ? 'Sending…' : 'Send reset link'}
            </button>

            <p className="text-center text-sm text-brand-muted mt-4">
              <Link to="/login" className="hover:text-brand-ink">&larr; Remember your password? Log in</Link>
            </p>
          </form>
        )}
      </div>
    </AuthLayout>
  );
}