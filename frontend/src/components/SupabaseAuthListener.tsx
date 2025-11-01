import { useEffect } from 'react';
import { supabase } from '@/lib/supabaseClient';
import { useAuthStore } from '@/store/authStore';

export const SupabaseAuthListener = () => {
  const { setSupabaseAuth, initializeSupabaseAuth } = useAuthStore();

  useEffect(() => {
    // Initialize auth state on mount
    initializeSupabaseAuth();

    // Listen for auth changes
    const { data: { subscription } } = supabase.auth.onAuthStateChange(
      async (event, session) => {
        if (event === 'SIGNED_IN' && session) {
          setSupabaseAuth(session.access_token, session.user);
        } else if (event === 'SIGNED_OUT') {
          // Auth store logout will handle cleanup
        }
      }
    );

    return () => {
      subscription.unsubscribe();
    };
  }, [setSupabaseAuth, initializeSupabaseAuth]);

  return null;
};
