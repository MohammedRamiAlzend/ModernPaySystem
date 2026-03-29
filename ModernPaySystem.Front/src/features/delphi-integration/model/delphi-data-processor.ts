/**
 * Delphi Transaction Data Types and Processor Logic
 */

export interface DelphiService {
    id_service: number;
    num: number;
}

export interface DelphiInput {
    id?: any;          // رقم المعاملة - قد يكون نصاً مثل "م/777"
    client?: any;
    clientas?: any;
    action_date?: any;
    id_reality?: any;
    region?: any;
    sum_amount?: any;  // المبلغ الإجمالي - اختياري
    list_services?: any[];
}

export interface DelphiValidationResult {
    status: 'success' | 'error';
    errors?: string[];
    data?: any;
    ui?: {
        highlight_missing: string[];
        warnings: string[];
        ready_to_submit: boolean;
    };
}

/**
 * Validates, Normalizes, and Transforms data received from Delphi application.
 */
export const processDelphiData = (raw: DelphiInput): DelphiValidationResult => {
    const errors: string[] = [];
    const highlight_missing: string[] = [];
    const warnings: string[] = [];

    // 1. Validation Logic
    if (!raw.id && raw.id !== 0) {
        errors.push("Missing transaction ID (id)");
        highlight_missing.push("id");
    }
    if (!raw.client || typeof raw.client !== 'string') {
        errors.push("Missing or invalid client name (client)");
        highlight_missing.push("client");
    }
    if (!raw.list_services || !Array.isArray(raw.list_services)) {
        errors.push("Missing or invalid services list (list_services)");
        highlight_missing.push("list_services");
    } else if (raw.list_services.length === 0) {
        errors.push("Services list is empty");
        highlight_missing.push("list_services");
    } else {
        raw.list_services.forEach((s, idx) => {
            if (!s.id_service || !s.num || s.num <= 0) {
                errors.push(`Invalid service at index ${idx}`);
            }
        });
    }

    if (errors.length > 0) {
        return {
            status: 'error',
            errors,
            ui: { highlight_missing, warnings, ready_to_submit: false }
        };
    }

    // 2. Normalization Logic
    const normalized = {
        // id قد يكون نصاً مثل "م/777" أو رقماً - نحتفظ به كنص
        id: String(raw.id).trim(),
        client: String(raw.client).trim(),
        clientas: raw.clientas ? String(raw.clientas).trim() : '',
        // Dates from Delphi may use / instead of -
        action_date: raw.action_date ? String(raw.action_date).replace(/\//g, '-') : new Date().toISOString().split('T')[0],
        id_reality: Number(raw.id_reality),
        region: String(raw.region || '').trim(),
        sum_amount: raw.sum_amount !== undefined && raw.sum_amount !== null ? Number(raw.sum_amount) : null
    };

    // Date normalization to ISO (YYYY-MM-DD for form data consistency)
    try {
        const d = new Date(normalized.action_date);
        if (!isNaN(d.getTime())) {
            normalized.action_date = d.toISOString().split('T')[0];
        } else {
            warnings.push("Could not parse date format, using as is");
        }
    } catch {
        warnings.push("Date parsing failed");
    }

    // 3. Transform Services (Merge duplicates)
    const mergedServices: Record<number, number> = {};
    (raw.list_services || []).forEach(s => {
        const id = Number(s.id_service);
        const qty = Number(s.num);
        mergedServices[id] = (mergedServices[id] || 0) + qty;
    });

    const finalServices = Object.entries(mergedServices).map(([id, num]) => ({
        id_service: Number(id),
        num: Number(num)
    }));

    // 4. Compute Transformation Fields
    const total_services_count = finalServices.reduce((sum, s) => sum + s.num, 0);
    const services_types_count = finalServices.length;

    // Helper for UI - Create a text summary for the FormRenderer to display
    const services_summary = finalServices.map(s => 
        `• نوع الخدمة (#${s.id_service}): ${s.num}`
    ).join('\n');

    return {
        status: 'success',
        data: {
            ...normalized,
            list_services: finalServices,
            total_services_count,
            services_types_count,
            services_summary // For the FormRenderer textarea display
        },
        ui: {
            highlight_missing,
            warnings,
            ready_to_submit: true
        }
    };
};
