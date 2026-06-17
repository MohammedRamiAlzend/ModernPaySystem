import { Input } from '@/shared/ui/input';
import { Calendar } from 'lucide-react';
import type { Department } from '@/entities/department/model/types';

interface ReportFiltersProps {
    departments: Department[];
    selectedDepartment: string;
    onDepartmentChange: (id: string) => void;
    fromDate: string;
    onFromDateChange: (date: string) => void;
    toDate: string;
    onToDateChange: (date: string) => void;
}

export function ReportFilters({
    departments,
    selectedDepartment,
    onDepartmentChange,
    fromDate,
    onFromDateChange,
    toDate,
    onToDateChange,
}: ReportFiltersProps) {
    return (
        <div className="grid grid-cols-1 md:grid-cols-6 gap-4">
            <div className="space-y-1.5">
                <label className="text-xs font-semibold text-muted-foreground">القسم</label>
                <select
                    className="w-full h-10 px-3 py-2 bg-background border border-input rounded-md text-sm focus:outline-none focus:ring-2 focus:ring-primary focus:border-transparent text-right"
                    value={selectedDepartment}
                    onChange={(e) => onDepartmentChange(e.target.value)}
                >
                    <option value="">كل الأقسام</option>
                    {departments.map((d) => (
                        <option key={d.id} value={d.id}>{d.name}</option>
                    ))}
                </select>
            </div>

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
