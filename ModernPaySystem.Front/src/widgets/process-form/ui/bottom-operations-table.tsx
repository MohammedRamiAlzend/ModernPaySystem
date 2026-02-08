import React from 'react';
import { OperationsTable } from '@/shared/ui/common/process-data-table';

interface BottomOperationsTableProps {
  operationsList: any[];
  currentRecordIndex: number;
  onRowClick: (op: any, index: number) => void;
  getClientFullName: (clientCode: number) => string;
}

export const BottomOperationsTable: React.FC<BottomOperationsTableProps> = ({
  operationsList,
  currentRecordIndex,
  onRowClick,
  getClientFullName
}) => {
  return (
    <div className="mt-12 bg-card rounded-2xl border shadow-sm p-6">
      <div className="flex items-center justify-between mb-6">
        <div className="flex items-center gap-2">
          <div className="w-1.5 h-6 bg-primary rounded-full" />
          <h3 className="font-bold text-lg">جدول بيانات المعاملات</h3>
        </div>
        <div className="text-xs text-muted-foreground font-medium bg-muted px-4 py-2 rounded-lg">
          عرض كافة المعاملات المسجلة في النظام
        </div>
      </div>

      <div className="rounded-xl border overflow-hidden">
        <OperationsTable 
          operations={operationsList}
          currentRecordIndex={currentRecordIndex}
          onRowClick={onRowClick}
          getClientFullName={getClientFullName}
        />
      </div>
    </div>
  );
};