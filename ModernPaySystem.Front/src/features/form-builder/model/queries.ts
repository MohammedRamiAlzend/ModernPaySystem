import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { queryKeys } from '@/shared/constants/query-keys';
import { QUERY_STRATEGIES, UpdateStrategy } from '@/shared/constants/query-strategies';
import { createReportQuery } from '@/shared/lib/query-factory';
import { formEndpoints } from '../api/formEndpoints';
import type {
    Template,
    FormSchema,
    CreateTemplateDto,
    CreateRequestDto,
    CreateResponseDto,
    TemplateRequest,
    TemplateResponse,
    CreateRequestTransactionDto,
    RequestPagedFilterDto,
} from '@/entities/form/model/types';

// ── Template Queries ──

export const useRequests = (hasResponse: boolean = false, filterOrPage: RequestPagedFilterDto | number = 1, pageSize: number = 15) => {
    const filter = typeof filterOrPage === 'object'
        ? filterOrPage
        : { page: filterOrPage, pageSize };

    return useQuery({
        queryKey: queryKeys.form.list({ hasResponse, ...filter }),
        queryFn: async () => {
            const filterToSend = hasResponse ? filter : { page: filter.page, pageSize: filter.pageSize };
            const res = await formEndpoints.getRequestsByActionStatus(hasResponse, filterToSend);
            return res.data;
        },
        ...QUERY_STRATEGIES[UpdateStrategy.LIVE]
    });
};

export const useAllPendingRequests = (page: number = 1, pageSize: number = 15) => {
    return useQuery({
        queryKey: queryKeys.form.list({ type: 'all-pending', page, pageSize }),
        queryFn: async () => {
            const res = await formEndpoints.getAllPendingRequestsPaged(page, pageSize);
            return res.data;
        },
        ...QUERY_STRATEGIES[UpdateStrategy.LIVE]
    });
};

export const useTemplates = (showExternal: boolean = false) => {
    return useQuery({
        queryKey: queryKeys.template.list({ showExternal }),
        queryFn: async (): Promise<Template[]> => {
            const res = await formEndpoints.getTemplates();
            const filterFn = (t: Template) => showExternal || (!t.isExternal && !t.templateName.toLocaleLowerCase().includes("delphi"));

            if (Array.isArray(res)) return res.filter(filterFn);

            if (res && !Array.isArray(res) && 'data' in res && Array.isArray(res.data)) {
                return (res.data as Template[]).filter(filterFn);
            }

            return [];
        },
        ...QUERY_STRATEGIES[UpdateStrategy.BACKGROUND]
    });
};

export const useTemplateById = (templateId: string | null) => {
    return useQuery({
        queryKey: queryKeys.template.detail(templateId),
        queryFn: async (): Promise<FormSchema | null> => {
            if (!templateId) return null;
            const t = await formEndpoints.getTemplateById(templateId);
            if (!t || !t.contentAsJson) return null;
            try {
                let parsed;
                try {
                    parsed = JSON.parse(t.contentAsJson);
                } catch {
                    parsed = JSON.parse(t.contentAsJson.replace(/'/g, '"'));
                }
                const baseSchema = Array.isArray(parsed) ? parsed[0] : parsed;
                if (!baseSchema || typeof baseSchema !== 'object') return null;
                const schema = baseSchema as FormSchema;
                schema.id = t.id;
                schema.title = t.templateName;
                schema.description = t.templateDescription || '';
                return schema;
            } catch {
                console.error('Failed to parse template content by ID', t);
                return null;
            }
        },
        enabled: !!templateId,
        ...QUERY_STRATEGIES[UpdateStrategy.BACKGROUND]
    });
};

// ── Template Mutations ──

export const useCreateTemplate = () => {
    const queryClient = useQueryClient();
    return useMutation({
        mutationFn: formEndpoints.createTemplate,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: queryKeys.template.all });
            queryClient.invalidateQueries({ queryKey: queryKeys.form.all });
        }
    });
};

export const useUpdateTemplate = () => {
    const queryClient = useQueryClient();
    return useMutation({
        mutationFn: ({ id, data }: { id: string; data: CreateTemplateDto }) =>
            formEndpoints.updateTemplate(id, data),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: queryKeys.template.all });
            queryClient.invalidateQueries({ queryKey: queryKeys.form.all });
        }
    });
};

// ── Request Queries ──

export const useCreateRequest = () => {
    return useMutation({
        mutationFn: (data: CreateRequestDto) => formEndpoints.createRequest(data)
    });
};

export const useRequestsPaged = (filter: RequestPagedFilterDto) => {
    return useQuery({
        queryKey: queryKeys.form.list({ type: 'paged', ...filter }),
        queryFn: async () => {
            const res = await formEndpoints.getRequestsPaged(filter);
            return res.data;
        },
        ...QUERY_STRATEGIES[UpdateStrategy.LIVE]
    });
};

export const useRequestById = (id: string | null) => {
    return useQuery({
        queryKey: queryKeys.form.detail(id),
        queryFn: async (): Promise<TemplateRequest | null> => {
            if (!id) return null;
            const res = await formEndpoints.getRequestById(id);
            return res;
        },
        enabled: !!id,
        ...QUERY_STRATEGIES[UpdateStrategy.BACKGROUND]
    });
};

export const useRequestRelations = (requestId: string | null) => {
    return useQuery({
        queryKey: [...queryKeys.form.detail(requestId), 'relations'],
        queryFn: async () => {
            if (!requestId) return [];
            const res = await formEndpoints.getRequestRelations(requestId);
            return res.data || [];
        },
        enabled: !!requestId,
        ...QUERY_STRATEGIES[UpdateStrategy.LIVE]
    });
};

export const useCreateRelation = () => {
    const queryClient = useQueryClient();
    return useMutation({
        mutationFn: formEndpoints.createRelation,
        onSuccess: (_, variables) => {
            queryClient.invalidateQueries({
                queryKey: [...queryKeys.form.detail(variables.sourceRequestId), 'relations']
            });
        }
    });
};

export const useDeleteRelation = () => {
    const queryClient = useQueryClient();
    return useMutation({
        mutationFn: ({ id }: { id: string; sourceRequestId: string }) => formEndpoints.deleteRelation(id),
        onSuccess: (_, variables) => {
            queryClient.invalidateQueries({
                queryKey: [...queryKeys.form.detail(variables.sourceRequestId), 'relations']
            });
        }
    });
};

// ── Response Queries/Mutations ──

export const useCreateResponse = () => {
    return useMutation({
        mutationFn: (data: CreateResponseDto) => formEndpoints.createResponse(data)
    });
};

export const useCreateReferral = () => {
    return useMutation({
        mutationFn: (data: CreateRequestTransactionDto) => formEndpoints.createReferral(data)
    });
};

export const useRequestResponses = (requestId: string | null) => {
    return useQuery({
        queryKey: queryKeys.form.responses(requestId),
        queryFn: async () => {
            if (!requestId) return [];
            const res = await formEndpoints.getResponsesByRequestId(requestId);

            if (res && typeof res === 'object' && 'data' in res) {
                const inner = res.data as any;
                if (Array.isArray(inner)) return inner;
                if (inner && Array.isArray(inner.items)) return inner.items;
            }

            if (Array.isArray(res)) return res as TemplateResponse[];

            return [] as TemplateResponse[];
        },
        enabled: !!requestId,
        ...QUERY_STRATEGIES[UpdateStrategy.BACKGROUND]
    });
};

export const useResponsesByRequester = (requesterId: string | null, filter: RequestPagedFilterDto) => {
    return useQuery({
        queryKey: queryKeys.form.list({ type: 'responses', requester: requesterId, ...filter }),
        queryFn: async () => {
            if (!requesterId) return null;
            const res = await formEndpoints.getResponsesByRequesterId(requesterId, filter);
            return res.data;
        },
        enabled: !!requesterId,
        ...QUERY_STRATEGIES[UpdateStrategy.LIVE]
    });
};

export const useRequestsByRequester = (requesterId: string | null, filter: RequestPagedFilterDto) => {
    return useQuery({
        queryKey: queryKeys.form.list({ requester: requesterId, ...filter }),
        queryFn: async () => {
            if (!requesterId) return null;
            const res = await formEndpoints.getRequestsByRequesterId(requesterId, filter);
            return res.data;
        },
        enabled: !!requesterId,
        ...QUERY_STRATEGIES[UpdateStrategy.LIVE]
    });
};

// ── Transaction Queries ──

export const useRequestTransactions = (status: number, filter: RequestPagedFilterDto) => {
    return useQuery({
        queryKey: queryKeys.process.list({ status, ...filter }),
        queryFn: async () => {
            const res = await formEndpoints.getRequestTransactions(status, filter);
            return res.data;
        },
        ...QUERY_STRATEGIES[UpdateStrategy.LIVE]
    });
};

export const useRequestTransactionsHistory = (requestId: string | null) => {
    return useQuery({
        queryKey: queryKeys.form.transactions(requestId),
        queryFn: async () => {
            if (!requestId) return [];
            const res = await formEndpoints.getRequestTransactionsByRequestId(requestId);
            return res.data;
        },
        enabled: !!requestId,
        ...QUERY_STRATEGIES[UpdateStrategy.LIVE]
    });
};

// ── General Report Queries ──

export const useRequestsReport = (pageNumber: number, pageSize: number, startDate?: string, endDate?: string, forCurrentDepartment: boolean = false, enabled: boolean = false) => {
    return useQuery({
        queryKey: queryKeys.form.list({ type: 'report-requests', page: pageNumber, pageSize, startDate, endDate, forCurrentDepartment }),
        queryFn: async () => {
            const res = await formEndpoints.getRequestsReport(pageNumber, pageSize, startDate, endDate, forCurrentDepartment);
            return (res as any).data ?? null;
        },
        enabled: enabled && pageNumber > 0,
        ...QUERY_STRATEGIES[UpdateStrategy.LIVE]
    });
};

export const useResponsesReport = (pageNumber: number, pageSize: number, startDate?: string, endDate?: string, forCurrentDepartment: boolean = false, enabled: boolean = false) => {
    return useQuery({
        queryKey: queryKeys.form.list({ type: 'report-responses', page: pageNumber, pageSize, startDate, endDate, forCurrentDepartment }),
        queryFn: async () => {
            const res = await formEndpoints.getResponsesReport(pageNumber, pageSize, startDate, endDate, forCurrentDepartment);
            return (res as any).data ?? null;
        },
        enabled: enabled && pageNumber > 0,
        ...QUERY_STRATEGIES[UpdateStrategy.LIVE]
    });
};

// ── Transaction Report Hooks (via factory) ──

export const useTransactionDashboard = createReportQuery(
    () => formEndpoints.getTransactionDashboard(),
    () => queryKeys.transactionReports.dashboard,
);

export const useTransactionDailyReport = createReportQuery(
    (date?: string | null) => formEndpoints.getTransactionDailyReport(date ?? undefined),
    (date?: string | null) => queryKeys.transactionReports.daily(date),
);

export const useTransactionWeeklyReport = createReportQuery(
    (weekStart?: string | null) => formEndpoints.getTransactionWeeklyReport(weekStart ?? undefined),
    (weekStart?: string | null) => queryKeys.transactionReports.weekly(weekStart),
);

export const useTransactionMonthlyReport = createReportQuery(
    (year?: number | null, month?: number | null) => formEndpoints.getTransactionMonthlyReport(year ?? undefined, month ?? undefined),
    (year?: number | null, month?: number | null) => queryKeys.transactionReports.monthly(year, month),
);

export const useTransactionUserActivityReport = createReportQuery(
    (fromDate?: string | null, toDate?: string | null) => formEndpoints.getTransactionUserActivity(fromDate ?? undefined, toDate ?? undefined),
    (fromDate?: string | null, toDate?: string | null) => queryKeys.transactionReports.userActivity(fromDate, toDate),
);

export const useTransactionActiveUsersReport = createReportQuery(
    (fromDate?: string | null, toDate?: string | null) => formEndpoints.getTransactionActiveUsers(fromDate ?? undefined, toDate ?? undefined),
    (fromDate?: string | null, toDate?: string | null) => queryKeys.transactionReports.activeUsers(fromDate, toDate),
);

export const useTransactionStorageReport = createReportQuery(
    () => formEndpoints.getTransactionStorageReport(),
    () => queryKeys.transactionReports.storage,
);

export const useTransactionChartsData = createReportQuery(
    (fromDate?: string | null, toDate?: string | null) => formEndpoints.getTransactionChartsData(fromDate ?? undefined, toDate ?? undefined),
    (fromDate?: string | null, toDate?: string | null) => queryKeys.transactionReports.charts(fromDate, toDate),
);

export const useTransactionDailyWorkReport = createReportQuery(
    (date?: string | null) => formEndpoints.getTransactionDailyWork(date ?? undefined),
    (date?: string | null) => queryKeys.transactionReports.dailyWork(date),
);
