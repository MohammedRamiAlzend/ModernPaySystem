// Types for processes
export interface Client {
    Client_No: number;
    FIRST_NAME: string;
    FATHER_NAME: string;
    LAST_NAME: string;
}

export interface KinshipType {
    CODE: number;
    DESCR: string;
}

export interface Service {
    code: number;
    descr: string;
}

export interface OperationDetail {
    code: number;
    id_services: number;
    num_copy: number;
    sum_copy: number;
    sum_amount: number;
}

export interface Operation {
    CODE: number;
    CODE_CLIENT: number;
    CODE_CLIENT2: number;
    CODE_KINSHIP: number;
    SUM_AMOUNT: number;
    DATE_ACTION: string;
    NOTES: string;
    user_no: number;
    Client_No: number;
    First_Name: string;
    FATHER_NAME: string;
    LAST_NAME: string;
    MOTHER_NAME: string;
}

export interface ProcessFormData {
    CODE: number;
    CODE_CLIENT: number;
    CODE_CLIENT2: number;
    CODE_KINSHIP: number;
    SUM_AMOUNT: number;
    DATE_ACTION: string;
    NOTES: string;
    user_no: number;
    Date_Of_work: string;
}