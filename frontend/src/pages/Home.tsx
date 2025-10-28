import { Link } from 'react-router-dom';
import { useEffect, useState } from 'react';
import { Product, ProductImage } from '@/types';
import api from '@/lib/api';
import { toast } from 'react-toastify';

const Home = () => {
  const [featuredProducts, setFeaturedProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetchFeaturedProducts();
  }, []);

  const fetchFeaturedProducts = async () => {
    try {
      const response = await api.get('/products/featured?count=8');
      console.log('Featured products response:', response.data);
      
      // Transform API response to match Product type (handle both PascalCase and camelCase)
      const products = (response.data.data || response.data || []).map((p: Record<string, unknown>) => ({
        id: (p.id || p.Id) as number,
        name: (p.name || p.Name) as string,
        description: (p.description || p.Description) as string,
        price: (p.price || p.Price) as number,
        discountPrice: (p.discountPrice || p.DiscountPrice) as number | undefined,
        sku: (p.sku || p.SKU) as string,
        stockQuantity: (p.stockQuantity || p.StockQuantity) as number,
        categoryId: (p.categoryId || p.CategoryId) as number,
        categoryName: (p.categoryName || p.CategoryName) as string,
        petType: (p.petType || p.PetType) as string,
        size: (p.size || p.Size) as string,
        color: (p.color || p.Color) as string,
        material: (p.material || p.Material) as string,
        isActive: (p.isActive || p.IsActive) as boolean,
        isFeatured: (p.isFeatured || p.IsFeatured) as boolean,
        rating: ((p.rating || p.Rating) as number) || 0,
        reviewCount: ((p.reviewCount || p.ReviewCount) as number) || 0,
        images: (p.images || p.Images || []) as ProductImage[],
        createdAt: (p.createdAt || p.CreatedAt) as string,
      }));
      
      setFeaturedProducts(products);
    } catch (error) {
      console.error('Error fetching featured products:', error);
      toast.error('Failed to load featured products');
      setFeaturedProducts([]);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      {/* Hero Section */}
      <section className="bg-gradient-to-r from-primary-600 to-primary-800 text-white py-20">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center">
            <h1 className="text-5xl font-bold mb-4">
              Stylish Outfits for Your Furry Friends
            </h1>
            <p className="text-xl mb-8">
              Discover the perfect clothing for your beloved pets - comfortable, fashionable, and affordable!
            </p>
            <Link
              to="/products"
              className="inline-block bg-white text-primary-600 px-8 py-3 rounded-lg font-semibold hover:bg-gray-100 transition"
            >
              Shop Now
            </Link>
          </div>
        </div>
      </section>

      {/* Categories */}
      <section className="py-16 bg-white">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <h2 className="text-3xl font-bold text-center mb-12">Shop by Pet Type</h2>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
            {['Dog', 'Cat', 'Bird'].map((pet) => (
              <Link
                key={pet}
                to={`/products?petType=${pet}`}
                className="group relative overflow-hidden rounded-lg shadow-lg hover:shadow-xl transition"
              >
                <div className="aspect-w-16 aspect-h-9 bg-gradient-to-br from-primary-400 to-secondary-400 flex items-center justify-center">
                  <span className="text-6xl">
                    {pet === 'Dog' ? '🐕' : pet === 'Cat' ? '🐈' : '🦜'}
                  </span>
                </div>
                <div className="absolute bottom-0 left-0 right-0 bg-black bg-opacity-50 text-white p-4 text-center">
                  <h3 className="text-xl font-semibold">{pet} Clothing</h3>
                </div>
              </Link>
            ))}
          </div>
        </div>
      </section>

      {/* Featured Products */}
      <section className="py-16 bg-gray-50">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <h2 className="text-3xl font-bold text-center mb-12">Featured Products</h2>
          {loading ? (
            <div className="text-center py-12">
              <div className="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-primary-600"></div>
            </div>
          ) : featuredProducts && featuredProducts.length > 0 ? (
            <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6">
              {featuredProducts.map((product) => (
                <Link
                  key={product.id}
                  to={`/products/${product.id}`}
                  className="card hover:shadow-lg transition group"
                >
                  <div className="aspect-square bg-gray-200 rounded-lg mb-4 overflow-hidden">
                    <img
                      src={product.images?.length > 0 ? product.images[0].imageUrl : '/placeholder.jpg'}
                      alt={product.name}
                      className="w-full h-full object-cover group-hover:scale-110 transition duration-300"
                      onError={(e) => {
                        (e.target as HTMLImageElement).src = 'data:image/svg+xml,%3Csvg xmlns=%22http://www.w3.org/2000/svg%22 width=%22400%22 height=%22400%22%3E%3Crect fill=%22%23e5e7eb%22 width=%22400%22 height=%22400%22/%3E%3Ctext x=%2250%25%22 y=%2250%25%22 dominant-baseline=%22middle%22 text-anchor=%22middle%22 font-family=%22sans-serif%22 font-size=%2248%22 fill=%22%239ca3af%22%3ENo Image%3C/text%3E%3C/svg%3E';
                      }}
                    />
                  </div>
                  <h3 className="font-semibold text-lg mb-2 line-clamp-2">{product.name}</h3>
                  <div className="flex items-center justify-between">
                    <div>
                      {product.discountPrice ? (
                        <>
                          <span className="text-lg font-bold text-primary-600">
                            ${product.discountPrice}
                          </span>
                          <span className="text-sm text-gray-500 line-through ml-2">
                            ${product.price}
                          </span>
                        </>
                      ) : (
                        <span className="text-lg font-bold text-primary-600">
                          ${product.price}
                        </span>
                      )}
                    </div>
                    <div className="flex items-center text-sm text-yellow-500">
                      ⭐ {product.rating.toFixed(1)}
                    </div>
                  </div>
                </Link>
              ))}
            </div>
          ) : (
            <div className="text-center py-12">
              <p className="text-gray-600">No featured products available</p>
            </div>
          )}
        </div>
      </section>

      {/* Features */}
      <section className="py-16 bg-white">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="grid grid-cols-1 md:grid-cols-3 gap-8 text-center">
            <div>
              <div className="text-5xl mb-4">🚚</div>
              <h3 className="text-xl font-semibold mb-2">Free Shipping</h3>
              <p className="text-gray-600">On orders over $50</p>
            </div>
            <div>
              <div className="text-5xl mb-4">💯</div>
              <h3 className="text-xl font-semibold mb-2">Quality Guarantee</h3>
              <p className="text-gray-600">100% satisfaction guaranteed</p>
            </div>
            <div>
              <div className="text-5xl mb-4">🔄</div>
              <h3 className="text-xl font-semibold mb-2">Easy Returns</h3>
              <p className="text-gray-600">30-day return policy</p>
            </div>
          </div>
        </div>
      </section>
    </div>
  );
};

export default Home;
