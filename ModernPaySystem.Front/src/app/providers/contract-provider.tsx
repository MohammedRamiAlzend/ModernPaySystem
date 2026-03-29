/* eslint-disable react-refresh/only-export-components */
import React, { createContext, useContext, useState, type ReactNode } from 'react';
import type { Contract } from '@/entities/contracts/types/contract-types';

// Define the context type
interface ContractContextType {
    contracts: Contract[];
    addContract: (contract: Omit<Contract, 'id'>) => void;
    updateContract: (id: number, contract: Partial<Contract>) => void;
    deleteContract: (id: number) => void;
    getContract: (id: number) => Contract | undefined;
}

const ContractContext = createContext<ContractContextType | undefined>(undefined);

export const useContracts = () => {
    const context = useContext(ContractContext);
    if (!context) {
        throw new Error('useContracts must be used within ContractProvider');
    }
    return context;
};

const ContractProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
    const [contracts, setContracts] = useState<Contract[]>([
        {
            id: 1,
            contractNo: 'CON-2023-001',
            contractDate: '2023-01-15',
            contractType: 'إيجار سكني',
            contractPeriod: 'سنة واحدة',
            rentAmount: 2500,
            rentStart: '2023-02-01',
            rentEnd: '2024-01-31',
            paymentMethod: 'شهري',
            contractOrganizer: 'الادارة العامة',
            ownershipProof: 'صك ملكية',
            specialConditions: 'لا توجد',
            propertyNo: 'P-001',
            city: 'الرياض',
            district: 'الملز',
            street: 'الأمير عبدالله بن عبدالعزيز',
            floor: 'الثالث',
            apartment: '301',
            occupancyType: 'سكني',
            phone: '0501234567',
            area: '150',
            realEstateRegion: 'شمال',
            taxReceiptNo: 'TR-001',
            taxReceiptAmount: '500',
            taxReceiptDate: '2023-01-10',
            assets: 'مكيفات، أثاث',
            tenantDescription: 'موظف حكومي',
            previousResidence: 'الرياض',
            lessorId: 1,
            lessorLegality: 'أصالة',
            tenantId: 1,
            tenantLegality: 'شخصي',
            escorts: 'الزوجة وثلاثة أبناء',
            status: 'active' as const,
        },
    ]);

    const addContract = (contract: Omit<Contract, 'id'>) => {
        const newId = contracts.length > 0 ? Math.max(...contracts.map(c => c.id)) + 1 : 1;
        const newContract: Contract = { ...contract, id: newId };
        setContracts([...contracts, newContract]);
    };

    const updateContract = (id: number, updates: Partial<Contract>) => {
        setContracts(contracts.map(contract =>
            contract.id === id ? { ...contract, ...updates } : contract
        ));
    };

    const deleteContract = (id: number) => {
        setContracts(contracts.filter(contract => contract.id !== id));
    };

    const getContract = (id: number) => {
        return contracts.find(contract => contract.id === id);
    };

    return (
        <ContractContext.Provider value={{
            contracts,
            addContract,
            updateContract,
            deleteContract,
            getContract,
        }}>
            {children}
        </ContractContext.Provider>
    );
};

export { ContractProvider };
// Separate export for fast refresh compatibility
const ContractProviderExport = ContractProvider;
export default ContractProviderExport;
