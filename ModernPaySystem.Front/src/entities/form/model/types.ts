
export type FieldType = 'text' | 'email' | 'password' | 'number' | 'textarea' | 'select' | 'radio' | 'checkbox' | 'date' | 'label';

export interface ValidationRule {
    rule: 'required' | 'minLength' | 'maxLength' | 'pattern' | 'minValue' | 'maxValue';
    value?: string | number;
    message?: string;
}

export interface LogicAction {
    targetField: string;
    effect: 'show' | 'hide' | 'enable' | 'disable' | 'require' | 'unrequire' | 'spell';
}

export interface LogicRule {
    when: {
        field: string;
        operator: 'equals' | 'notEquals' | 'contains' | 'greaterThan' | 'lessThan' | 'greaterThanOrEqual' | 'lessThanOrEqual' | 'startsWith' | 'endsWith';
        value: string | number | boolean;
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
    defaultValue?: string | number | boolean | string[];
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
    numberSpelling?: {
        sourceField: string; // The numeric field to watch
    };
}

export interface FormSchema {
    id: string;
    title: string;
    description?: string;
    isRequireAttachments?: boolean;
    defaultReceiverDepartmentId?: string;
    fields: FormField[];
    logic?: LogicRule[];
}


// --- New Types for Backend ---

export interface CreateTemplateDto {
    contentAsJson: string; // JSON string of the form schema
    templateName: string;
    templateDescription?: string | null;
    isExternal?: boolean;
    isRequireAttachments?: boolean;
    defaultReceiverDepartmentId?: string;
    ownerId?: string;
    departmentId?: string;
}


export interface Template {
    id: string;
    contentAsJson: string;
    templateName: string;
    templateDescription: string | null;
    isRequireAttachments: boolean;
    defaultReceiverDepartmentId: string;
    createdByUserId: string | null;
    createdAt: string | null; // Date string
    updatedByUserId: string | null;
    updatedAt: string | null; // Date string
    isExternal: boolean;
}

export interface TemplateOwnershipDto {
    id: string;
    templateId: string;
    departmentId: string;
    departmentName: string | null;
}

export interface CreateTemplateOwnershipDto {
    departmentId: string;
}

export interface UserTemplateOwnershipDto {
    id: string;
    templateId: string;
    userId: string;
    userName: string | null;
}

export interface CreateUserTemplateOwnershipDto {
    userId: string;
}

export interface InputValueDto {
    key: string;
    value: string;
}

export interface CreateRequestDto {
    TemplateId: string;
    DepartmentId: string;
    ReadOnlyUsers?: string[]; // List of user IDs for CC/Read-only
    Content: InputValueDto[]; // Updated to use structured content
    files?: File[]; // For multi-part file upload
}


/** Represents a file attachment from the API */
export interface AttachmentDto {
    id: string;
    fileName: string;
    contentType: string | null;
    fileSize: number | null;
    filePath: string | null;
    createdAt: string | null;
}

/** Represents the join entity between a request and its attachments */
export interface RequestAttachmentDto {
    id: string;
    requestId: string;
    attachmentId: string;
    attachmentDto: AttachmentDto | null;
}

/** Lightweight user reference as returned in nested API responses */
export interface UserReference {
    id: string;
    userName: string;
    subSystemUserId: string | null;
    subSystem: number | null;
}

export interface TemplateRequest {
    id: string;
    templateId: string;
    requesterId: string;
    approverId: string;
    content: InputValueDto[];
    status: number; // RequestStatus
    currentTransactionId?: string | null;
    firstTransactionId?: string | null;
    requestAttachmentDtos?: RequestAttachmentDto[];
    template?: Template | null;
    requester?: UserReference | null;
    approver?: UserReference | null;
    createdAt?: string | null;
}

export interface CreateResponseDto {
    comment: string | null;
    requestId: string;
    respondedByUserId: string;
    files?: File[];
}

export interface CreateRequestTransactionDto {
    requestId: string;
    notes: string | null;
    parentTransactionId?: string | null;
    targetUserId: string; // We'll map this to CurrentUserHolderId in the API service
    files?: File[];
}

export interface RequestTransactionAttachmentDto {
    id: string;
    requestTransactionId: string;
    attachmentId: string;
    attachmentDto: AttachmentDto | null;
}

export interface RequestTransactionDto {
    id: string;
    requestId: string;
    notes: string | null;
    level: number;
    path: string;
    parentTransactionId: string | null;
    currentUserHolderId: string;
    status: number; // TransactionStatus (0: PendingAction, 1: Transferred)
    createdAt: string;
    createdByUserId: string | null;
    request?: TemplateRequest | null;
    currentUserHolder?: UserReference | null;
    requestTransactionAttachments?: RequestTransactionAttachmentDto[];
}

/** Represents the join entity between a response and its attachments */
export interface ResponseAttachmentDto {
    id: string;
    responseId: string;
    attachmentId: string;
    attachmentDto: AttachmentDto | null;
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
    request: TemplateRequest | null;
    responseAttachments: ResponseAttachmentDto[];
    attachmentCount: number;
}

export interface FormResponse {
    id: string;
    formId: string;
    submittedAt: string;
    data: Record<string, string | number | boolean | string[] | null>;
    /** The complete form schema at the time of submission */
    schema: FormSchema;
    attachments?: RequestAttachmentDto[];
}

// --- Pagination Types ---

export interface PagedResult<T> {
    items: T[];
    totalItems: number;
    page: number;
    pageSize: number;
    totalPages: number;
}

export interface InputValueFilterDto {
    key: string;
    value: string;
}

export enum FilterLogicalOperator {
    And = 1,
    Or = 2
}

export interface RequestPagedFilterDto {
    page: number;
    pageSize: number;
    fromDate?: string;
    toDate?: string;
    inputValueFilters?: InputValueFilterDto[];
    logicalOperator?: FilterLogicalOperator;
}
