import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import { User, AuthResponse } from '@/types';
import api from '@/lib/api';

interface AuthState {
  user: User | null;
  accessToken: string | null;
  refreshToken: string | null;
  isAuthenticated: boolean;
  login: (email: string, password: string) => Promise<void>;
  register: (data: { email: string; password: string; firstName: string; lastName: string; phoneNumber?: string }) => Promise<void>;
  logout: () => void;
  setAuth: (data: AuthResponse) => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      user: null,
      accessToken: null,
      refreshToken: null,
      isAuthenticated: false,

      login: async (email: string, password: string) => {
        const response = await api.post('/auth/login', { email, password });
        const { data } = response.data;
        
        // Handle both camelCase and PascalCase from backend
        const accessToken = data.accessToken || data.AccessToken;
        const refreshToken = data.refreshToken || data.RefreshToken;
        
        localStorage.setItem('accessToken', accessToken);
        localStorage.setItem('refreshToken', refreshToken);
        // Also set 'token' for backward compatibility
        localStorage.setItem('token', accessToken);
        
        set({
          user: {
            userId: data.userId || data.UserId,
            email: data.email || data.Email,
            firstName: data.firstName || data.FirstName,
            lastName: data.lastName || data.LastName,
            role: data.role || data.Role,
          },
          accessToken,
          refreshToken,
          isAuthenticated: true,
        });
      },

      register: async (data) => {
        const response = await api.post('/auth/register', data);
        const { data: authData } = response.data;
        
        // Handle both camelCase and PascalCase from backend
        const accessToken = authData.accessToken || authData.AccessToken;
        const refreshToken = authData.refreshToken || authData.RefreshToken;
        
        localStorage.setItem('accessToken', accessToken);
        localStorage.setItem('refreshToken', refreshToken);
        // Also set 'token' for backward compatibility
        localStorage.setItem('token', accessToken);
        
        set({
          user: {
            userId: authData.userId || authData.UserId,
            email: authData.email || authData.Email,
            firstName: authData.firstName || authData.FirstName,
            lastName: authData.lastName || authData.LastName,
            role: authData.role || authData.Role,
          },
          accessToken,
          refreshToken,
          isAuthenticated: true,
        });
      },

      logout: () => {
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
        localStorage.removeItem('token');
        set({
          user: null,
          accessToken: null,
          refreshToken: null,
          isAuthenticated: false,
        });
      },

      setAuth: (data: AuthResponse) => {
        localStorage.setItem('accessToken', data.accessToken);
        localStorage.setItem('refreshToken', data.refreshToken);
        
        set({
          user: {
            userId: data.userId,
            email: data.email,
            firstName: data.firstName,
            lastName: data.lastName,
            role: data.role,
          },
          accessToken: data.accessToken,
          refreshToken: data.refreshToken,
          isAuthenticated: true,
        });
      },
    }),
    {
      name: 'auth-storage',
      partialize: (state) => ({
        user: state.user,
        isAuthenticated: state.isAuthenticated,
        accessToken: state.accessToken,
        refreshToken: state.refreshToken,
      }),
    }
  )
);
