export interface User {
  userId: number;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
}

export interface AuthResponse {
  userId: number;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  phoneNumber?: string;
}

export interface Product {
  id: number;
  name: string;
  description: string;
  price: number;
  discountPrice?: number;
  sku: string;
  stockQuantity: number;
  categoryId: number;
  categoryName: string;
  petType: string;
  size: string;
  color: string;
  material: string;
  isActive: boolean;
  isFeatured: boolean;
  rating: number;
  reviewCount: number;
  images: ProductImage[];
  createdAt: string;
}

export interface ProductImage {
  id: number;
  imageUrl: string;
  altText?: string;
  isPrimary: boolean;
  displayOrder: number;
}

export interface ProductFilter {
  searchTerm?: string;
  categoryId?: number;
  petType?: string;
  size?: string;
  minPrice?: number;
  maxPrice?: number;
  isFeatured?: boolean;
  sortBy?: 'price_asc' | 'price_desc' | 'name' | 'rating' | 'newest';
  page?: number;
  pageSize?: number;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface CartItem {
  id: number;
  productId: number;
  productName: string;
  productImage: string;
  price: number;
  quantity: number;
  subtotal: number;
  stockQuantity: number;
}

export interface Cart {
  id: number;
  items: CartItem[];
  subTotal: number;
  totalItems: number;
}

export interface Category {
  id: number;
  name: string;
  description: string;
  imageUrl?: string;
  parentCategoryId?: number;
  isActive: boolean;
  displayOrder: number;
  subCategories: Category[];
}

export interface Order {
  id: number;
  orderNumber: string;
  status: string;
  paymentStatus: string;
  subTotal: number;
  shippingCost: number;
  tax: number;
  total: number;
  shippingAddress: Address;
  items: OrderItem[];
  createdAt: string;
  shippedAt?: string;
  deliveredAt?: string;
}

export interface OrderItem {
  id: number;
  productId: number;
  productName: string;
  productSKU: string;
  productImage: string;
  quantity: number;
  price: number;
  subtotal: number;
}

export interface Address {
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

export interface Review {
  id: number;
  productId: number;
  userName: string;
  rating: number;
  title: string;
  comment: string;
  isVerifiedPurchase: boolean;
  createdAt: string;
}

export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  message?: string;
}
