import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { toast } from 'react-hot-toast';
import { useEffect } from 'react';

// Backend may send role as string "Admin" or int 1
function roleMatches(allowedRoles, userRole) {
  if (!allowedRoles || !userRole) return false;
  const userRoleStr = String(userRole).toLowerCase();
  const roleMap = { 0: 'customer', 1: 'admin', 2: 'rentalagent' };
  const normalized = roleMap[userRoleStr] || userRoleStr;
  return allowedRoles.some((r) => r.toLowerCase() === normalized);
}

export default function ProtectedRoute({ roles, children }) {
  const { isAuthenticated, loading, user } = useAuth();
  const location = useLocation();

  useEffect(() => {
    if (!loading && roles && user && !roleMatches(roles, user.role)) {
      toast.error('You do not have access to that page.');
    }
  }, [loading, roles, user]);

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center text-brand-muted text-sm">
        <div className="flex items-center gap-2">
          <div className="w-5 h-5 border-2 border-brand-ink border-t-transparent rounded-full animate-spin" />
          Loading…
        </div>
      </div>
    );
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  if (roles && !roleMatches(roles, user?.role)) {
    return <Navigate to="/" replace />;
  }

  return children ?? <Outlet />;
}
