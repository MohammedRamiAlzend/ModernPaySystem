export const RoutePermissions = {
  PUBLIC: 'PUBLIC',
  AUTHENTICATED: 'AUTHENTICATED',
  ADMIN: 'ADMIN',
  USER: 'USER',
} as const;

export type RoutePermissions = (typeof RoutePermissions)[keyof typeof RoutePermissions];

export interface RouteHandle {
  crumb?: () => string;
  permission?: RoutePermissions;
  preload?: () => void;
}
