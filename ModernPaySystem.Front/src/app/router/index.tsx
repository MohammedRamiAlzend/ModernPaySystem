import { Suspense } from 'react';
import { createBrowserRouter, type RouteObject, Outlet, matchRoutes, Navigate } from 'react-router-dom';
import { ProtectedRoute } from './ProtectedRoute';
import { LoadingSpinner } from '@/shared/ui/common/loading-spinner';
import { ErrorBoundary } from '@/shared/ui/common/error-boundary';
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
// const ProfilePage = lazyWithPreload(() => import('@/pages/profile/profile-page'));
const SettingsPage = lazyWithPreload(() => import('@/pages/settings/settings-page'));
const FormEditorPage = lazyWithPreload(() => import('@/pages/form-builder/FormEditorPage').then(module => ({ default: module.FormEditorPage })));
const RequestPage = lazyWithPreload(() => import('@/pages/form-builder/RequestPage').then(module => ({ default: module.RequestPage })));
const ResponsesPage = lazyWithPreload(() => import('@/pages/form-builder/ResponsesPage').then(module => ({ default: module.ResponsesPage })));
const ActionedRequestsPage = lazyWithPreload(() => import('@/pages/form-builder/ActionedRequestsPage'));

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
        element: <ErrorBoundary context="عقود الإيجار"><Outlet /></ErrorBoundary>,
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
          <ErrorBoundary context="معاملات سريعة">
            <Suspense fallback={<LoadingSpinner />}>
              <ProcessFormPage />
            </Suspense>
          </ErrorBoundary>
        ),
        handle: {
          crumb: () => 'العمليات',
          permission: RoutePermissions.AUTHENTICATED,
          preload: () => ProcessFormPage.preload(),
        },
      },
      // {
      //   path: 'profile',
      //   element: (
      //     <Suspense fallback={<LoadingSpinner />}>
      //       <ProfilePage />
      //     </Suspense>
      //   ),
      //   handle: {
      //     crumb: () => 'الملف الشخصي',
      //     permission: RoutePermissions.AUTHENTICATED,
      //     preload: () => ProfilePage.preload(),
      //   },
      // },
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
        element: <ErrorBoundary context="نظام المراسلات"><Outlet /></ErrorBoundary>,
        handle: {
          crumb: () => 'بناء النماذج',
          permission: RoutePermissions.AUTHENTICATED,
        },
        children: [
          {
            index: true,
            element: <Navigate to="/settings" replace />,
          },
          {
            path: 'templates/new',
            element: (
              <Suspense fallback={<LoadingSpinner />}>
                <FormEditorPage />
              </Suspense>
            ),
            handle: {
              crumb: () => 'نموذج جديد',
              permission: RoutePermissions.AUTHENTICATED,
              preload: () => FormEditorPage.preload(),
            },
          },
          {
            path: 'templates/:id',
            element: (
              <Suspense fallback={<LoadingSpinner />}>
                <FormEditorPage />
              </Suspense>
            ),
            handle: {
              crumb: () => 'تعديل نموذج',
              permission: RoutePermissions.AUTHENTICATED,
              preload: () => FormEditorPage.preload(),
            },
          },
          {
            path: 'requests/new',
            element: (
              <Suspense fallback={<LoadingSpinner />}>
                <RequestPage />
              </Suspense>
            ),
            handle: {
              crumb: () => 'تقديم طلب',
              permission: RoutePermissions.AUTHENTICATED,
              preload: () => RequestPage.preload(),
            },
          },
          {
            path: 'responses',
            element: (
              <Suspense fallback={<LoadingSpinner />}>
                <ResponsesPage />
              </Suspense>
            ),
            handle: {
              crumb: () => 'الردود',
              permission: RoutePermissions.AUTHENTICATED,
              preload: () => ResponsesPage.preload(),
            },
          },
          {
            path: 'actioned',
            element: (
              <Suspense fallback={<LoadingSpinner />}>
                <ActionedRequestsPage />
              </Suspense>
            ),
            handle: {
              crumb: () => 'الأرشيف',
              permission: RoutePermissions.AUTHENTICATED,
              preload: () => ActionedRequestsPage.preload(),
            },
          },
        ]
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

interface RouteHandle {
  crumb?: () => string;
  permission?: RoutePermissions;
  preload?: () => void;
}

export const prefetchRoute = (path: string) => {
  const matches = matchRoutes(routesConfig, path);
  if (matches) {
    matches.forEach((match) => {
      const handle = match.route.handle as RouteHandle | undefined;
      if (handle?.preload) {
        handle.preload();
      }
    });
  }
};

const router = createBrowserRouter(routesConfig);

export default router;