import { useEffect } from 'react';
import { useLocation, Navigate } from 'react-router-dom';
// import { useLocation, Navigate, useSearchParams } from 'react-router-dom';
import { useAppSelector } from '@/app/store';
import { selectCurrentUser, selectIsAuthenticated } from '@/app/store/authSlice'; // We'll create this slice

interface ProtectedRouteProps {
  children: React.ReactNode;
  permission?: 'PUBLIC' | 'AUTHENTICATED' | 'ADMIN' | 'USER';
}

export const ProtectedRoute: React.FC<ProtectedRouteProps> = ({
  children,
  permission = 'AUTHENTICATED'
}) => {
  const isAuthenticated = useAppSelector(selectIsAuthenticated);
  const currentUser = useAppSelector(selectCurrentUser);
  const location = useLocation();
  // const [searchParams] = useSearchParams();

  // Determine if user has required permission
  const hasPermission = (): boolean => {
    if (permission === 'PUBLIC') return true;
    if (!isAuthenticated) return false;


    switch (permission) {
      case 'ADMIN':
        return currentUser?.role === 'admin';
      case 'USER':
        return currentUser?.role === 'user';
      case 'AUTHENTICATED':
      default:
        return isAuthenticated;
    }
  };

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