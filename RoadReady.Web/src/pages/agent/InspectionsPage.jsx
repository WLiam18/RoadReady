import { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import AppShell from '../../components/AppShell';
import { CardSkeleton } from '../../components/Skeleton';
import { resolveAssetUrl } from '../../lib/api';
import ApiV1 from '../../lib/apiV1';

function isSameDay(date1, date2) {
  const d1 = new Date(date1);
  const d2 = new Date(date2);
  return d1.getFullYear() === d2.getFullYear() &&
    d1.getMonth() === d2.getMonth() &&
    d1.getDate() === d2.getDate();
}

const IST_TZ = 'Asia/Kolkata';
const fmtIstTime = (iso) => iso ? new Date(iso).toLocaleString('en-IN', {
  timeZone: IST_TZ, hour: '2-digit', minute: '2-digit', hour12: true,
}) : '—';
const fmtIstDate = (iso) => iso ? new Date(iso).toLocaleDateString('en-IN', {
  timeZone: IST_TZ, day: '2-digit', month: 'short', year: 'numeric',
}) : '—';

export default function AgentInspectionsPage() {
  const navigate = useNavigate();
  const [bookings, setBookings] = useState([]);
  const [inspectionHistories, setInspectionHistories] = useState({});
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    ApiV1.getAllBookings()
      .then((res) => {
        if (res.data?.success) setBookings(res.data.data || []);
      })
      .catch(() => {})
      .finally(() => setLoading(false));
  }, []);

  const today = new Date();
  const checkOutBookings = bookings.filter(
    (b) => b.status === 'Confirmed' && isSameDay(b.pickupDate, today)
  );
  const checkInBookings = bookings.filter((b) => {
    if (b.status !== 'Active') return false;
    const dropoff = new Date(b.dropoffDate);
    const pickup = new Date(b.pickupDate);
    return isSameDay(dropoff, today) || dropoff < today || isSameDay(pickup, today);
  });

  const handleToggleHistory = async (bookingId) => {
    if (inspectionHistories[bookingId] !== undefined) {
      setInspectionHistories((p) => {
        const next = { ...p };
        delete next[bookingId];
        return next;
      });
      return;
    }
    setInspectionHistories((p) => ({ ...p, [bookingId]: 'loading' }));
    try {
      const res = await ApiV1.getBookingInspectionHistory(bookingId);
      if (res.data?.success) {
        setInspectionHistories((p) => ({ ...p, [bookingId]: res.data.data?.inspections || [] }));
      } else {
        setInspectionHistories((p) => ({ ...p, [bookingId]: [] }));
      }
    } catch {
      setInspectionHistories((p) => ({ ...p, [bookingId]: [] }));
    }
  };

  if (loading) {
    return (
      <AppShell>
        <div className="max-w-3xl mx-auto space-y-4">
          {[1, 2].map((i) => <CardSkeleton key={i} lines={2} />)}
        </div>
      </AppShell>
    );
  }

  const isEmpty = checkOutBookings.length === 0 && checkInBookings.length === 0;

  return (
    <AppShell>
      <div className="max-w-5xl mx-auto">
        <header className="flex items-center justify-between flex-wrap gap-3 mb-6">
          <div>
            <h1 className="text-2xl font-bold text-brand-ink">Today's inspections</h1>
            <p className="text-sm text-brand-muted">
              {new Date().toLocaleDateString('en-US', { weekday: 'long', month: 'long', day: 'numeric' })}
            </p>
          </div>
          <AgentTabs active="today" />
        </header>

        {isEmpty ? (
          <div className="card p-12 text-center">
            <h2 className="font-semibold mb-1">No inspections scheduled today</h2>
            <p className="text-sm text-brand-muted">
              Vehicles with pick-up or drop-off today will appear here.
            </p>
            <button
              onClick={async () => {
                const id = window.prompt('Enter booking ID to view inspection history:');
                if (id) {
                  setInspectionHistories((p) => ({ ...p, [id]: 'loading' }));
                  try {
                    const res = await ApiV1.getBookingInspectionHistory(Number(id));
                    if (res.data?.success) {
                      setInspectionHistories((p) => ({ ...p, [id]: res.data.data?.inspections || [] }));
                    }
                  } catch {/*ignore*/}
                }
              }}
              className="btn btn-outline btn-sm mt-4"
            >
              View past inspection history
            </button>
          </div>
        ) : (
          <div className="space-y-8">
            <BookingSection
              title="Check-out"
              bookings={checkOutBookings}
              navigate={navigate}
              inspectionHistories={inspectionHistories}
              onToggleHistory={handleToggleHistory}
              action="checkout"
            />
            <BookingSection
              title="Check-in / Return"
              bookings={checkInBookings}
              navigate={navigate}
              inspectionHistories={inspectionHistories}
              onToggleHistory={handleToggleHistory}
              action="checkin"
            />
          </div>
        )}
      </div>
    </AppShell>
  );
}

export function AgentTabs({ active }) {
  const baseCls = "px-3 py-1.5 rounded-lg text-sm font-medium transition";
  const inactive = "text-brand-muted hover:bg-gray-100";
  const activeCls = "bg-brand-ink text-white shadow-sm";
  return (
    <div className="inline-flex items-center gap-1 p-1 rounded-lg bg-gray-100">
      <Link to="/agent" className={`${baseCls} ${active === 'today' ? activeCls : inactive}`}>Today</Link>
      <Link to="/agent/completed" className={`${baseCls} ${active === 'completed' ? activeCls : inactive}`}>Completed</Link>
      <Link to="/agent/history" className={`${baseCls} ${active === 'history' ? activeCls : inactive}`}>All history</Link>
    </div>
  );
}

function BookingSection({ title, bookings, navigate, action, inspectionHistories, onToggleHistory }) {
  if (bookings.length === 0) return null;
  return (
    <div>
      <h2 className="font-semibold text-brand-ink mb-3 flex items-center gap-2">{title}</h2>
      <div className="space-y-4">
        {bookings.map((b) => {
          const historyState = inspectionHistories[b.id];
          const showHistory = Array.isArray(historyState);
          return (
            <div key={b.id} className="card p-5">
              <div className="flex items-center justify-between gap-4">
                <div className="flex items-center gap-4 min-w-0">
                  {b.carImageUrl && (
                    <img src={resolveAssetUrl(b.carImageUrl)} alt="" className="w-16 h-14 rounded-lg object-cover flex-shrink-0" />
                  )}
                  <div className="min-w-0">
                    <p className="font-bold text-brand-ink">{b.carMake} {b.carModel}</p>
                    <p className="text-xs text-brand-muted mt-1">
                      Booking #{b.id} &middot;{' '}
                      {fmtIstDate(b.pickupDate)} {fmtIstTime(b.pickupDate)} &rarr;{' '}
                      {fmtIstDate(b.dropoffDate)} {fmtIstTime(b.dropoffDate)}
                    </p>
                    <p className="text-xs text-brand-muted">{b.pickupLocation}</p>
                  </div>
                </div>
                <span className={`badge-${action === 'checkout' ? 'info' : 'success'} flex-shrink-0`}>
                  {action === 'checkout' ? 'Confirmed' : 'Active'}
                </span>
              </div>

              <div className="flex gap-2 mt-4 pt-3 border-t border-gray-100">
                <button
                  onClick={() => navigate(`/agent/inspections/${b.id}/${action}`)}
                  className={`btn btn-${action === 'checkout' ? 'primary' : 'outline'} btn-sm`}
                >
                  {action === 'checkout' ? 'Check-out vehicle' : 'Check-in vehicle'}
                </button>
                <button
                  onClick={() => onToggleHistory(b.id)}
                  className="btn btn-ghost btn-sm"
                >
                  {showHistory ? 'Hide' : (historyState === 'loading' ? 'Loading…' : 'Show')} inspection history
                </button>
              </div>

              {historyState === 'loading' && (
                <div className="mt-3 p-3 rounded-lg bg-gray-50 text-xs text-brand-muted">Loading inspection history…</div>
              )}

              {showHistory && historyState.length > 0 && (
                <BookingInspectionDetail inspections={historyState} />
              )}

              {showHistory && historyState.length === 0 && (
                <div className="mt-3 p-3 rounded-lg bg-gray-50 text-xs text-brand-muted">
                  No prior inspections recorded on this booking yet.
                </div>
              )}
            </div>
          );
        })}
      </div>
    </div>
  );
}

function BookingInspectionDetail({ inspections }) {
  return (
    <div className="mt-4 pt-4 border-t border-gray-100 space-y-4">
      <div className="flex items-center justify-between">
        <p className="font-medium text-brand-ink">
          {inspections.length} inspection{inspections.length !== 1 ? 's' : ''} recorded
        </p>
      </div>
      {inspections.map((insp) => (
        <div key={insp.id} className="border border-gray-100 rounded-lg p-4 bg-gray-50/40">
          <div className="flex items-center gap-2 mb-2 flex-wrap">
            <span className={`badge-${insp.type === 'CheckOut' ? 'info' : 'success'}`}>{insp.type}</span>
            <span className="text-xs text-brand-muted">{fmtIstDate(insp.createdAt)} {fmtIstTime(insp.createdAt)}</span>
            {insp.agentName && <span className="text-xs text-brand-muted">· by {insp.agentName}</span>}
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-3 text-xs">
            <div className="space-y-1 text-brand-muted">
              <p><strong>Customer:</strong> {insp.customerName || '—'}</p>
              <p><strong>Phone:</strong> {insp.customerPhone || '—'}</p>
              <p><strong>Email:</strong> {insp.customerEmail || '—'}</p>
              <p><strong>Vehicle:</strong> {insp.carMake} {insp.carModel} (booking #{insp.bookingId})</p>
              <p><strong>Odometer:</strong> {insp.odometerReading} km &middot; <strong>Fuel:</strong> {insp.fuelLevel}</p>
              {insp.notes && (
                <p className="mt-1 italic"><strong>Notes:</strong> {insp.notes}</p>
              )}
            </div>
            <div>
              <p className="text-brand-muted mb-1">
                Vehicle photos ({insp.vehicleImageUrls?.length || 0})
              </p>
              <div className="grid grid-cols-3 gap-2">
                {(insp.vehicleImageUrls || []).length > 0 ? (
                  (insp.vehicleImageUrls).map((u, i) => (
                    <a key={i} href={u} target="_blank" rel="noreferrer">
                      <img
                        src={resolveAssetUrl(u)}
                        alt={`Vehicle ${i + 1}`}
                        className="w-full h-20 object-cover rounded-md border"
                        loading="lazy"
                      />
                    </a>
                  ))
                ) : (
                  <p className="col-span-3 text-brand-muted italic">No vehicle photos recorded.</p>
                )}
              </div>
              {insp.type === 'CheckOut' && (insp.documentImageUrls?.length || 0) > 0 && (
                <>
                  <p className="text-brand-muted mb-1 mt-3">
                    Customer KYC ({insp.documentImageUrls.length})
                  </p>
                  <div className="grid grid-cols-3 gap-2">
                    {insp.documentImageUrls.map((u, i) => (
                      <a key={i} href={u} target="_blank" rel="noreferrer">
                        <img
                          src={resolveAssetUrl(u)}
                          alt={`KYC ${i + 1}`}
                          className="w-full h-20 object-cover rounded-md border"
                          loading="lazy"
                        />
                      </a>
                    ))}
                  </div>
                </>
              )}
            </div>
          </div>
        </div>
      ))}
    </div>
  );
}
