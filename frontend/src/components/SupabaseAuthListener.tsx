import { useEffect } from 'react';
import { supabase } from '@/lib/supabaseClient';
import { useAuthStore } from '@/store/authStore';
import api from '@/lib/api';
import { toast } from 'react-toastify';

export const SupabaseAuthListener = () => {
  const { setSupabaseAuth, initializeSupabaseAuth, setAuth } = useAuthStore();

  useEffect(() => {
    // Initialize auth state on mount
    initializeSupabaseAuth();

    // Listen for auth changes
    const { data: { subscription } } = supabase.auth.onAuthStateChange(
      async (event, session) => {
        if (event === 'SIGNED_IN' && session) {
          // Set Supabase auth state
          setSupabaseAuth(session.access_token, session.user);
          
          // Sync OAuth user with backend database
          try {
            const names = session.user.user_metadata?.full_name?.split(' ') || [];
            const response = await api.post('/auth/sync-oauth-user', {
              email: session.user.email,
              firstName: session.user.user_metadata?.first_name || names[0] || '',
              lastName: session.user.user_metadata?.last_name || names[1] || '',
              provider: 'google',
              providerUserId: session.user.id
            });
            
            if (response.data.success) {
              // Update store with backend user data including internal JWT
              setAuth(response.data.data);
              toast.success('Signed in successfully!');
            }
          } catch (error) {
            console.error('Error syncing OAuth user:', error);
            toast.error('Signed in but failed to sync user data');
          }
        } else if (event === 'SIGNED_OUT') {
          // Auth store logout will handle cleanup
        }
      }
    );

    return () => {
      subscription.unsubscribe();
    };
  }, [setSupabaseAuth, initializeSupabaseAuth, setAuth]);

  return null;
};
