import React from 'react';
import { cn } from '../../lib/utils';

// Table Root Component
const DataTable = React.forwardRef<
  HTMLDivElement,
  React.HTMLAttributes<HTMLDivElement>
>(({ className, ...props }, ref) => (
  <div ref={ref} className={cn("w-full", className)} {...props} />
));
DataTable.displayName = "DataTable";

// Table Header Component
const DataTableHeader = React.forwardRef<
  HTMLTableSectionElement,
  React.HTMLAttributes<HTMLTableSectionElement>
>(({ className, ...props }, ref) => (
  <thead ref={ref} className={cn("[&_tr]:border-b", className)} {...props} />
));
DataTableHeader.displayName = "DataTableHeader";

// Table Body Component
const DataTableBody = React.forwardRef<
  HTMLTableSectionElement,
  React.HTMLAttributes<HTMLTableSectionElement>
>(({ className, ...props }, ref) => (
  <tbody
    ref={ref}
    className={cn("[&_tr:last-child]:border-0", className)}
    {...props}
  />
));
DataTableBody.displayName = "DataTableBody";

// Table Footer Component
const DataTableFooter = React.forwardRef<
  HTMLTableSectionElement,
  React.HTMLAttributes<HTMLTableSectionElement>
>(({ className, ...props }, ref) => (
  <tfoot
    ref={ref}
    className={cn("bg-primary font-medium text-primary-foreground", className)}
    {...props}
  />
));
DataTableFooter.displayName = "DataTableFooter";

// Table Element
const DataTableElement = React.forwardRef<
  HTMLTableElement,
  React.TableHTMLAttributes<HTMLTableElement>
>(({ className, ...props }, ref) => (
  <table
    ref={ref}
    className={cn("w-full caption-bottom text-sm", className)}
    {...props}
  />
));
DataTableElement.displayName = "DataTable";

// Table Row Component
const DataTableRow = React.forwardRef<
  HTMLTableRowElement,
  React.HTMLAttributes<HTMLTableRowElement>
>(({ className, ...props }, ref) => (
  <tr
    ref={ref}
    className={cn(
      "border-b transition-colors hover:bg-muted/50 data-[state=selected]:bg-muted",
      className
    )}
    {...props}
  />
));
DataTableRow.displayName = "DataTableRow";

// Table Head Component
const DataTableHead = React.forwardRef<
  HTMLTableCellElement,
  React.ThHTMLAttributes<HTMLTableCellElement>
>(({ className, ...props }, ref) => (
  <th
    ref={ref}
    className={cn(
      "h-12 px-4 text-left align-middle font-medium text-muted-foreground [&:has([role=checkbox])]:pr-0",
      className
    )}
    {...props}
  />
));
DataTableHead.displayName = "DataTableHead";

// Table Cell Component
const DataTableCell = React.forwardRef<
  HTMLTableCellElement,
  React.TdHTMLAttributes<HTMLTableCellElement>
>(({ className, ...props }, ref) => (
  <td
    ref={ref}
    className={cn("p-4 align-middle [&:has([role=checkbox])]:pr-0", className)}
    {...props}
  />
));
DataTableCell.displayName = "DataTableCell";

// Table Caption Component
const DataTableCaption = React.forwardRef<
  HTMLTableCaptionElement,
  React.HTMLAttributes<HTMLTableCaptionElement>
>(({ className, ...props }, ref) => (
  <caption
    ref={ref}
    className={cn("mt-4 text-sm text-muted-foreground", className)}
    {...props}
  />
));
DataTableCaption.displayName = "DataTableCaption";

// Table Actions Component for common row actions
interface DataTableActionsProps<T> {
  item: T;
  actions: Array<{
    icon: React.ReactNode;
    label: string;
    onClick: (item: T) => void;
    variant?: 'primary' | 'secondary' | 'destructive' | 'ghost' | 'outline' | 'link';
    className?: string;
  }>;
  className?: string;
}

const DataTableActions = <T extends {}>({
  item,
  actions,
  className
}: DataTableActionsProps<T>) => {
  return (
    <div className={cn("flex items-center gap-1", className)}>
      {actions.map((action, index) => (
        <button
          key={index}
          onClick={() => action.onClick(item)}
          className={cn(
            "p-1.5 rounded-md transition-colors",
            action.variant === 'destructive' && "text-destructive hover:bg-destructive/10",
            action.variant === 'ghost' && "hover:bg-muted",
            action.variant === 'outline' && "border hover:bg-accent",
            action.className
          )}
          title={action.label}
        >
          {action.icon}
        </button>
      ))}
    </div>
  );
};

export {
  DataTable,
  DataTableHeader,
  DataTableBody,
  DataTableFooter,
  DataTableElement,
  DataTableRow,
  DataTableHead,
  DataTableCell,
  DataTableCaption,
  DataTableActions
};