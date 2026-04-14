/**
 * Application-wide configuration derived from environment variables.
 * Centralizing this here makes the code cleaner and easier to maintain.
 */
export const APP_CONFIG = {
    /** Whether to show subsystem selection and columns in the UI */
    SHOW_SUB_SYSTEM: import.meta.env.VITE_SHOW_SUB_SYSTEM !== 'false',
    
    /** The default subsystem ID to use when selection is hidden or for initial states */
    DEFAULT_SUB_SYSTEM_ID: import.meta.env.VITE_DEFAULT_SUB_SYSTEM_ID || '1',

    /** API base URL */
    API_BASE_URL: import.meta.env.VITE_API_BASE_URL,

    /** Live data refetch interval in ms */
    LIVE_REFETCH_INTERVAL: parseInt(import.meta.env.VITE_LIVE_REFETCH_INTERVAL || '5000'),

    /** Whether we are in development mode */
    IS_DEV: import.meta.env.VITE_DEV === 'true',
};
