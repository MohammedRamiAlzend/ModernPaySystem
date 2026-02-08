import { Suspense } from 'react';
import { createBrowserRouter, type RouteObject, Outlet, matchRoutes } from 'react-router-dom';
import { ProtectedRoute } from './ProtectedRoute';
import { LoadingSpinner } from '@/shared/ui/common/loading-spinner';
import { NotFoundPage } from '@/pages/not-found-page';
import { ErrorPage } from '@/pages/error-page';
import { lazyWithPreload } from '@/shared/utils/lazy-with-preload';
import { MainLayout } from '../layouts/main-layout';
import { AuthLayout } from '../layouts/auth-layout';

// Lazy load pages for better performance
const HomePage = lazyWithPreload(() => import('@/pages/home-page'));
const ContractsPage = lazyWithPreload(() => import('@/pages/contracts-page'));
const ContractFormPage = lazyWithPreload(() => import('@/pages/contract-form-page'));
const ProcessFormPage = lazyWithPreload(() => import('@/pages/process-form-page'));
const LoginPage = lazyWithPreload(() => import('@/pages/auth/login-page'));
const RegisterPage = lazyWithPreload(() => import('@/pages/auth/register-page'));
const ProfilePage = lazyWithPreload(() => import('@/pages/profile/profile-page'));
const SettingsPage = lazyWithPreload(() => import('@/pages/settings/settings-page'));
const FormBuilderPage = lazyWithPreload(() => import('@/pages/form-builder/FormBuilderPage'));

// Define route permissions
 const RoutePermissions = {
  PUBLIC: 'PUBLIC',
  AUTHENTICATED: 'AUTHENTICATED',
  ADMIN: 'ADMIN',
  USER: 'USER',
} as const;

 type RoutePermissions = (typeof RoutePermissions)[keyof typeof RoutePermissions];

// Enhanced route configuration
const routesConfig: RouteObject[] = [
  {
    path: '/',
    element: (
      <Suspense fallback={<LoadingSpinner />}>
        <MainLayout />
      </Suspense>
    ),
    errorElement: <ErrorPage />,
    children: [
      {
        index: true,
        element: (
          <Suspense fallback={<LoadingSpinner />}>
            <HomePage />
          </Suspense>
        ),
        handle: {
          crumb: () => 'Home',
          permission: RoutePermissions.PUBLIC,
          preload: () => HomePage.preload(),
        },
      },
      {
        path: 'auth',
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
              crumb: () => 'Login',
              permission: RoutePermissions.PUBLIC,
              preload: () => LoginPage.preload(),
            },
          },
          {
            path: 'register',
            element: (
              <Suspense fallback={<LoadingSpinner />}>
                <RegisterPage />
              </Suspense>
            ),
            handle: {
              crumb: () => 'Register',
              permission: RoutePermissions.PUBLIC,
              preload: () => RegisterPage.preload(),
            },
          },
        ],
      },
      {
        path: 'contracts',
        element: <Outlet />,
        children: [
          {
            index: true,
            element: (
              <ProtectedRoute permission={RoutePermissions.PUBLIC}>
                <Suspense fallback={<LoadingSpinner />}>
                  <ContractsPage />
                </Suspense>
              </ProtectedRoute>
            ),
            handle: {
              crumb: () => 'Contracts',
              permission: RoutePermissions.PUBLIC,
              preload: () => ContractsPage.preload(),
            },
          },
          {
            path: 'new',
            element: (
              <ProtectedRoute permission={RoutePermissions.PUBLIC}>
                <Suspense fallback={<LoadingSpinner />}>
                  <ContractFormPage />
                </Suspense>
              </ProtectedRoute>
            ),
            handle: {
              crumb: () => 'New Contract',
              permission: RoutePermissions.PUBLIC,
              preload: () => ContractFormPage.preload(),
            },
          },
          {
            path: 'edit/:id',
            element: (
              <ProtectedRoute permission={RoutePermissions.PUBLIC}>
                <Suspense fallback={<LoadingSpinner />}>
                  <ContractFormPage />
                </Suspense>
              </ProtectedRoute>
            ),
            handle: {
              crumb: () => 'Edit Contract',
              permission: RoutePermissions.PUBLIC,
              preload: () => ContractFormPage.preload(),
            },
          },
        ],
      },
      {
        path: 'processes',
        element: (
          <ProtectedRoute permission={RoutePermissions.PUBLIC}>
            <Suspense fallback={<LoadingSpinner />}>
              <ProcessFormPage />
            </Suspense>
          </ProtectedRoute>
        ),
        handle: {
          crumb: () => 'Processes',
          permission: RoutePermissions.PUBLIC,
          preload: () => ProcessFormPage.preload(),
        },
      },
      {
        path: 'profile',
        element: (
          <ProtectedRoute permission={RoutePermissions.PUBLIC}>
            <Suspense fallback={<LoadingSpinner />}>
              <ProfilePage />
            </Suspense>
          </ProtectedRoute>
        ),
        handle: {
          crumb: () => 'Profile',
          permission: RoutePermissions.AUTHENTICATED,
          preload: () => ProfilePage.preload(),
        },
      },
      {
        path: 'settings',
        element: (
          <ProtectedRoute permission={RoutePermissions.AUTHENTICATED}>
            <Suspense fallback={<LoadingSpinner />}>
              <SettingsPage />
            </Suspense>
          </ProtectedRoute>
        ),
        handle: {
          crumb: () => 'Settings',
          permission: RoutePermissions.AUTHENTICATED,
          preload: () => SettingsPage.preload(),
        },
      },
      {
        path: 'form-builder',
        element: (
          <ProtectedRoute permission={RoutePermissions.PUBLIC}>
            <Suspense fallback={<LoadingSpinner />}>
              <FormBuilderPage />
            </Suspense>
          </ProtectedRoute>
        ),
        handle: {
          crumb: () => 'Form Builder',
          permission: RoutePermissions.PUBLIC,
          preload: () => FormBuilderPage.preload(),
        },
      },
    ],
  },
  {
    path: '*',
    element: (
      <Suspense fallback={<LoadingSpinner />}>
        <NotFoundPage />
      </Suspense>
    ),
    errorElement: <ErrorPage />,
  },
];

/**
 * Prefetches the code for a given route path
 */
export const prefetchRoute = (path: string) => {
  const matches = matchRoutes(routesConfig, path);
  if (matches) {
    matches.forEach((match) => {
      const handle = match.route.handle as any;
      if (handle?.preload) {
        handle.preload();
      }
    });
  }
};

// Create the router
 const router = createBrowserRouter(routesConfig);

export default router;