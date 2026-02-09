import { Suspense } from 'react';
import { createBrowserRouter, type RouteObject, Outlet, matchRoutes } from 'react-router-dom';
import { ProtectedRoute } from './ProtectedRoute';
import { LoadingSpinner } from '@/shared/ui/common/loading-spinner';
import { NotFoundPage } from '@/pages/not-found-page';
import { ErrorPage } from '@/pages/error-page';
import { lazyWithPreload } from '@/shared/utils/lazy-with-preload';
import { MainLayout } from '../layouts/main-layout';
import { AuthLayout } from '../layouts/auth-layout';

// Lazy load pages
const HomePage = lazyWithPreload(() => import('@/pages/home-page'));
const ContractsPage = lazyWithPreload(() => import('@/pages/contracts-page'));
const ContractFormPage = lazyWithPreload(() => import('@/pages/contract-form-page'));
const ProcessFormPage = lazyWithPreload(() => import('@/pages/process-form-page'));
const LoginPage = lazyWithPreload(() => import('@/pages/auth/login-page'));
const RegisterPage = lazyWithPreload(() => import('@/pages/auth/register-page'));
const ProfilePage = lazyWithPreload(() => import('@/pages/profile/profile-page'));
const SettingsPage = lazyWithPreload(() => import('@/pages/settings/settings-page'));
const FormBuilderPage = lazyWithPreload(() => import('@/pages/form-builder/FormBuilderPage'));

const RoutePermissions = {
  PUBLIC: 'PUBLIC',
  AUTHENTICATED: 'AUTHENTICATED',
  ADMIN: 'ADMIN',
  USER: 'USER',
} as const;

type RoutePermissions = (typeof RoutePermissions)[keyof typeof RoutePermissions];

const routesConfig: RouteObject[] = [
  // 1. Authenticated Routes (With Sidebar/MainLayout)
  {
    path: '/',
    element: (
      <ProtectedRoute permission={RoutePermissions.AUTHENTICATED}>
        <Suspense fallback={<LoadingSpinner />}>
          <MainLayout />
        </Suspense>
      </ProtectedRoute>
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
          crumb: () => 'الرئيسية',
          permission: RoutePermissions.AUTHENTICATED,
          preload: () => HomePage.preload(),
        },
      },
      {
        path: 'contracts',
        element: <Outlet />,
        children: [
          {
            index: true,
            element: (
              <Suspense fallback={<LoadingSpinner />}>
                <ContractsPage />
              </Suspense>
            ),
            handle: {
              crumb: () => 'العقود',
              permission: RoutePermissions.AUTHENTICATED,
              preload: () => ContractsPage.preload(),
            },
          },
          {
            path: 'new',
            element: (
              <Suspense fallback={<LoadingSpinner />}>
                <ContractFormPage />
              </Suspense>
            ),
            handle: {
              crumb: () => 'عقد جديد',
              permission: RoutePermissions.AUTHENTICATED,
              preload: () => ContractFormPage.preload(),
            },
          },
          {
            path: 'edit/:id',
            element: (
              <Suspense fallback={<LoadingSpinner />}>
                <ContractFormPage />
              </Suspense>
            ),
            handle: {
              crumb: () => 'تعديل عقد',
              permission: RoutePermissions.AUTHENTICATED,
              preload: () => ContractFormPage.preload(),
            },
          },
        ],
      },
      {
        path: 'processes',
        element: (
          <Suspense fallback={<LoadingSpinner />}>
            <ProcessFormPage />
          </Suspense>
        ),
        handle: {
          crumb: () => 'العمليات',
          permission: RoutePermissions.AUTHENTICATED,
          preload: () => ProcessFormPage.preload(),
        },
      },
      {
        path: 'profile',
        element: (
          <Suspense fallback={<LoadingSpinner />}>
            <ProfilePage />
          </Suspense>
        ),
        handle: {
          crumb: () => 'الملف الشخصي',
          permission: RoutePermissions.AUTHENTICATED,
          preload: () => ProfilePage.preload(),
        },
      },
      {
        path: 'settings',
        element: (
          <Suspense fallback={<LoadingSpinner />}>
            <SettingsPage />
          </Suspense>
        ),
        handle: {
          crumb: () => 'الإعدادات',
          permission: RoutePermissions.AUTHENTICATED,
          preload: () => SettingsPage.preload(),
        },
      },
      {
        path: 'form-builder',
        element: (
          <Suspense fallback={<LoadingSpinner />}>
            <FormBuilderPage />
          </Suspense>
        ),
        handle: {
          crumb: () => 'منشئ النماذج',
          permission: RoutePermissions.AUTHENTICATED,
          preload: () => FormBuilderPage.preload(),
        },
      },
    ],
  },
  // 2. Auth Routes (Without Sidebar/MainLayout)
  {
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
      // {
      //   path: 'register',
      //   element: (
      //     <Suspense fallback={<LoadingSpinner />}>
      //       <RegisterPage />
      //     </Suspense>
      //   ),
      //   handle: {
      //     crumb: () => 'إنشاء حساب',
      //     permission: RoutePermissions.PUBLIC,
      //     preload: () => RegisterPage.preload(),
      //   },
      // },
    ],
  },
  // 3. Fallback Routes
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

const router = createBrowserRouter(routesConfig);

export default router;