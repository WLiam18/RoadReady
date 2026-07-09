import { useState, useEffect, useCallback } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import AppShell from '../../components/AppShell';
import { CardSkeleton } from '../../components/Skeleton';
import { StarIcon, MapPinIcon, ChevronDownIcon, CalendarIcon } from '../../components/icons';
import { resolveAssetUrl } from '../../lib/api';
import ApiV1 from '../../lib/apiV1';

const FUEL_TYPES = ['Petrol', 'Diesel', 'Electric', 'Hybrid'];
const TRANSMISSIONS = ['Automatic', 'Manual'];
const SEATING_OPTIONS = [2, 5, 7, 8];

const DATETIME_IST = new Intl.DateTimeFormat('en-IN', { day: '2-digit', month: 'short', timeZone: 'Asia/Kolkata' });
function fmtRange(p, d) {
  if (!p || !d) return '';
  return `${DATETIME_IST.format(new Date(p))} → ${DATETIME_IST.format(new Date(d))}`;
}

export default function CarBrowsePage() {
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();

  const [cars, setCars] = useState([]);
  const [totalCount, setTotalCount] = useState(0);
  const [brands, setBrands] = useState([]);
  const [loading, setLoading] = useState(true);

  const [page, setPage] = useState(Number(searchParams.get('page')) || 1);
  const [showFilters, setShowFilters] = useState(false);

  // Filters from URL
  const [location, setLocation] = useState(searchParams.get('location') || '');
  const [make, setMake] = useState(searchParams.get('make') || '');
  const [model, setModel] = useState(searchParams.get('model') || '');
  const [minPrice, setMinPrice] = useState(searchParams.get('minPrice') || '');
  const [maxPrice, setMaxPrice] = useState(searchParams.get('maxPrice') || '');
  const [transmission, setTransmission] = useState(searchParams.get('transmission') || '');
  const [fuelType, setFuelType] = useState(searchParams.get('fuelType') || '');
  const [seating, setSeating] = useState(searchParams.get('seatingCapacity') || '');
  // Date-range filter (matches `PickupDate` / `DropoffDate` on CarSearchRequestDto).
  const [pickupDate, setPickupDate] = useState(searchParams.get('pickupDate') || '');
  const [dropoffDate, setDropoffDate] = useState(searchParams.get('dropoffDate') || '');

  const pageSize = 12;

  const fetchCars = useCallback(async () => {
    setLoading(true);
    try {
      const params = { page, pageSize };
      if (location) params.location = location;
      if (make) params.make = make;
      if (model) params.model = model;
      if (minPrice) params.minPrice = Number(minPrice);
      if (maxPrice) params.maxPrice = Number(maxPrice);
      if (transmission) params.transmission = transmission;
      if (fuelType) params.fuelType = fuelType;
      if (seating) params.seatingCapacity = Number(seating);
      // CarService requires BOTH pickupDate and dropoffDate together.
      if (pickupDate && dropoffDate) {
        params.pickupDate = pickupDate;
        params.dropoffDate = dropoffDate;
      }

      const res = await ApiV1.searchCars(params);
      if (res.data?.success) {
        setCars(res.data.data || []);
        setTotalCount(res.data.totalCount || 0);
      }
    } catch (err) {
      setCars([]);
      setTotalCount(0);
    } finally {
      setLoading(false);
    }
  }, [page, location, make, model, minPrice, maxPrice, transmission, fuelType, seating, pickupDate, dropoffDate]);

  useEffect(() => {
    fetchCars();
  }, [fetchCars]);

  useEffect(() => {
    ApiV1.getBrands()
      .then((res) => {
        if (res.data?.success) setBrands(res.data.data || []);
      })
      .catch(() => {});
  }, []);

  useEffect(() => {
    const p = {};
    if (page > 1) p.page = page;
    if (location) p.location = location;
    if (make) p.make = make;
    if (model) p.model = model;
    if (minPrice) p.minPrice = minPrice;
    if (maxPrice) p.maxPrice = maxPrice;
    if (transmission) p.transmission = transmission;
    if (fuelType) p.fuelType = fuelType;
    if (seating) p.seatingCapacity = seating;
    if (pickupDate) p.pickupDate = pickupDate;
    if (dropoffDate) p.dropoffDate = dropoffDate;
    setSearchParams(p, { replace: true });
  }, [page, location, make, model, minPrice, maxPrice, transmission, fuelType, seating, pickupDate, dropoffDate, setSearchParams]);

  const resetFilters = () => {
    setLocation(''); setMake(''); setModel('');
    setMinPrice(''); setMaxPrice('');
    setTransmission(''); setFuelType(''); setSeating('');
    setPickupDate(''); setDropoffDate('');
    setPage(1);
  };

  const totalPages = Math.ceil(totalCount / pageSize);

  return (
    <AppShell>
      <div className="flex gap-6">
        {/* Sidebar filters - desktop */}
        <aside className="hidden lg:block w-64 flex-shrink-0">
          <div className="card p-5 sticky top-24 space-y-5">
            <div className="flex items-center justify-between">
              <h3 className="font-semibold text-brand-ink">Filters</h3>
              <button onClick={resetFilters} className="text-xs text-brand-muted hover:text-brand-ink">Reset all</button>
            </div>

            {/* Location */}
            <div>
              <label className="label">
                <MapPinIcon className="w-4 h-4 inline mr-1" /> Location
              </label>
              <input className="input" placeholder="City…" value={location} onChange={(e) => { setLocation(e.target.value); setPage(1); }} />
            </div>

            {/* Make / Brand */}
            <div>
              <label className="label">Make</label>
              <select className="input" value={make} onChange={(e) => { setMake(e.target.value); setPage(1); }}>
                <option value="">All makes</option>
                {brands.map((b) => <option key={b.id} value={b.name}>{b.name}</option>)}
              </select>
            </div>

            {/* Model */}
            <div>
              <label className="label">Model</label>
              <input className="input" placeholder="e.g. Civic" value={model} onChange={(e) => { setModel(e.target.value); setPage(1); }} />
            </div>

            {/* Price */}
            <div>
              <label className="label">Price per day</label>
              <div className="flex gap-2">
                <input className="input w-1/2" placeholder="Min" type="number" min="0" value={minPrice} onChange={(e) => { setMinPrice(e.target.value); setPage(1); }} />
                <input className="input w-1/2" placeholder="Max" type="number" min="0" value={maxPrice} onChange={(e) => { setMaxPrice(e.target.value); setPage(1); }} />
              </div>
            </div>

            {/* Trip dates — pre-fills the booking calendar and excludes cars
                already booked in that window. Both fields are required (the
                backend treats a half-set query as no date filter). */}
            <div>
              <label className="label">
                <CalendarIcon className="w-4 h-4 inline mr-1" /> Trip dates
              </label>
              <div className="space-y-2">
                <input
                  type="date"
                  className="input"
                  placeholder="Pick-up date"
                  value={pickupDate}
                  min={new Date().toISOString().slice(0, 10)}
                  onChange={(e) => {
                    setPickupDate(e.target.value);
                    setPage(1);
                    if (dropoffDate && e.target.value && e.target.value > dropoffDate) {
                      setDropoffDate(e.target.value);
                    }
                  }}
                />
                <input
                  type="date"
                  className="input"
                  placeholder="Drop-off date"
                  value={dropoffDate}
                  min={pickupDate || undefined}
                  onChange={(e) => { setDropoffDate(e.target.value); setPage(1); }}
                />
                {(pickupDate || dropoffDate) && (pickupDate === '' || dropoffDate === '') && (
                  <p className="text-[11px] text-brand-muted">Pick a drop-off date to enable same-window availability.</p>
                )}
              </div>
            </div>

            {/* Transmission */}
            <div>
              <label className="label">Transmission</label>
              <div className="flex flex-wrap gap-2">
                {TRANSMISSIONS.map((t) => (
                  <button
                    key={t}
                    onClick={() => { setTransmission(transmission === t ? '' : t); setPage(1); }}
                    className={`px-3 py-1.5 rounded-lg text-xs font-semibold transition ${
                      transmission === t ? 'bg-brand-ink text-white shadow-sm' : 'bg-white border border-brand-divider text-brand-ink hover:bg-gray-50'
                    }`}
                  >
                    {t}
                  </button>
                ))}
              </div>
            </div>

            {/* Fuel */}
            <div>
              <label className="label">Fuel type</label>
              <div className="flex flex-wrap gap-2">
                {FUEL_TYPES.map((f) => (
                  <button
                    key={f}
                    onClick={() => { setFuelType(fuelType === f ? '' : f); setPage(1); }}
                    className={`px-3 py-1.5 rounded-lg text-xs font-semibold transition ${
                      fuelType === f ? 'bg-brand-ink text-white shadow-sm' : 'bg-white border border-brand-divider text-brand-ink hover:bg-gray-50'
                    }`}
                  >
                    {f}
                  </button>
                ))}
              </div>
            </div>

            {/* Seating */}
            <div>
              <label className="label">Seats</label>
              <div className="flex flex-wrap gap-2">
                {SEATING_OPTIONS.map((s) => (
                  <button
                    key={s}
                    onClick={() => { setSeating(seating === String(s) ? '' : String(s)); setPage(1); }}
                    className={`px-3 py-1.5 rounded-lg text-xs font-semibold transition ${
                      seating === String(s) ? 'bg-brand-ink text-white shadow-sm' : 'bg-white border border-brand-divider text-brand-ink hover:bg-gray-50'
                    }`}
                  >
                    {s}+ seats
                  </button>
                ))}
              </div>
            </div>
          </div>
        </aside>

        {/* Mobile filter toggle */}
        <div className="lg:hidden w-full mb-3">
          <button onClick={() => setShowFilters(!showFilters)} className="btn btn-outline w-full">
            <ChevronDownIcon className={`w-4 h-4 transition ${showFilters ? 'rotate-180' : ''}`} /> Filters
          </button>
          {showFilters && (
            <div className="card p-4 mt-2 space-y-3">
              {/* Same filter UI collapsed for mobile */}
              <input className="input" placeholder="Location" value={location} onChange={(e) => { setLocation(e.target.value); setPage(1); }} />
              <select className="input" value={make} onChange={(e) => { setMake(e.target.value); setPage(1); }}>
                <option value="">All makes</option>
                {brands.map((b) => <option key={b.id} value={b.name}>{b.name}</option>)}
              </select>
              <div className="flex gap-2">
                <input className="input w-1/2" placeholder="Min price" type="number" value={minPrice} onChange={(e) => { setMinPrice(e.target.value); setPage(1); }} />
                <input className="input w-1/2" placeholder="Max price" type="number" value={maxPrice} onChange={(e) => { setMaxPrice(e.target.value); setPage(1); }} />
              </div>
              <div className="grid grid-cols-2 gap-2">
                <div>
                  <label className="text-[11px] uppercase tracking-wider text-brand-muted">Pick-up</label>
                  <input type="date" className="input" value={pickupDate} min={new Date().toISOString().slice(0, 10)}
                    onChange={(e) => {
                      setPickupDate(e.target.value);
                      setPage(1);
                      if (dropoffDate && e.target.value && e.target.value > dropoffDate) setDropoffDate(e.target.value);
                    }}/>
                </div>
                <div>
                  <label className="text-[11px] uppercase tracking-wider text-brand-muted">Drop-off</label>
                  <input type="date" className="input" value={dropoffDate} min={pickupDate || undefined}
                    onChange={(e) => { setDropoffDate(e.target.value); setPage(1); }}/>
                </div>
              </div>
              <div className="flex flex-wrap gap-2">
                {TRANSMISSIONS.map((t) => (
                  <button key={t} onClick={() => { setTransmission(transmission === t ? '' : t); setPage(1); }}
                    className={`px-3 py-1.5 rounded-lg text-xs font-semibold ${transmission === t ? 'bg-brand-ink text-white shadow-sm' : 'bg-white border border-brand-divider text-brand-ink hover:bg-gray-50'}`}>{t}</button>
                ))}
              </div>
              <div className="flex flex-wrap gap-2">
                {FUEL_TYPES.map((f) => (
                  <button key={f} onClick={() => { setFuelType(fuelType === f ? '' : f); setPage(1); }}
                    className={`px-3 py-1.5 rounded-lg text-xs font-semibold ${fuelType === f ? 'bg-brand-ink text-white shadow-sm' : 'bg-white border border-brand-divider text-brand-ink hover:bg-gray-50'}`}>{f}</button>
                ))}
              </div>
              <button onClick={resetFilters} className="text-sm text-brand-muted hover:text-brand-ink">Reset filters</button>
            </div>
          )}
        </div>

        {/* Main content */}
        <div className="flex-1 min-w-0">
          <div className="flex items-center justify-between flex-wrap gap-2 mb-4">
            <p className="text-sm text-brand-muted">
              {loading ? 'Searching…' : totalCount === 0 ? 'No cars found' : `${totalCount} car${totalCount !== 1 ? 's' : ''} available`}
              {pickupDate && dropoffDate && (
                <span className="ml-2 inline-flex items-center gap-1 text-brand-ink font-medium">
                  · {fmtRange(pickupDate, dropoffDate)}
                </span>
              )}
            </p>
            {(location || make || model || transmission || fuelType || seating || (pickupDate && dropoffDate) || minPrice || maxPrice) && (
              <button
                onClick={resetFilters}
                className="text-xs text-brand-muted hover:text-brand-ink"
              >
                Reset all filters
              </button>
            )}
          </div>

              {loading ? (
            <div className="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-3 gap-6">
              {[1,2,3,4,5,6].map((i) => (
                <CardSkeleton key={i} lines={3} />
              ))}
            </div>
          ) : (
            <>
              <div className="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-3 gap-6">
                {cars.map((car) => (
                  <div
                    key={car.id}
                    className="card-hover group"
                    onClick={() => navigate(`/cars/${car.id}`)}
                  >
                    {/* Image */}
                    <div className="relative h-48 bg-gray-200 overflow-hidden">
                      {car.imageUrls?.[0] ? (
                        <img
                          src={resolveAssetUrl(car.imageUrls[0])}
                          alt={`${car.make} ${car.model}`}
                          className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300"
                          loading="lazy"
                        />
                      ) : (
                        <div className="w-full h-full flex items-center justify-center text-brand-muted text-sm">
                          No image
                        </div>
                      )}
                      {car.status !== 'Available' && (
                        <span className="absolute top-2 left-2 badge-warning text-xs">{car.status}</span>
                      )}
                    </div>

                    {/* Info */}
                    <div className="p-4">
                      <div className="flex items-center gap-2 mb-1">
                        <span className="text-sm text-brand-muted">{car.year}</span>
                        {car.brandName && <span className="text-xs bg-brand-bg px-2 py-0.5 rounded font-medium">{car.brandName}</span>}
                      </div>
                      <h3 className="font-bold text-lg text-brand-ink leading-tight">{car.make} {car.model}</h3>

                      <div className="flex items-center gap-3 text-sm text-brand-muted mt-2">
                        {car.transmission && <span>{car.transmission}</span>}
                        {car.fuelType && <span>{car.fuelType}</span>}
                        {car.seatingCapacity && <span>{car.seatingCapacity} seats</span>}
                      </div>

                      <div className="flex items-center gap-2 mt-3">
                        <div className="flex items-center gap-1 text-brand-gold">
                          <StarIcon className="w-4 h-4" />
                          <span className="font-semibold text-sm text-brand-ink">
                            {car.averageRating > 0 ? car.averageRating.toFixed(1) : '—'}
                          </span>
                        </div>
                        {car.reviewCount > 0 && (
                          <span className="text-xs text-brand-muted">({car.reviewCount})</span>
                        )}
                        {car.location && (
                          <span className="ml-auto text-xs flex items-center gap-1 text-brand-muted">
                            <MapPinIcon className="w-3 h-3" /> {car.location.split(',').pop()?.trim() || car.location}
                          </span>
                        )}
                      </div>

                      <div className="flex items-center justify-between mt-3 pt-3 border-t border-brand-divider">
                        <div>
                          <span className="font-bold text-lg text-brand-ink">₹{car.pricePerDay.toLocaleString()}</span>
                          <span className="text-sm text-brand-muted"> / day</span>
                        </div>
                        <button className="btn btn-primary btn-sm text-xs py-1.5 px-4">View</button>
                      </div>
                    </div>
                  </div>
                ))}
              </div>

              {/* Pagination */}
              {totalPages > 1 && (
                <div className="flex items-center justify-center gap-2 mt-8">
                  <button
                    disabled={page <= 1}
                    onClick={() => setPage((p) => Math.max(1, p - 1))}
                    className="btn btn-outline btn-sm"
                  >
                    Previous
                  </button>
                  {Array.from({ length: Math.min(totalPages, 7) }, (_, i) => i + 1).map((p) => (
                    <button
                      key={p}
                      onClick={() => setPage(p)}
                      className={`btn btn-sm ${p === page ? 'btn-primary' : 'btn-outline'}`}
                    >
                      {p}
                    </button>
                  ))}
                  <button
                    disabled={page >= totalPages}
                    onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                    className="btn btn-outline btn-sm"
                  >
                    Next
                  </button>
                </div>
              )}
            </>
          )}
        </div>
      </div>
    </AppShell>
  );
}
