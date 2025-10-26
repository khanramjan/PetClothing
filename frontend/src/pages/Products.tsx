import { useEffect, useState, useCallback } from 'react';
import { useSearchParams, Link } from 'react-router-dom';
import { FaFilter, FaSearch } from 'react-icons/fa';
import { Product, Category } from '@/types';
import api from '@/lib/api';
import { toast } from 'react-toastify';

const Products = () => {
  const [products, setProducts] = useState<Product[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchParams, setSearchParams] = useSearchParams();
  
  const searchTerm = searchParams.get('search') || '';
  const petType = searchParams.get('petType') || '';
  const categoryId = searchParams.get('categoryId') || '';
  const currentPage = parseInt(searchParams.get('page') || '1');
  
  const [totalPages, setTotalPages] = useState(1);

  const fetchProducts = useCallback(async () => {
    try {
      setLoading(true);
      const params = new URLSearchParams({
        page: currentPage.toString(),
        pageSize: '12',
        ...(searchTerm && { searchTerm }),
        ...(petType && { petType }),
        ...(categoryId && { categoryId }),
      });
      
      const response = await api.get(`/products?${params}`);
      setProducts(response.data.data.items);
      setTotalPages(Math.ceil(response.data.data.totalCount / 12));
    } catch (error) {
      toast.error('Failed to load products');
      console.error(error);
    } finally {
      setLoading(false);
    }
  }, [searchTerm, petType, categoryId, currentPage]);

  useEffect(() => {
    fetchProducts();
    fetchCategories();
  }, [fetchProducts]);

  const fetchCategories = async () => {
    try {
      const response = await api.get('/categories');
      setCategories(response.data.data);
    } catch (error) {
      console.error('Failed to load categories', error);
    }
  };

  const handleSearch = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const formData = new FormData(e.currentTarget);
    const term = formData.get('search') as string;
    if (term) {
      setSearchParams({ search: term, page: '1' });
    }
  };

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <div className="bg-white border-b">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
          <h1 className="text-3xl font-bold text-gray-900 mb-4">Products</h1>
          <p className="text-gray-600">Browse our collection of pet clothing</p>
        </div>
      </div>

      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="grid grid-cols-1 lg:grid-cols-4 gap-8">
          {/* Sidebar Filters */}
          <div className="lg:col-span-1">
            <div className="bg-white rounded-lg shadow-md p-6 sticky top-20">
              <h2 className="text-lg font-semibold mb-4 flex items-center gap-2">
                <FaFilter /> Filters
              </h2>

              {/* Search */}
              <form onSubmit={handleSearch} className="mb-6">
                <div className="relative">
                  <input
                    type="text"
                    name="search"
                    placeholder="Search products..."
                    defaultValue={searchTerm}
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                  />
                  <button
                    type="submit"
                    className="absolute right-2 top-1/2 -translate-y-1/2 text-gray-500 hover:text-primary-600"
                  >
                    <FaSearch />
                  </button>
                </div>
              </form>

              {/* Pet Type Filter */}
              <div className="mb-6">
                <h3 className="font-semibold text-gray-900 mb-3">Pet Type</h3>
                <div className="space-y-2">
                  {['Dog', 'Cat', 'Bird'].map((type) => (
                    <label key={type} className="flex items-center gap-2 cursor-pointer">
                      <input
                        type="radio"
                        name="petType"
                        value={type}
                        checked={petType === type}
                        onChange={(e) => {
                          if (e.target.checked) {
                            setSearchParams({ petType: type, page: '1' });
                          } else {
                            const params = new URLSearchParams(searchParams);
                            params.delete('petType');
                            params.set('page', '1');
                            setSearchParams(params);
                          }
                        }}
                        className="w-4 h-4"
                      />
                      <span className="text-gray-700">{type}</span>
                    </label>
                  ))}
                  <label className="flex items-center gap-2 cursor-pointer">
                    <input
                      type="radio"
                      name="petType"
                      value=""
                      checked={!petType}
                      onChange={() => {
                        const params = new URLSearchParams(searchParams);
                        params.delete('petType');
                        params.set('page', '1');
                        setSearchParams(params);
                      }}
                      className="w-4 h-4"
                    />
                    <span className="text-gray-700">All Types</span>
                  </label>
                </div>
              </div>

              {/* Category Filter */}
              <div className="mb-6">
                <h3 className="font-semibold text-gray-900 mb-3">Category</h3>
                <div className="space-y-2">
                  {categories.map((cat) => (
                    <label key={cat.id} className="flex items-center gap-2 cursor-pointer">
                      <input
                        type="radio"
                        name="category"
                        value={cat.id}
                        checked={categoryId === cat.id.toString()}
                        onChange={(e) => {
                          if (e.target.checked) {
                            setSearchParams({ categoryId: cat.id.toString(), page: '1' });
                          } else {
                            const params = new URLSearchParams(searchParams);
                            params.delete('categoryId');
                            params.set('page', '1');
                            setSearchParams(params);
                          }
                        }}
                        className="w-4 h-4"
                      />
                      <span className="text-gray-700">{cat.name}</span>
                    </label>
                  ))}
                  <label className="flex items-center gap-2 cursor-pointer">
                    <input
                      type="radio"
                      name="category"
                      value=""
                      checked={!categoryId}
                      onChange={() => {
                        const params = new URLSearchParams(searchParams);
                        params.delete('categoryId');
                        params.set('page', '1');
                        setSearchParams(params);
                      }}
                      className="w-4 h-4"
                    />
                    <span className="text-gray-700">All Categories</span>
                  </label>
                </div>
              </div>
            </div>
          </div>

          {/* Products Grid */}
          <div className="lg:col-span-3">
            {loading ? (
              <div className="text-center py-12">
                <div className="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-primary-600"></div>
              </div>
            ) : products.length === 0 ? (
              <div className="text-center py-12 bg-white rounded-lg">
                <p className="text-gray-500 text-lg">No products found</p>
              </div>
            ) : (
              <>
                <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-6 mb-8">
                  {products.map((product) => (
                    <Link
                      key={product.id}
                      to={`/products/${product.id}`}
                      className="group bg-white rounded-lg shadow-md hover:shadow-lg transition overflow-hidden"
                    >
                      <div className="aspect-square bg-gray-200 overflow-hidden">
                        <img
                          src={product.images?.[0]?.imageUrl}
                          alt={product.name}
                          className="w-full h-full object-cover group-hover:scale-110 transition duration-300"
                          onError={(e) => {
                            (e.target as HTMLImageElement).src = 'data:image/svg+xml,%3Csvg xmlns=%22http://www.w3.org/2000/svg%22 width=%22400%22 height=%22400%22%3E%3Crect fill=%22%23e5e7eb%22 width=%22400%22 height=%22400%22/%3E%3Ctext x=%2250%25%22 y=%2250%25%22 dominant-baseline=%22middle%22 text-anchor=%22middle%22 font-family=%22sans-serif%22 font-size=%2248%22 fill=%22%239ca3af%22%3ENo Image%3C/text%3E%3C/svg%3E';
                          }}
                        />
                      </div>
                      <div className="p-4">
                        <h3 className="font-semibold text-gray-900 line-clamp-2 mb-2">
                          {product.name}
                        </h3>
                        <div className="flex items-center justify-between mb-2">
                          <div>
                            {product.discountPrice ? (
                              <>
                                <span className="text-lg font-bold text-primary-600">
                                  ${product.discountPrice}
                                </span>
                                <span className="ml-2 text-sm text-gray-500 line-through">
                                  ${product.price}
                                </span>
                              </>
                            ) : (
                              <span className="text-lg font-bold text-primary-600">
                                ${product.price}
                              </span>
                            )}
                          </div>
                          <div className="flex items-center gap-1">
                            <span className="text-yellow-400">★</span>
                            <span className="text-sm font-semibold">
                              {product.rating?.toFixed(1) || '0.0'}
                            </span>
                          </div>
                        </div>
                        <p className="text-xs text-gray-500">
                          {product.stockQuantity > 0 ? `${product.stockQuantity} in stock` : 'Out of stock'}
                        </p>
                      </div>
                    </Link>
                  ))}
                </div>

                {/* Pagination */}
                {totalPages > 1 && (
                  <div className="flex justify-center items-center gap-2">
                    <button
                      onClick={() => {
                        const newPage = Math.max(1, currentPage - 1);
                        setSearchParams({ ...Object.fromEntries(searchParams), page: newPage.toString() });
                      }}
                      disabled={currentPage === 1}
                      className="px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-50"
                    >
                      Previous
                    </button>
                    {Array.from({ length: totalPages }, (_, i) => i + 1).map((page) => (
                      <button
                        key={page}
                        onClick={() => {
                          setSearchParams({ ...Object.fromEntries(searchParams), page: page.toString() });
                        }}
                        className={`px-3 py-2 rounded-lg ${
                          currentPage === page
                            ? 'bg-primary-600 text-white'
                            : 'border border-gray-300 hover:bg-gray-50'
                        }`}
                      >
                        {page}
                      </button>
                    ))}
                    <button
                      onClick={() => {
                        const newPage = Math.min(totalPages, currentPage + 1);
                        setSearchParams({ ...Object.fromEntries(searchParams), page: newPage.toString() });
                      }}
                      disabled={currentPage === totalPages}
                      className="px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-50"
                    >
                      Next
                    </button>
                  </div>
                )}
              </>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

export default Products;
