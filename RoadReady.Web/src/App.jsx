import { Routes, Route } from 'react-router-dom';
import ProtectedRoute from './components/ProtectedRoute';

import LoginPage from './pages/auth/LoginPage';
import RegisterPage from './pages/auth/RegisterPage';
import ForgotPasswordPage from './pages/auth/ForgotPasswordPage';
import ResetPasswordPage from './pages/auth/ResetPasswordPage';

import HomePage from './pages/HomePage';
import CarBrowsePage from './pages/cars/CarBrowsePage';
import CarDetailPage from './pages/cars/CarDetailPage';
import BookingPage from './pages/bookings/BookingPage';
import MyBookingsPage from './pages/bookings/MyBookingsPage';
import BookingDetailPage from './pages/bookings/BookingDetailPage';
import FavoritesPage from './pages/FavoritesPage';
import HistoryPage from './pages/HistoryPage';
import ProfilePage from './pages/ProfilePage';

import AdminDashboardPage from './pages/admin/DashboardPage';
import AdminCarsPage from './pages/admin/CarsPage';
import AdminBrandsPage from './pages/admin/BrandsPage';
import AdminBookingsPage from './pages/admin/BookingsPage';
import AdminUsersPage from './pages/admin/UsersPage';
import AdminPromoCodesPage from './pages/admin/PromoCodesPage';

import AgentInspectionsPage from './pages/agent/InspectionsPage';
import AgentCompletedPage from './pages/agent/CompletedPage';
import AgentInspectionFormPage from './pages/agent/InspectionFormPage';
import AgentHistoryPage from './pages/agent/HistoryPage';

import NotFoundPage from './pages/NotFoundPage';

export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/register" element={<RegisterPage />} />
      <Route path="/forgot-password" element={<ForgotPasswordPage />} />
      <Route path="/reset-password" element={<ResetPasswordPage />} />

      <Route element={<ProtectedRoute roles={['Admin']} />}>
        <Route path="/admin" element={<AdminDashboardPage />} />
        <Route path="/admin/cars" element={<AdminCarsPage />} />
        <Route path="/admin/brands" element={<AdminBrandsPage />} />
        <Route path="/admin/bookings" element={<AdminBookingsPage />} />
        <Route path="/admin/users" element={<AdminUsersPage />} />
        <Route path="/admin/promo-codes" element={<AdminPromoCodesPage />} />
      </Route>

      <Route element={<ProtectedRoute roles={['Admin', 'RentalAgent']} />}>
        <Route path="/agent" element={<AgentInspectionsPage />} />
        <Route path="/agent/completed" element={<AgentCompletedPage />} />
        <Route path="/agent/history" element={<AgentHistoryPage />} />
        <Route path="/agent/inspections/:bookingId/checkout" element={<AgentInspectionFormPage action="checkout" />} />
        <Route path="/agent/inspections/:bookingId/checkin" element={<AgentInspectionFormPage action="checkin" />} />
      </Route>

      <Route element={<ProtectedRoute />}>
        <Route path="/cars/:id/book" element={<BookingPage />} />
        <Route path="/my-bookings" element={<MyBookingsPage />} />
        <Route path="/my-bookings/:id" element={<BookingDetailPage />} />
        <Route path="/favorites" element={<FavoritesPage />} />
        <Route path="/history" element={<HistoryPage />} />
        <Route path="/profile" element={<ProfilePage />} />
      </Route>

      <Route path="/" element={<HomePage />} />
      <Route path="/cars" element={<CarBrowsePage />} />
      <Route path="/cars/:id" element={<CarDetailPage />} />

      <Route path="*" element={<NotFoundPage />} />
    </Routes>
  );
}
