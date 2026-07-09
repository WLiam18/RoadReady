import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { toast } from 'react-hot-toast';
import AppShell from '../../components/AppShell';
import MapView from '../../components/MapView';
import { StarIcon, MapPinIcon, CarIcon, CalendarIcon, CreditCardIcon, UserIcon, CloseIcon } from '../../components/icons';
import { useAuth } from '../../context/AuthContext';
import ApiV1 from '../../lib/apiV1';

export default function CarDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { isAuthenticated } = useAuth();

  const [car, setCar] = useState(null);
  const [reviews, setReviews] = useState([]);
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);

  const [selectedImage, setSelectedImage] = useState(null);
  const [comment, setComment] = useState('');
  const [rating, setRating] = useState(5);

  // My reviews for edit/delete
  const [myUserId, setMyUserId] = useState(null);

  useEffect(() => {
    const stored = localStorage.getItem('rr_user');
    if (stored) {
      try {
        const u = JSON.parse(stored);
        setMyUserId(u.id);
      } catch {}
    }
  }, []);

  useEffect(() => {
    setLoading(true);
    Promise.all([
      ApiV1.getCarById(id),
      ApiV1.getReviews(id),
    ])
      .then(([carRes, reviewsRes]) => {
        if (carRes.data?.success) {
          setCar(carRes.data.data);
        } else {
          toast.error('Could not load car details.');
        }
        if (reviewsRes.data?.success) {
          setReviews(reviewsRes.data.data || []);
        }
      })
      .catch(() => toast.error('Could not load car details.'))
      .finally(() => setLoading(false));
  }, [id]);

  const handleAddReview = async (e) => {
    e.preventDefault();
    if (!comment.trim()) return toast.error('Please write a comment.');
    setSubmitting(true);
    try {
      await ApiV1.addReview(id, { rating, comment: comment.trim() });
      const res = await ApiV1.getReviews(id);
      if (res.data?.success) setReviews(res.data.data || []);
      toast.success('Review added!');
      setComment('');
      setRating(5);
    } catch (err) {
      toast.error(err?.response?.data?.message || 'Failed to add review.');
    } finally {
      setSubmitting(false);
    }
  };

  const handleDeleteReview = async (reviewId) => {
    if (!window.confirm('Delete this review?')) return;
    try {
      await ApiV1.deleteReview(id, reviewId);
      setReviews((prev) => prev.filter((r) => r.id !== reviewId));
      toast.success('Review deleted.');
    } catch (err) {
      toast.error(err?.response?.data?.message || 'Failed to delete.');
    }
  };

  if (loading) {
    return (
      <AppShell>
        <div className="max-w-5xl mx-auto">
          <div className="card animate-pulse">
            <div className="h-64 md:h-80 bg-gray-200" />
            <div className="p-6 space-y-3">
              <div className="h-6 bg-gray-200 rounded w-1/3" />
              <div className="h-4 bg-gray-200 rounded w-2/3" />
            </div>
          </div>
        </div>
      </AppShell>
    );
  }

  if (!car) {
    return (
      <AppShell>
        <div className="max-w-5xl mx-auto text-center py-20">
          <h1 className="text-2xl font-bold mb-2">Car not found</h1>
          <p className="text-brand-muted mb-6">The car you are looking for does not exist.</p>
          <button onClick={() => navigate('/cars')} className="btn btn-primary">Browse cars</button>
        </div>
      </AppShell>
    );
  }

  const images = car.imageUrls?.length > 0 ? car.imageUrls : null;

  return (
    <AppShell>
      <div className="max-w-5xl mx-auto">
        {/* Image gallery */}
        <div className="mb-6">
          {selectedImage ? (
            <div className="relative rounded-xl overflow-hidden">
              <img src={selectedImage} alt="" className="w-full h-80 md:h-96 object-cover" />
              <button
                onClick={() => setSelectedImage(null)}
                className="absolute top-3 right-3 bg-black/50 text-white rounded-full p-2 hover:bg-black/70"
              >
                <CloseIcon className="w-5 h-5" />
              </button>
            </div>
          ) : images ? (
            <div className="grid grid-cols-2 md:grid-cols-3 gap-2">
              {images.slice(0, 5).map((img, i) => (
                <div
                  key={i}
                  className={`relative overflow-hidden rounded-xl cursor-pointer group ${i === 0 ? 'col-span-2 md:col-span-3 h-64 md:h-80' : 'h-32 md:h-40'}`}
                  onClick={() => setSelectedImage(img)}
                >
                  <img src={img} alt="" className="w-full h-full object-cover group-hover:scale-105 transition duration-300" loading="lazy" />
                  {i === 4 && images.length > 5 && (
                    <div className="absolute inset-0 bg-black/50 flex items-center justify-center text-white font-bold text-lg">
                      +{images.length - 5}
                    </div>
                  )}
                </div>
              ))}
            </div>
          ) : (
            <div className="h-64 bg-gray-200 rounded-xl flex items-center justify-center text-brand-muted">
              No images available
            </div>
          )}
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* Main info */}
          <div className="lg:col-span-2 space-y-6">
            <div className="card p-6">
              <div className="flex items-start justify-between">
                <div>
                  <div className="flex items-center gap-2 text-sm text-brand-muted mb-1">
                    <span>{car.year}</span>
                    {car.brandName && <span className="bg-brand-bg px-2 py-0.5 rounded font-medium">{car.brandName}</span>}
                    <span className="flex items-center gap-1"><MapPinIcon className="w-3 h-3" />{car.location}</span>
                  </div>
                  <h1 className="text-2xl md:text-3xl font-bold text-brand-ink">{car.make} {car.model}</h1>
                </div>
                <div className="text-right">
                  <div className="text-2xl font-bold text-brand-ink">₹{car.pricePerDay.toLocaleString()}</div>
                  <div className="text-sm text-brand-muted">per day</div>
                </div>
              </div>

              <div className="flex items-center gap-4 mt-4 pt-4 border-t border-brand-divider flex-wrap text-sm">
                <span className="flex items-center gap-1"><StarIcon className="w-4 h-4 text-brand-gold" /> {car.averageRating > 0 ? `${car.averageRating.toFixed(1)} (${car.reviewCount} reviews)` : 'No reviews yet'}</span>
                <span>{car.transmission}</span>
                <span>{car.fuelType}</span>
                <span>{car.seatingCapacity} seats</span>
                <span>{car.color}</span>
              </div>
            </div>

            {/* Description */}
            {car.description && (
              <div className="card p-6">
                <h2 className="font-semibold mb-2">Description</h2>
                <p className="text-sm text-brand-muted leading-relaxed">{car.description}</p>
              </div>
            )}

            {/* Map */}
            <div className="card overflow-hidden">
              <MapView location={car.location} className="h-56 w-full" />
              <div className="px-4 py-2 text-xs text-brand-muted border-t border-brand-divider flex items-center gap-1">
                <MapPinIcon className="w-3 h-3" /> {car.location}
              </div>
            </div>

            {/* Reviews */}
            <div className="card p-6">
              <h2 className="font-semibold mb-4">
                Reviews {reviews.length > 0 && <span className="text-brand-muted font-normal">({reviews.length})</span>}
              </h2>

              {reviews.length === 0 ? (
                <p className="text-sm text-brand-muted">No reviews yet. Be the first!</p>
              ) : (
                <div className="space-y-4">
                  {reviews.map((r) => (
                    <div key={r.id} className="pb-4 border-b border-brand-divider last:border-b-0 last:pb-0">
                      <div className="flex items-center gap-3 mb-1">
                        <div className="w-8 h-8 rounded-full bg-brand-secondary text-white flex items-center justify-center text-xs font-bold uppercase">
                          {r.userName?.[0] || 'A'}
                        </div>
                        <div>
                          <p className="font-medium text-sm text-brand-ink">{r.userName}</p>
                          <p className="text-xs text-brand-muted">{new Date(r.createdAt).toLocaleDateString()}</p>
                        </div>
                        <div className="ml-auto flex items-center gap-1 text-brand-gold">
                          {Array.from({ length: 5 }, (_, i) => (
                            <StarIcon key={i} className={`w-3.5 h-3.5 ${i < r.rating ? 'text-brand-gold' : 'text-gray-300'}`} />
                          ))}
                        </div>
                      </div>
                      <p className="text-sm text-brand-ink ml-11">{r.comment}</p>
                      {myUserId && r.userId === myUserId && (
                        <div className="flex gap-2 ml-11 mt-2">
                          <button
                            onClick={() => handleDeleteReview(r.id)}
                            className="text-xs text-brand-danger hover:underline"
                          >
                            Delete
                          </button>
                        </div>
                      )}
                    </div>
                  ))}
                </div>
              )}

              {/* Add review */}
              {isAuthenticated && (
                <form onSubmit={handleAddReview} className="mt-6 pt-4 border-t border-brand-divider">
                  <h3 className="font-medium text-sm mb-3">Write a review</h3>
                  <div className="flex items-center gap-1 mb-3">
                    {[1,2,3,4,5].map((s) => (
                      <button key={s} type="button" onClick={() => setRating(s)}>
                        <StarIcon className={`w-6 h-6 ${s <= rating ? 'text-brand-gold' : 'text-gray-300'}`} />
                      </button>
                    ))}
                  </div>
                  <textarea
                    value={comment}
                    onChange={(e) => setComment(e.target.value)}
                    placeholder="Tell others about your experience…"
                    rows={3}
                    className="input resize-none mb-3"
                    maxLength={500}
                  />
                  <button type="submit" className="btn btn-primary" disabled={submitting || !comment.trim()}>
                    {submitting ? 'Submitting…' : 'Submit review'}
                  </button>
                </form>
              )}
            </div>
          </div>

          {/* Sidebar: Book CTA */}
          <div className="space-y-4">
            <div className="card p-6 sticky top-24">
              <div className="text-center mb-4">
                <div className="text-3xl font-bold text-brand-ink">₹{car.pricePerDay.toLocaleString()}</div>
                <div className="text-sm text-brand-muted">per day</div>
              </div>

              <button
                onClick={() => {
                  if (!isAuthenticated) {
                    toast.error('Please log in to book this car.');
                    navigate(`/login?redirect=/cars/${car.id}/book`);
                    return;
                  }
                  navigate(`/cars/${car.id}/book`);
                }}
                className="btn btn-primary w-full mb-3"
              >
                <CalendarIcon className="w-4 h-4" /> Book now
              </button>

              <div className="space-y-2 text-sm text-brand-muted">
                <p className="flex items-center gap-2"><CreditCardIcon className="w-4 h-4" /> Pay with Razorpay</p>
                <p className="flex items-center gap-2"><CarIcon className="w-4 h-4" /> Free cancellation</p>
                <p className="flex items-center gap-2"><UserIcon className="w-4 h-4" /> Agent-assisted check-in</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </AppShell>
  );
}
