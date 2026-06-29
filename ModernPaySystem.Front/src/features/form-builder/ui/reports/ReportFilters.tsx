import { Input } from '@/shared/ui/input';
import { Calendar } from 'lucide-react';

interface ReportFiltersProps {
    fromDate: string;
    onFromDateChange: (date: string) => void;
    toDate: string;
    onToDateChange: (date: string) => void;
}

export function ReportFilters({
    fromDate,
    onFromDateChange,
    toDate,
    onToDateChange,
}: ReportFiltersProps) {
    return (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div className="space-y-1.5">
                <label className="text-xs font-semibold text-muted-foreground">من تاريخ</label>
                <div className="relative">
                    <Input
                        type="date"
                        className="w-full pl-9 text-right"
                        value={fromDate}
                        onChange={(e) => onFromDateChange(e.target.value)}
                    />
                    <Calendar className="absolute left-3 top-3 h-4 w-4 text-muted-foreground pointer-events-none" />
                </div>
            </div>
            <div className="space-y-1.5">
                <label className="text-xs font-semibold text-muted-foreground">إلى تاريخ</label>
                <div className="relative">
                    <Input
                        type="date"
                        className="w-full pl-9 text-right"
                        value={toDate}
                        onChange={(e) => onToDateChange(e.target.value)}
                    />
                    <Calendar className="absolute left-3 top-3 h-4 w-4 text-muted-foreground pointer-events-none" />
                </div>
            </div>
        </div>
    );
}
