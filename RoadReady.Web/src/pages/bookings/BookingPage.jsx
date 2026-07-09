import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { toast } from 'react-hot-toast';
import AppShell from '../../components/AppShell';
import { CalendarIcon } from '../../components/icons';
import { Field, TextInput } from '../../components/FormControls';
import ApiV1 from '../../lib/apiV1';

export default function BookingPage() {
  const { id } = useParams();
  const navigate = useNavigate();

  const [car, setCar] = useState(null);
  const [loadingCar, setLoadingCar] = useState(true);

  // Form
  const [location, setLocation] = useState('');
  const [pickupDate, setPickupDate] = useState('');
  const [dropoffDate, setDropoffDate] = useState('');
  const [pickupTime, setPickupTime] = useState('10:00');
  const [dropoffTime, setDropoffTime] = useState('10:00');
  const [carSeat, setCarSeat] = useState(false);
  const [promoCode, setPromoCode] = useState('');
  const [promoResult, setPromoResult] = useState(null);
  const [validatingPromo, setValidatingPromo] = useState(false);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    ApiV1.getCarById(id)
      .then((res) => {
        if (res.data?.success) {
          setCar(res.data.data);
          setLocation(res.data.data.location || '');
        } else {
          toast.error('Car not found');
          navigate('/cars');
        }
      })
      .catch(() => {
        toast.error('Car not found');
        navigate('/cars');
      })
      .finally(() => setLoadingCar(false));
  }, [id, navigate]);

  const calcDays = () => {
    if (!pickupDate || !dropoffDate) return 0;
    const p = new Date(pickupDate);
    const d = new Date(dropoffDate);
    const diff = Math.ceil((d - p) / (1000 * 60 * 60 * 24));
    return Math.max(diff, 0);
  };

  const days = calcDays();
  const basePrice = car ? car.pricePerDay * days : 0;
  const seatCharge = carSeat ? 200 * days : 0;
  const subtotal = basePrice + seatCharge;
  const discountAmount = (promoResult?.valid && promoResult.discountAmount) ? promoResult.discountAmount : 0;
  const finalTotal = Math.max(0, subtotal - discountAmount);

  const handleValidatePromo = async () => {
    if (!promoCode.trim()) return;
    setValidatingPromo(true);
    try {
      const res = await ApiV1.validatePromoCode({ code: promoCode.trim(), bookingAmount: subtotal });
      if (res.data?.success && res.data.data?.isValid) {
        const data = res.data.data;
        setPromoResult({ valid: true, code: data.code, discountAmount: data.discountAmount, finalAmount: data.finalAmount, message: data.message });
        toast.success(data.message || 'Promo code applied!');
      } else {
        const msg = res.data?.data?.message || 'Promo code is invalid.';
        setPromoResult({ valid: false, message: msg });
        toast.error(msg);
      }
    } catch (err) {
      setPromoResult({ valid: false, message: 'Validation failed.' });
      toast.error(err?.message || 'Promo validation failed.');
    } finally {
      setValidatingPromo(false);
    }
  };

  const handleSubmit = async () => {
    if (!pickupDate || !dropoffDate) return toast.error('Please select pick-up and drop-off dates.');
    if (days < 1) return toast.error('Drop-off must be after pick-up.');
    if (!location.trim()) return toast.error('Please enter a pick-up location.');

    setSubmitting(true);
    try {
      // IST is UTC+5:30. When the user enters a date+time in the form, we
      // treat them as IST wall-clock and convert to the corresponding UTC
      // instant. The browser's TZ must NOT influence the stored date.
      // Compute the UTC ms by subtracting IST's offset (5h30m) from the
      // picked parts read as if they were UTC.
      const IST_OFFSET_MINUTES = 330; // IST = UTC + 5h 30m
      const buildIstAsUtcIso = (dateStr, timeStr) => {
        const [y, m, d] = dateStr.split('-').map(Number);
        const [hh, mm] = (timeStr || '00:00').split(':').map(Number);
        if ([y, m, d, hh, mm].some(Number.isNaN)) {
          throw new Error('Invalid date or time selected.');
        }
        const utcMs = Date.UTC(y, m - 1, d, hh, mm, 0) - IST_OFFSET_MINUTES * 60_000;
        return new Date(utcMs).toISOString();
      };

      const body = {
        carId: Number(id),
        pickupDate: buildIstAsUtcIso(pickupDate, pickupTime),
        dropoffDate: buildIstAsUtcIso(dropoffDate, dropoffTime),
        pickupTime,
        dropoffTime,
        pickupLocation: location.trim(),
        includesCarSeat: carSeat,
        promoCode: promoResult?.valid ? promoResult.code : undefined,
      };
      const res = await ApiV1.createBooking(body);
      if (res.data?.success && res.data.data) {
        const booking = res.data.data;
        if (booking.paymentUrl) {
          window.open(booking.paymentUrl, '_blank');
        }
        toast.success('Booking created! Complete payment to confirm.');
        if (booking.id) {
          navigate(`/my-bookings/${booking.id}`, { replace: true });
          return;
        }
      } else {
        toast.error(res.data?.message || 'Booking failed.');
      }
    } catch (err) {
      const serverMsg = err?.response?.data?.message || err?.response?.data?.error?.description;
      toast.error(serverMsg || err?.message || 'Something went wrong.');
    } finally {
      setSubmitting(false);
    }
  };

  if (loadingCar) {
    return <AppShell><div className="max-w-3xl mx-auto card p-8 animate-pulse"><div className="h-8 bg-gray-200 rounded w-1/3 mb-4" /><div className="h-40 bg-gray-200 rounded" /></div></AppShell>;
  }

  if (!car) return null;

  return (
    <AppShell>
      <div className="max-w-3xl mx-auto">
        <button onClick={() => navigate(-1)} className="text-sm text-brand-muted hover:text-brand-ink mb-4">&larr; Back</button>

        <div className="grid grid-cols-1 md:grid-cols-5 gap-6">
          {/* Form */}
          <div className="md:col-span-3 space-y-4">
            <div className="card p-6">
              <h2 className="font-semibold text-lg mb-4">Booking details</h2>

              <Field label="Pick-up location" required>
                <TextInput value={location} onChange={(e) => setLocation(e.target.value)} placeholder="Airport, city center, …" />
              </Field>

              <div className="grid grid-cols-2 gap-4">
                <Field label="Pick-up date" required>
                  <input type="date" className="input" value={pickupDate} onChange={(e) => setPickupDate(e.target.value)}
                    min={new Date().toISOString().split('T')[0]} />
                </Field>
                <Field label="Pick-up time" required>
                  <input type="time" className="input" value={pickupTime} onChange={(e) => setPickupTime(e.target.value)} />
                </Field>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <Field label="Drop-off date" required>
                  <input type="date" className="input" value={dropoffDate} onChange={(e) => setDropoffDate(e.target.value)}
                    min={pickupDate || new Date().toISOString().split('T')[0]} />
                </Field>
                <Field label="Drop-off time" required>
                  <input type="time" className="input" value={dropoffTime} onChange={(e) => setDropoffTime(e.target.value)} />
                </Field>
              </div>

              {/* Car seat */}
              <label className="flex items-center gap-3 py-2 cursor-pointer">
                <input type="checkbox" checked={carSeat} onChange={() => setCarSeat(!carSeat)} className="w-5 h-5 rounded border-gray-300 text-brand-ink focus:ring-brand-ink" />
                <div>
                  <span className="font-medium text-sm">Add child car seat</span>
                  <span className="text-brand-muted text-xs ml-2">₹200 / day</span>
                </div>
              </label>

              {/* Promo code */}
              <div className="pt-3 border-t border-brand-divider">
                <label className="label">Promo code</label>
                <div className="flex gap-2">
                  <input className="input flex-1" placeholder="Have a code?" value={promoCode}
                    onChange={(e) => { setPromoCode(e.target.value); setPromoResult(null); }} />
                  <button onClick={handleValidatePromo} className="btn btn-outline" disabled={validatingPromo || !promoCode.trim()}>
                    {validatingPromo ? '…' : 'Apply'}
                  </button>
                </div>
                {promoResult && (
                  <p className={`text-xs mt-1 ${promoResult.valid ? 'text-brand-success' : 'text-brand-danger'}`}>
                    {promoResult.valid ? `You save ₹${promoResult.discountAmount.toLocaleString()}!` : promoResult.message}
                  </p>
                )}
              </div>
            </div>
          </div>

          {/* Summary */}
          <div className="md:col-span-2">
            <div className="card p-6 sticky top-24">
              <div className="flex items-center gap-3 mb-4 pb-4 border-b border-brand-divider">
                {car.imageUrls?.[0] && (
                  <img src={car.imageUrls[0]} alt="" className="w-16 h-16 rounded-lg object-cover" />
                )}
                <div>
                  <p className="font-semibold text-brand-ink">{car.make} {car.model}</p>
                  <p className="text-sm text-brand-muted">{car.year}</p>
                </div>
              </div>

              <div className="space-y-2 text-sm">
                <div className="flex justify-between">
                  <span className="text-brand-muted">Daily rate</span>
                  <span>₹{car.pricePerDay.toLocaleString()}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-brand-muted">Days</span>
                  <span>{days || '—'}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-brand-muted">Subtotal</span>
                  <span>₹{subtotal.toLocaleString()}</span>
                </div>
                {carSeat && (
                  <div className="flex justify-between text-xs text-brand-muted">
                    <span>Child seat surcharge</span>
                    <span>₹{seatCharge.toLocaleString()}</span>
                  </div>
                )}
                {promoResult?.valid && (
                  <div className="flex justify-between text-brand-success font-medium">
                    <span>Discount</span>
                    <span>-₹{discountAmount.toLocaleString()}</span>
                  </div>
                )}
                <div className="flex justify-between font-bold text-lg pt-3 border-t border-brand-divider">
                  <span>Total</span>
                  <span>₹{finalTotal.toLocaleString()}</span>
                </div>
              </div>

              <button
                onClick={handleSubmit}
                disabled={submitting || days < 1}
                className="btn btn-primary w-full mt-6"
              >
                {submitting ? 'Creating…' : 'Confirm booking'}
              </button>

              <p className="text-xs text-brand-muted text-center mt-3">
                You will be redirected to Razorpay to complete payment.
              </p>
            </div>
          </div>
        </div>
      </div>
    </AppShell>
  );
}