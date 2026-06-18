/* eslint-disable react-refresh/only-export-components */
import { type RouteObject, Outlet, Navigate } from 'react-router-dom';
import { lazyWithPreload } from '@/shared/utils/lazy-with-preload';
import { RoutePermissions } from '../route-permissions';
import { ErrorBoundary } from '@/shared/ui/common/error-boundary';
import { Suspense } from 'react';
import { LoadingSpinner } from '@/shared/ui/common/loading-spinner';
import { useLedDepartments } from '@/features/archiving/model/queries';

const ExplorerPage = lazyWithPreload(() => import('@/pages/archiving/explorer-page'));
// const FolderIconsPage = lazyWithPreload(() => import('@/pages/archiving/folder-icons-page'));
// const TemplatesPage = lazyWithPreload(() => import('@/pages/archiving/templates-page'));
const ArchiveEditRequestsPage = lazyWithPreload(() => import('@/pages/archiving/archive-edit-requests-page'));
const ArchiveSearchPage = lazyWithPreload(() => import('@/pages/archiving/search-page'));
const SemanticSearchPage = lazyWithPreload(() => import('@/pages/archiving/semantic-search-page'));
const AuditLogsPage = lazyWithPreload(() => import('@/pages/archiving/audit-logs-page'));
const ReportsPage = lazyWithPreload(() => import('@/pages/archiving/reports-page'));

const ArchiveLeaderRoute = ({ children }: { children: React.ReactNode }) => {
  const { data: departments = [], isLoading: isLoadingDeps } = useLedDepartments();

  if (isLoadingDeps) {
    return (
      <div className="flex h-screen items-center justify-center">
        <LoadingSpinner />
      </div>
    );
  }

  if (departments.length === 0) {
    return <Navigate to="/" replace />;
  }

  return <>{children}</>;
};

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
      path: 'search',
      element: (
        <Suspense fallback={<LoadingSpinner />}>
          <ArchiveSearchPage />
        </Suspense>
      ),
      handle: {
        crumb: () => 'البحث المتقدم في الأرشيف',
        preload: () => ArchiveSearchPage.preload(),
      }
    },
    {
      path: 'semantic-search',
      element: (
        <Suspense fallback={<LoadingSpinner />}>
          <SemanticSearchPage />
        </Suspense>
      ),
      handle: {
        crumb: () => 'البحث الدلالي',
        preload: () => SemanticSearchPage.preload(),
      }
    },
    {
      path: 'folder-icons',
      element: <Navigate to="/settings?tab=folder-icons" replace />,
    },
    {
      path: 'templates',
      element: <Navigate to="/settings?tab=archiving-templates" replace />,
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
    },
    {
      path: 'audit-logs',
      element: (
        <Suspense fallback={<LoadingSpinner />}>
          <ArchiveLeaderRoute>
            <AuditLogsPage />
          </ArchiveLeaderRoute>
        </Suspense>
      ),
      handle: {
        crumb: () => 'سجلات النشاط (Audit Logs)',
        preload: () => AuditLogsPage.preload(),
      }
    },
    {
      path: 'reports',
      element: (
        <Suspense fallback={<LoadingSpinner />}>
          <ArchiveLeaderRoute>
            <ReportsPage />
          </ArchiveLeaderRoute>
        </Suspense>
      ),
      handle: {
        crumb: () => 'تقارير الأرشيف',
        preload: () => ReportsPage.preload(),
      }
    }
  ]
};
