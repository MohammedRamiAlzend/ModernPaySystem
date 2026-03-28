import { useState, useEffect, useMemo } from 'react';
import { useLocation } from 'react-router-dom';

/**
 * Reusable hook to calculate badge count for a specific path based on total count from a query.
 * 
 * @param totalCount Current total number of items from the API
 * @param storageKey Key used to store 'last seen' count in localStorage
 * @param targetPath The page path where the badge should be cleared/synced
 * @returns The number of new items since the last visit to the targetPath
 */
export const useBadgeCount = (totalCount: number, storageKey: string, targetPath: string) => {
    const location = useLocation();
    
    // Initialize lastSeen from localStorage
    const [lastSeen, setLastSeen] = useState<number>(() => {
        const stored = localStorage.getItem(storageKey);
        return stored ? Number(stored) : 0;
    });

    // Synchronize lastSeen when user visits the target page
    useEffect(() => {
        if (location.pathname === targetPath && totalCount > 0) {
            setLastSeen(totalCount);
            localStorage.setItem(storageKey, String(totalCount));
        }
    }, [location.pathname, totalCount, targetPath, storageKey]);

    // Calculate unread count
    const badgeCount = useMemo(() => {
        return Math.max(0, totalCount - lastSeen);
    }, [totalCount, lastSeen]);

    return badgeCount;
};
