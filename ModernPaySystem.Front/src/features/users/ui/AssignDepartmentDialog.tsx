import React, { useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import { departmentApi } from '@/entities/department/api/departmentApi';
import { queryKeys } from '@/shared/lib/query-keys';
import { useDepartmentActions } from '@/features/department-management/model/useDepartmentActions';
import { SearchableSelect, SearchableSelectOption } from '@/shared/ui/searchable-select';
import {
    Dialog,
    DialogContent,
    DialogHeader,
    DialogTitle,
    DialogFooter,
    DialogDescription
} from '@/shared/ui/dialog';
import { Button } from '@/shared/ui/button';
import { Label } from '@/shared/ui/label';
import { Building2, Loader2 } from 'lucide-react';

interface AssignDepartmentDialogProps {
    isOpen: boolean;
    onClose: () => void;
    userId: string;
    userName: string;
    initialDepartmentId?: string;
}

export const AssignDepartmentDialog: React.FC<AssignDepartmentDialogProps> = ({
    isOpen,
    onClose,
    userId,
    userName,
    initialDepartmentId
}) => {
    const [selectedDepartmentId, setSelectedDepartmentId] = React.useState<string>(initialDepartmentId || '');
    const { assignUserToDepartment, isLoading } = useDepartmentActions();

    // Reset selection when dialog opens with new user
    React.useEffect(() => {
        if (isOpen) {
            setSelectedDepartmentId(initialDepartmentId || '');
        }
    }, [isOpen, initialDepartmentId]);

    // Fetch all departments for selection
    const { data: allDepartments, isLoading: isAllLoading } = useQuery({
        queryKey: queryKeys.department.lists(),
        queryFn: () => departmentApi.search('', 0),
        enabled: isOpen
    });

    const departmentOptions: SearchableSelectOption[] = useMemo(() => {
        return (allDepartments || []).map(d => ({
            value: d.id,
            label: d.name,
            order: d.level,
            meta: { level: d.level }
        }));
    }, [allDepartments]);

    const handleAssign = async () => {
        if (!selectedDepartmentId) return;
        
        try {
            await assignUserToDepartment({ 
                departmentId: selectedDepartmentId, 
                userId 
            });
            onClose();
        } catch (error) {
            // Error handled in hook
        }
    };

    return (
        <Dialog open={isOpen} onOpenChange={(open) => !open && onClose()}>
            <DialogContent className="rounded-2xl max-w-md" style={{ direction: 'rtl' }}>
                <DialogHeader>
                    <DialogTitle className="flex items-center gap-2">
                        <Building2 className="w-5 h-5 text-primary" />
                        تعيين قسم للمستخدم
                    </DialogTitle>
                    <DialogDescription>
                        اختر القسم الذي يتبع له المستخدم <span className="font-bold text-foreground">"{userName}"</span>
                    </DialogDescription>
                </DialogHeader>

                <div className="space-y-4 py-4">
                    <div className="space-y-2">
                        <Label>القسم المستهدف</Label>
                        <SearchableSelect
                            options={departmentOptions}
                            value={selectedDepartmentId}
                            onValueChange={setSelectedDepartmentId}
                            placeholder="ابحث عن قسم..."
                            isLoading={isAllLoading}
                        />
                    </div>
                </div>

                <DialogFooter className="gap-2 sm:gap-0 mt-2">
                    <Button
                        onClick={handleAssign}
                        disabled={!selectedDepartmentId || isLoading}
                        className="rounded-xl h-11 px-8 font-bold flex-1 sm:flex-none shadow-lg shadow-primary/20"
                    >
                        {isLoading ? (
                            <Loader2 className="w-4 h-4 animate-spin ml-2" />
                        ) : null}
                        حفظ التعيين
                    </Button>
                    <Button
                        variant="outline"
                        onClick={onClose}
                        className="rounded-xl h-11 px-8 border-none bg-muted hover:bg-muted/80 flex-1 sm:flex-none"
                    >
                        إلغاء
                    </Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
};
