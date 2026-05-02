import { Suspense } from 'react';
import { type RouteObject, Outlet, Navigate } from 'react-router-dom';
import { ErrorBoundary } from '@/shared/ui/common/error-boundary';
import { LoadingSpinner } from '@/shared/ui/common/loading-spinner';
import { lazyWithPreload } from '@/shared/utils/lazy-with-preload';
import { RoutePermissions } from '../route-permissions';

const FormEditorPage = lazyWithPreload(() => import('@/pages/form-builder/form-editor-page').then(module => ({ default: module.FormEditorPage })));
const RequestPage = lazyWithPreload(() => import('@/pages/form-builder/request-page').then(module => ({ default: module.RequestPage })));
const ResponsesPage = lazyWithPreload(() => import('@/pages/form-builder/responses-page').then(module => ({ default: module.ResponsesPage })));
const ActionedRequestsPage = lazyWithPreload(() => import('@/pages/form-builder/actioned-requests-page'));
const MyResponsesPage = lazyWithPreload(() => import('@/pages/form-builder/my-responses-page'));
const MyRequestsPage = lazyWithPreload(() => import('@/pages/form-builder/my-requests-page'));
const AllPendingRequestsPage = lazyWithPreload(() => import('@/pages/form-builder/all-pending-requests-page'));
const ReferralsPage = lazyWithPreload(() => import('@/pages/form-builder/referrals-page'));
const DelphiTransactionPage = lazyWithPreload(() => import('@/pages/delphi-transaction/delphi-transaction-page').then(module => ({ default: module.DelphiTransactionPage })));

export const formBuilderRoutes: RouteObject = {
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
        crumb: () => 'الرد على الطلبات',
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
        crumb: () => 'الردود الصادرة',
        permission: RoutePermissions.AUTHENTICATED,
        preload: () => ActionedRequestsPage.preload(),
      },
    },
    {
      path: 'my-requests',
      element: (
        <Suspense fallback={<LoadingSpinner />}>
          <MyRequestsPage />
        </Suspense>
      ),
      handle: {
        crumb: () => 'طلباتي المرسلة',
        permission: RoutePermissions.AUTHENTICATED,
        preload: () => MyRequestsPage.preload(),
      },
    },
    {
      path: 'my-responses',
      element: (
        <Suspense fallback={<LoadingSpinner />}>
          <MyResponsesPage />
        </Suspense>
      ),
      handle: {
        crumb: () => 'الردود الواردة',
        permission: RoutePermissions.AUTHENTICATED,
        preload: () => MyResponsesPage.preload(),
      },
    },
    {
      path: 'all-pending',
      element: (
        <Suspense fallback={<LoadingSpinner />}>
          <AllPendingRequestsPage />
        </Suspense>
      ),
      handle: {
        crumb: () => 'الطلبات المعلقة',
        permission: RoutePermissions.AUTHENTICATED,
        preload: () => AllPendingRequestsPage.preload(),
      },
    },
    {
      path: 'referrals/pending',
      element: (
        <Suspense fallback={<LoadingSpinner />}>
          <ReferralsPage status={0} />
        </Suspense>
      ),
      handle: {
        crumb: () => 'الرد على الإحالات',
        permission: RoutePermissions.AUTHENTICATED,
        preload: () => ReferralsPage.preload(),
      },
    },
    {
      path: 'referrals/sent',
      element: (
        <Suspense fallback={<LoadingSpinner />}>
          <ReferralsPage status={1} />
        </Suspense>
      ),
      handle: {
        crumb: () => 'الإحالات الصادرة',
        permission: RoutePermissions.AUTHENTICATED,
        preload: () => ReferralsPage.preload(),
      },
    },
    {
      path: 'delphi-transaction',
      element: (
        <Suspense fallback={<LoadingSpinner />}>
          <DelphiTransactionPage />
        </Suspense>
      ),
      handle: {
        crumb: () => 'معاملات دلفي',
        permission: RoutePermissions.AUTHENTICATED,
        preload: () => DelphiTransactionPage.preload(),
      },
    },
  ],
};
