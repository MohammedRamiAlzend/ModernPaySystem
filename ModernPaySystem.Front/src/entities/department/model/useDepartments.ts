import { useQuery } from '@tanstack/react-query';
import { departmentApi } from '../api/departmentApi';
import { queryKeys } from '@/shared/constants/query-keys';
import { SearchableSelectOption } from '@/shared/ui/searchable-select';
import { useMemo } from 'react';

export const useDepartments = (searchTerm: string = '', level: number = 0) => {
    const query = useQuery({
        queryKey: queryKeys.department.list({ searchTerm, level }),
        queryFn: () => departmentApi.search(searchTerm, level),
    });

    const departmentOptions: SearchableSelectOption[] = useMemo(() => {
        return (query.data || []).map(d => ({
            value: d.id,
            label: d.name,
            order: d.level,
            meta: { level: d.level }
        }));
    }, [query.data]);

    return {
        ...query,
        departments: query.data || [],
        departmentOptions
    };
};
