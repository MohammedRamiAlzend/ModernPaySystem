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
    type: 'static' | 'api';
    options?: DataSourceOption[];
    url?: string; // If API
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
