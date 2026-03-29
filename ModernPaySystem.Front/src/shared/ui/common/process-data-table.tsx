import React from 'react';
import { Button } from '../button';
import { cn } from '../../lib/utils';
import {
  DataTable,
  DataTableElement,
  DataTableHeader,
  DataTableBody,
  DataTableRow,
  DataTableHead,
  DataTableCell,
} from './data-table';

type ColumnDef<T> = {
  accessorKey: keyof T;
  header: string;
  cell?: (row: T, index?: number) => React.ReactNode;
  className?: string;
};

interface DataTableProps<T> {
  columns: ColumnDef<T>[];
  data: T[];
  caption?: string;
  className?: string;
  onRowClick?: (row: T, index: number) => void;
  rowClassName?: (row: T, index: number) => string;
  emptyMessage?: string;
}

const ProcessDataTable = <T extends Record<string, unknown>>({
  columns,
  data,
  caption,
  className,
  onRowClick,
  rowClassName,
  emptyMessage = "لا توجد بيانات لعرضها"
}: DataTableProps<T>) => {
  return (
    <DataTable className={className}>
      <DataTableElement className="w-full border rounded-xl overflow-hidden">
        {caption && (
          <caption className="mt-4 text-sm text-muted-foreground px-4 py-2 text-right">
            {caption}
          </caption>
        )}
        <DataTableHeader className="bg-muted/50">
          <DataTableRow>
            {columns.map((column, index) => (
              <DataTableHead 
                key={index} 
                className={cn("text-right py-3 px-4", column.className)}
              >
                {column.header}
              </DataTableHead>
            ))}
          </DataTableRow>
        </DataTableHeader>
        <DataTableBody>
          {data.length > 0 ? (
            data.map((row, rowIndex) => (
              <DataTableRow
                key={rowIndex}
                onClick={() => onRowClick && onRowClick(row, rowIndex)}
                className={cn(
                  "transition-colors cursor-pointer hover:bg-muted/50",
                  onRowClick && "cursor-pointer hover:bg-muted/30",
                  rowClassName && rowClassName(row, rowIndex)
                )}
              >
                {columns.map((column, colIndex) => (
                  <DataTableCell
                    key={colIndex}
                    className="py-3 px-4 text-right"
                  >
                    {column.cell ? column.cell(row, rowIndex) : String(row[column.accessorKey])}
                  </DataTableCell>
                ))}
              </DataTableRow>
            ))
          ) : (
            <DataTableRow>
              <DataTableCell 
                colSpan={columns.length} 
                className="h-24 text-center text-muted-foreground py-8"
              >
                {emptyMessage}
              </DataTableCell>
            </DataTableRow>
          )}
        </DataTableBody>
      </DataTableElement>
    </DataTable>
  );
};

// Specific table for services selection
interface ServicesTableProps {
  services: any[];
  selectedServices: any[];
  onRemoveService: (code: number) => void;
  className?: string;
}

const ServicesTable = ({ 
  services, 
  selectedServices, 
  onRemoveService,
  className 
}: ServicesTableProps) => {
  const columns = [
    {
      accessorKey: 'index' as const,
      header: '#',
      cell: (_: any, index?: number) => index !== undefined ? index + 1 : '',
    },
    {
      accessorKey: 'serviceName' as const,
      header: 'الخدمة',
      cell: (row: any) => {
        const service = services.find(s => s.code === row.id_services);
        return service?.descr || 'غير معروف';
      },
    },
    {
      accessorKey: 'num_copy' as const,
      header: 'العدد',
      className: 'text-center',
    },
    {
      accessorKey: 'sum_copy' as const,
      header: 'مبلغ النسخة',
      cell: (row: any) => row.sum_copy.toLocaleString(),
    },
    {
      accessorKey: 'sum_amount' as const,
      header: 'الإجمالي',
      cell: (row: any) => (
        <span className="font-bold text-primary">{row.sum_amount.toLocaleString()}</span>
      ),
    },
    {
      accessorKey: 'actions' as const,
      header: '',
      cell: (row: any) => (
        <div className="flex justify-end">
          <Button
            size="icon"
            variant="ghost"
            className="h-8 w-8 opacity-0 group-hover:opacity-100 hover:text-destructive transition-all"
            onClick={(e) => {
              e.stopPropagation();
              onRemoveService(row.code);
            }}
            title="إزالة الخدمة"
          >
            <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" className="lucide lucide-x"><path d="M18 6 6 18"/><path d="m6 6 12 12"/></svg>
          </Button>
        </div>
      ),
    },
  ];

  return (
    <ProcessDataTable
      columns={columns}
      data={selectedServices}
      emptyMessage="لم يتم إضافة أي خدمات بعد"
      className={className}
    />
  );
};

// Specific table for operations list
interface OperationsTableProps {
  operations: any[];
  currentRecordIndex: number;
  onRowClick: (op: any, index: number) => void;
  getClientFullName: (clientCode: number) => string;
  className?: string;
}

const OperationsTable = ({ 
  operations, 
  currentRecordIndex, 
  onRowClick,
  getClientFullName,
  className 
}: OperationsTableProps) => {
  const columns = [
    {
      accessorKey: 'CODE' as const,
      header: '#',
    },
    {
      accessorKey: 'CODE_CLIENT' as const,
      header: 'اسم المواطن',
      cell: (row: any) => getClientFullName(row.CODE_CLIENT),
    },
    {
      accessorKey: 'SUM_AMOUNT' as const,
      header: 'المبلغ الإجمالي',
      cell: (row: any) => (
        <span className="font-bold text-primary">{row.SUM_AMOUNT.toLocaleString()} L.L</span>
      ),
    },
    {
      accessorKey: 'DATE_ACTION' as const,
      header: 'تاريخ العملية',
      cell: (row: any) => new Date(row.DATE_ACTION).toLocaleDateString('ar-SA'),
    },
    {
      accessorKey: 'NOTES' as const,
      header: 'رقم الدور',
    },
  ];

  return (
    <ProcessDataTable
      columns={columns}
      data={operations}
      emptyMessage="لا توجد معاملات لعرضها"
      className={className}
      onRowClick={onRowClick}
      rowClassName={(_, index) =>
        index === currentRecordIndex ? "bg-primary/5 hover:bg-primary/10" : "hover:bg-muted/50"
      }
    />
  );
};

export { ProcessDataTable, ServicesTable, OperationsTable };