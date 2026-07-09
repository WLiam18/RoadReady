import { useState, useEffect } from 'react';
import AppShell from '../../components/AppShell';
import { CardSkeleton } from '../../components/Skeleton';
import { resolveAssetUrl } from '../../lib/api';
import ApiV1 from '../../lib/apiV1';

const IST_TZ = 'Asia/Kolkata';
const fmtIst = (iso) => iso ? new Date(iso).toLocaleString('en-IN', {
  timeZone: IST_TZ, day: '2-digit', month: 'short', year: 'numeric',
  hour: '2-digit', minute: '2-digit', hour12: true,
}) : '—';

function InspectionPanel({ heading, inspection }) {
  if (!inspection) {
    return (
      <div className="card p-5 border-dashed text-center bg-gray-50">
        <p className="text-sm text-brand-muted">No {heading.toLowerCase()} recorded for this booking.</p>
      </div>
    );
  }

  return (
    <div className="card p-5">
      <div className="flex items-center justify-between mb-3">
        <h3 className="font-semibold text-brand-ink flex items-center gap-2">
          <span className={`badge-${inspection.type === 'CheckOut' ? 'info' : 'success'}`}>
            {inspection.type}
          </span>
          {heading}
        </h3>
        <span className="text-xs text-brand-muted">{fmtIst(inspection.createdAt)}</span>
      </div>

      <div className="grid grid-cols-2 md:grid-cols-4 gap-3 text-xs mb-4">
        <div>
          <p className="text-brand-muted">Customer</p>
          <p className="font-medium">{inspection.customerName || '—'}</p>
        </div>
        <div>
          <p className="text-brand-muted">Phone</p>
          <p className="font-medium">{inspection.customerPhone || '—'}</p>
        </div>
        <div>
          <p className="text-brand-muted">Vehicle</p>
          <p className="font-medium">{inspection.carMake} {inspection.carModel}</p>
        </div>
        <div>
          <p className="text-brand-muted">Agent</p>
          <p className="font-medium">{inspection.agentName || '—'}</p>
        </div>
        <div>
          <p className="text-brand-muted">Odometer</p>
          <p className="font-medium">{inspection.odometerReading} km</p>
        </div>
        <div>
          <p className="text-brand-muted">Fuel level</p>
          <p className="font-medium">{inspection.fuelLevel}</p>
        </div>
      </div>

      {inspection.notes && (
        <p className="text-xs italic text-brand-muted mb-3">
          <strong>Notes:</strong> {inspection.notes}
        </p>
      )}

      <p className="text-xs text-brand-muted mb-1">
        Vehicle photos ({inspection.vehicleImageUrls?.length || 0})
      </p>
      <div className="grid grid-cols-3 md:grid-cols-4 gap-2">
        {(inspection.vehicleImageUrls || []).length > 0 ? (
          inspection.vehicleImageUrls.map((u, i) => (
            <a key={i} href={u} target="_blank" rel="noreferrer">
              <img
                src={resolveAssetUrl(u)}
                alt={`${heading} vehicle ${i + 1}`}
                className="w-full h-20 object-cover rounded-md border"
                loading="lazy"
              />
            </a>
          ))
        ) : (
          <p className="col-span-3 text-xs text-brand-muted italic">No vehicle photos.</p>
        )}
      </div>

      {inspection.type === 'CheckOut' && (inspection.documentImageUrls?.length || 0) > 0 && (
        <>
          <p className="text-xs text-brand-muted mb-1 mt-3">
            Customer KYC ({inspection.documentImageUrls.length})
          </p>
          <div className="grid grid-cols-3 md:grid-cols-4 gap-2">
            {inspection.documentImageUrls.map((u, i) => (
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

      {inspection.type === 'CheckIn' && (
        <p className="text-xs text-brand-muted mt-3">
          <strong>Tip:</strong> Use these "after-return" photos to compare against the check-out photos
          for any new damage claim.
        </p>
      )}
    </div>
  );
}

export default function AgentCompletedPage() {
  const [records, setRecords] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    ApiV1.getAgentCompletedToday()
      .then((res) => {
        if (res.data?.success) setRecords(res.data.data || []);
      })
      .catch(() => {})
      .finally(() => setLoading(false));
  }, []);

  return (
    <AppShell>
      <div className="max-w-5xl mx-auto">
        <h1 className="text-2xl font-bold text-brand-ink mb-2">Completed inspections today</h1>
        <p className="text-sm text-brand-muted mb-6">
          Booking returns handled today — paired check-out and check-in records.
        </p>

        {loading ? (
          <div className="space-y-4">
            {[1, 2].map((i) => <CardSkeleton key={i} lines={4} />)}
          </div>
        ) : records.length === 0 ? (
          <div className="card p-12 text-center">
            <h2 className="font-semibold mb-1">No completed inspections today</h2>
            <p className="text-sm text-brand-muted">
              When you finish a check-in, the booking will appear here with paired photos.
            </p>
          </div>
        ) : (
          <div className="space-y-8">
            {records.map((record) => (
              <div key={record.booking.id} className="card p-6">
                <div className="flex items-center justify-between mb-5">
                  <div className="flex items-center gap-3">
                    {record.carImageUrl && (
                      <img
                        src={resolveAssetUrl(record.carImageUrl)}
                        alt=""
                        className="w-16 h-14 rounded-lg object-cover"
                      />
                    )}
                    <div>
                      <p className="font-bold text-brand-ink">
                        {record.carMake} {record.carModel}
                      </p>
                      <p className="text-xs text-brand-muted">
                        Booking #{record.booking.id} &middot;{' '}
                        {record.customerName || record.customerEmail}
                      </p>
                    </div>
                  </div>
                  <span className="badge-success">Completed</span>
                </div>

                <div className="grid grid-cols-1 lg:grid-cols-2 gap-5">
                  <InspectionPanel heading="Check-out (pickup)" inspection={record.checkOutInspection} />
                  <InspectionPanel heading="Check-in (return)" inspection={record.checkInInspection} />
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </AppShell>
  );
}
