import { supabase } from '@/lib/supabaseClient';
import { toast } from 'react-toastify';

export const signInWithGoogle = async () => {
  const { error } = await supabase.auth.signInWithOAuth({
    provider: 'google',
    options: {
      redirectTo: `${window.location.origin}/`,
    }
  });
  
  if (error) {
    toast.error('Google sign-in failed: ' + error.message);
    throw error;
  }
};

export const signOut = async () => {
  const { error } = await supabase.auth.signOut();
  if (error) {
    toast.error('Sign out failed: ' + error.message);
    throw error;
  }
};

export const getSupabaseUser = async () => {
  const { data, error } = await supabase.auth.getUser();
  if (error) {
    console.error('Failed to fetch user:', error.message);
    return null;
  }
  return data.user;
};

export const getSupabaseSession = async () => {
  const { data, error } = await supabase.auth.getSession();
  if (error) {
    console.error('Failed to fetch session:', error.message);
    return null;
  }
  return data.session;
};
