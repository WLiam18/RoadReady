import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { toast } from 'react-hot-toast';
import AppShell from '../../components/AppShell';
import { Field } from '../../components/FormControls';
import { WrenchIcon, CheckIcon, ClockIcon, MapPinIcon } from '../../components/icons';
import { CardSkeleton } from '../../components/Skeleton';
import ApiV1 from '../../lib/apiV1';
import { api } from '../../lib/api';

export default function AgentInspectionFormPage({ action = 'checkout' }) {
  const { bookingId } = useParams();
  const navigate = useNavigate();
  const isCheckout = action === 'checkout';

  const [bookings, setBookings] = useState([]);
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    ApiV1.getAllBookings()
      .then((res) => {
        if (res.data?.success) setBookings(res.data.data || []);
      })
      .catch(() => {})
      .finally(() => setLoading(false));
  }, []);

  const booking = bookings.find((b) => String(b.id) === String(bookingId));

  useEffect(() => {
    if (!loading && !booking) navigate('/agent');
  }, [loading, booking, navigate]);

  const [odometer, setOdometer] = useState('');
  const [fuelLevel, setFuelLevel] = useState('Full');
  const [notes, setNotes] = useState('');
  const [vehicleImages, setVehicleImages] = useState([]);
  const [documentImages, setDocumentImages] = useState([]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!odometer || odometer < 0) {
      toast.error('Valid odometer reading is required.');
      return;
    }

    setSubmitting(true);
    try {
      const fd = new FormData();
      fd.append('OdometerReading', odometer);
      fd.append('FuelLevel', fuelLevel);
      fd.append('Notes', notes || '');
      vehicleImages.forEach((f) => fd.append('VehicleImages', f));
      if (isCheckout) {
        documentImages.forEach((f) => fd.append('DocumentImages', f));
      }

      const url = `/api/v1/bookings/${bookingId}/inspections/${action}`;
      const res = await api.post(url, fd, {
        headers: { 'Content-Type': 'multipart/form-data' },
      });

      if (res.data?.success) {
        toast.success(isCheckout ? 'Check-out completed!' : 'Check-in completed!');
        navigate('/agent');
      } else if (res.data?.errors?.length) {
        const errs = res.data.errors.join(', ');
        toast.error(`Validation: ${errs}`);
      } else {
        toast.error(res.data?.message || 'Failed.');
      }
    } catch (err) {
      const data = err?.response?.data;
      const msg = data?.message || (data?.errors?.length ? data.errors.join(', ') : null) || err?.message || 'Failed.';
      toast.error(msg);
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) {
    return (
      <AppShell>
        <div className="max-w-2xl mx-auto">
          <CardSkeleton lines={5} />
        </div>
      </AppShell>
    );
  }

  if (!booking) return null;

  return (
    <AppShell>
      <div className="max-w-2xl mx-auto">
        <button
          onClick={() => navigate('/agent')}
          className="text-sm text-brand-muted hover:text-brand-ink mb-4"
        >
          &larr; Back to inspections
        </button>

        <div className="card p-6 mb-6">
          <div className="flex items-center gap-4">
            <div className="w-14 h-14 rounded-xl bg-gray-50 flex items-center justify-center flex-shrink-0">
              {isCheckout ? (
                <WrenchIcon className="w-7 h-7 text-brand-ink" />
              ) : (
                <CheckIcon className="w-7 h-7 text-emerald-600" />
              )}
            </div>
            <div>
              <h1 className="text-xl font-bold text-brand-ink">
                {isCheckout ? 'Vehicle Check-out' : 'Vehicle Check-in'}
              </h1>
              <p className="text-sm text-brand-muted">
                Booking #{booking.id} · {booking.carMake} {booking.carModel}
              </p>
            </div>
          </div>

          <div className="grid grid-cols-2 gap-3 mt-4 pt-4 border-t border-gray-100 text-sm">
            <div>
              <p className="text-xs text-brand-muted uppercase tracking-wide">Pick-up</p>
              <p className="font-medium flex items-center gap-1 mt-0.5">
                <ClockIcon className="w-3.5 h-3.5" /> {new Date(booking.pickupDate).toLocaleString()}
              </p>
            </div>
            <div>
              <p className="text-xs text-brand-muted uppercase tracking-wide">Drop-off</p>
              <p className="font-medium flex items-center gap-1 mt-0.5">
                <ClockIcon className="w-3.5 h-3.5" /> {new Date(booking.dropoffDate).toLocaleString()}
              </p>
            </div>
            <div className="col-span-2">
              <p className="text-xs text-brand-muted uppercase tracking-wide">Pick-up location</p>
              <p className="font-medium flex items-center gap-1 mt-0.5">
                <MapPinIcon className="w-3.5 h-3.5" /> {booking.pickupLocation}
              </p>
            </div>
          </div>

          {isCheckout && (
            <div className="mt-4 p-3 bg-amber-50 rounded-lg text-xs text-amber-700">
              Check-out is allowed up to 2 hours before the pick-up time. The booking
              status will change from <strong>Confirmed</strong> to <strong>Active</strong>.
            </div>
          )}
          {!isCheckout && (
            <div className="mt-4 p-3 bg-emerald-50 rounded-lg text-xs text-emerald-700">
              Check-in completes the rental. The booking status will change from
              <strong> Active</strong> to <strong>Completed</strong>.
            </div>
          )}
        </div>

        <form onSubmit={handleSubmit} className="card p-6 space-y-5">
          <h2 className="font-semibold text-brand-ink">Inspection details</h2>

          <div className="grid grid-cols-2 gap-4">
            <Field label="Odometer reading (km)" required>
              <input
                className="input"
                type="number"
                min="0"
                value={odometer}
                onChange={(e) => setOdometer(e.target.value)}
                placeholder="e.g. 15420"
                required
              />
            </Field>
            <Field label="Fuel level" required>
              <select
                className="input"
                value={fuelLevel}
                onChange={(e) => setFuelLevel(e.target.value)}
              >
                <option>Empty</option>
                <option>1/4</option>
                <option>1/2</option>
                <option>3/4</option>
                <option>Full</option>
              </select>
            </Field>
          </div>

          <Field label="Inspection notes" hint="Document any pre-existing damage, scratches, or issues">
            <textarea
              className="input resize-none"
              rows={3}
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
              placeholder="e.g. Small scratch on front bumper, tyre condition good..."
              maxLength={1000}
            />
          </Field>

          <Field label="Vehicle photos" hint="Take photos of all sides of the vehicle">
            <input
              type="file"
              accept="image/*"
              multiple
              className="text-sm"
              onChange={(e) => setVehicleImages(Array.from(e.target.files || []))}
            />
            {vehicleImages.length > 0 && (
              <p className="text-xs text-brand-muted mt-1">
                {vehicleImages.length} photo{vehicleImages.length !== 1 ? 's' : ''} selected
              </p>
            )}
          </Field>

          {isCheckout && (
            <Field label="Customer KYC documents" hint="Upload driver's license and ID photos">
              <input
                type="file"
                accept="image/*,application/pdf"
                multiple
                className="text-sm"
                onChange={(e) => setDocumentImages(Array.from(e.target.files || []))}
              />
              {documentImages.length > 0 && (
                <p className="text-xs text-brand-muted mt-1">
                  {documentImages.length} document{documentImages.length !== 1 ? 's' : ''} selected
                </p>
              )}
            </Field>
          )}

          <div className="flex gap-3 pt-2">
            <button type="submit" className="btn btn-primary" disabled={submitting}>
              {submitting ? 'Processing…' : isCheckout ? 'Complete check-out' : 'Complete check-in'}
            </button>
            <button type="button" onClick={() => navigate('/agent')} className="btn btn-outline">
              Cancel
            </button>
          </div>
        </form>
      </div>
    </AppShell>
  );
}
