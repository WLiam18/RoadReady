import { useState, useEffect } from 'react';
import AppShell from '../../components/AppShell';
import { BarChart3Icon, CarIcon, TicketIcon, UserCircleIcon, CreditCardIcon } from '../../components/icons';
import ApiV1 from '../../lib/apiV1';

export default function AdminDashboardPage() {
  const [stats, setStats] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    ApiV1.getAdminAnalytics()
      .then((res) => {
        if (res.data?.success && res.data.data) {
          setStats(res.data.data);
        }
      })
      .catch(() => {})
      .finally(() => setLoading(false));
  }, []);

  return (
    <AppShell>
      <div>
        <h1 className="text-2xl font-bold text-brand-ink mb-6">Dashboard</h1>
        {loading ? (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
            {[1,2,3,4].map((i) => <div key={i} className="card h-32 shimmer" />)}
          </div>
        ) : stats ? (
          <>
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
              <StatCard icon={TicketIcon} label="Total reservations" value={stats.totalReservations} color="text-brand-ink" />
              <StatCard icon={CarIcon} label="Active bookings" value={stats.activeBookings} color="text-emerald-600" />
              <StatCard icon={UserCircleIcon} label="Cancelled" value={stats.cancelledBookings} color="text-red-600" />
              <StatCard icon={CreditCardIcon} label="Net revenue" value={`₹${(stats.netRevenue || 0).toLocaleString()}`} color="text-brand-ink" />
            </div>
            {stats.totalRefunded > 0 && (
              <div className="card p-5 text-sm text-brand-muted">
                Total refunded: <span className="font-medium text-brand-ink">₹{stats.totalRefunded.toLocaleString()}</span>
              </div>
            )}
          </>
        ) : (
          <div className="card p-10 text-center text-brand-muted">
            <BarChart3Icon className="w-12 h-12 mx-auto mb-3 text-gray-200" />
            <p>Could not load analytics. Make sure the backend is running.</p>
          </div>
        )}
      </div>
    </AppShell>
  );
}

function StatCard({ icon: Icon, label, value, color }) {
  return (
    <div className="card p-5">
      <div className="flex items-center gap-3 mb-3">
        <div className="w-10 h-10 rounded-lg bg-gray-50 flex items-center justify-center">
          <Icon className={`w-5 h-5 ${color}`} />
        </div>
        <p className="text-sm text-brand-muted">{label}</p>
      </div>
      <p className={`text-2xl font-bold ${color}`}>{value ?? '—'}</p>
    </div>
  );
}
