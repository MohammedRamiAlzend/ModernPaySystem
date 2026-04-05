import { Suspense } from 'react';
import { type RouteObject, Outlet } from 'react-router-dom';
import { AuthLayout } from '../../layouts/auth-layout';
import { LoadingSpinner } from '@/shared/ui/common/loading-spinner';
import { lazyWithPreload } from '@/shared/utils/lazy-with-preload';
import { RoutePermissions } from '../route-permissions';

const LoginPage = lazyWithPreload(() => import('@/pages/auth/login-page'));

export const authRoutes: RouteObject = {
  path: '/auth',
  element: (
    <AuthLayout>
      <Suspense fallback={<LoadingSpinner />}>
        <Outlet />
      </Suspense>
    </AuthLayout>
  ),
  children: [
    {
      path: 'login',
      element: (
        <Suspense fallback={<LoadingSpinner />}>
          <LoginPage />
        </Suspense>
      ),
      handle: {
        crumb: () => 'تسجيل الدخول',
        permission: RoutePermissions.PUBLIC,
        preload: () => LoginPage.preload(),
      },
    },
  ],
};
