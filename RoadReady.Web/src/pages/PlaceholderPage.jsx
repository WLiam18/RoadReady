import AppShell from '../components/AppShell';

export default function PlaceholderPage({ title, message }) {
  return (
    <AppShell>
      <div className="card p-10 text-center">
        <h1 className="text-2xl font-bold mb-2 text-brand-ink">{title}</h1>
        <p className="text-brand-muted">{message}</p>
      </div>
    </AppShell>
  );
}
