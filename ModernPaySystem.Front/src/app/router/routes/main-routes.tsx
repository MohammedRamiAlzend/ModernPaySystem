import { Suspense } from 'react';
import { type RouteObject, Outlet, Navigate } from 'react-router-dom';
import { ErrorBoundary } from '@/shared/ui/common/error-boundary';
import { LoadingSpinner } from '@/shared/ui/common/loading-spinner';
import { lazyWithPreload } from '@/shared/utils/lazy-with-preload';
import { RoutePermissions } from '../route-permissions';
import { MainLayout } from '../../layouts/main-layout';
import { ProtectedRoute } from '../ProtectedRoute';
import { ErrorPage } from '@/pages/error-page';
import { formBuilderRoutes } from './form-builder-routes';

const ContractsPage = lazyWithPreload(() => import('@/pages/contracts-page'));
const ContractFormPage = lazyWithPreload(() => import('@/pages/contract-form-page'));
const ProcessFormPage = lazyWithPreload(() => import('@/pages/process-form-page'));
const SettingsPage = lazyWithPreload(() => import('@/pages/settings/settings-page'));

export const mainRoutes: RouteObject = {
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
      element: <Navigate to="/form-builder/responses" replace />,
      handle: {
        crumb: () => 'الرئيسية',
        permission: RoutePermissions.AUTHENTICATED,
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
    formBuilderRoutes,
  ],
};
