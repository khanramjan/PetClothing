import { useEffect, useState } from 'react';
import { FaStar, FaCheck, FaTimes, FaTrash } from 'react-icons/fa';
import api from '@/lib/api';
import { toast } from 'react-toastify';

interface Review {
  id: number;
  productId: number;
  productName: string;
  userId: number;
  userName: string;
  rating: number;
  title: string;
  comment: string;
  isVerifiedPurchase: boolean;
  isApproved: boolean;
  createdAt: string;
}

export default function Reviews() {
  const [reviews, setReviews] = useState<Review[]>([]);
  const [loading, setLoading] = useState(true);
  const [filter, setFilter] = useState<'all' | 'pending' | 'approved'>('all');

  useEffect(() => {
    fetchReviews();
  }, [filter]);

  const fetchReviews = async () => {
    try {
      setLoading(true);
      const response = await api.get(`/reviews${filter !== 'all' ? `?approved=${filter === 'approved'}` : ''}`);
      setReviews(response.data.data);
    } catch (error) {
      toast.error('Failed to load reviews');
    } finally {
      setLoading(false);
    }
  };

  const handleApprove = async (id: number) => {
    try {
      await api.patch(`/reviews/${id}/approve`);
      toast.success('Review approved');
      fetchReviews();
    } catch (error) {
      toast.error('Failed to approve review');
    }
  };

  const handleReject = async (id: number) => {
    try {
      await api.patch(`/reviews/${id}/reject`);
      toast.success('Review rejected');
      fetchReviews();
    } catch (error) {
      toast.error('Failed to reject review');
    }
  };

  const handleDelete = async (id: number) => {
    if (!confirm('Are you sure you want to delete this review?')) return;
    
    try {
      await api.delete(`/reviews/${id}`);
      toast.success('Review deleted');
      fetchReviews();
    } catch (error) {
      toast.error('Failed to delete review');
    }
  };

  const renderStars = (rating: number) => {
    return (
      <div className="flex gap-1">
        {[1, 2, 3, 4, 5].map((star) => (
          <FaStar
            key={star}
            className={star <= rating ? 'text-yellow-400' : 'text-gray-300'}
          />
        ))}
      </div>
    );
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center h-[70vh]">
        <div className="spinner"></div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-bold">Reviews</h1>
        <select
          value={filter}
          onChange={(e) => setFilter(e.target.value as any)}
          className="input w-auto"
        >
          <option value="all">All Reviews</option>
          <option value="pending">Pending</option>
          <option value="approved">Approved</option>
        </select>
      </div>

      <div className="space-y-4">
        {reviews.map((review) => (
          <div key={review.id} className="card">
            <div className="flex items-start justify-between mb-4">
              <div className="flex-1">
                <div className="flex items-center gap-4 mb-2">
                  <h3 className="font-bold text-lg">{review.title}</h3>
                  {renderStars(review.rating)}
                  {review.isVerifiedPurchase && (
                    <span className="badge badge-success text-xs">Verified Purchase</span>
                  )}
                  <span className={`badge ${review.isApproved ? 'badge-success' : 'badge-warning'}`}>
                    {review.isApproved ? 'Approved' : 'Pending'}
                  </span>
                </div>
                <p className="text-sm text-gray-600 mb-2">
                  By {review.userName} on {new Date(review.createdAt).toLocaleDateString()}
                </p>
                <p className="text-sm text-gray-500 mb-2">Product: {review.productName}</p>
                <p className="text-gray-700">{review.comment}</p>
              </div>
              <div className="flex gap-2">
                {!review.isApproved && (
                  <button
                    onClick={() => handleApprove(review.id)}
                    className="p-2 text-green-600 hover:bg-green-50 rounded"
                    title="Approve"
                  >
                    <FaCheck />
                  </button>
                )}
                {review.isApproved && (
                  <button
                    onClick={() => handleReject(review.id)}
                    className="p-2 text-orange-600 hover:bg-orange-50 rounded"
                    title="Reject"
                  >
                    <FaTimes />
                  </button>
                )}
                <button
                  onClick={() => handleDelete(review.id)}
                  className="p-2 text-red-600 hover:bg-red-50 rounded"
                  title="Delete"
                >
                  <FaTrash />
                </button>
              </div>
            </div>
          </div>
        ))}

        {reviews.length === 0 && (
          <div className="text-center py-12 text-gray-400">
            No reviews found
          </div>
        )}
      </div>
    </div>
  );
}
