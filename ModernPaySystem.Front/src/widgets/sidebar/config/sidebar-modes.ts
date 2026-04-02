import type { SidebarModeConfig, SidebarMode } from '../model/sidebar-mode.types';

/**
 * Sidebar Mode Registry
 *
 * Maps route patterns to sidebar modes.
 * Higher priority wins when multiple patterns match.
 *
 * To add a new mode:
 * 1. Add the mode type to `SidebarMode` in `sidebar-mode.types.ts`
 * 2. Add a config entry below
 * 3. Create the content component in `../ui/`
 * 4. Register the component in `sidebar.tsx`
 */
export const SIDEBAR_MODE_REGISTRY: SidebarModeConfig[] = [
    {
        mode: 'settings',
        routePatterns: ['/settings'],
        priority: 10,
    },
    // Default mode — matches everything as fallback
    {
        mode: 'main',
        routePatterns: ['/'],
        priority: 0,
    },
];

/**
 * Resolves the active sidebar mode based on the current pathname.
 *
 * Algorithm:
 * 1. Finds all modes whose route patterns match the current path.
 * 2. Returns the mode with the highest priority.
 * 3. Falls back to 'main' if nothing matches.
 */
export function resolveSidebarMode(pathname: string): SidebarMode {
    const matches = SIDEBAR_MODE_REGISTRY
        .filter((config) =>
            config.routePatterns.some((pattern) => pathname.startsWith(pattern))
        )
        .sort((a, b) => b.priority - a.priority);

    return matches[0]?.mode ?? 'main';
}
