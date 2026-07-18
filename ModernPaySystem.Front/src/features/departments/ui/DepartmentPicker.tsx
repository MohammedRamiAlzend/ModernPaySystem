import { useState } from 'react';
import { useDepartments } from '@/entities/department/model/useDepartments';
import { Department } from '@/entities/department/model/types';
import { Badge } from '@/shared/ui/badge';
import { X, Building2, ChevronDown, Check } from 'lucide-react';
import { Input } from '@/shared/ui/input';
import { cn } from '@/shared/lib/utils';

interface DepartmentPickerProps {
    selectedDepartmentIds: string[];
    onDepartmentsChange: (ids: string[]) => void;
    label?: string;
    placeholder?: string;
}

export const DepartmentPicker = ({
    selectedDepartmentIds,
    onDepartmentsChange,
    label = 'اختيار أقسام',
    placeholder = 'ابحث عن قسم...'
}: DepartmentPickerProps) => {
    const [isOpen, setIsOpen] = useState(false);
    const [search, setSearch] = useState('');
    const { data: departments = [], isLoading } = useDepartments();

    const filtered = departments.filter(d =>
        d.name.toLowerCase().includes(search.toLowerCase())
    );

    const toggleDepartment = (id: string) => {
        if (selectedDepartmentIds.includes(id)) {
            onDepartmentsChange(selectedDepartmentIds.filter(x => x !== id));
        } else {
            onDepartmentsChange([...selectedDepartmentIds, id]);
        }
    };

    const selectedDepartments = departments.filter(d => selectedDepartmentIds.includes(d.id));

    return (
        <div className="space-y-2">
            <label className="text-xs font-bold text-muted-foreground flex items-center gap-2">
                <Building2 className="w-3 h-3" />
                {label}
            </label>

            <div className="relative">
                <button
                    type="button"
                    onClick={() => setIsOpen(!isOpen)}
                    className="w-full flex items-center justify-between h-10 px-3 rounded-xl border border-border bg-background/50 text-sm hover:bg-accent/50 transition-colors"
                >
                    <span className={cn('text-sm', selectedDepartmentIds.length === 0 && 'text-muted-foreground')}>
                        {selectedDepartmentIds.length === 0
                            ? placeholder
                            : `${selectedDepartmentIds.length} قسم مختار`}
                    </span>
                    <ChevronDown className={cn('w-4 h-4 transition-transform', isOpen && 'rotate-180')} />
                </button>

                {isOpen && (
                    <div className="absolute top-full mt-1 left-0 right-0 z-50 bg-card border border-border rounded-xl shadow-xl max-h-60 overflow-hidden flex flex-col">
                        <div className="p-2 border-b border-border">
                            <Input
                                value={search}
                                onChange={(e) => setSearch(e.target.value)}
                                placeholder="ابحث عن قسم..."
                                className="h-9 text-sm rounded-lg"
                                autoFocus
                            />
                        </div>
                        <div className="overflow-y-auto flex-1 p-1">
                            {isLoading ? (
                                <div className="p-4 text-center text-sm text-muted-foreground">جاري التحميل...</div>
                            ) : filtered.length === 0 ? (
                                <div className="p-4 text-center text-sm text-muted-foreground">لا توجد أقسام</div>
                            ) : (
                                filtered.map(dept => (
                                    <button
                                        key={dept.id}
                                        type="button"
                                        onClick={() => toggleDepartment(dept.id)}
                                        className="w-full flex items-center gap-3 px-3 py-2.5 rounded-lg text-right text-sm hover:bg-accent transition-colors"
                                    >
                                        <div className={cn(
                                            'w-5 h-5 rounded-md border-2 flex items-center justify-center transition-colors shrink-0',
                                            selectedDepartmentIds.includes(dept.id)
                                                ? 'bg-primary border-primary text-primary-foreground'
                                                : 'border-muted-foreground/30'
                                        )}>
                                            {selectedDepartmentIds.includes(dept.id) && (
                                                <Check className="w-3 h-3" />
                                            )}
                                        </div>
                                        <span className="font-medium truncate">{dept.name}</span>
                                    </button>
                                ))
                            )}
                        </div>
                    </div>
                )}
            </div>

            {selectedDepartments.length > 0 && (
                <div className="flex flex-wrap gap-1.5">
                    {selectedDepartments.map(dept => (
                        <Badge
                            key={dept.id}
                            variant="secondary"
                            className="pl-1 pr-2 py-1 gap-1 flex items-center bg-background/80 border-primary/10"
                        >
                            <Building2 className="w-3 h-3 text-primary/70 shrink-0" />
                            <span className="text-[10px] font-medium">{dept.name}</span>
                            <button
                                onClick={() => toggleDepartment(dept.id)}
                                className="hover:text-destructive transition-colors p-0.5 rounded-full hover:bg-destructive/10"
                            >
                                <X className="w-3 h-3" />
                            </button>
                        </Badge>
                    ))}
                </div>
            )}
        </div>
    );
};
