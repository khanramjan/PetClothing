import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import { User, AuthResponse } from '@/types';
import api from '@/lib/api';
import { supabase } from '@/lib/supabaseClient';
import { signOut as supabaseSignOut } from '@/lib/supabaseAuth';

interface SupabaseUser {
  email?: string;
  user_metadata?: {
    first_name?: string;
    last_name?: string;
    full_name?: string;
  };
}

interface AuthState {
  user: User | null;
  accessToken: string | null;
  refreshToken: string | null;
  isAuthenticated: boolean;
  login: (email: string, password: string) => Promise<void>;
  register: (data: { email: string; password: string; firstName: string; lastName: string; phoneNumber?: string }) => Promise<void>;
  logout: () => void;
  setAuth: (data: AuthResponse) => void;
  setSupabaseAuth: (accessToken: string, user: SupabaseUser) => void;
  initializeSupabaseAuth: () => Promise<void>;
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

      logout: async () => {
        // Sign out from Supabase
        await supabaseSignOut();
        
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

      setSupabaseAuth: (accessToken: string, supabaseUser: SupabaseUser) => {
        localStorage.setItem('accessToken', accessToken);
        localStorage.setItem('token', accessToken);
        
        // Extract user data from Supabase user object
        const names = supabaseUser.user_metadata?.full_name?.split(' ') || ['', ''];
        set({
          user: {
            userId: 0, // Will be synced with backend
            email: supabaseUser.email || '',
            firstName: supabaseUser.user_metadata?.first_name || names[0] || '',
            lastName: supabaseUser.user_metadata?.last_name || names[1] || '',
            role: 'Customer', // Default role, will be synced with backend
          },
          accessToken,
          refreshToken: null,
          isAuthenticated: true,
        });
      },

      initializeSupabaseAuth: async () => {
        // Check for existing Supabase session
        const { data: { session } } = await supabase.auth.getSession();
        
        if (session) {
          const accessToken = session.access_token;
          localStorage.setItem('accessToken', accessToken);
          localStorage.setItem('token', accessToken);
          
          const names = session.user.user_metadata?.full_name?.split(' ') || ['', ''];
          set({
            user: {
              userId: 0,
              email: session.user.email || '',
              firstName: session.user.user_metadata?.first_name || names[0] || '',
              lastName: session.user.user_metadata?.last_name || names[1] || '',
              role: 'Customer',
            },
            accessToken,
            refreshToken: null,
            isAuthenticated: true,
          });
        }
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
