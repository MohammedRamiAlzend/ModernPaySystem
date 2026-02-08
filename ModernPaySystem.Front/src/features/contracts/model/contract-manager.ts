import { useState, useMemo } from 'react';
import { useContracts as useContractProvider } from '@/app/providers/contract-provider';
import type { ContractFormData } from '@/entities/contracts/types/contract-types';

export const useContractManager = () => {
  const {
    contracts,
    addContract: providerAddContract,
    updateContract: providerUpdateContract,
    getContract: providerGetContract
  } = useContractProvider();

  const [searchTerm, setSearchTerm] = useState('');

  const filteredContracts = useMemo(() => {
    if (!searchTerm) return contracts;

    const term = searchTerm.toLowerCase();

    return contracts.filter(contract =>
      contract.contractNo?.toLowerCase().includes(term) ||
      contract.district?.toLowerCase().includes(term) ||
      contract.tenantDescription?.toLowerCase().includes(term)
    );
  }, [contracts, searchTerm]);

  const handleAddContract = (contractData: ContractFormData) => {
    providerAddContract({
      ...contractData,
      rentAmount: parseFloat(contractData.rentAmount) || 0,
      lessorId: parseInt(contractData.lessorId) || 0,
      tenantId: parseInt(contractData.tenantId) || 0,
      status: 'active',
      
    });
  };

  const handleUpdateContract = (id: number, contractData: ContractFormData) => {
    providerUpdateContract(id, {
      ...contractData,
      rentAmount: parseFloat(contractData.rentAmount) || 0,
      lessorId: parseInt(contractData.lessorId) || 0,
      tenantId: parseInt(contractData.tenantId) || 0,
      status: 'active',
      id
    });
  };

  return {
    contracts: filteredContracts,
    searchTerm,
    setSearchTerm,
    addContract: handleAddContract,
    updateContract: handleUpdateContract,
    getContract: providerGetContract
  };
};
