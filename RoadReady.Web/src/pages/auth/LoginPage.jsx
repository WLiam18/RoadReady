import { useState, useEffect, useRef } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { toast } from 'react-hot-toast';
import AuthLayout from '../../components/AuthLayout';
import { Field, TextInput, PasswordInput } from '../../components/FormControls';
import { GoogleIcon } from '../../components/icons';
import { validateEmail, validatePasswordLogin } from '../../lib/validators';
import { useAuth } from '../../context/AuthContext';

const GOOGLE_CLIENT_ID = '775816209536-s9bq9t69nu5oqoltharsot38jg6mc88f.apps.googleusercontent.com';

export default function LoginPage() {
  const { login, loginWithGoogle } = useAuth();
  const navigate = useNavigate();
  const googleBtnRef = useRef(null);

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [errors, setErrors] = useState({});
  const [busy, setBusy] = useState(false);

  const validate = () => {
    const e = {};
    e.email = validateEmail(email);
    e.password = validatePasswordLogin(password);
    setErrors(e);
    return !e.email && !e.password;
  };

  function redirectAfterLogin(user) {
    const role = String(user?.role || '').toLowerCase();
    if (role === 'admin' || role === '1') return '/admin';
    if (role === 'rentalagent' || role === '2') return '/agent';
    return '/';
  }

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!validate()) return;
    setBusy(true);
    try {
      const { ok, message } = await login(email, password);
      if (ok) {
        toast.success('Welcome back!');
        const stored = localStorage.getItem('rr_user');
        if (stored) {
          try { navigate(redirectAfterLogin(JSON.parse(stored)), { replace: true }); return; } catch {}
        }
        navigate('/', { replace: true });
      } else {
        toast.error(message || 'Login failed');
      }
    } finally {
      setBusy(false);
    }
  };

  // Handle the Google credential response
  const handleGoogleCredential = async (response) => {
    if (!response?.credential) {
      toast.error('Google Sign-In was cancelled.');
      setBusy(false);
      return;
    }

    try {
      const { ok, message } = await loginWithGoogle(response.credential);
      if (ok) {
        toast.success('Welcome to RoadReady!');
        const stored = localStorage.getItem('rr_user');
        const dest = stored ? redirectAfterLogin(JSON.parse(stored)) : '/';
        navigate(dest, { replace: true });
      } else {
        toast.error(message || 'Google login failed.');
      }
    } catch {
      toast.error('Google Sign-In failed. Try email instead.');
    } finally {
      setBusy(false);
    }
  };

  // Initialize Google Identity Services when the script loads.
  // Guard so we only call window.google.accounts.id.initialize once per page load
  // even if React re-renders this component (Strict Mode, HMR, route revisits).
  useEffect(() => {
    let attempts = 0;
    const initGoogle = () => {
      if (window.__rrGoogleInited) {
        // already initialized, but we still need to make sure the button is rendered
        if (googleBtnRef.current && !googleBtnRef.current.hasChildNodes()) {
          try {
            window.google?.accounts?.id?.renderButton(googleBtnRef.current, {
              theme: 'outline', size: 'large', width: 320,
              text: 'continue_with', shape: 'pill', logo_alignment: 'left',
            });
          } catch {/* ignore */}
        }
        return;
      }
      if (window.google?.accounts?.id && googleBtnRef.current) {
        window.google.accounts.id.initialize({
          client_id: GOOGLE_CLIENT_ID,
          callback: handleGoogleCredential,
          cancel_on_tap_outside: true,
        });

        const parent = googleBtnRef.current.parentElement;
        const w = Math.max(240, Math.min(parent?.clientWidth || 320, 400));
        window.google.accounts.id.renderButton(googleBtnRef.current, {
          theme: 'outline',
          size: 'large',
          width: w,
          text: 'continue_with',
          shape: 'pill',
          logo_alignment: 'left',
        });
        window.__rrGoogleInited = true;
      } else if (attempts < 20) {
        attempts++;
        setTimeout(initGoogle, 300);
      }
    };
    initGoogle();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return (
    <AuthLayout
      title="Welcome back"
      subtitle="Sign in to manage your bookings, check rental history, and hit the road."
    >
      <div className="flex flex-col gap-6">
        <h2 className="text-2xl font-bold text-brand-ink">Log in</h2>

        <form onSubmit={handleSubmit} noValidate>
          <Field label="Email" error={errors.email} required>
            <TextInput
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder="you@example.com"
              autoComplete="email"
              error={errors.email}
            />
          </Field>

          <Field label="Password" error={errors.password} required>
            <PasswordInput
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              error={errors.password}
            />
          </Field>

          <button
            type="submit"
            className="btn btn-primary w-full mt-2"
            disabled={busy}
          >
            {busy ? 'Signing in…' : 'Log in'}
          </button>
        </form>

        <div className="relative text-center">
          <span className="bg-white px-4 text-xs text-brand-muted relative z-10">OR</span>
          <div className="absolute top-1/2 left-0 w-full h-px bg-brand-divider -z-10" />
        </div>

        {/* Google renders its own button here via GIS */}
        <div ref={googleBtnRef} className="flex justify-center min-h-[44px]" />

        <p className="text-center text-sm text-brand-muted">
          <Link to="/forgot-password" className="hover:text-brand-ink">Forgot password?</Link>
        </p>

        <p className="text-center text-sm text-brand-muted">
          Don&rsquo;t have an account?{' '}
          <Link to="/register" className="text-brand-ink font-semibold underline underline-offset-2">
            Sign up
          </Link>
        </p>
      </div>
    </AuthLayout>
  );
}
