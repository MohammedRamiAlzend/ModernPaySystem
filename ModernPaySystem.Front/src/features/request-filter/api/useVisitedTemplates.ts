import { useQuery } from '@tanstack/react-query';
import { userEndpoints } from '@/entities/user/api/userEndpoints';
import { queryKeys } from '@/shared/constants/query-keys';
import { QUERY_STRATEGIES, UpdateStrategy } from '@/shared/constants/query-strategies';
import type { FormSchema } from '@/entities/form/model/types';

export const useVisitedTemplates = () => {
    return useQuery({
        queryKey: queryKeys.user.visitedTemplates(),
        queryFn: async () => {
            const res = await userEndpoints.getVisitedTemplates();
            const templates = res.data || [];
            
            return templates.map(t => {
                try {
                    const parsed = JSON.parse(t.contentAsJson);
                    const baseSchema = Array.isArray(parsed) ? parsed[0] : parsed;
                    if (!baseSchema || typeof baseSchema !== 'object') return null;
                    
                    const schema = baseSchema as FormSchema;
                    schema.id = t.id;
                    schema.title = t.templateName;
                    return schema;
                } catch (e) {
                    console.error('Failed to parse visited template content', t, e);
                    return null;
                }
            }).filter((t): t is FormSchema => t !== null);
        },
        ...QUERY_STRATEGIES[UpdateStrategy.BACKGROUND]
    });
};
