import { useLocation } from 'react-router-dom';
import SideNav from './SideNav';
import TopHeader from './TopHeader';

export default function AppShell({ children, hideHeader = false, hideNav = false, hideSidebar = false, fullBleed = false }) {
  const { pathname } = useLocation();

  // Auto-hide the side navigation when the visitor is on the marketing
  // landing page (`/`). Authenticated users who navigate to any other page
  // get the regular customer / admin / agent side nav. Pages can still
  // force `hideNav` or `hideHeader` explicitly.
  const isOnHome = pathname === '/';
  const showNav = !hideNav && !hideSidebar && !isOnHome;

  return (
    <div className="min-h-screen bg-brand-bg flex">
      {showNav && <SideNav />}
      <div className="flex-1 flex flex-col min-w-0">
        {!hideHeader && <TopHeader />}
        <main
          className={
            fullBleed
              ? 'flex-1 w-full'
              : 'flex-1 px-4 lg:px-6 py-6 lg:py-8 max-w-[1400px] w-full mx-auto'
          }
        >
          {children}
        </main>
      </div>
    </div>
  );
}
