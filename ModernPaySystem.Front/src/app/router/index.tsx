import { Suspense } from 'react';
import { createBrowserRouter, type RouteObject, matchRoutes } from 'react-router-dom';
import { LoadingSpinner } from '@/shared/ui/common/loading-spinner';
import { NotFoundPage } from '@/pages/not-found-page';
import { ErrorPage } from '@/pages/error-page';
import { authRoutes } from './routes/auth-routes';
import { mainRoutes } from './routes/main-routes';
import type { RouteHandle } from './route-permissions';

const routesConfig: RouteObject[] = [
  // 1. Authenticated Routes (With Sidebar/MainLayout)
  mainRoutes,
  
  // 2. Auth Routes (Without Sidebar/MainLayout)
  authRoutes,
  
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

/**
 * Preload resources associated with a specific path
 * Uses react-router matching to find relevant route handles
 */
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