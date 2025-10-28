import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { CartItem } from '@/types';
import api from '@/lib/api';
import { toast } from 'react-toastify';
import { FaTrash, FaMinus, FaPlus } from 'react-icons/fa';

const Cart = () => {
  const [cartItems, setCartItems] = useState<CartItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [updating, setUpdating] = useState<Record<number, boolean>>({});

  useEffect(() => {
    fetchCart();
  }, []);

  const fetchCart = async () => {
    try {
      setLoading(true);
      const response = await api.get('/cart');
      console.log('Cart response:', response.data);
      setCartItems(response.data.data?.items || []);
    } catch (error) {
      console.error('Error fetching cart:', error);
      toast.error('Failed to load cart');
      setCartItems([]);
    } finally {
      setLoading(false);
    }
  };

  const handleUpdateQuantity = async (cartItemId: number, newQuantity: number) => {
    if (newQuantity < 1) return;

    setUpdating({ ...updating, [cartItemId]: true });
    try {
      const response = await api.put(`/cart/items/${cartItemId}`, { quantity: newQuantity });
      console.log('Update response:', response.data);
      setCartItems(response.data.data?.items || []);
      toast.success('Cart updated');
    } catch (error) {
      console.error('Error updating cart:', error);
      toast.error('Failed to update cart');
    } finally {
      setUpdating({ ...updating, [cartItemId]: false });
    }
  };

  const handleRemoveItem = async (cartItemId: number) => {
    setUpdating({ ...updating, [cartItemId]: true });
    try {
      const response = await api.delete(`/cart/items/${cartItemId}`);
      console.log('Remove response:', response.data);
      setCartItems(response.data.data?.items || []);
      toast.success('Item removed from cart');
    } catch (error) {
      console.error('Error removing item:', error);
      toast.error('Failed to remove item');
    } finally {
      setUpdating({ ...updating, [cartItemId]: false });
    }
  };

  const subTotal = cartItems.reduce((sum, item) => sum + (item.subtotal || item.price * item.quantity), 0);
  const tax = subTotal * 0.1; // 10% tax
  const shipping = subTotal > 50 ? 0 : 9.99;
  const total = subTotal + tax + shipping;

  if (loading) {
    return (
      <div className="container mx-auto px-4 py-8">
        <div className="flex justify-center py-12">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-600"></div>
        </div>
      </div>
    );
  }

  if (cartItems.length === 0) {
    return (
      <div className="container mx-auto px-4 py-8">
        <h1 className="text-3xl font-bold mb-8">Shopping Cart</h1>
        <div className="text-center py-12">
          <p className="text-gray-600 mb-4">Your cart is empty</p>
          <Link to="/products" className="btn btn-primary">
            Continue Shopping
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="container mx-auto px-4 py-8">
      <h1 className="text-3xl font-bold mb-8">Shopping Cart</h1>
      
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        {/* Cart Items */}
        <div className="lg:col-span-2">
          <div className="bg-white rounded-lg shadow">
            {cartItems.map((item) => (
              <div key={item.id} className="flex gap-4 p-6 border-b last:border-b-0">
                {/* Product Image */}
                <div className="w-24 h-24 bg-gray-200 rounded flex-shrink-0">
                  <img
                    src={item.productImage || '/placeholder.jpg'}
                    alt={item.productName}
                    className="w-full h-full object-cover rounded"
                    onError={(e) => {
                      (e.target as HTMLImageElement).src = '/placeholder.jpg';
                    }}
                  />
                </div>

                {/* Product Details */}
                <div className="flex-1">
                  <h3 className="font-semibold text-lg mb-1">{item.productName}</h3>
                  <p className="text-gray-600 mb-3">Price: ${item.price.toFixed(2)}</p>

                  {/* Quantity Controls */}
                  <div className="flex items-center gap-2">
                    <button
                      onClick={() => handleUpdateQuantity(item.id, item.quantity - 1)}
                      disabled={updating[item.id] || item.quantity <= 1}
                      className="p-1 hover:bg-gray-200 rounded disabled:opacity-50"
                      title="Decrease quantity"
                    >
                      <FaMinus className="text-sm" />
                    </button>
                    <input
                      type="number"
                      min="1"
                      max={item.stockQuantity}
                      value={item.quantity}
                      onChange={(e) => handleUpdateQuantity(item.id, parseInt(e.target.value) || 1)}
                      disabled={updating[item.id]}
                      className="w-12 text-center border rounded px-2 py-1"
                    />
                    <button
                      onClick={() => handleUpdateQuantity(item.id, item.quantity + 1)}
                      disabled={updating[item.id] || item.quantity >= item.stockQuantity}
                      className="p-1 hover:bg-gray-200 rounded disabled:opacity-50"
                      title="Increase quantity"
                    >
                      <FaPlus className="text-sm" />
                    </button>
                  </div>
                </div>

                {/* Subtotal and Remove */}
                <div className="text-right">
                  <p className="font-semibold text-lg mb-4">${(item.subtotal || item.price * item.quantity).toFixed(2)}</p>
                  <button
                    onClick={() => handleRemoveItem(item.id)}
                    disabled={updating[item.id]}
                    className="text-red-600 hover:text-red-700 disabled:opacity-50"
                    title="Remove item"
                  >
                    <FaTrash />
                  </button>
                </div>
              </div>
            ))}
          </div>
        </div>

        {/* Order Summary */}
        <div className="lg:col-span-1">
          <div className="bg-white rounded-lg shadow p-6 sticky top-20">
            <h2 className="text-2xl font-bold mb-6">Order Summary</h2>

            <div className="space-y-3 mb-6">
              <div className="flex justify-between">
                <span className="text-gray-600">Subtotal</span>
                <span>${subTotal.toFixed(2)}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-gray-600">Tax (10%)</span>
                <span>${tax.toFixed(2)}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-gray-600">Shipping</span>
                <span>{shipping === 0 ? 'Free' : `$${shipping.toFixed(2)}`}</span>
              </div>
              <div className="border-t pt-3 flex justify-between font-semibold text-lg">
                <span>Total</span>
                <span className="text-primary-600">${total.toFixed(2)}</span>
              </div>
            </div>

            <Link to="/checkout" className="w-full btn btn-primary block text-center mb-3">
              Proceed to Checkout
            </Link>
            <Link to="/products" className="w-full btn btn-outline block text-center">
              Continue Shopping
            </Link>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Cart;
