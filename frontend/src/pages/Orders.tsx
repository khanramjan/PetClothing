import React, { useEffect, useState, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import axios from 'axios';
import { useAuthStore } from '@/store/authStore';

interface OrderItem {
  id: number;
  productId: number;
  productName: string;
  productSKU: string;
  productImage: string;
  quantity: number;
  price: number;
  subtotal: number;
}

interface Order {
  id: number;
  orderNumber: string;
  createdAt: string;
  status: string;
  paymentStatus: string;
  total: number;
  subTotal: number;
  shippingCost: number;
  tax: number;
  items: OrderItem[];
  shippingAddress: {
    fullName: string;
    phoneNumber: string;
    addressLine1: string;
    addressLine2?: string;
    city: string;
    state: string;
    postalCode: string;
    country: string;
  };
  paymentMethod: string;
}

const Orders: React.FC = () => {
  const [orders, setOrders] = useState<Order[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [expandedOrderId, setExpandedOrderId] = useState<number | null>(null);
  const navigate = useNavigate();
  const { accessToken, isAuthenticated, logout } = useAuthStore();

  const fetchOrders = useCallback(async () => {
    try {
      // Check if user is authenticated
      if (!isAuthenticated || !accessToken) {
        console.log('❌ Not authenticated, redirecting to login');
        navigate('/login');
        return;
      }

      console.log('✅ Token found, making request...');
      const apiUrl = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';
      const response = await axios.get(`${apiUrl}/orders`, {
        headers: { Authorization: `Bearer ${accessToken}` }
      });

      if (response.data.success) {
        setOrders(response.data.data);
      }
    } catch (err: unknown) {
      const error = err as { response?: { status?: number } };
      if (error.response?.status === 401) {
        console.log('❌ 401 Unauthorized, logging out');
        logout();
        navigate('/login');
      } else {
        setError('Failed to load orders');
      }
    } finally {
      setLoading(false);
    }
  }, [isAuthenticated, accessToken, navigate, logout]);

  useEffect(() => {
    fetchOrders();
  }, [fetchOrders]);

  const getStatusColor = (status: string) => {
    const colors: { [key: string]: string } = {
      'Pending': 'bg-yellow-100 text-yellow-800',
      'Processing': 'bg-blue-100 text-blue-800',
      'Shipped': 'bg-purple-100 text-purple-800',
      'Delivered': 'bg-green-100 text-green-800',
      'Cancelled': 'bg-red-100 text-red-800',
    };
    return colors[status] || 'bg-gray-100 text-gray-800';
  };

  const getPaymentStatusColor = (status: string) => {
    const colors: { [key: string]: string } = {
      'Paid': 'bg-green-100 text-green-800',
      'Pending': 'bg-yellow-100 text-yellow-800',
      'Failed': 'bg-red-100 text-red-800',
    };
    return colors[status] || 'bg-gray-100 text-gray-800';
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-purple-50 via-pink-50 to-blue-50 flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-16 w-16 border-b-2 border-purple-600 mx-auto mb-4"></div>
          <p className="text-gray-600">Loading your orders...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-purple-50 via-pink-50 to-blue-50 py-12 px-4 sm:px-6 lg:px-8">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-4xl font-bold bg-gradient-to-r from-purple-600 to-pink-600 bg-clip-text text-transparent mb-2">
            My Orders
          </h1>
          <p className="text-gray-600">View and track your orders</p>
        </div>

        {error && (
          <div className="mb-6 bg-red-50 border-l-4 border-red-500 p-4 rounded-r-xl">
            <p className="text-red-700">{error}</p>
          </div>
        )}

        {orders.length === 0 ? (
          <div className="bg-white rounded-2xl shadow-lg p-12 text-center">
            <svg className="w-24 h-24 mx-auto mb-6 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 11V7a4 4 0 00-8 0v4M5 9h14l1 12H4L5 9z" />
            </svg>
            <h3 className="text-2xl font-bold text-gray-700 mb-2">No Orders Yet</h3>
            <p className="text-gray-500 mb-6">Start shopping to see your orders here!</p>
            <button
              onClick={() => navigate('/shop')}
              className="inline-flex items-center px-6 py-3 bg-gradient-to-r from-purple-600 to-pink-600 text-white rounded-xl hover:from-purple-700 hover:to-pink-700 font-semibold transition-all duration-300"
            >
              <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 3h2l.4 2M7 13h10l4-8H5.4M7 13L5.4 5M7 13l-2.293 2.293c-.63.63-.184 1.707.707 1.707H17m0 0a2 2 0 100 4 2 2 0 000-4zm-8 2a2 2 0 11-4 0 2 2 0 014 0z" />
              </svg>
              Start Shopping
            </button>
          </div>
        ) : (
          <div className="space-y-6">
            {orders.map((order) => (
              <div key={order.id} className="bg-white rounded-2xl shadow-lg overflow-hidden hover:shadow-xl transition-shadow duration-300">
                {/* Order Header */}
                <div className="bg-gradient-to-r from-purple-600 to-pink-600 p-6 text-white">
                  <div className="flex flex-wrap items-center justify-between gap-4">
                    <div>
                      <p className="text-sm opacity-90">Order Number</p>
                      <p className="text-xl font-bold">{order.orderNumber}</p>
                    </div>
                    <div className="text-right">
                      <p className="text-sm opacity-90">Order Date</p>
                      <p className="font-semibold">{new Date(order.createdAt).toLocaleDateString()}</p>
                    </div>
                    <div className="text-right">
                      <p className="text-sm opacity-90">Total Amount</p>
                      <p className="text-2xl font-bold">${order.total.toFixed(2)}</p>
                      <div className="text-xs opacity-75 mt-1 space-y-0.5">
                        <p>Subtotal: ${order.subTotal.toFixed(2)}</p>
                        <p>Tax: ${order.tax.toFixed(2)}</p>
                        <p>Shipping: ${order.shippingCost.toFixed(2)}</p>
                      </div>
                    </div>
                  </div>
                </div>

                {/* Order Body */}
                <div className="p-6">
                  {/* Status Badges */}
                  <div className="flex flex-wrap gap-3 mb-6">
                    <span className={`px-4 py-2 rounded-full text-sm font-semibold ${getStatusColor(order.status)}`}>
                      📦 {order.status}
                    </span>
                    <span className={`px-4 py-2 rounded-full text-sm font-semibold ${getPaymentStatusColor(order.paymentStatus)}`}>
                      💳 {order.paymentStatus}
                    </span>
                    <span className="px-4 py-2 rounded-full text-sm font-semibold bg-gray-100 text-gray-800">
                      {order.paymentMethod || 'SSLCommerz'}
                    </span>
                  </div>

                  {/* Collapsible Details */}
                  {expandedOrderId === order.id && (
                    <>
                      {/* Order Items */}
                      <div className="space-y-4 mb-6">
                        <h4 className="font-semibold text-gray-900 mb-3">Order Items</h4>
                        {order.items?.map((item, index) => (
                          <div key={index} className="flex items-center gap-4 p-4 bg-gray-50 rounded-xl hover:bg-gray-100 transition-colors">
                            <div className="w-20 h-20 bg-white rounded-lg flex items-center justify-center overflow-hidden">
                              {item.productImage ? (
                                <img src={item.productImage} alt={item.productName} className="w-full h-full object-cover" />
                              ) : (
                                <svg className="w-10 h-10 text-gray-400" fill="currentColor" viewBox="0 0 20 20">
                                  <path fillRule="evenodd" d="M4 3a2 2 0 00-2 2v10a2 2 0 002 2h12a2 2 0 002-2V5a2 2 0 00-2-2H4zm12 12H4l4-8 3 6 2-4 3 6z" clipRule="evenodd" />
                                </svg>
                              )}
                            </div>
                            <div className="flex-1">
                              <h4 className="font-semibold text-gray-900">{item.productName}</h4>
                              <p className="text-sm text-gray-600">SKU: {item.productSKU} • Qty: {item.quantity}</p>
                            </div>
                            <div className="text-right">
                              <p className="font-bold text-gray-900">${item.subtotal.toFixed(2)}</p>
                              <p className="text-sm text-gray-600">${item.price.toFixed(2)} each</p>
                            </div>
                          </div>
                        ))}
                      </div>

                      {/* Shipping Address */}
                      {order.shippingAddress && (
                        <div className="bg-blue-50 border-l-4 border-blue-500 p-4 rounded-r-xl mb-4">
                          <div className="flex items-start">
                            <svg className="w-5 h-5 text-blue-500 mr-3 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
                              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
                            </svg>
                            <div>
                              <p className="text-sm font-semibold text-blue-900">Shipping Address</p>
                              <p className="text-sm text-blue-700 mt-1">
                                {order.shippingAddress.fullName}<br />
                                {order.shippingAddress.addressLine1}
                                {order.shippingAddress.addressLine2 && `, ${order.shippingAddress.addressLine2}`}<br />
                                {order.shippingAddress.city}, {order.shippingAddress.state} {order.shippingAddress.postalCode}<br />
                                {order.shippingAddress.country}<br />
                                Phone: {order.shippingAddress.phoneNumber}
                              </p>
                            </div>
                          </div>
                        </div>
                      )}
                    </>
                  )}

                  {/* Price Breakdown */}
                  <div className="bg-gray-50 border border-gray-200 rounded-xl p-4 mb-4">
                    <h4 className="font-semibold text-gray-900 mb-3">Order Summary</h4>
                    <div className="space-y-2 text-sm">
                      <div className="flex justify-between">
                        <span className="text-gray-600">Subtotal</span>
                        <span className="font-semibold text-gray-900">${order.subTotal.toFixed(2)}</span>
                      </div>
                      <div className="flex justify-between">
                        <span className="text-gray-600">Tax (10%)</span>
                        <span className="font-semibold text-gray-900">${order.tax.toFixed(2)}</span>
                      </div>
                      <div className="flex justify-between">
                        <span className="text-gray-600">Shipping</span>
                        <span className="font-semibold text-gray-900">${order.shippingCost.toFixed(2)}</span>
                      </div>
                      <div className="border-t border-gray-300 pt-2 mt-2">
                        <div className="flex justify-between">
                          <span className="font-bold text-gray-900">Total</span>
                          <span className="font-bold text-purple-600 text-lg">${order.total.toFixed(2)}</span>
                        </div>
                      </div>
                    </div>
                  </div>

                  {/* Actions */}
                  <div className="flex flex-wrap gap-3">
                    <button
                      onClick={() => setExpandedOrderId(expandedOrderId === order.id ? null : order.id)}
                      className="flex-1 min-w-[200px] px-6 py-3 bg-gradient-to-r from-purple-600 to-pink-600 text-white rounded-xl hover:from-purple-700 hover:to-pink-700 font-semibold transition-all duration-300 flex items-center justify-center"
                    >
                      <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d={expandedOrderId === order.id ? "M5 15l7-7 7 7" : "M19 9l-7 7-7-7"} />
                      </svg>
                      {expandedOrderId === order.id ? 'Hide Details' : 'View Details'}
                    </button>
                    {order.status === 'Pending' && (
                      <button className="px-6 py-3 bg-red-100 text-red-700 rounded-xl hover:bg-red-200 font-semibold transition-colors duration-300">
                        Cancel Order
                      </button>
                    )}
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
};

export default Orders;
