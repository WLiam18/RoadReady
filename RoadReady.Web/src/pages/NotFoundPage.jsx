import AppShell from '../components/AppShell';
import { Logo } from '../components/Logo';

export default function NotFoundPage() {
  return (
    <AppShell hideNav>
      <div className="min-h-[60vh] flex flex-col items-center justify-center text-center px-4">
        <Logo size={48} className="mb-6" />
        <h1 className="text-3xl font-bold text-brand-ink mb-2">Page not found</h1>
        <p className="text-brand-muted mb-6">
          We couldn't find what you were looking for.
        </p>
        <a href="/" className="btn btn-primary">Back to home</a>
      </div>
    </AppShell>
  );
}
