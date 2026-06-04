import { type RouteObject, Outlet } from 'react-router-dom';
import { lazyWithPreload } from '@/shared/utils/lazy-with-preload';
import { RoutePermissions } from '../route-permissions';
import { ErrorBoundary } from '@/shared/ui/common/error-boundary';
import { Suspense } from 'react';
import { LoadingSpinner } from '@/shared/ui/common/loading-spinner';

const ExplorerPage = lazyWithPreload(() => import('@/pages/archiving/explorer-page'));
const TemplatesPage = lazyWithPreload(() => import('@/pages/archiving/templates-page'));
const ArchiveEditRequestsPage = lazyWithPreload(() => import('@/pages/archiving/archive-edit-requests-page'));

export const archivingRoutes: RouteObject = {
  path: 'archiving',
  element: (
    <ErrorBoundary context="نظام الأرشفة">
      <Outlet />
    </ErrorBoundary>
  ),
  handle: {
    crumb: () => 'نظام الأرشفة',
    permission: RoutePermissions.AUTHENTICATED,
  },
  children: [
    {
      index: true,
      element: (
        <Suspense fallback={<LoadingSpinner />}>
          <ExplorerPage />
        </Suspense>
      ),
      handle: {
        preload: () => ExplorerPage.preload(),
      }
    },
    {
      path: 'templates',
      element: (
        <Suspense fallback={<LoadingSpinner />}>
          <TemplatesPage />
        </Suspense>
      ),
      handle: {
        crumb: () => 'إدارة النماذج الأرشيفية',
        preload: () => TemplatesPage.preload(),
      }
    },
    {
      path: 'edit-requests',
      element: (
        <Suspense fallback={<LoadingSpinner />}>
          <ArchiveEditRequestsPage />
        </Suspense>
      ),
      handle: {
        crumb: () => 'طلبات تعديل الأرشيف',
        preload: () => ArchiveEditRequestsPage.preload(),
      }
    }
  ]
};
