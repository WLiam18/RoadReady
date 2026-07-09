import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { toast } from 'react-hot-toast';
import AppShell from '../../components/AppShell';
import { CalendarIcon, MapPinIcon, CreditCardIcon } from '../../components/icons';
import ApiV1 from '../../lib/apiV1';

// All booking timestamps are stored as UTC. Display them in IST (the
// deployment region) so every user sees the same wall-clock value the user
// entered when they booked.
const IST_TZ = 'Asia/Kolkata';
const formatIstDateTime = (iso) => {
  if (!iso) return '—';
  const d = new Date(iso);
  return d.toLocaleString('en-IN', {
    timeZone: IST_TZ,
    year: 'numeric', month: 'short', day: '2-digit',
    hour: '2-digit', minute: '2-digit', hour12: true,
  });
};

const STATUS_STYLES = {
  PendingPayment: 'badge-info',
  Confirmed: 'badge-info',
  Active: 'badge-success',
  Completed: 'badge-success',
  Cancelled: 'badge-danger',
  Modified: 'badge-neutral',
};

// Hide "Pending payment" wording once a paymentUrl exists — still record for the user
const STATUS_LABEL = {
  PendingPayment: 'Awaiting confirmation',
  Confirmed: 'Confirmed',
  Active: 'Active',
  Completed: 'Completed',
  Cancelled: 'Cancelled',
  Modified: 'Modified',
};

export default function BookingDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();

  const [booking, setBooking] = useState(null);
  const [payments, setPayments] = useState([]);
  const [loading, setLoading] = useState(true);

  const fetchBooking = async () => {
    try {
      const [bookingRes, paymentsRes] = await Promise.all([
        ApiV1.getMyBookingById(id),
        ApiV1.getMyPayments(),
      ]);
      if (bookingRes.data?.success) {
        setBooking(bookingRes.data.data);
      }
      if (paymentsRes.data?.success) {
        const all = paymentsRes.data.data || [];
        setPayments(all.filter((p) => String(p.bookingId) === String(id)));
      }
    } catch {
      // ignore during polling
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchBooking(); }, [id]);

  // Poll every 3s while booking is still PendingPayment — webhook usually
  // arrives within 1-3s of payment completion on Razorpay's end.
  useEffect(() => {
    if (!booking || booking.status !== 'PendingPayment') return;
    const id = setInterval(fetchBooking, 3000);
    return () => clearInterval(id);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [booking?.status]);

  const handleCancel = async () => {
    if (!window.confirm('Cancel this booking? Any refund will be processed.')) return;
    try {
      const res = await ApiV1.cancelBooking(id);
      if (res.data?.success) {
        toast.success('Booking cancelled.');
        setBooking((prev) => prev ? { ...prev, status: 'Cancelled' } : prev);
      } else {
        toast.error(res.data?.message || 'Cancellation failed.');
      }
    } catch {
      toast.error('Failed to cancel.');
    }
  };

  const handleDownloadReceipt = async () => {
    try {
      const res = await ApiV1.downloadReceipt(id);
      const url = URL.createObjectURL(new Blob([res.data], { type: 'application/pdf' }));
      const a = document.createElement('a');
      a.href = url;
      a.download = `receipt-booking-${id}.pdf`;
      a.click();
      URL.revokeObjectURL(url);
      toast.success('Receipt downloaded!');
    } catch {
      toast.error('Receipt not available yet.');
    }
  };

  if (loading) {
    return <AppShell><div className="max-w-3xl mx-auto card h-64 animate-pulse" /></AppShell>;
  }

  if (!booking) return null;

  const canCancel = booking.status === 'Confirmed' || booking.status === 'PendingPayment';

  return (
    <AppShell>
      <div className="max-w-3xl mx-auto">
        <button onClick={() => navigate('/my-bookings')} className="text-sm text-brand-muted hover:text-brand-ink mb-4">&larr; Back to bookings</button>

        <div className="card p-6 mb-6">
          <div className="flex items-start justify-between mb-4">
            <div>
              <h1 className="text-xl font-bold text-brand-ink">Booking #{booking.id}</h1>
              <p className="text-sm text-brand-muted">Created {formatIstDateTime(booking.createdAt)}</p>
            </div>
            <span className={STATUS_STYLES[booking.status] || 'badge-neutral'}>
              {STATUS_LABEL[booking.status] || booking.status}
            </span>
          </div>

          <div className="flex items-center gap-4 mb-6">
            {booking.carImageUrl && (
              <img src={booking.carImageUrl} alt="" className="w-24 h-20 rounded-lg object-cover" />
            )}
            <div>
              <p className="font-bold text-lg text-brand-ink">{booking.carMake} {booking.carModel}</p>
              <p className="text-sm text-brand-muted flex items-center gap-1">
                <MapPinIcon className="w-3.5 h-3.5" /> {booking.pickupLocation}
              </p>
            </div>
          </div>

          <div className="grid grid-cols-2 gap-4 text-sm mb-6">
            <div className="bg-brand-bg rounded-lg p-3">
              <p className="text-xs text-brand-muted uppercase tracking-wide mb-1">Pick-up</p>
              <p className="font-medium">{formatIstDateTime(booking.pickupDate)}</p>
            </div>
            <div className="bg-brand-bg rounded-lg p-3">
              <p className="text-xs text-brand-muted uppercase tracking-wide mb-1">Drop-off</p>
              <p className="font-medium">{formatIstDateTime(booking.dropoffDate)}</p>
            </div>
          </div>

          <div className="space-y-1 text-sm mb-6">
            {booking.subtotal > 0 && (
              <div className="flex justify-between"><span className="text-brand-muted">Subtotal</span><span>₹{booking.subtotal.toLocaleString()}</span></div>
            )}
            {booking.discountAmount > 0 && (
              <div className="flex justify-between text-brand-success"><span>Discount{booking.appliedPromoCode ? ` (${booking.appliedPromoCode})` : ''}</span><span>-₹{booking.discountAmount.toLocaleString()}</span></div>
            )}
            <div className="flex justify-between font-bold text-lg pt-2 border-t border-brand-divider">
              <span>Total</span>
              <span>₹{booking.totalAmount.toLocaleString()}</span>
            </div>
          </div>

          {booking.includesCarSeat && (<p className="text-xs text-brand-muted">Includes child car seat</p>)}

          <div className="flex flex-wrap gap-2 mt-4 pt-4 border-t border-brand-divider">
            {canCancel && (
              <button onClick={handleCancel} className="btn btn-outline text-brand-danger border-brand-danger hover:bg-red-50">Cancel booking</button>
            )}
            {booking.status === 'PendingPayment' && booking.paymentUrl && (
              <a href={booking.paymentUrl} target="_blank" rel="noreferrer" className="btn btn-primary">
                <CreditCardIcon className="w-4 h-4" /> Complete payment
              </a>
            )}
            {(booking.status === 'Confirmed' || booking.status === 'Completed' || booking.status === 'Active') && (
              <button onClick={handleDownloadReceipt} className="btn btn-outline">
                Download receipt
              </button>
            )}
          </div>
        </div>

        {/* Payment history */}
        {payments.length > 0 && (
          <div className="card p-6">
            <h2 className="font-semibold mb-4">Payment history</h2>
            <div className="space-y-3">
              {payments.map((p) => (
                <div key={p.id} className="flex items-center justify-between text-sm pb-3 border-b border-brand-divider last:border-0">
                  <div>
                    <p className="font-medium">
                      {p.type === 'Refund' ? 'Refund' : 'Payment'}
                      {p.type === 'Refund' ? ` (#${p.id})` : ''}
                    </p>
                    <p className="text-xs text-brand-muted">{formatIstDateTime(p.createdAt)}</p>
                  </div>
                  <span className={p.status === 'Succeeded' ? 'badge-success' : p.status === 'Failed' ? 'badge-danger' : 'badge-warning'}>
                    {p.status}
                  </span>
                  <span className="font-medium">₹{p.amount.toLocaleString()}</span>
                </div>
              ))}
            </div>
          </div>
        )}
      </div>
    </AppShell>
  );
}
