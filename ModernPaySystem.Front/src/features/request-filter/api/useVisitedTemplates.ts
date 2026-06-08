import { useQuery } from '@tanstack/react-query';
import { userEndpoints } from '@/entities/user/api/userEndpoints';
import { formEndpoints } from '@/features/form-builder/api/formEndpoints';
import { queryKeys } from '@/shared/constants/query-keys';
import { QUERY_STRATEGIES, UpdateStrategy } from '@/shared/constants/query-strategies';
import type { FormSchema, Template } from '@/entities/form/model/types';

export const useVisitedTemplates = () => {
    return useQuery({
        queryKey: queryKeys.user.visitedTemplates(),
        queryFn: async () => {
            let templates: Template[] = [];
            try {
                const res = await userEndpoints.getVisitedTemplates();
                templates = res.data || [];
            } catch (e) {
                console.error('Failed to get visited templates', e);
            }

            if (templates.length === 0) {
                try {
                    const res = await formEndpoints.getTemplates();
                    if (Array.isArray(res)) {
                        templates = res;
                    } else if (res && typeof res === 'object' && 'data' in res && Array.isArray(res.data)) {
                        templates = res.data as Template[];
                    }
                } catch (e) {
                    console.error('Failed to get all templates as fallback', e);
                }
            }

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
