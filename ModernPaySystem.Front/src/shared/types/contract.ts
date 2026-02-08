export interface Contract {
    id: number;
    contractNo: string;
    contractDate: string;
    rentAmount: number;
    district: string;
    street: string;
    floor: string;
    apartment: string;
    rentStart: string;
    rentEnd: string;
    city: string;
    paymentMethod: string;
    tenantDescription: string;
    lessorId: number;
    tenantId: number;
    status: 'active' | 'expired' | 'pending';
}

export type CreateContractInput = Omit<Contract, 'id'>;
export type UpdateContractInput = Partial<CreateContractInput>;
