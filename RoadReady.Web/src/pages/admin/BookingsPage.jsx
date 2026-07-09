import { useState, useEffect } from 'react';
import { toast } from 'react-hot-toast';
import AppShell from '../../components/AppShell';
import { TableSkeleton } from '../../components/Skeleton';
import ApiV1 from '../../lib/apiV1';

const STS = { PendingPayment: 'badge-warning', Confirmed: 'badge-info', Active: 'badge-success', Completed: 'badge-success', Cancelled: 'badge-danger', Modified: 'badge-neutral' };

export default function AdminBookingsPage() {
  const [bookings, setBookings] = useState([]);
  const [loading, setLoading] = useState(true);

  const fetchBookings = () => {
    setLoading(true);
    ApiV1.getAllBookings()
      .then((res) => {
        if (res.data?.success) setBookings(res.data.data || []);
      })
      .catch(() => toast.error('Failed to load bookings.'))
      .finally(() => setLoading(false));
  };

  useEffect(() => { fetchBookings(); }, []);

  const handleCancel = async (id) => {
    if (!window.confirm('Cancel this booking?')) return;
    try {
      const res = await ApiV1.cancelBooking(id);
      if (res.data?.success) {
        toast.success('Booking cancelled.');
        fetchBookings();
      } else {
        toast.error(res.data?.message || 'Cancellation failed.');
      }
    } catch (err) {
      toast.error(err?.response?.data?.message || 'Failed to cancel.');
    }
  };

  return (
    <AppShell>
      <div className="max-w-5xl mx-auto">
        <h1 className="text-2xl font-bold text-brand-ink mb-6">All bookings</h1>
        {loading ? <TableSkeleton rows={6} /> : (
          <div className="card overflow-hidden">
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead><tr className="border-b border-gray-100">
                  <th className="text-left px-5 py-3 font-medium text-brand-muted">Booking</th>
                  <th className="text-left px-5 py-3 font-medium text-brand-muted hidden sm:table-cell">Vehicle</th>
                  <th className="text-left px-5 py-3 font-medium text-brand-muted hidden md:table-cell">Customer</th>
                  <th className="text-right px-5 py-3 font-medium text-brand-muted">Total</th>
                  <th className="text-center px-5 py-3 font-medium text-brand-muted">Status</th>
                  <th className="text-right px-5 py-3 font-medium text-brand-muted">Actions</th>
                </tr></thead>
                <tbody className="divide-y divide-gray-50">
                  {bookings.length === 0 ? (
                    <tr><td colSpan={6} className="p-8 text-center text-brand-muted">No bookings yet.</td></tr>
                  ) : bookings.map(b => (
                    <tr key={b.id} className="hover:bg-gray-50/50 transition-colors">
                      <td className="px-5 py-3"><p className="font-medium">#{b.id}</p><p className="text-xs text-brand-muted">{new Date(b.createdAt).toLocaleDateString()}</p></td>
                      <td className="px-5 py-3 hidden sm:table-cell">{b.carMake} {b.carModel}</td>
                      <td className="px-5 py-3 text-brand-muted hidden md:table-cell">{b.userId?.slice(0, 8)}…</td>
                      <td className="px-5 py-3 text-right font-medium">₹{(b.totalAmount || 0).toLocaleString()}</td>
                      <td className="px-5 py-3 text-center"><span className={STS[b.status] || 'badge-neutral'}>{b.status}</span></td>
                      <td className="px-5 py-3 text-right">
                        {!['Cancelled', 'Completed'].includes(b.status) && (
                          <button onClick={() => handleCancel(b.id)} className="text-xs text-red-500 hover:underline">Cancel</button>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        )}
      </div>
    </AppShell>
  );
}
