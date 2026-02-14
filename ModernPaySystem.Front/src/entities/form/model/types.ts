
export type FieldType = 'text' | 'email' | 'password' | 'number' | 'textarea' | 'select' | 'radio' | 'checkbox' | 'date';

export interface ValidationRule {
    rule: 'required' | 'minLength' | 'maxLength' | 'pattern' | 'minValue' | 'maxValue';
    value?: string | number;
    message?: string;
}

export interface LogicAction {
    targetField: string;
    effect: 'show' | 'hide' | 'enable' | 'disable' | 'require' | 'unrequire';
}

export interface LogicRule {
    when: {
        field: string;
        operator: 'equals' | 'notEquals' | 'contains' | 'greaterThan' | 'lessThan' | 'greaterThanOrEqual' | 'lessThanOrEqual' | 'startsWith' | 'endsWith';
        value: any;
    };
    actions: LogicAction[];
}

export interface DataSourceOption {
    label: string;
    value: string | number;
}

export interface DataSource {
    type: 'static' | 'api' | 'lookup';
    options?: DataSourceOption[];
    url?: string; // If API
    lookUpFieldId?: string; // For LookUp fields
}

export type InitialVisibility = 'visible' | 'hidden';
export type InitialEnabled = 'enabled' | 'disabled';
export type FieldDirection = 'horizontal' | 'vertical';

export interface FormField {
    id: string;
    name: string;
    type: FieldType;
    label: string;
    placeholder?: string;
    defaultValue?: any;
    validation?: ValidationRule[];
    dataSource?: DataSource;
    hidden?: boolean;
    disabled?: boolean;
    readOnly?: boolean;
    // Initial state - can be controlled by logic rules
    initialVisibility?: InitialVisibility;
    initialEnabled?: InitialEnabled;
    // Direction for radio/checkbox fields
    direction?: FieldDirection;
    rows?: number; // For textarea
    layout?: {
        colSpan?: number; // 1-12
        className?: string; // Custom classes
    };
}

export interface FormSchema {
    id: string;
    title: string;
    description?: string;
    fields: FormField[];
    logic?: LogicRule[];
}

// --- New Types for Backend ---

export interface CreateTemplateDto {
    contentAsJson: string; // JSON string of the form schema
    templateName: string;
    templateDescription?: string | null;
}

export interface Template {
    id: string;
    contentAsJson: string;
    templateName: string;
    templateDescription: string | null;
    createdByUserId: string | null;
    createdAt: string | null; // Date string
    updatedByUserId: string | null;
    updatedAt: string | null; // Date string
}

export interface CreateRequestDto {
    TemplateId: string;
    RequesterId: string; // UUID
    ApproverId?: string; // UUID
    Content: string; // JSON content
    files?: File[]; // For multi-part file upload
}

export interface TemplateRequest {
    id: string;
    templateId: string;
    requesterId: string;
    approverId: string;
    content: string;
    requestAttachmentDtos?: any[];
    template?: Template | null;
    requester?: any | null;
    approver?: any | null;
    createdAt?: string | null;
}

export interface CreateResponseDto {
    comment: string | null;
    requestId: string;
    respondedByUserId: string;
    files?: File[];
}

export interface TemplateResponse {
    id: string;
    requestId: string;
    respondedByUserId: string;
    comment: string | null;
    createdByUserId: string | null;
    createdAt: string | null;
    updatedByUserId: string | null;
    updatedAt: string | null;
    request: any | null;
    responseAttachments: {
        id: string;
        responseId: string;
        attachmentId: string;
        attachmentDto: any | null;
    }[];
    attachmentCount: number;
}

export interface FormResponse {
    id: string;
    formId: string;
    submittedAt: string;
    data: Record<string, any>;
    /** The complete form schema at the time of submission */
    schema: FormSchema;
    attachments?: any[];
}
