import React, { useEffect, useState, useCallback } from 'react';
import { FaPlus, FaEdit, FaTrash, FaSearch, FaImage, FaTimes, FaCheck, FaStar } from 'react-icons/fa';
import api from '@/lib/api';
import { toast } from 'react-toastify';
import { Product, Category } from '@/types';

export default function Products() {
  const [products, setProducts] = useState<Product[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [loading, setLoading] = useState(true);
  const [showModal, setShowModal] = useState(false);
  const [editingProduct, setEditingProduct] = useState<Product | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [filterCategory, setFilterCategory] = useState<number | ''>('');
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);

  const [formData, setFormData] = useState({
    name: '',
    description: '',
    price: '',
    discountPrice: '',
    categoryId: '',
    petType: '',
    sizes: [] as string[], // Changed to array for multiple sizes
    color: '',
    material: '',
    stockQuantity: '',
    sku: '',
    isFeatured: false,
    isActive: true,
    imageUrls: [''],
    imageFiles: [] as File[], // For direct file uploads
  });

  const [uploadingImages, setUploadingImages] = useState(false);

  const fetchProducts = useCallback(async () => {
    try {
      setLoading(true);
      const params = new URLSearchParams({
        page: currentPage.toString(),
        pageSize: '12',
        ...(searchTerm && { searchTerm }),
        ...(filterCategory && { categoryId: filterCategory.toString() }),
      });
      
      const response = await api.get(`/products?${params}`);
      setProducts(response.data.data.items);
      setTotalCount(response.data.data.totalCount);
      setTotalPages(Math.ceil(response.data.data.totalCount / 12));
    } catch (error) {
      toast.error('Failed to load products');
    } finally {
      setLoading(false);
    }
  }, [currentPage, searchTerm, filterCategory]);

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

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    try {
      setUploadingImages(true);
      
      // Upload image files if any
      const uploadedImageUrls: string[] = [];
      if (formData.imageFiles.length > 0) {
        for (const file of formData.imageFiles) {
          const formDataObj = new FormData();
          formDataObj.append('file', file);
          
          try {
            const uploadResponse = await api.post('/upload/image', formDataObj, {
              headers: { 'Content-Type': 'multipart/form-data' }
            });
            // The response is { url: "http://..." }
            uploadedImageUrls.push(uploadResponse.data.url);
          } catch (uploadError) {
            console.error('Failed to upload image:', uploadError);
            toast.error(`Failed to upload ${file.name}`);
          }
        }
      }
      
      // Combine uploaded URLs with manually entered URLs
      const allImageUrls = [
        ...uploadedImageUrls,
        ...formData.imageUrls.filter(url => url.trim() !== '')
      ];
      
      const payload = {
        ...formData,
        size: formData.sizes.join(','), // Convert array to comma-separated string
        price: parseFloat(formData.price),
        discountPrice: formData.discountPrice ? parseFloat(formData.discountPrice) : null,
        categoryId: parseInt(formData.categoryId),
        stockQuantity: parseInt(formData.stockQuantity),
        images: allImageUrls.map((url, index) => ({
          imageUrl: url,
          isPrimary: index === 0,
          displayOrder: index + 1,
        })),
      };

      if (editingProduct) {
        await api.put(`/products/${editingProduct.id}`, payload);
        toast.success('Product updated successfully!');
      } else {
        await api.post('/products', payload);
        toast.success('Product created successfully!');
      }
      
      setShowModal(false);
      resetForm();
      fetchProducts();
    } catch (error: unknown) {
      const err = error as { response?: { data?: { message?: string } } };
      toast.error(err.response?.data?.message || 'Failed to save product');
    } finally {
      setUploadingImages(false);
    }
  };

  const handleDelete = async (id: number) => {
    if (!window.confirm('Are you sure you want to delete this product?')) return;
    
    try {
      await api.delete(`/products/${id}`);
      toast.success('Product deleted successfully!');
      fetchProducts();
    } catch (error) {
      toast.error('Failed to delete product');
    }
  };

  const handleEdit = (product: Product) => {
    setEditingProduct(product);
    setFormData({
      name: product.name,
      description: product.description,
      price: product.price.toString(),
      discountPrice: product.discountPrice?.toString() || '',
      categoryId: product.categoryId.toString(),
      petType: product.petType || '',
      sizes: product.size ? product.size.split(',').map(s => s.trim()) : [],
      color: product.color || '',
      material: product.material || '',
      stockQuantity: product.stockQuantity.toString(),
      sku: product.sku,
      isFeatured: product.isFeatured,
      isActive: product.isActive,
      imageUrls: product.images?.map(img => img.imageUrl) || [''],
      imageFiles: [],
    });
    setShowModal(true);
  };

  const resetForm = () => {
    setEditingProduct(null);
    setFormData({
      name: '',
      description: '',
      price: '',
      discountPrice: '',
      categoryId: '',
      petType: '',
      sizes: [],
      color: '',
      material: '',
      stockQuantity: '',
      sku: '',
      isFeatured: false,
      isActive: true,
      imageUrls: [''],
      imageFiles: [],
    });
  };

  const addImageUrl = () => {
    setFormData({ ...formData, imageUrls: [...formData.imageUrls, ''] });
  };

  const removeImageUrl = (index: number) => {
    const newUrls = formData.imageUrls.filter((_, i) => i !== index);
    setFormData({ ...formData, imageUrls: newUrls.length > 0 ? newUrls : [''] });
  };

  const updateImageUrl = (index: number, value: string) => {
    const newUrls = [...formData.imageUrls];
    newUrls[index] = value;
    setFormData({ ...formData, imageUrls: newUrls });
  };

  if (loading) {
    return (
      <div className="flex flex-col items-center justify-center h-[70vh]">
        <div className="spinner mb-4"></div>
        <p className="text-gray-600">Loading products...</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Products</h1>
          <p className="text-gray-600 mt-1">{totalCount} total products</p>
        </div>
        <button
          onClick={() => {
            resetForm();
            setShowModal(true);
          }}
          className="btn btn-primary flex items-center gap-2 shadow-lg hover:shadow-xl transition"
        >
          <FaPlus /> Add New Product
        </button>
      </div>

      {/* Filters */}
      <div className="card">
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div className="relative">
            <FaSearch className="absolute left-4 top-1/2 transform -translate-y-1/2 text-gray-400" />
            <input
              type="text"
              placeholder="Search by name, SKU..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="input pl-12 w-full"
            />
          </div>
          <select
            value={filterCategory}
            onChange={(e) => setFilterCategory(e.target.value ? parseInt(e.target.value) : '')}
            className="input"
          >
            <option value="">All Categories</option>
            {categories.map((cat) => (
              <option key={cat.id} value={cat.id}>{cat.name}</option>
            ))}
          </select>
          <button
            onClick={() => {
              setSearchTerm('');
              setFilterCategory('');
              setCurrentPage(1);
            }}
            className="btn btn-secondary"
          >
            Clear Filters
          </button>
        </div>
      </div>

      {/* Products Grid */}
      {products.length === 0 ? (
        <div className="card text-center py-16">
          <FaImage className="mx-auto text-6xl text-gray-300 mb-4" />
          <h3 className="text-xl font-semibold text-gray-600 mb-2">No products found</h3>
          <p className="text-gray-500 mb-6">Get started by adding your first product</p>
          <button
            onClick={() => {
              resetForm();
              setShowModal(true);
            }}
            className="btn btn-primary inline-flex items-center gap-2"
          >
            <FaPlus /> Add Product
          </button>
        </div>
      ) : (
        <>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
            {products.map((product) => (
              <div key={product.id} className="card hover:shadow-xl transition-all duration-300 group">
                {/* Product Image */}
                <div className="relative overflow-hidden rounded-lg mb-4 bg-gray-100 aspect-square">
                  <img
                    src={product.images?.[0]?.imageUrl || '/placeholder.png'}
                    alt={product.name}
                    className="w-full h-full object-cover group-hover:scale-110 transition-transform duration-300"
                    onError={(e) => {
                      (e.target as HTMLImageElement).src = '/placeholder.png';
                    }}
                  />
                  {product.isFeatured && (
                    <div className="absolute top-2 left-2 bg-yellow-500 text-white px-3 py-1 rounded-full text-xs font-semibold flex items-center gap-1">
                      <FaStar /> Featured
                    </div>
                  )}
                  {product.stockQuantity < 10 && (
                    <div className="absolute top-2 right-2 bg-red-500 text-white px-3 py-1 rounded-full text-xs font-semibold">
                      Low Stock
                    </div>
                  )}
                  {!product.isActive && (
                    <div className="absolute inset-0 bg-black bg-opacity-50 flex items-center justify-center">
                      <span className="bg-red-600 text-white px-4 py-2 rounded-lg font-semibold">
                        Inactive
                      </span>
                    </div>
                  )}
                </div>

                {/* Product Info */}
                <div className="space-y-3">
                  <div>
                    <h3 className="font-bold text-lg text-gray-900 line-clamp-2 group-hover:text-primary-600 transition">
                      {product.name}
                    </h3>
                    <p className="text-sm text-gray-500 mt-1">SKU: {product.sku}</p>
                    {product.size && (
                      <div className="flex flex-wrap gap-1 mt-2">
                        {product.size.split(',').map((size, idx) => (
                          <span
                            key={idx}
                            className="bg-gray-100 text-gray-700 text-xs px-2 py-1 rounded font-medium"
                          >
                            {size.trim()}
                          </span>
                        ))}
                      </div>
                    )}
                  </div>

                  <div className="flex items-center justify-between">
                    <div>
                      <p className="text-2xl font-bold text-primary-600">
                        ${product.price.toFixed(2)}
                      </p>
                      {product.discountPrice && (
                        <p className="text-sm text-gray-400 line-through">
                          ${product.discountPrice.toFixed(2)}
                        </p>
                      )}
                    </div>
                    <div className="text-right">
                      <p className="text-sm font-semibold text-gray-700">Stock</p>
                      <p className={`text-lg font-bold ${
                        product.stockQuantity < 10 ? 'text-red-600' : 'text-green-600'
                      }`}>
                        {product.stockQuantity}
                      </p>
                    </div>
                  </div>

                  {/* Actions */}
                  <div className="flex gap-2 pt-3 border-t">
                    <button
                      onClick={() => handleEdit(product)}
                      className="flex-1 btn btn-secondary text-sm flex items-center justify-center gap-2"
                    >
                      <FaEdit /> Edit
                    </button>
                    <button
                      onClick={() => handleDelete(product.id)}
                      className="flex-1 btn bg-red-50 text-red-600 hover:bg-red-100 text-sm flex items-center justify-center gap-2"
                    >
                      <FaTrash /> Delete
                    </button>
                  </div>
                </div>
              </div>
            ))}
          </div>

          {/* Pagination */}
          {totalPages > 1 && (
            <div className="flex items-center justify-center gap-2 mt-8">
              <button
                onClick={() => setCurrentPage(Math.max(1, currentPage - 1))}
                disabled={currentPage === 1}
                className="btn btn-secondary disabled:opacity-50 disabled:cursor-not-allowed"
              >
                Previous
              </button>
              <span className="text-gray-600 px-4">
                Page {currentPage} of {totalPages}
              </span>
              <button
                onClick={() => setCurrentPage(Math.min(totalPages, currentPage + 1))}
                disabled={currentPage === totalPages}
                className="btn btn-secondary disabled:opacity-50 disabled:cursor-not-allowed"
              >
                Next
              </button>
            </div>
          )}
        </>
      )}

      {/* Modal */}
      {showModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50 overflow-y-auto">
          <div className="bg-white rounded-2xl shadow-2xl max-w-4xl w-full my-8 max-h-[90vh] overflow-y-auto">
            {/* Modal Header */}
            <div className="sticky top-0 bg-white border-b px-8 py-6 flex items-center justify-between rounded-t-2xl">
              <div>
                <h2 className="text-3xl font-bold text-gray-900">
                  {editingProduct ? 'Edit Product' : 'Add New Product'}
                </h2>
                <p className="text-gray-600 mt-1">
                  {editingProduct ? 'Update product information' : 'Fill in the details below'}
                </p>
              </div>
              <button
                onClick={() => {
                  setShowModal(false);
                  resetForm();
                }}
                className="p-2 hover:bg-gray-100 rounded-full transition"
              >
                <FaTimes className="text-xl text-gray-600" />
              </button>
            </div>

            {/* Modal Body */}
            <form onSubmit={handleSubmit} className="p-8 space-y-6">
              {/* Basic Info */}
              <div className="space-y-4">
                <h3 className="text-lg font-semibold text-gray-900 flex items-center gap-2 border-b pb-2">
                  <span className="w-8 h-8 bg-primary-100 text-primary-600 rounded-full flex items-center justify-center text-sm font-bold">
                    1
                  </span>
                  Basic Information
                </h3>
                
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-semibold text-gray-700 mb-2">
                      Product Name <span className="text-red-500">*</span>
                    </label>
                    <input
                      type="text"
                      required
                      value={formData.name}
                      onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                      className="input w-full"
                      placeholder="e.g., Cozy Dog Sweater"
                    />
                  </div>

                  <div>
                    <label className="block text-sm font-semibold text-gray-700 mb-2">
                      SKU <span className="text-red-500">*</span>
                    </label>
                    <input
                      type="text"
                      required
                      value={formData.sku}
                      onChange={(e) => setFormData({ ...formData, sku: e.target.value })}
                      className="input w-full"
                      placeholder="e.g., DOG-SW-001"
                    />
                  </div>
                </div>

                <div>
                  <label className="block text-sm font-semibold text-gray-700 mb-2">
                    Description <span className="text-red-500">*</span>
                  </label>
                  <textarea
                    required
                    rows={4}
                    value={formData.description}
                    onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                    className="input w-full resize-none"
                    placeholder="Describe your product in detail..."
                  />
                </div>
              </div>

              {/* Category & Type */}
              <div className="space-y-4">
                <h3 className="text-lg font-semibold text-gray-900 flex items-center gap-2 border-b pb-2">
                  <span className="w-8 h-8 bg-primary-100 text-primary-600 rounded-full flex items-center justify-center text-sm font-bold">
                    2
                  </span>
                  Category & Type
                </h3>
                
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-semibold text-gray-700 mb-2">
                      Category <span className="text-red-500">*</span>
                    </label>
                    <select
                      required
                      value={formData.categoryId}
                      onChange={(e) => setFormData({ ...formData, categoryId: e.target.value })}
                      className="input w-full"
                    >
                      <option value="">Select Category</option>
                      {categories.map((cat) => (
                        <option key={cat.id} value={cat.id}>{cat.name}</option>
                      ))}
                    </select>
                  </div>

                  <div>
                    <label className="block text-sm font-semibold text-gray-700 mb-2">
                      Pet Type
                    </label>
                    <select
                      value={formData.petType}
                      onChange={(e) => setFormData({ ...formData, petType: e.target.value })}
                      className="input w-full"
                    >
                      <option value="">Select Pet Type</option>
                      <option value="Dog">Dog</option>
                      <option value="Cat">Cat</option>
                      <option value="Bird">Bird</option>
                      <option value="Small Pet">Small Pet</option>
                      <option value="All Pets">All Pets</option>
                    </select>
                  </div>
                </div>
              </div>

              {/* Pricing */}
              <div className="space-y-4">
                <h3 className="text-lg font-semibold text-gray-900 flex items-center gap-2 border-b pb-2">
                  <span className="w-8 h-8 bg-primary-100 text-primary-600 rounded-full flex items-center justify-center text-sm font-bold">
                    3
                  </span>
                  Pricing & Stock
                </h3>
                
                <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                  <div>
                    <label className="block text-sm font-semibold text-gray-700 mb-2">
                      Price <span className="text-red-500">*</span>
                    </label>
                    <div className="relative">
                      <span className="absolute left-4 top-1/2 transform -translate-y-1/2 text-gray-500">$</span>
                      <input
                        type="number"
                        required
                        step="0.01"
                        min="0"
                        value={formData.price}
                        onChange={(e) => setFormData({ ...formData, price: e.target.value })}
                        className="input w-full pl-8"
                        placeholder="0.00"
                      />
                    </div>
                  </div>

                  <div>
                    <label className="block text-sm font-semibold text-gray-700 mb-2">
                      Discount Price
                    </label>
                    <div className="relative">
                      <span className="absolute left-4 top-1/2 transform -translate-y-1/2 text-gray-500">$</span>
                      <input
                        type="number"
                        step="0.01"
                        min="0"
                        value={formData.discountPrice}
                        onChange={(e) => setFormData({ ...formData, discountPrice: e.target.value })}
                        className="input w-full pl-8"
                        placeholder="0.00"
                      />
                    </div>
                  </div>

                  <div>
                    <label className="block text-sm font-semibold text-gray-700 mb-2">
                      Stock Quantity <span className="text-red-500">*</span>
                    </label>
                    <input
                      type="number"
                      required
                      min="0"
                      value={formData.stockQuantity}
                      onChange={(e) => setFormData({ ...formData, stockQuantity: e.target.value })}
                      className="input w-full"
                      placeholder="0"
                    />
                  </div>
                </div>
              </div>

              {/* Product Details */}
              <div className="space-y-4">
                <h3 className="text-lg font-semibold text-gray-900 flex items-center gap-2 border-b pb-2">
                  <span className="w-8 h-8 bg-primary-100 text-primary-600 rounded-full flex items-center justify-center text-sm font-bold">
                    4
                  </span>
                  Product Details
                </h3>
                
                <div className="grid grid-cols-1 gap-4">
                  {/* Multiple Size Selection */}
                  <div>
                    <label className="block text-sm font-semibold text-gray-700 mb-3">
                      Available Sizes (Select all that apply)
                    </label>
                    <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-6 gap-3">
                      {['XS', 'S', 'M', 'L', 'XL', 'XXL'].map((size) => (
                        <label
                          key={size}
                          className={`flex items-center justify-center p-3 border-2 rounded-lg cursor-pointer transition-all ${
                            formData.sizes.includes(size)
                              ? 'border-primary-600 bg-primary-50 text-primary-700 font-semibold'
                              : 'border-gray-300 hover:border-primary-300 text-gray-700'
                          }`}
                        >
                          <input
                            type="checkbox"
                            checked={formData.sizes.includes(size)}
                            onChange={(e) => {
                              if (e.target.checked) {
                                setFormData({ ...formData, sizes: [...formData.sizes, size] });
                              } else {
                                setFormData({ ...formData, sizes: formData.sizes.filter(s => s !== size) });
                              }
                            }}
                            className="sr-only"
                          />
                          <span className="text-sm font-medium">{size}</span>
                        </label>
                      ))}
                    </div>
                    {formData.sizes.length > 0 && (
                      <p className="text-sm text-gray-600 mt-2">
                        Selected: <span className="font-semibold text-primary-600">{formData.sizes.join(', ')}</span>
                      </p>
                    )}
                  </div>

                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <div>
                      <label className="block text-sm font-semibold text-gray-700 mb-2">Color</label>
                      <input
                        type="text"
                        value={formData.color}
                        onChange={(e) => setFormData({ ...formData, color: e.target.value })}
                        className="input w-full"
                        placeholder="e.g., Red, Blue, Multi"
                      />
                    </div>

                    <div>
                      <label className="block text-sm font-semibold text-gray-700 mb-2">Material</label>
                      <input
                        type="text"
                        value={formData.material}
                        onChange={(e) => setFormData({ ...formData, material: e.target.value })}
                        className="input w-full"
                        placeholder="e.g., Cotton, Polyester"
                      />
                    </div>
                  </div>
                </div>
              </div>

              {/* Images */}
              <div className="space-y-4">
                <div className="flex items-center justify-between border-b pb-2">
                  <h3 className="text-lg font-semibold text-gray-900 flex items-center gap-2">
                    <span className="w-8 h-8 bg-primary-100 text-primary-600 rounded-full flex items-center justify-center text-sm font-bold">
                      5
                    </span>
                    Product Images
                  </h3>
                </div>
                
                {/* Direct File Upload */}
                <div className="border-2 border-dashed border-gray-300 rounded-lg p-6 hover:border-primary-500 transition-colors">
                  <div className="text-center">
                    <FaImage className="mx-auto text-4xl text-gray-400 mb-3" />
                    <label className="cursor-pointer">
                      <span className="btn btn-primary inline-flex items-center gap-2">
                        <FaPlus /> Upload Images
                      </span>
                      <input
                        type="file"
                        multiple
                        accept="image/*"
                        className="hidden"
                        onChange={(e) => {
                          const files = Array.from(e.target.files || []);
                          setFormData({ ...formData, imageFiles: [...formData.imageFiles, ...files] });
                        }}
                      />
                    </label>
                    <p className="text-sm text-gray-500 mt-2">or drag and drop images here</p>
                    <p className="text-xs text-gray-400 mt-1">PNG, JPG, GIF up to 10MB</p>
                  </div>
                </div>

                {/* Preview Uploaded Files */}
                {formData.imageFiles.length > 0 && (
                  <div>
                    <h4 className="text-sm font-semibold text-gray-700 mb-2">Uploaded Files:</h4>
                    <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
                      {formData.imageFiles.map((file, index) => (
                        <div key={index} className="relative group">
                          <img
                            src={URL.createObjectURL(file)}
                            alt={file.name}
                            className="w-full h-24 object-cover rounded-lg border-2 border-gray-200"
                          />
                          <button
                            type="button"
                            onClick={() => {
                              const newFiles = formData.imageFiles.filter((_, i) => i !== index);
                              setFormData({ ...formData, imageFiles: newFiles });
                            }}
                            className="absolute top-1 right-1 p-1 bg-red-600 text-white rounded-full opacity-0 group-hover:opacity-100 transition"
                          >
                            <FaTimes size={12} />
                          </button>
                          <p className="text-xs text-gray-600 mt-1 truncate">{file.name}</p>
                        </div>
                      ))}
                    </div>
                  </div>
                )}

                {/* Image URLs */}
                <div>
                  <div className="flex items-center justify-between mb-3">
                    <h4 className="text-sm font-semibold text-gray-700">Or add image URLs:</h4>
                    <button
                      type="button"
                      onClick={addImageUrl}
                      className="text-sm text-primary-600 hover:text-primary-700 font-semibold flex items-center gap-1"
                    >
                      <FaPlus /> Add URL
                    </button>
                  </div>
                  
                  <div className="space-y-3">
                    {formData.imageUrls.map((url, index) => (
                      <div key={index} className="flex gap-2 items-start">
                        <div className="flex-1">
                          <div className="flex items-center gap-2 mb-1">
                            <FaImage className="text-gray-400" />
                            <label className="text-sm font-medium text-gray-700">
                              Image URL {index + 1}
                              {index === 0 && formData.imageFiles.length === 0 && (
                                <span className="ml-2 text-xs bg-primary-100 text-primary-600 px-2 py-0.5 rounded">
                                  Primary
                                </span>
                              )}
                            </label>
                          </div>
                          <input
                            type="url"
                            value={url}
                            onChange={(e) => updateImageUrl(index, e.target.value)}
                            className="input w-full"
                            placeholder="https://example.com/image.jpg"
                          />
                        </div>
                        {formData.imageUrls.length > 1 && (
                          <button
                            type="button"
                            onClick={() => removeImageUrl(index)}
                            className="mt-7 p-2 text-red-600 hover:bg-red-50 rounded transition"
                          >
                            <FaTrash />
                          </button>
                        )}
                      </div>
                    ))}
                  </div>
                </div>
              </div>

              {/* Options */}
              <div className="space-y-4">
                <h3 className="text-lg font-semibold text-gray-900 flex items-center gap-2 border-b pb-2">
                  <span className="w-8 h-8 bg-primary-100 text-primary-600 rounded-full flex items-center justify-center text-sm font-bold">
                    6
                  </span>
                  Options
                </h3>
                
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <label className="flex items-center gap-3 p-4 border-2 border-gray-200 rounded-lg cursor-pointer hover:border-primary-500 transition">
                    <input
                      type="checkbox"
                      checked={formData.isFeatured}
                      onChange={(e) => setFormData({ ...formData, isFeatured: e.target.checked })}
                      className="w-5 h-5 text-primary-600 rounded focus:ring-primary-500"
                    />
                    <div>
                      <p className="font-semibold text-gray-900">Featured Product</p>
                      <p className="text-sm text-gray-500">Show on homepage</p>
                    </div>
                  </label>

                  <label className="flex items-center gap-3 p-4 border-2 border-gray-200 rounded-lg cursor-pointer hover:border-green-500 transition">
                    <input
                      type="checkbox"
                      checked={formData.isActive}
                      onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
                      className="w-5 h-5 text-green-600 rounded focus:ring-green-500"
                    />
                    <div>
                      <p className="font-semibold text-gray-900">Active Product</p>
                      <p className="text-sm text-gray-500">Available for purchase</p>
                    </div>
                  </label>
                </div>
              </div>

              {/* Actions */}
              <div className="flex gap-3 pt-6 border-t sticky bottom-0 bg-white">
                <button
                  type="button"
                  onClick={() => {
                    setShowModal(false);
                    resetForm();
                  }}
                  className="flex-1 btn btn-secondary"
                  disabled={uploadingImages}
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  disabled={uploadingImages}
                  className="flex-1 btn btn-primary flex items-center justify-center gap-2 disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  {uploadingImages ? (
                    <>
                      <div className="spinner border-white"></div>
                      Uploading Images...
                    </>
                  ) : (
                    <>
                      <FaCheck />
                      {editingProduct ? 'Update Product' : 'Create Product'}
                    </>
                  )}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
