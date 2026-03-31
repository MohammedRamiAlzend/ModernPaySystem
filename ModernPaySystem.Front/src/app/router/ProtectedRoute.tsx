import { useEffect, useCallback } from 'react';
import { useLocation, Navigate } from 'react-router-dom';
// import { useLocation, Navigate, useSearchParams } from 'react-router-dom';
import { useAuthStore } from '@/app/store/authStore';

interface ProtectedRouteProps {
  children: React.ReactNode;
  permission?: 'PUBLIC' | 'AUTHENTICATED' | 'ADMIN' | 'USER';
}

export const ProtectedRoute: React.FC<ProtectedRouteProps> = ({
  children,
  permission = 'AUTHENTICATED'
}) => {
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated);
  const currentUser = useAuthStore((state) => state.user);
  const location = useLocation();
  // const [searchParams] = useSearchParams();

  // Determine if user has required permission - wrapped in useCallback to stabilize reference
  const hasPermission = useCallback((): boolean => {
    if (permission === 'PUBLIC') return true;
    if (!isAuthenticated) return false;


    switch (permission) {
      case 'ADMIN':
        return currentUser?.roles.includes('admin') || false;
      case 'USER':
        return currentUser?.roles.includes('user') || false;
      case 'AUTHENTICATED':
      default:
        return isAuthenticated;
    }
  }, [isAuthenticated, currentUser?.roles, permission]);

  // Redirect URL for after login
  const redirectUrl = `${location.pathname}${location.search}`;

  useEffect(() => {
    if (!hasPermission()) {
      // Store the attempted URL in session storage for redirect after login
      sessionStorage.setItem('redirectAfterLogin', redirectUrl);
    }
  }, [hasPermission, redirectUrl]);

  // Handle unauthorized access
  if (!hasPermission()) {
    // If user is not authenticated, redirect to login
    if (!isAuthenticated) {
      return (
        <Navigate
          to={`/auth/login?redirect=${encodeURIComponent(redirectUrl)}`}
          replace
        />
      );
    }

    // If user is authenticated but lacks specific permission, redirect to home
    return <Navigate to="/" replace />;
  }

  return <>{children}</>;
};