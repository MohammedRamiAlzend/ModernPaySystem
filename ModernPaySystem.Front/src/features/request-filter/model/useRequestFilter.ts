import { useState, useEffect, useMemo } from 'react';
import { useAuthStore } from '@/app/store/authStore';
import { useVisitedTemplates } from '../api/useVisitedTemplates';
import { useQueryState, parseAsInteger } from 'nuqs';
import type { RequestPagedFilterDto, InputValueFilterDto } from '@/entities/form/model/types';

interface FilterPreference {
    templateId: string;
    fieldKeys: string[];
}

const getStoredPrefs = (key: string): FilterPreference | null => {
    if (typeof window === 'undefined') return null;
    const saved = localStorage.getItem(key);
    if (!saved) return null;
    try {
        return JSON.parse(saved);
    } catch {
        return null;
    }
};

export const useRequestFilter = (pageKey: string) => {
    const user = useAuthStore(state => state.user);
    const { data: visitedTemplates = [], isLoading: isTemplatesLoading } = useVisitedTemplates();
    
    const prefKey = useMemo(() => `filter_prefs_${user?.id}_${pageKey}`, [user?.id, pageKey]);
    
    const [selectedTemplateId, setSelectedTemplateId] = useState<string>(() => {
        const prefs = getStoredPrefs(`filter_prefs_${user?.id}_${pageKey}`);
        return prefs?.templateId || '';
    });

    const [selectedFieldKeys, setSelectedFieldKeys] = useState<string[]>(() => {
        const prefs = getStoredPrefs(`filter_prefs_${user?.id}_${pageKey}`);
        return prefs?.fieldKeys || [];
    });

    const [filterValues, setFilterValues] = useState<Record<string, string>>({});
    const [appliedFilters, setAppliedFilters] = useState<Record<string, string>>({});

    // Pagination state
    const [page, setPage] = useQueryState('page', parseAsInteger.withDefault(1));
    const pageSize = 15;
    
    // Save preferences to localStorage
    useEffect(() => {
        if (selectedTemplateId) {
            const pref: FilterPreference = {
                templateId: selectedTemplateId,
                fieldKeys: selectedFieldKeys
            };
            localStorage.setItem(prefKey, JSON.stringify(pref));
        }
    }, [prefKey, selectedTemplateId, selectedFieldKeys]);
    
    const selectedTemplate = useMemo(() => 
        visitedTemplates.find(t => t.id === selectedTemplateId),
    [visitedTemplates, selectedTemplateId]);
    
    const availableFields = useMemo(() => 
        selectedTemplate?.fields || [],
    [selectedTemplate]);
    
    const handleTemplateChange = (templateId: string) => {
        setSelectedTemplateId(templateId);
        setSelectedFieldKeys([]);
        setFilterValues({});
    };

    const handleFieldSelectionChange = (fieldKeys: string[]) => {
        setSelectedFieldKeys(fieldKeys);
        // Clean up filter values for removed fields
        setFilterValues(prev => {
            const next = { ...prev };
            Object.keys(next).forEach(key => {
                if (!fieldKeys.includes(key)) delete next[key];
            });
            return next;
        });
    };

    const handleFilterValueChange = (key: string, value: string) => {
        setFilterValues(prev => ({ ...prev, [key]: value }));
    };

    const applyFilters = () => {
        setAppliedFilters({ ...filterValues });
        setPage(1); // Reset to page 1 when applying filters
    };

    const resetFilters = () => {
        setFilterValues({});
        setAppliedFilters({});
        setPage(1);
    };

    const filterParams = useMemo((): RequestPagedFilterDto => {
        const inputValueFilters: InputValueFilterDto[] = Object.entries(appliedFilters)
            .filter(([, value]) => value.trim() !== '')
            .map(([key, value]) => ({ key, value }));

        return {
            page: page,
            pageSize: pageSize,
            inputValueFilters: inputValueFilters.length > 0 ? inputValueFilters : undefined
        };
    }, [appliedFilters, page, pageSize]);

    return {
        visitedTemplates,
        isTemplatesLoading,
        selectedTemplateId,
        selectedTemplate,
        availableFields,
        selectedFieldKeys,
        filterValues,
        page,
        setPage,
        handleTemplateChange,
        handleFieldSelectionChange,
        handleFilterValueChange,
        applyFilters,
        resetFilters,
        filterParams,
        hasActiveFilters: Object.values(appliedFilters).some(v => v.trim() !== '')
    };
};
