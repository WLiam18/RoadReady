import { useNavigate } from 'react-router-dom';
import AppShell from '../components/AppShell';
import { HeartIcon } from '../components/icons';

export default function FavoritesPage() {
  const navigate = useNavigate();
  return (
    <AppShell>
      <div className="max-w-4xl mx-auto">
        <h1 className="text-2xl font-bold text-brand-ink mb-6">Favorites</h1>
        <div className="card p-12 text-center">
          <HeartIcon className="w-16 h-16 mx-auto mb-4 text-gray-200" />
          <h2 className="text-lg font-semibold text-brand-ink mb-1">No favorites yet</h2>
          <p className="text-brand-muted text-sm mb-6">Save cars you like by tapping the heart icon on any vehicle card.</p>
          <button onClick={() => navigate('/cars')} className="btn btn-primary">
            Browse vehicles
          </button>
        </div>
      </div>
    </AppShell>
  );
}
