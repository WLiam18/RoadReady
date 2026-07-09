# RoadReady Web (Frontend)

React SPA for the RoadReady car rental platform. Talks to the .NET microservices
through the Ocelot gateway at `http://localhost:5000`.

## Stack
- **React 18** + **Vite** (JS, no TypeScript)
- **React Router 6** — routing
- **TanStack Query** — server state, caching, retries
- **Axios** — HTTP + auto-refresh interceptor (handles 401 transparently)
- **Tailwind CSS 3** — styling (brand colour palette matches backend email templates)
- **react-hot-toast** — notifications

## Token storage strategy
- **Access token**: held in JS module-scope memory only (not localStorage). Disappears on hard refresh → silent auto-refresh hit via Axios interceptor.
- **Refresh token**: `localStorage`. Rotated on every use.
- **User profile**: `localStorage` for instant header restoration.

## Running locally

```bash
# 1. Start the backend services (Auth + Car + Booking + Gateway)
#    (See RoadReady.slnx - run each project in a separate terminal)

# 2. From this folder:
npm install
npm run dev
# Open http://localhost:3000
```

Ports: the Vite dev server proxies `/api` and `/uploads` → `http://localhost:5000`.

## Building for production

```bash
npm run build
npm run preview
```

## Folder layout

```
src/
├── components/       Shared UI (Logo, SideNav, TopHeader, icons, ProtectedRoute)
├── context/          AuthContext (Token store + login/logout/register/refresh)
├── lib/              api (axios instance), env (VITE_API_BASE_URL), cn (classnames)
├── pages/            Route components grouped by feature
│   ├── auth/         Login / Register / Forgot / Reset
│   ├── cars/         Browse + detail
│   ├── bookings/     Booking form, list, detail
│   ├── admin/        Cars/Brands/Bookings/Users/PromoCodes + Dashboard
│   ├── agent/        Inspections & form
│   └── (top-level:   HomePage, FavoritesPage, HistoryPage, ProfilePage, NotFoundPage)
├── types/            Shared JSDoc typedefs
├── App.jsx           Route table
├── main.jsx          Bootstraps QueryClient + Auth + Router
└── index.css         Tailwind + component classes
```
