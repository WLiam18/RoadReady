import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import AppShell from '../components/AppShell';
import { CardSkeleton } from '../components/Skeleton';
import { ClockIcon } from '../components/icons';
import ApiV1 from '../lib/apiV1';

export default function HistoryPage() {
  const navigate = useNavigate();
  const [bookings, setBookings] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    ApiV1.getMyBookings()
      .then((res) => {
        if (res.data?.success) {
          const all = res.data.data || [];
          setBookings(all.filter((b) => b.status === 'Completed' || b.status === 'Cancelled'));
        }
      })
      .catch(() => {})
      .finally(() => setLoading(false));
  }, []);

  return (
    <AppShell>
      <div className="max-w-4xl mx-auto">
        <h1 className="text-2xl font-bold text-brand-ink mb-6">Rental history</h1>
        {loading ? (
          <div className="space-y-4">{[1,2].map(i => <CardSkeleton key={i} lines={2} />)}</div>
        ) : bookings.length === 0 ? (
          <div className="card p-12 text-center">
            <ClockIcon className="w-16 h-16 mx-auto mb-4 text-gray-200" />
            <h2 className="text-lg font-semibold mb-1">No past rentals</h2>
            <p className="text-brand-muted text-sm mb-6">Your completed and cancelled bookings will appear here.</p>
            <button onClick={() => navigate('/my-bookings')} className="btn btn-primary">View active bookings</button>
          </div>
        ) : (
          <div className="space-y-4">
            {bookings.map(b => (
              <div key={b.id} className="card p-5 cursor-pointer hover:shadow-sm transition-shadow" onClick={() => navigate(`/bookings/${b.id}`)}>
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-4">
                    {b.carImageUrl && <img src={b.carImageUrl} alt="" className="w-16 h-14 rounded-lg object-cover" />}
                    <div>
                      <p className="font-semibold">{b.carMake} {b.carModel}</p>
                      <p className="text-xs text-brand-muted">{new Date(b.pickupDate).toLocaleDateString()} — {new Date(b.dropoffDate).toLocaleDateString()}</p>
                    </div>
                  </div>
                  <div className="text-right">
                    <p className="font-semibold">₹{(b.totalAmount || 0).toLocaleString()}</p>
                    <span className={b.status === 'Completed' ? 'badge-success' : 'badge-danger'}>{b.status}</span>
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </AppShell>
  );
}
