import { api } from './api';

const ApiV1 = {
  // Auth
  login: (body) => api.post('/api/v1/auth/login', body),
  register: (body) => api.post('/api/v1/auth/register', body),
  googleLogin: (body) => api.post('/api/v1/auth/google', body),
  logout: () => api.post('/api/v1/auth/logout'),
  refresh: (body) => api.post('/api/v1/auth/refresh', body),
  forgotPassword: (body) => api.post('/api/v1/auth/forgot-password', body),
  resetPassword: (body) => api.post('/api/v1/auth/reset-password', body),
  profile: () => api.get('/api/v1/auth/profile'),
  updateProfile: (body) => api.put('/api/v1/auth/profile', body),
  updatePassword: (body) => api.put('/api/v1/auth/update-password', body),

  // Cars
  getAllCars: (params) => api.get('/api/v1/cars', { params }),
  searchCars: (params) => api.get('/api/v1/cars/search', { params }),
  getCarById: (id) => api.get(`/api/v1/cars/${id}`),
  createCar: (body) => api.post('/api/v1/cars', body),
  updateCar: (id, body) => api.put(`/api/v1/cars/${id}`, body),
  deleteCar: (id) => api.delete(`/api/v1/cars/${id}`),
  getBrands: () => api.get('/api/v1/brands'),

  // Reviews
  getReviews: (carId) => api.get(`/api/v1/cars/${carId}/reviews`),
  addReview: (carId, body) => api.post(`/api/v1/cars/${carId}/reviews`, body),
  updateReview: (carId, reviewId, body) => api.put(`/api/v1/cars/${carId}/reviews/${reviewId}`, body),
  deleteReview: (carId, reviewId) => api.delete(`/api/v1/cars/${carId}/reviews/${reviewId}`),

  // Bookings
  getMyBookings: () => api.get('/api/v1/bookings/me'),
  getAllBookings: () => api.get('/api/v1/bookings'),
  getMyBookingById: (id) => api.get(`/api/v1/bookings/${id}`),
  createBooking: (body) => api.post('/api/v1/bookings', body),
  cancelBooking: (id) => api.put(`/api/v1/bookings/${id}/cancel`),
  modifyBooking: (id, body) => api.put(`/api/v1/bookings/${id}/modify`, body),
  getMyPayments: () => api.get('/api/v1/bookings/me/payments'),
  downloadReceipt: (id) => api.get(`/api/v1/bookings/${id}/receipt`, { responseType: 'blob' }),

  // Admin
  getAdminAnalytics: () => api.get('/api/v1/admin/analytics'),
  getAllUsers: () => api.get('/api/v1/auth/users'),
  updateUserStatus: (userId, body) => api.put(`/api/v1/auth/users/${userId}/status`, body),

  // Brands (admin)
  createBrand: (body) => api.post('/api/v1/brands', body),
  updateBrand: (id, body) => api.put(`/api/v1/brands/${id}`, body),
  deleteBrand: (id) => api.delete(`/api/v1/brands/${id}`),

  // Upload
  uploadCarImage: (file) => {
    const fd = new FormData();
    fd.append('file', file);
    return api.post('/api/v1/cars/upload', fd, { headers: { 'Content-Type': 'multipart/form-data' } });
  },

  // PromoCodes
  getPromoCodes: () => api.get('/api/v1/promo-codes'),
  getPromoCode: (id) => api.get(`/api/v1/promo-codes/${id}`),
  createPromoCode: (body) => api.post('/api/v1/promo-codes', body),
  updatePromoCode: (id, body) => api.put(`/api/v1/promo-codes/${id}`, body),
  deletePromoCode: (id) => api.delete(`/api/v1/promo-codes/${id}`),
  validatePromoCode: (body) => api.post('/api/v1/promo-codes/validate', body),

  // Inspections (rental agent)
  getBookingInspectionHistory: (bookingId) => api.get(`/api/v1/bookings/${bookingId}/inspections`),
  getAgentCompletedToday: () => api.get('/api/v1/agent/completed'),
  getAgentCompleted: (date) => {
    const qs = date ? `?date=${encodeURIComponent(date)}` : '';
    return api.get(`/api/v1/agent/completed${qs}`);
  },
};

export default ApiV1;
