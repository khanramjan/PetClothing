import { useEffect, useState, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { FaShoppingCart, FaStar, FaArrowLeft } from 'react-icons/fa';
import { Product } from '@/types';
import api from '@/lib/api';
import { toast } from 'react-toastify';
import { useCartStore } from '@/store/cartStore';

const ProductDetail = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [product, setProduct] = useState<Product | null>(null);
  const [loading, setLoading] = useState(true);
  const [quantity, setQuantity] = useState(1);
  const [selectedSize, setSelectedSize] = useState<string>('');
  const { addToCart } = useCartStore();

  const fetchProduct = useCallback(async () => {
    if (!id) return;
    try {
      setLoading(true);
      const response = await api.get(`/products/${id}`);
      setProduct(response.data.data);
      // Set default size if available
      if (response.data.data.size) {
        const sizes = response.data.data.size.split(',').map((s: string) => s.trim());
        setSelectedSize(sizes[0]);
      }
    } catch (error) {
      toast.error('Failed to load product details');
      console.error(error);
      navigate('/products');
    } finally {
      setLoading(false);
    }
  }, [id, navigate]);

  useEffect(() => {
    fetchProduct();
  }, [fetchProduct]);

  const handleAddToCart = async () => {
    if (!product) return;

    try {
      await addToCart(product.id, quantity);
      toast.success('Product added to cart!');
      setQuantity(1);
    } catch (error) {
      toast.error('Failed to add product to cart');
      console.error(error);
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-50">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-600"></div>
      </div>
    );
  }

  if (!product) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <p className="text-gray-500 text-lg mb-4">Product not found</p>
          <button
            onClick={() => navigate('/products')}
            className="btn btn-primary inline-flex items-center gap-2"
          >
            <FaArrowLeft /> Back to Products
          </button>
        </div>
      </div>
    );
  }

  const sizes = product.size ? product.size.split(',').map((s) => s.trim()) : [];

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <div className="bg-white border-b">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4">
          <button
            onClick={() => navigate('/products')}
            className="flex items-center gap-2 text-primary-600 hover:text-primary-700 font-semibold"
          >
            <FaArrowLeft /> Back to Products
          </button>
        </div>
      </div>

      {/* Product Details */}
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-12">
          {/* Images */}
          <div>
            <div className="bg-white rounded-lg shadow-md overflow-hidden mb-4">
              <img
                src={product.images?.[0]?.imageUrl}
                alt={product.name}
                className="w-full h-auto object-cover"
                onError={(e) => {
                  (e.target as HTMLImageElement).src = 'data:image/svg+xml,%3Csvg xmlns=%22http://www.w3.org/2000/svg%22 width=%22400%22 height=%22400%22%3E%3Crect fill=%22%23e5e7eb%22 width=%22400%22 height=%22400%22/%3E%3Ctext x=%2250%25%22 y=%2250%25%22 dominant-baseline=%22middle%22 text-anchor=%22middle%22 font-family=%22sans-serif%22 font-size=%2248%22 fill=%22%239ca3af%22%3ENo Image%3C/text%3E%3C/svg%3E';
                }}
              />
            </div>
            {product.images && product.images.length > 1 && (
              <div className="grid grid-cols-4 gap-2">
                {product.images.map((img, idx) => (
                  <img
                    key={idx}
                    src={img.imageUrl}
                    alt={`${product.name} ${idx + 1}`}
                    className="w-full h-24 object-cover rounded-lg cursor-pointer hover:opacity-75 transition"
                    onError={(e) => {
                      (e.target as HTMLImageElement).src = 'data:image/svg+xml,%3Csvg xmlns=%22http://www.w3.org/2000/svg%22 width=%22400%22 height=%22400%22%3E%3Crect fill=%22%23e5e7eb%22 width=%22400%22 height=%22400%22/%3E%3C/svg%3E';
                    }}
                  />
                ))}
              </div>
            )}
          </div>

          {/* Details */}
          <div className="bg-white rounded-lg shadow-md p-8">
            {/* Title & Rating */}
            <h1 className="text-3xl font-bold text-gray-900 mb-2">{product.name}</h1>
            <div className="flex items-center gap-4 mb-6">
              <div className="flex items-center gap-1">
                {[...Array(5)].map((_, i) => (
                  <FaStar
                    key={i}
                    className={i < Math.round(product.rating || 0) ? 'text-yellow-400' : 'text-gray-300'}
                  />
                ))}
              </div>
              <span className="text-gray-600">
                {product.rating?.toFixed(1) || '0.0'} ({product.reviewCount || 0} reviews)
              </span>
            </div>

            {/* Price */}
            <div className="flex items-center gap-4 mb-6">
              {product.discountPrice ? (
                <>
                  <span className="text-3xl font-bold text-primary-600">
                    ${product.discountPrice}
                  </span>
                  <span className="text-xl text-gray-500 line-through">
                    ${product.price}
                  </span>
                  <span className="bg-red-100 text-red-800 px-3 py-1 rounded-full text-sm font-semibold">
                    Save ${(product.price - product.discountPrice).toFixed(2)}
                  </span>
                </>
              ) : (
                <span className="text-3xl font-bold text-primary-600">
                  ${product.price}
                </span>
              )}
            </div>

            {/* Description */}
            <p className="text-gray-600 mb-6 leading-relaxed">{product.description}</p>

            {/* Stock Status */}
            <div className="mb-6 p-4 bg-gray-50 rounded-lg">
              {product.stockQuantity > 0 ? (
                <p className="text-green-700 font-semibold">
                  ✓ {product.stockQuantity} in stock
                </p>
              ) : (
                <p className="text-red-700 font-semibold">Out of Stock</p>
              )}
            </div>

            {/* Product Info */}
            <div className="grid grid-cols-2 gap-4 mb-6 p-4 bg-gray-50 rounded-lg">
              <div>
                <p className="text-sm text-gray-600">Pet Type</p>
                <p className="font-semibold text-gray-900">{product.petType}</p>
              </div>
              <div>
                <p className="text-sm text-gray-600">Color</p>
                <p className="font-semibold text-gray-900">{product.color}</p>
              </div>
              <div>
                <p className="text-sm text-gray-600">Material</p>
                <p className="font-semibold text-gray-900">{product.material}</p>
              </div>
              <div>
                <p className="text-sm text-gray-600">SKU</p>
                <p className="font-semibold text-gray-900">{product.sku}</p>
              </div>
            </div>

            {/* Sizes */}
            {sizes.length > 0 && (
              <div className="mb-6">
                <label className="block text-sm font-semibold text-gray-700 mb-3">
                  Available Sizes
                </label>
                <div className="grid grid-cols-3 gap-2">
                  {sizes.map((size) => (
                    <button
                      key={size}
                      onClick={() => setSelectedSize(size)}
                      className={`py-2 px-3 border-2 rounded-lg font-semibold transition ${
                        selectedSize === size
                          ? 'border-primary-600 bg-primary-50 text-primary-700'
                          : 'border-gray-300 text-gray-700 hover:border-primary-300'
                      }`}
                    >
                      {size}
                    </button>
                  ))}
                </div>
              </div>
            )}

            {/* Quantity & Add to Cart */}
            <div className="mb-6 flex gap-4">
              <div className="flex-1">
                <label className="block text-sm font-semibold text-gray-700 mb-2">
                  Quantity
                </label>
                <div className="flex items-center border border-gray-300 rounded-lg">
                  <button
                    onClick={() => setQuantity(Math.max(1, quantity - 1))}
                    className="px-4 py-2 hover:bg-gray-100"
                  >
                    -
                  </button>
                  <input
                    type="number"
                    value={quantity}
                    onChange={(e) => setQuantity(Math.max(1, parseInt(e.target.value) || 1))}
                    className="flex-1 text-center border-0 focus:ring-0 focus:outline-none"
                    min="1"
                  />
                  <button
                    onClick={() => setQuantity(quantity + 1)}
                    className="px-4 py-2 hover:bg-gray-100"
                  >
                    +
                  </button>
                </div>
              </div>
            </div>

            <button
              onClick={handleAddToCart}
              disabled={product.stockQuantity <= 0}
              className="w-full btn btn-primary py-3 flex items-center justify-center gap-2 text-lg disabled:opacity-50 disabled:cursor-not-allowed"
            >
              <FaShoppingCart /> Add to Cart
            </button>

            {/* Additional Info */}
            <div className="mt-8 pt-8 border-t space-y-4">
              <div>
                <h3 className="font-semibold text-gray-900 mb-2">Shipping Information</h3>
                <p className="text-sm text-gray-600">
                  Free shipping on orders over $50. Standard shipping takes 5-7 business days.
                </p>
              </div>
              <div>
                <h3 className="font-semibold text-gray-900 mb-2">Return Policy</h3>
                <p className="text-sm text-gray-600">
                  30-day money-back guarantee on all products.
                </p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default ProductDetail;
