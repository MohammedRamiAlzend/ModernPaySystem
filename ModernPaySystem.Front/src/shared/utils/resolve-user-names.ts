import { fetchUserById } from '@/features/users/api/usersApi';

/**
 * Resolves user IDs to user names.
 * Tries to find names in a provided cache or fetches them from the API.
 * 
 * @param userIds List of unique user IDs to resolve
 * @returns A Map of user IDs to user names
 */
export const resolveUserNames = async (
    userIds: string[]
): Promise<Map<string, string>> => {
    const userMap = new Map<string, string>();
    const uniqueIds = Array.from(new Set(userIds.filter(id => !!id)));

    if (uniqueIds.length === 0) return userMap;

    const fetchPromises = uniqueIds.map(async (id) => {
        try {
            const user = await fetchUserById(id);
            if (user && user.userName) {
                userMap.set(id, user.userName);
            } else {
                userMap.set(id, `مستخدم (${id.split('-')[0]})`);
            }
        } catch (error) {
            console.error(`Failed to resolve user name for ID: ${id}`, error);
            userMap.set(id, `مستخدم (${id.split('-')[0]})`);
        }
    });

    await Promise.all(fetchPromises);
    return userMap;
};
