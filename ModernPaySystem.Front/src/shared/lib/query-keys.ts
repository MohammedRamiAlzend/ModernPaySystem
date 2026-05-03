export const queryKeys = {
    department: {
        all: ['departments'] as const,
        lists: () => [...queryKeys.department.all, 'list'] as const,
        list: (filters: any) => [...queryKeys.department.lists(), filters] as const,
        details: () => [...queryKeys.department.all, 'detail'] as const,
        detail: (id: string) => [...queryKeys.department.details(), id] as const,
        tree: (rootId?: string, mode?: string) => [...queryKeys.department.all, 'tree', rootId, mode] as const,
    },
    user: {
        all: ['users'] as const,
        lists: () => [...queryKeys.user.all, 'list'] as const,
        list: (filters: any) => [...queryKeys.user.lists(), filters] as const,
        details: () => [...queryKeys.user.all, 'detail'] as const,
        detail: (id: string) => [...queryKeys.user.details(), id] as const,
    }
};
