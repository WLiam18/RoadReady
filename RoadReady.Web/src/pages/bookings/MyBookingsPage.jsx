import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { toast } from 'react-hot-toast';
import AppShell from '../../components/AppShell';
import { CardSkeleton } from '../../components/Skeleton';
import { StarIcon, CalendarIcon, CreditCardIcon, MapPinIcon, ArrowRightIcon } from '../../components/icons';
import ApiV1 from '../../lib/apiV1';
import { handleApiError } from '../../lib/api';

// Booking timestamps are stored as UTC; display them in IST so every
// user sees the same wall-clock value the user originally entered.
const IST_TZ = 'Asia/Kolkata';
const formatIstDate = (iso) => {
  if (!iso) return '—';
  return new Date(iso).toLocaleDateString('en-IN', { timeZone: IST_TZ, year: 'numeric', month: 'short', day: '2-digit' });
};

const STATUS_STYLES = {
  // PendingPayment uses the same visual style as Confirmed — it's an
  // internal-only state and we don't want the customer to think they're
  // "in trouble" while Razorpay is finalizing (typically <2s).
  PendingPayment: 'badge-info',
  Confirmed: 'badge-info',
  Active: 'badge-success',
  Completed: 'badge-success',
  Cancelled: 'badge-danger',
  Modified: 'badge-neutral',
};

// Friendly label that hides the "Pending payment" wording once a paymentUrl exists
const STATUS_LABEL = {
  PendingPayment: 'Awaiting confirmation',
  Confirmed: 'Confirmed',
  Active: 'Active',
  Completed: 'Completed',
  Cancelled: 'Cancelled',
  Modified: 'Modified',
};

export default function MyBookingsPage() {
  const navigate = useNavigate();
  const [bookings, setBookings] = useState([]);
  const [loading, setLoading] = useState(true);

  const fetchBookings = async () => {
    try {
      const res = await ApiV1.getMyBookings();
      if (res.data?.success) setBookings(res.data.data || []);
    } catch (err) {
      toast.error(handleApiError(err) || 'Failed to load bookings');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchBookings(); }, []);

  // If any booking is still pending, re-check every 5s so the badge
  // updates the moment the Razorpay webhook flips status to Confirmed.
  useEffect(() => {
    if (!bookings.some((b) => b.status === 'PendingPayment')) return;
    const id = setInterval(fetchBookings, 5000);
    return () => clearInterval(id);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [bookings]);

  const handleCancel = async (id) => {
    if (!window.confirm('Cancel this booking? Any refund will be processed automatically.')) return;
    try {
      const res = await ApiV1.cancelBooking(id);
      if (res.data?.success) {
        toast.success('Booking cancelled.');
        setBookings((prev) => prev.map((b) => (b.id === id ? { ...b, status: 'Cancelled' } : b)));
      } else {
        toast.error(res.data?.message || 'Cancellation failed.');
      }
    } catch {
      toast.error('Failed to cancel.');
    }
  };

  if (loading) {
    return (
      <AppShell>
        <div className="max-w-4xl mx-auto space-y-4">
          {[1,2].map((i) => <CardSkeleton key={i} lines={2} />)}
        </div>
      </AppShell>
    );
  }

  return (
    <AppShell>
      <div className="max-w-4xl mx-auto">
        <h1 className="text-2xl font-bold text-brand-ink mb-6">My bookings</h1>

        {bookings.length === 0 ? (
          <div className="card p-10 text-center">
            <CalendarIcon className="w-12 h-12 mx-auto mb-3 text-brand-muted" />
            <h2 className="font-semibold mb-1">No bookings yet</h2>
            <p className="text-sm text-brand-muted mb-4">Browse cars and make your first reservation.</p>
            <button onClick={() => navigate('/cars')} className="btn btn-primary">Browse cars</button>
          </div>
        ) : (
          <div className="space-y-4">
            {bookings.map((b) => (
              <div key={b.id} className="card-hover"
                onClick={() => navigate(`/my-bookings/${b.id}`)}>
                <div className="p-5">
                  <div className="flex items-start justify-between gap-4">
                    <div className="flex items-center gap-4 min-w-0">
                      {b.carImageUrl && (
                        <img src={b.carImageUrl} alt="" className="w-20 h-16 rounded-lg object-cover flex-shrink-0" />
                      )}
                      <div className="min-w-0">
                        <div className="flex items-center gap-2 mb-1">
                          <h3 className="font-bold text-brand-ink truncate">{b.carMake} {b.carModel}</h3>
                          <span className={STATUS_STYLES[b.status] || 'badge-neutral'}>
                            {STATUS_LABEL[b.status] || b.status}
                          </span>
                        </div>
                        <div className="flex flex-wrap items-center gap-x-4 gap-y-1 text-xs text-brand-muted">
                          <span className="flex items-center gap-1"><CalendarIcon className="w-3 h-3" /> {formatIstDate(b.pickupDate)} → {formatIstDate(b.dropoffDate)}</span>
                          <span className="flex items-center gap-1"><MapPinIcon className="w-3 h-3" /> {b.pickupLocation}</span>
                        </div>
                      </div>
                    </div>
                    <div className="text-right flex-shrink-0">
                      <div className="font-bold text-brand-ink">₹{b.totalAmount.toLocaleString()}</div>
                      <div className="text-xs text-brand-muted">
                        {b.status === 'PendingPayment' ? 'Awaiting payment confirmation' : b.paymentStatus}
                      </div>
                    </div>
                  </div>
                  <div className="flex items-center gap-2 mt-3 pt-3 border-t border-brand-divider">
                    <button onClick={(e) => { e.stopPropagation(); navigate(`/my-bookings/${b.id}`); }}
                      className="btn btn-primary btn-sm">View details</button>
                    {(b.status === 'Confirmed' || b.status === 'PendingPayment') && (
                      <button onClick={(e) => { e.stopPropagation(); handleCancel(b.id); }}
                        className="btn btn-outline btn-sm text-brand-danger">Cancel</button>
                    )}
                    {b.status === 'PendingPayment' && b.paymentUrl && (
                      <a href={b.paymentUrl} target="_blank" rel="noreferrer"
                        onClick={(e) => e.stopPropagation()}
                        className="btn btn-danger btn-sm ml-auto">
                        <CreditCardIcon className="w-4 h-4" /> Resume payment
                      </a>
                    )}
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
