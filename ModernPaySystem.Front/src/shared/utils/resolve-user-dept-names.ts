import { fetchUserById } from '@/features/users/api/usersApi';

export const resolveUserDeptNames = async (
    userIds: string[]
): Promise<Map<string, string>> => {
    const deptMap = new Map<string, string>();
    const uniqueIds = Array.from(new Set(userIds.filter(id => !!id)));

    if (uniqueIds.length === 0) return deptMap;

    const fetchPromises = uniqueIds.map(async (id) => {
        try {
            const user = await fetchUserById(id);
            if (user && user.departmentName) {
                deptMap.set(id, user.departmentName);
            } else {
                deptMap.set(id, '');
            }
        } catch (error) {
            console.error(`Failed to resolve department name for user ID: ${id}`, error);
            deptMap.set(id, '');
        }
    });

    await Promise.all(fetchPromises);
    return deptMap;
};
