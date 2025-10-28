import { create } from 'zustand';
import axios from 'axios';

export interface ShippingMethod {
  id: number;
  name: string;
  description: string;
  baseCost: number;
  minDeliveryDays: number;
  maxDeliveryDays: number;
  isActive: boolean;
}

export interface TaxRate {
  id: number;
  stateCode: string;
  stateName: string;
  taxPercentage: number;
  isActive: boolean;
}

export interface CheckoutLineItem {
  id: number;
  productId: number;
  productName: string;
  price: number;
  quantity: number;
  subtotal: number;
}

export interface CheckoutSummary {
  items: CheckoutLineItem[];
  subtotal: number;
  taxAmount: number;
  shippingCost: number;
  discountAmount: number;
  total: number;
  userAddresses: Address[];
  availableShippingMethods: ShippingMethod[];
}

interface Address {
  id: number;
  fullName: string;
  phoneNumber: string;
  addressLine1: string;
  addressLine2?: string;
  city: string;
  state: string;
  postalCode: string;
  country: string;
  isDefault: boolean;
}

export interface TaxCalculation {
  subtotal: number;
  taxRate: number;
  taxAmount: number;
  stateCode: string;
}

export interface ShippingCalculation {
  shippingMethodId: number;
  shippingMethodName: string;
  shippingCost: number;
  minDeliveryDays: number;
  maxDeliveryDays: number;
  estimatedDeliveryDate: string;
}

export interface OrderConfirmation {
  orderId: number;
  orderNumber: string;
  total: number;
  items: OrderItem[];
  shippingAddress: Address;
  shippingMethod: ShippingMethod;
  estimatedDelivery: string;
  createdAt: string;
}

interface OrderItem {
  id: number;
  productId: number;
  productName: string;
  productSKU: string;
  quantity: number;
  price: number;
  subtotal: number;
}

interface CheckoutState {
  // UI State
  currentStep: number;
  loading: boolean;
  error: string | null;

  // Data
  checkoutSummary: CheckoutSummary | null;
  selectedAddress: number | null;
  selectedShippingMethod: number | null;
  selectedTaxRate: string | null;
  couponCode: string | null;
  orderConfirmation: OrderConfirmation | null;

  // Actions
  setCurrentStep: (step: number) => void;
  setError: (error: string | null) => void;
  setSelectedAddress: (addressId: number | null) => void;
  setSelectedShippingMethod: (methodId: number | null) => void;
  setSelectedTaxRate: (stateCode: string | null) => void;
  setCouponCode: (code: string | null) => void;

  // API Calls
  fetchCheckoutSummary: (token: string) => Promise<void>;
  calculateTax: (stateCode: string, subtotal: number, token: string) => Promise<TaxCalculation>;
  calculateShipping: (shippingMethodId: number, stateCode: string, weight: number, token: string) => Promise<ShippingCalculation>;
  getShippingMethods: (token: string) => Promise<ShippingMethod[]>;
  getTaxRates: (token: string) => Promise<TaxRate[]>;
  createOrder: (addressId: number, shippingMethodId: number, couponCode: string | null, token: string) => Promise<OrderConfirmation>;
  resetCheckout: () => void;
}

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';

export const useCheckoutStore = create<CheckoutState>((set) => ({
  currentStep: 1,
  loading: false,
  error: null,
  checkoutSummary: null,
  selectedAddress: null,
  selectedShippingMethod: null,
  selectedTaxRate: 'US',
  couponCode: null,
  orderConfirmation: null,

  setCurrentStep: (step) => set({ currentStep: step }),
  setError: (error) => set({ error }),
  setSelectedAddress: (addressId) => set({ selectedAddress: addressId }),
  setSelectedShippingMethod: (methodId) => set({ selectedShippingMethod: methodId }),
  setSelectedTaxRate: (stateCode) => set({ selectedTaxRate: stateCode }),
  setCouponCode: (code) => set({ couponCode: code }),

  fetchCheckoutSummary: async (token: string) => {
    try {
      set({ loading: true, error: null });
      const response = await axios.get(`${API_URL}/checkout/summary`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      set({ checkoutSummary: response.data });
    } catch (error) {
      const message = (error as any)?.response?.data?.error || 'Failed to fetch checkout summary';
      set({ error: message });
      throw error;
    } finally {
      set({ loading: false });
    }
  },

  calculateTax: async (stateCode: string, subtotal: number, token: string) => {
    try {
      const response = await axios.post(
        `${API_URL}/checkout/calculate-tax`,
        { stateCode, subtotalAmount: subtotal },
        { headers: { Authorization: `Bearer ${token}` } }
      );
      return response.data;
    } catch (error) {
      const message = (error as any)?.response?.data?.error || 'Failed to calculate tax';
      set({ error: message });
      throw error;
    }
  },

  calculateShipping: async (shippingMethodId: number, stateCode: string, weight: number, token: string) => {
    try {
      const response = await axios.post(
        `${API_URL}/checkout/calculate-shipping`,
        { shippingMethodId, stateCode, weight },
        { headers: { Authorization: `Bearer ${token}` } }
      );
      return response.data;
    } catch (error) {
      const message = (error as any)?.response?.data?.error || 'Failed to calculate shipping';
      set({ error: message });
      throw error;
    }
  },

  getShippingMethods: async (token: string) => {
    try {
      const response = await axios.get(`${API_URL}/checkout/shipping-methods`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      return response.data;
    } catch (error) {
      const message = (error as any)?.response?.data?.error || 'Failed to fetch shipping methods';
      set({ error: message });
      throw error;
    }
  },

  getTaxRates: async (token: string) => {
    try {
      const response = await axios.get(`${API_URL}/checkout/tax-rates`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      return response.data;
    } catch (error) {
      const message = (error as any)?.response?.data?.error || 'Failed to fetch tax rates';
      set({ error: message });
      throw error;
    }
  },

  createOrder: async (addressId: number, shippingMethodId: number, couponCode: string | null, token: string) => {
    try {
      set({ loading: true, error: null });
      const response = await axios.post(
        `${API_URL}/checkout/create-order`,
        { addressId, shippingMethodId, couponCode },
        { headers: { Authorization: `Bearer ${token}` } }
      );
      set({ orderConfirmation: response.data });
      return response.data;
    } catch (error) {
      const message = (error as any)?.response?.data?.error || 'Failed to create order';
      set({ error: message });
      throw error;
    } finally {
      set({ loading: false });
    }
  },

  resetCheckout: () => set({
    currentStep: 1,
    loading: false,
    error: null,
    checkoutSummary: null,
    selectedAddress: null,
    selectedShippingMethod: null,
    selectedTaxRate: 'US',
    couponCode: null,
    orderConfirmation: null,
  }),
}));
