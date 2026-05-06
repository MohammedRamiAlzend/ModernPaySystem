import { useState, useEffect, useRef, useMemo } from 'react';
import { useTemplates, useCreateTemplate, useUpdateTemplate } from '@/features/form-builder/api/formEndpoints';
import { DELPHI_TEMPLATE_NAME, DELPHI_FIXED_SCHEMA } from './delphi-schema';
import { processDelphiData, type DelphiInput, type DelphiValidationResult } from './delphi-data-processor';
import type { FormSchema } from '@/entities/form/model/types';

/**
 * Hook to manage Delphi transaction processing and template synchronization.
 * It ensures the fixed "Delphi" template exists on the server and is up-to-date before use.
 */
export const useDelphiTransaction = (rawInput?: DelphiInput | string) => {
    const { data: templates = [], isLoading: isLoadingTemplates } = useTemplates(true);
    const { mutateAsync: createTemplate, isPending: isCreatingTemplate } = useCreateTemplate();
    const { mutateAsync: updateTemplate, isPending: isUpdatingTemplate } = useUpdateTemplate();

    const [templateToUse, setTemplateToUse] = useState<FormSchema | null>(null);
    const [syncError, setSyncError] = useState<string | null>(null);

    // Guard ref: prevents concurrent or duplicate creation calls
    const isSyncingRef = useRef(false);
    const hasCreatedRef = useRef(false);

    // 1. Template Sync Logic
    useEffect(() => {
        const syncTemplate = async () => {
            // Wait until templates are loaded
            if (isLoadingTemplates) return;

            // Find if existing
            const existing = templates.find(t => t.templateName === DELPHI_TEMPLATE_NAME);

            if (existing) {
                // Template found — reset creation guard
                hasCreatedRef.current = false;
                isSyncingRef.current = false;

                // Check if the stored schema is outdated (field count differs)
                try {
                    const storedSchema = JSON.parse(existing.contentAsJson) as FormSchema;
                    const isOutdated = storedSchema.fields.length !== DELPHI_FIXED_SCHEMA.fields.length;

                    if (isOutdated) {
                        // Schema has changed (e.g. new field added) — update in DB silently
                        await updateTemplate({
                            id: existing.id,
                            data: {
                                templateName: DELPHI_TEMPLATE_NAME,
                                templateDescription: DELPHI_FIXED_SCHEMA.description || null,
                                contentAsJson: JSON.stringify(DELPHI_FIXED_SCHEMA),
                                isExternal: true
                            }
                        });
                    }
                } catch {
                    // If parsing fails, we still continue with the local schema
                }

                // Always use the local DELPHI_FIXED_SCHEMA for rendering
                // (DB only stores the record for ID tracking — local is always authoritative)
                setTemplateToUse({ ...DELPHI_FIXED_SCHEMA, id: existing.id });

            } else {
                // Not found — create only once (guard against re-runs while pending)
                if (isSyncingRef.current || hasCreatedRef.current) return;

                isSyncingRef.current = true;
                try {
                    const newTemplateDto = {
                        templateName: DELPHI_TEMPLATE_NAME,
                        templateDescription: DELPHI_FIXED_SCHEMA.description || null,
                        contentAsJson: JSON.stringify(DELPHI_FIXED_SCHEMA),
                        isExternal: true
                    };
                    const response = await createTemplate(newTemplateDto);

                    hasCreatedRef.current = true;

                    // Use local schema + real DB id
                    const createdTemplate = response.data;
                    setTemplateToUse({ ...DELPHI_FIXED_SCHEMA, id: createdTemplate.id });
                } catch (error) {
                    setSyncError("Failed to create fixed Delphi template on server");
                    console.error("Sync error:", error);
                } finally {
                    isSyncingRef.current = false;
                }
            }
        };

        syncTemplate();
    }, [templates, isLoadingTemplates, createTemplate, updateTemplate]);

    // 2. Data Processing Logic (Derived State)
    const derivedProcessedData = useMemo(() => {
        if (!rawInput) return null;

        try {
            const inputObj = typeof rawInput === 'string' ? JSON.parse(rawInput) : rawInput;
            return processDelphiData(inputObj);
        } catch {
            return {
                status: 'error',
                errors: ["Invalid JSON received from source app"],
                ui: { highlight_missing: [], warnings: [], ready_to_submit: false }
            } as DelphiValidationResult;
        }
    }, [rawInput]);

    return {
        template: templateToUse,
        processed: derivedProcessedData,
        isLoading: isLoadingTemplates || isCreatingTemplate || isUpdatingTemplate,
        error: syncError || (derivedProcessedData?.status === 'error' ? derivedProcessedData.errors?.join(', ') : null)
    };
};
