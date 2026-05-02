/**
 * Sidebar Mode System Types
 *
 * Defines the extensible type system for sidebar modes.
 * Each mode corresponds to a different navigation context in the application.
 *
 * To add a new mode:
 * 1. Add the mode key to `SidebarMode`
 * 2. Create the content component
 * 3. Register it in `sidebar-modes.ts`
 */

/** All available sidebar modes */
export type SidebarMode = 'main' | 'settings';

/** Configuration for a single sidebar mode */
export interface SidebarModeConfig {
    /** Unique mode identifier */
    mode: SidebarMode;
    /** Route pattern(s) that activate this mode */
    routePatterns: string[];
    /** Priority — higher wins when multiple patterns match */
    priority: number;
}

/** Props shared by all sidebar content components */
export interface SidebarContentProps {
    isCollapsed: boolean;
    onItemClick?: () => void;
}
