import { create } from 'zustand';
import { Cart, CartItem } from '@/types';
import api from '@/lib/api';

interface CartState {
  cart: Cart | null;
  isLoading: boolean;
  fetchCart: () => Promise<void>;
  addToCart: (productId: number, quantity: number) => Promise<void>;
  updateCartItem: (cartItemId: number, quantity: number) => Promise<void>;
  removeFromCart: (cartItemId: number) => Promise<void>;
  clearCart: () => Promise<void>;
}

export const useCartStore = create<CartState>((set) => ({
  cart: null,
  isLoading: false,

  fetchCart: async () => {
    set({ isLoading: true });
    try {
      const response = await api.get('/cart');
      set({ cart: response.data.data, isLoading: false });
    } catch (error) {
      set({ isLoading: false });
      throw error;
    }
  },

  addToCart: async (productId: number, quantity: number) => {
    try {
      const response = await api.post('/cart/add', { productId, quantity });
      set({ cart: response.data.data });
    } catch (error) {
      throw error;
    }
  },

  updateCartItem: async (cartItemId: number, quantity: number) => {
    try {
      const response = await api.put('/cart/update', { cartItemId, quantity });
      set({ cart: response.data.data });
    } catch (error) {
      throw error;
    }
  },

  removeFromCart: async (cartItemId: number) => {
    try {
      await api.delete(`/cart/${cartItemId}`);
      set((state) => ({
        cart: state.cart
          ? {
              ...state.cart,
              items: state.cart.items.filter((item) => item.id !== cartItemId),
            }
          : null,
      }));
    } catch (error) {
      throw error;
    }
  },

  clearCart: async () => {
    try {
      await api.delete('/cart/clear');
      set({ cart: null });
    } catch (error) {
      throw error;
    }
  },
}));
