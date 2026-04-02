import { useMemo } from 'react';
import { useLocation } from 'react-router-dom';
import { resolveSidebarMode } from '../config/sidebar-modes';
import type { SidebarMode } from './sidebar-mode.types';

/**
 * Hook to determine the current sidebar mode based on the active route.
 *
 * @returns The active `SidebarMode`
 */
export function useSidebarMode(): SidebarMode {
    const { pathname } = useLocation();

    const mode = useMemo(() => resolveSidebarMode(pathname), [pathname]);

    return mode;
}
