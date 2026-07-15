import { useQuery } from '@tanstack/react-query';
import { QUERY_STRATEGIES, UpdateStrategy } from '@/shared/constants/query-strategies';
import type { QueryKey } from '@tanstack/react-query';

interface ReportQueryOptions {
  strategy?: UpdateStrategy;
}

function buildReportHook<TArgs extends unknown[]>(
  queryKeyFn: (...args: TArgs) => QueryKey,
  queryFn: (...args: TArgs) => Promise<unknown>,
  options: ReportQueryOptions = {},
) {
  return (...allArgs: [...TArgs, boolean?]) => {
    const lastArg = allArgs[allArgs.length - 1];
    const hasEnabled = typeof lastArg === 'boolean';
    const args = (hasEnabled ? allArgs.slice(0, -1) : allArgs) as TArgs;
    const enabled = hasEnabled ? (lastArg as boolean) : true;

    return useQuery({
      queryKey: queryKeyFn(...args),
      queryFn: () => queryFn(...args),
      enabled,
      ...QUERY_STRATEGIES[options.strategy ?? UpdateStrategy.CRITICAL]
    });
  };
}

/** For APIs that wrap responses in `{ data: T }` (e.g., formEndpoints) */
export function createReportQuery<TData, TArgs extends unknown[]>(
  endpoint: (...args: TArgs) => Promise<{ data: TData }>,
  queryKeyFn: (...args: TArgs) => QueryKey,
  options?: ReportQueryOptions,
) {
  return buildReportHook(
    queryKeyFn,
    async (...args: TArgs) => {
      const res = await endpoint(...args);
      return res.data ?? null;
    },
    options,
  );
}

/** For APIs that return data directly (e.g., archivingService) */
export function createDirectReportQuery<TData, TArgs extends unknown[]>(
  endpoint: (...args: TArgs) => Promise<TData>,
  queryKeyFn: (...args: TArgs) => QueryKey,
  options?: ReportQueryOptions,
) {
  return buildReportHook(queryKeyFn, endpoint, options);
}
