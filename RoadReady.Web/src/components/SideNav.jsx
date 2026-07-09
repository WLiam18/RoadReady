import { NavLink, useNavigate } from 'react-router-dom';
import { toast } from 'react-hot-toast';
import {
  HomeIcon,
  CarIcon,
  HeartIcon,
  ClockIcon,
  UserCircleIcon,
  TicketIcon,
  WrenchIcon,
  ShieldIcon,
  LogOutIcon,
  BarChart3Icon,
  TagIcon,
  CalendarIcon,
  CheckIcon,
} from './icons';
import { Logo } from './Logo';
import { useAuth } from '../context/AuthContext';

const customerLinks = [
  { to: '/', label: 'Home', Icon: HomeIcon },
  { to: '/cars', label: 'Vehicles', Icon: CarIcon },
  { to: '/my-bookings', label: 'My Bookings', Icon: TicketIcon },
  { to: '/favorites', label: 'Favorites', Icon: HeartIcon },
  { to: '/history', label: 'History', Icon: ClockIcon },
  { to: '/profile', label: 'Profile', Icon: UserCircleIcon },
];

const adminLinks = [
  { to: '/admin', label: 'Dashboard', Icon: BarChart3Icon },
  { to: '/admin/cars', label: 'Cars', Icon: CarIcon },
  { to: '/admin/brands', label: 'Brands', Icon: ShieldIcon },
  { to: '/admin/bookings', label: 'Bookings', Icon: TicketIcon },
  { to: '/admin/users', label: 'Users', Icon: UserCircleIcon },
  { to: '/admin/promo-codes', label: 'Promo Codes', Icon: TagIcon },
];

const agentLinks = [
  { to: '/agent', label: 'Today', Icon: WrenchIcon },
  { to: '/agent/completed', label: 'Completed', Icon: CheckIcon },
  { to: '/agent/history', label: 'All history', Icon: CalendarIcon },
];

export default function SideNav() {
  const navigate = useNavigate();
  const { user, isAdmin, isAgent, isAuthenticated, logout } = useAuth();

  const handleLogout = async () => {
    const res = await logout();
    if (res?.ok) toast.success('Logged out.');
    navigate('/login', { replace: true });
  };

  let mainLinks = customerLinks;
  if (isAdmin) mainLinks = adminLinks;
  else if (isAgent) mainLinks = agentLinks;

  return (
    <aside className="hidden lg:flex flex-col w-20 xl:w-56 bg-white border-r border-brand-divider sticky top-0 h-screen">
      <div className="px-4 xl:px-6 py-5 border-b border-brand-divider">
        <Logo size={32} />
      </div>

      <nav className="flex-1 px-2 xl:px-3 py-4 space-y-1 overflow-y-auto">
        {mainLinks.map(({ to, label, Icon }) => (
          <NavLink
            key={to}
            to={to}
            end={to === '/' || to === '/admin' || to === '/agent'}
            className={({ isActive }) =>
              `flex items-center justify-center xl:justify-start gap-3 px-2 xl:px-3 py-2.5 rounded-lg text-sm font-medium transition ${
                isActive
                  ? 'bg-brand-ink text-white shadow-sm'
                  : 'text-brand-muted hover:bg-gray-100'
              }`
            }
            title={label}
          >
            <Icon className="w-5 h-5 flex-shrink-0" />
            <span className="hidden xl:inline truncate">{label}</span>
          </NavLink>
        ))}
      </nav>

      <div className="mt-auto px-2 xl:px-3 py-4 border-t border-brand-divider">
        <NavLink
          to="/license"
          className="flex items-center justify-center xl:justify-start gap-3 px-2 xl:px-3 py-2 rounded-lg text-xs text-brand-muted hover:bg-brand-bg"
        >
          <span className="hidden xl:inline">License</span>
        </NavLink>
        <NavLink
          to="/support"
          className="flex items-center justify-center xl:justify-start gap-3 px-2 xl:px-3 py-2 rounded-lg text-xs text-brand-muted hover:bg-brand-bg"
        >
          <span className="hidden xl:inline">Support</span>
        </NavLink>
        {isAuthenticated && (
          <button
            onClick={handleLogout}
            className="w-full flex items-center justify-center xl:justify-start gap-3 px-2 xl:px-3 py-2 rounded-lg text-xs text-brand-muted hover:bg-brand-bg hover:text-brand-ink"
          >
            <LogOutIcon className="w-4 h-4" />
            <span className="hidden xl:inline">Logout</span>
          </button>
        )}
      </div>
    </aside>
  );
}
