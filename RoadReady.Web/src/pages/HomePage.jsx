import { useState, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import AppShell from '../components/AppShell';
import { Logo } from '../components/Logo';
import {
  Search as SearchIcon,
  MapPin as MapPinIcon,
  Calendar as CalendarIcon,
  Phone as PhoneIcon,
  Award as AwardIcon,
  ShieldCheck as ShieldIcon,
  XCircle as XCircleIcon,
} from 'lucide-react';

/* ------------------------------------------------------------------ */
/*  DATE HELPERS                                                       */
/* ------------------------------------------------------------------ */
function todayISO() {
  const d = new Date();
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
}
function addDaysISO(n) {
  const d = new Date();
  d.setDate(d.getDate() + n);
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
}
function buildQS({ location, pickupDate, dropoffDate }) {
  const p = new URLSearchParams();
  if (location) p.set('location', location);
  if (pickupDate) p.set('pickupDate', pickupDate);
  if (dropoffDate) p.set('dropoffDate', dropoffDate);
  return p.toString();
}

/* ------------------------------------------------------------------ */
/*  CONTENT DATA                                                       */
/* ------------------------------------------------------------------ */
const BRANDS = [
  { src: '/logos/bmw.svg', alt: 'BMW' },
  { src: '/logos/mercedes.svg', alt: 'Mercedes-Benz' },
  { src: '/logos/audi.svg', alt: 'Audi' },
  { src: '/logos/toyota.svg', alt: 'Toyota' },
  { src: '/logos/vw.svg', alt: 'Volkswagen' },
  { src: '/logos/honda-car.svg', alt: 'Honda' },
  { src: '/logos/kia.svg', alt: 'Kia' },
  { src: '/logos/suzuki.svg', alt: 'Suzuki' },
];

const WHY_US = [
  { icon: PhoneIcon, title: '24 Hour Support', desc: 'Real humans on call, day or night, for anything on the road.' },
  { icon: AwardIcon, title: 'Best Price', desc: 'Transparent fares with no surprise fees at pickup.' },
  { icon: ShieldIcon, title: 'Verified License', desc: 'Every renter and owner checked before keys change hands.' },
  { icon: XCircleIcon, title: 'Free Cancelation', desc: 'Plans change. Cancel free up to 24 hours before pickup.' },
];

const STATS = [
  { value: '4000', label: 'Active Member' },
  { value: '3000', label: 'Car Model' },
  { value: '6K', label: 'Positive Rating' },
];

const TESTIMONIALS = [
  { name: 'Deepak', role: 'Customer – Bengaluru', quote: 'Booking took two minutes and the car was spotless. Easiest rental I have used.' },
  { name: 'Ritik', role: 'Customer – Chennai', quote: 'Clear pricing, no haggling at the counter. Exactly what it said online.' },
  { name: 'Adithya', role: 'Customer – Kochi', quote: 'Roadside support answered in under a minute when I needed it. Reassuring.' },
];

/* ------------------------------------------------------------------ */
/*  COMPONENT                                                          */
/* ------------------------------------------------------------------ */
export default function HomePage() {
  const navigate = useNavigate();
  const marqueeRef = useRef(null);

  const [pickupLocation, setPickupLocation] = useState('');
  const [pickupDate, setPickupDate] = useState(todayISO());
  const [dropoffDate, setDropoffDate] = useState(addDaysISO(2));

  const handleSearch = (e) => {
    e?.preventDefault?.();
    const qs = buildQS({ location: pickupLocation, pickupDate, dropoffDate });
    navigate(`/cars${qs ? `?${qs}` : ''}`);
  };

  return (
    <AppShell fullBleed hideHeader hideSidebar>
      {/* ============================================================
          NAV BAR — logo + auth actions only
      ============================================================ */}
      <header className="relative z-30 px-4 sm:px-8 lg:px-12 py-5 flex items-center justify-between bg-white/80 backdrop-blur">
        <Logo size={32} />
        <div className="flex items-center gap-3">
          <button onClick={() => navigate('/login')} className="text-sm font-semibold text-black px-5 py-2 rounded-full border border-black/25 hover:border-black transition">Log in</button>
          <button onClick={() => navigate('/register')} className="text-sm font-semibold text-white bg-black px-5 py-2 rounded-full hover:bg-black/85 transition">Sign Up</button>
        </div>
      </header>

      {/* ============================================================
          HERO — full-bleed yellow car background, copy + search
      ============================================================ */}
      <section className="relative w-full overflow-hidden" style={{ minHeight: '100vh' }}>
        {/* Yellow car fills the entire background */}
        <img
          src="/yellowcar.png"
          alt=""
          className="absolute inset-0 w-full h-full object-cover z-0"
          loading="eager"
        />
        {/* Dark overlay so text is readable */}
        <div className="absolute inset-0 bg-gradient-to-b from-black/55 via-black/45 to-black/70 z-10 pointer-events-none" />

        {/* Content layer — sized to fit inside one screen on desktop; can grow slightly on mobile where fields stack */}
        <div className="relative z-20 flex flex-col min-h-full max-w-7xl mx-auto px-4 sm:px-8 lg:px-12 pt-20 sm:pt-24 pb-8 sm:pb-10">
          {/* Headline + CTA */}
          <div className="max-w-xl">
            <h1 className="text-3xl sm:text-4xl lg:text-5xl font-extrabold leading-[1.1] tracking-tight text-white">
              Easy And Fast Way<br />
              To <span className="underline decoration-4 underline-offset-8 decoration-white">Rent</span> Your Car
            </h1>
            <p className="mt-4 text-white/70 text-sm sm:text-base max-w-md leading-relaxed">
              We offer a wide range of rental cars to suit your needs. Whether you're planning a weekend getaway, a business trip, or a same-day city hop.
            </p>
            <button
              onClick={() => navigate('/login')}
              className="mt-6 inline-flex items-center gap-2 bg-white text-black font-semibold px-7 py-3 rounded-full hover:bg-white/90 transition shadow-lg"
            >
              Rent Car
            </button>
          </div>

          {/* Spacer — clear visual gap before the form, but bounded so it never pushes the card off-screen */}
          <div className="flex-1 min-h-16 sm:min-h-20" />

          {/* Floating search card */}
          <form
            id="search-card"
            onSubmit={handleSearch}
            className="relative z-40 w-full max-w-5xl mx-auto mb-0"
          >
            <div className="bg-white rounded-2xl shadow-[0_20px_60px_-15px_rgba(0,0,0,0.45)] p-4 sm:p-6 grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 text-left">
              <label className="block">
                <span className="block text-sm font-semibold text-black mb-2">Pick up Location</span>
                <div className="flex items-center gap-2 border border-black/15 rounded-xl px-3 h-12 focus-within:border-black transition">
                  <MapPinIcon className="w-4 h-4 text-black/40 flex-shrink-0" />
                  <input type="text" placeholder="City, airport, station…" value={pickupLocation} onChange={(e) => setPickupLocation(e.target.value)} className="w-full bg-transparent text-sm outline-none placeholder:text-black/30" autoComplete="off" />
                </div>
              </label>
              <label className="block">
                <span className="block text-sm font-semibold text-black mb-2">Pick up Date</span>
                <div className="flex items-center gap-2 border border-black/15 rounded-xl px-3 h-12 focus-within:border-black transition">
                  <CalendarIcon className="w-4 h-4 text-black/40 flex-shrink-0" />
                  <input type="date" min={todayISO()} value={pickupDate} onChange={(e) => { setPickupDate(e.target.value); if (dropoffDate && e.target.value > dropoffDate) setDropoffDate(e.target.value); }} className="w-full bg-transparent text-sm outline-none" />
                </div>
              </label>
              <label className="block">
                <span className="block text-sm font-semibold text-black mb-2">Return Date</span>
                <div className="flex items-center gap-2 border border-black/15 rounded-xl px-3 h-12 focus-within:border-black transition">
                  <CalendarIcon className="w-4 h-4 text-black/40 flex-shrink-0" />
                  <input type="date" min={pickupDate || todayISO()} value={dropoffDate} onChange={(e) => setDropoffDate(e.target.value)} className="w-full bg-transparent text-sm outline-none" />
                </div>
              </label>
              <div className="flex items-end">
                <button type="submit" className="w-full h-12 inline-flex items-center justify-center gap-2 px-4 rounded-xl bg-black text-white font-semibold text-sm hover:bg-black/85 active:scale-[0.97] transition">
                  <SearchIcon className="w-4 h-4" />
                  Search Car
                </button>
              </div>
            </div>
          </form>
        </div>
      </section>

      {/* ============================================================
          BRAND MARQUEE — seamless infinite loop
      ============================================================ */}
      <section className="py-10 bg-white overflow-hidden">
        <p className="text-center text-[11px] uppercase tracking-[0.22em] text-black/35 mb-6">Trusted by fleet partners across the country</p>
        <div className="relative w-full overflow-hidden">
          <div className="absolute inset-y-0 left-0 w-24 bg-gradient-to-r from-white to-transparent z-10 pointer-events-none" />
          <div className="absolute inset-y-0 right-0 w-24 bg-gradient-to-l from-white to-transparent z-10 pointer-events-none" />
          <div ref={marqueeRef} className="flex w-max animate-marquee items-center"
            onMouseEnter={() => marqueeRef.current?.classList.add('paused')}
            onMouseLeave={() => marqueeRef.current?.classList.remove('paused')}>
            {[...BRANDS, ...BRANDS, ...BRANDS].map((logo, i) => (
              <div key={i} className="flex-shrink-0 mx-10 sm:mx-14 flex items-center justify-center" style={{ width: 110 }}>
                <img src={logo.src} alt={logo.alt} className="h-14 sm:h-16 w-auto object-contain opacity-40 hover:opacity-70 transition-all duration-300" loading="lazy" />
              </div>
            ))}
          </div>
        </div>
        <style>{`
          @keyframes marquee { from { transform: translateX(0); } to { transform: translateX(-33.333%); } }
          .animate-marquee { animation: marquee 24s linear infinite; }
          .animate-marquee.paused { animation-play-state: paused; }
          @media (prefers-reduced-motion: reduce) { .animate-marquee { animation: none; } }
        `}</style>
      </section>

      {/* ============================================================
          WHY CHOOSE US — single grey BMW car on the right
      ============================================================ */}
      <section className="relative py-20 sm:py-28 bg-white overflow-hidden">
        {/* Grey BMW bleeding off the right edge */}
        <div className="hidden lg:block absolute right-0 top-1/2 -translate-y-1/2 translate-x-[5%]" style={{ width: '42%' }}>
          <img src="/greybmw.png" alt="" className="w-full h-auto object-contain" loading="lazy" />
        </div>

        <div className="max-w-3xl mx-auto px-4 sm:px-6 relative z-10 lg:mr-[38%]">
          <div className="mb-12">
            <h2 className="text-2xl sm:text-3xl font-bold tracking-tight text-black">Why <span className="underline decoration-4 underline-offset-4">Choose</span> Us</h2>
            <p className="text-black/50 mt-2 max-w-md text-sm">No branch queues, no surprise fees, no waiting on hold. Just a car when you need one.</p>
          </div>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-x-8 gap-y-8">
            {WHY_US.map(({ icon: Icon, title, desc }) => (
              <div key={title} className="flex items-start gap-4">
                <div className="w-11 h-11 rounded-xl bg-black text-white flex items-center justify-center flex-shrink-0">
                  <Icon className="w-5 h-5" />
                </div>
                <div>
                  <h3 className="font-semibold text-black mb-1">{title}</h3>
                  <p className="text-sm text-black/50 leading-relaxed">{desc}</p>
                </div>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* ============================================================
          OUR ACHIEVEMENT — elevated stat card
      ============================================================ */}
      <section className="pb-16 sm:pb-20 bg-white">
        <div className="max-w-4xl mx-auto px-4 sm:px-6">
          <div className="text-center mb-10">
            <h2 className="text-2xl sm:text-3xl font-bold tracking-tight text-black">Our <span className="underline decoration-4 underline-offset-4">Achievement</span></h2>
            <p className="text-black/50 mt-2 text-sm">Our journey of success is a testament to the collective efforts and determination of our team.</p>
          </div>
          <div className="bg-white rounded-2xl shadow-[0_20px_50px_-20px_rgba(0,0,0,0.25)] ring-1 ring-black/5 grid grid-cols-1 sm:grid-cols-3 divide-y sm:divide-y-0 sm:divide-x divide-black/10">
            {STATS.map(({ value, label }) => (
              <div key={label} className="px-6 py-8 text-center">
                <div className="text-3xl sm:text-4xl font-extrabold text-black">{value}+</div>
                <div className="text-black/45 text-sm mt-1">{label}</div>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* ============================================================
          WHAT OUR CUSTOMERS SAY
      ============================================================ */}
      <section className="py-16 sm:py-20 bg-[#F4F5F7]">
        <div className="max-w-5xl mx-auto px-4 sm:px-6">
          <div className="text-center max-w-md mx-auto mb-10">
            <h2 className="text-2xl sm:text-3xl font-bold tracking-tight text-black">What Our <span className="underline decoration-4 underline-offset-4">Customers</span> Have To Say</h2>
            <p className="text-black/50 mt-2 text-sm">Real trips, real feedback.</p>
          </div>
          <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
            {TESTIMONIALS.map((t) => (
              <div key={t.name} className="bg-white rounded-2xl p-6 shadow-sm">
                <div className="w-10 h-10 rounded-full bg-black text-white flex items-center justify-center text-sm font-semibold mb-4">
                  {t.name.split(' ').map((n) => n[0]).join('')}
                </div>
                <div className="text-sm font-semibold text-black">{t.name}</div>
                <div className="text-xs text-black/40 mb-3">{t.role}</div>
                <p className="text-sm text-black/55 leading-relaxed">&ldquo;{t.quote}&rdquo;</p>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* ============================================================
          CTA — dark panel with black tail light car
      ============================================================ */}
      <section className="py-16 sm:py-20 bg-white">
        <div className="max-w-5xl mx-auto px-4 sm:px-6">
          <div className="relative bg-black text-white rounded-3xl p-8 sm:p-12 overflow-hidden">
            <div className="relative z-10 flex flex-col sm:flex-row sm:items-center gap-8 sm:gap-12">
              <div className="flex-1">
                <h2 className="text-2xl sm:text-3xl font-bold tracking-tight">Ready to hit the road?</h2>
                <p className="text-white/60 mt-2 max-w-sm text-sm">Browse our verified fleet, lock in your price, and pick up your car today. No paperwork headaches, no hidden fees.</p>
                <div className="mt-6 flex items-center gap-4">
                  <button onClick={() => navigate('/register')} className="bg-white text-black font-semibold px-7 py-3 rounded-full hover:bg-white/90 transition">Sign Up</button>
                </div>
              </div>
              <div className="relative w-full sm:w-80 h-44 sm:h-52 flex-shrink-0">
                <img src="/blackcartailight.png" alt="Rental car" className="w-full h-full object-cover rounded-2xl" />
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* ============================================================
          FOOTER — minimal single row
      ============================================================ */}
      <footer className="bg-black text-white py-6">
        <div className="max-w-5xl mx-auto px-4 sm:px-6 flex flex-col sm:flex-row items-center justify-between gap-3">
          <p className="text-white/35 text-xs">&copy; {new Date().getFullYear()} Powered by RoadReady.</p>
          <div className="flex items-center gap-5 text-white/35 text-xs">
            <a href="#" className="hover:text-white transition">Privacy Policy</a>
            <a href="#" className="hover:text-white transition">Website Terms</a>
            <a href="#" className="hover:text-white transition">Cookie Policy</a>
          </div>
        </div>
      </footer>
    </AppShell>
  );
}