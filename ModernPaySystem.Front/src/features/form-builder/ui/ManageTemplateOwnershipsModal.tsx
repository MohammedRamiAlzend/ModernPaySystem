import React, { useState } from 'react';
import { BaseModal } from '@/shared/ui/modals/base-modal';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/shared/ui/tabs';
import { Button } from '@/shared/ui/button';
import { Building2, Users, Trash2, Plus } from 'lucide-react';
import {
    useTemplateOwnerships,
    useUserTemplateOwnerships,
    useAddTemplateOwnership,
    useRemoveTemplateOwnership,
    useAddUserTemplateOwnership,
    useRemoveUserTemplateOwnership
} from '@/features/form-builder/model/useTemplateOwnerships';
import { useDepartments } from '@/entities/department/model/useDepartments';
import { useUsers } from '@/entities/user/api/userEndpoints';
import { useUIStore } from '@/app/store/uiStore';
import { SearchableSelect } from '@/shared/ui/searchable-select';

interface ManageTemplateOwnershipsModalProps {
    templateId: string;
    isOpen: boolean;
    onClose: () => void;
}

export const ManageTemplateOwnershipsModal: React.FC<ManageTemplateOwnershipsModalProps> = ({
    templateId,
    isOpen,
    onClose
}) => {
    const { showConfirm } = useUIStore();

    const { data: departmentOwnerships = [], isLoading: isLoadingDept } = useTemplateOwnerships(templateId);
    const { data: userOwnerships = [], isLoading: isLoadingUser } = useUserTemplateOwnerships(templateId);

    const { departmentOptions } = useDepartments();
    const { data: users = [] } = useUsers();

    const addDeptMut = useAddTemplateOwnership();
    const removeDeptMut = useRemoveTemplateOwnership();
    const addUserMut = useAddUserTemplateOwnership();
    const removeUserMut = useRemoveUserTemplateOwnership();

    const [selectedDept, setSelectedDept] = useState<string>('');
    const [selectedUser, setSelectedUser] = useState<string>('');

    const userOptions = users.map(u => ({ value: u.id, label: u.userName }));

    const handleAddDepartment = () => {
        if (!selectedDept) return;
        addDeptMut.mutate({ templateId, departmentId: selectedDept }, {
            onSuccess: () => setSelectedDept('')
        });
    };

    const handleRemoveDepartment = (departmentId: string, departmentName: string) => {
        showConfirm({
            title: 'إزالة الصلاحية',
            message: `هل أنت متأكد من إزالة صلاحية القسم (${departmentName}) من هذا النموذج؟`,
            variant: 'destructive',
            confirmLabel: 'إزالة',
            onConfirm: () => removeDeptMut.mutate({ templateId, departmentId })
        });
    };

    const handleAddUser = () => {
        if (!selectedUser) return;
        addUserMut.mutate({ templateId, userId: selectedUser }, {
            onSuccess: () => setSelectedUser('')
        });
    };

    const handleRemoveUser = (userId: string, userName: string) => {
        showConfirm({
            title: 'إزالة الصلاحية',
            message: `هل أنت متأكد من إزالة الصلاحية المباشرة للمستخدم (${userName}) من هذا النموذج؟`,
            variant: 'destructive',
            confirmLabel: 'إزالة',
            onConfirm: () => removeUserMut.mutate({ templateId, userId })
        });
    };

    return (
        <BaseModal
            isOpen={isOpen}
            onClose={onClose}
            title="إدارة صلاحيات النموذج"
            description="حدد الأقسام والمستخدمين المصرح لهم باستخدام هذا النموذج"
            maxWidth="xl"
            maxHeight="xl"
        >
            <Tabs defaultValue="departments" className="w-full mt-4">
                <TabsList className="grid w-full grid-cols-2">
                    <TabsTrigger value="departments" className="gap-2">
                        <Building2 className="w-4 h-4" /> الأقسام المصرحة
                    </TabsTrigger>
                    <TabsTrigger value="users" className="gap-2">
                        <Users className="w-4 h-4" /> المستخدمين المصرحين
                    </TabsTrigger>
                </TabsList>

                {/* Departments Tab */}
                <TabsContent value="departments" className="space-y-4 mt-4 min-h-[300px]">
                    <div className="flex gap-2 items-end">
                        <div className="flex-1">
                            <label className="text-sm font-medium mb-1 block">اختر القسم</label>
                            <SearchableSelect
                                options={departmentOptions}
                                value={selectedDept}
                                onValueChange={setSelectedDept}
                                placeholder="ابحث عن قسم لإضافته..."
                            />
                        </div>
                        <Button
                            onClick={handleAddDepartment}
                            disabled={!selectedDept || addDeptMut.isPending}
                            className="gap-2"
                        >
                            <Plus className="w-4 h-4" /> إضافة
                        </Button>
                    </div>

                    <div className="rounded-md border mt-4">
                        {isLoadingDept ? (
                            <div className="p-4 text-center text-muted-foreground">جاري التحميل...</div>
                        ) : departmentOwnerships.length === 0 ? (
                            <div className="p-8 text-center text-muted-foreground">لا توجد أقسام مصرح لها بعد.</div>
                        ) : (
                            <ul className="divide-y">
                                {departmentOwnerships.map((own) => (
                                    <li key={own.id} className="flex items-center justify-between p-3 hover:bg-muted/50">
                                        <span className="font-medium">{own.departmentName || 'قسم غير معروف'}</span>
                                        <Button
                                            variant="ghost"
                                            size="sm"
                                            className="text-destructive hover:text-destructive/90 hover:bg-destructive/10"
                                            onClick={() => handleRemoveDepartment(own.departmentId, own.departmentName || '')}
                                        >
                                            <Trash2 className="w-4 h-4" />
                                        </Button>
                                    </li>
                                ))}
                            </ul>
                        )}
                    </div>
                </TabsContent>

                {/* Users Tab */}
                <TabsContent value="users" className="space-y-4 mt-4 min-h-[300px]">
                    <div className="flex gap-2 items-end">
                        <div className="flex-1">
                            <label className="text-sm font-medium mb-1 block">اختر المستخدم</label>
                            <SearchableSelect
                                options={userOptions}
                                value={selectedUser}
                                onValueChange={setSelectedUser}
                                placeholder="ابحث عن مستخدم لمنحه الصلاحية المباشرة..."
                            />
                        </div>
                        <Button
                            onClick={handleAddUser}
                            disabled={!selectedUser || addUserMut.isPending}
                            className="gap-2"
                        >
                            <Plus className="w-4 h-4" /> إضافة
                        </Button>
                    </div>

                    <div className="rounded-md border mt-4">
                        {isLoadingUser ? (
                            <div className="p-4 text-center text-muted-foreground">جاري التحميل...</div>
                        ) : userOwnerships.length === 0 ? (
                            <div className="p-8 text-center text-muted-foreground">لا يوجد مستخدمين مصرحين بشكل مباشر.</div>
                        ) : (
                            <ul className="divide-y">
                                {userOwnerships.map((own) => (
                                    <li key={own.id} className="flex items-center justify-between p-3 hover:bg-muted/50">
                                        <span className="font-medium">{own.userName || 'مستخدم غير معروف'}</span>
                                        <Button
                                            variant="ghost"
                                            size="sm"
                                            className="text-destructive hover:text-destructive/90 hover:bg-destructive/10"
                                            onClick={() => handleRemoveUser(own.userId, own.userName || '')}
                                        >
                                            <Trash2 className="w-4 h-4" />
                                        </Button>
                                    </li>
                                ))}
                            </ul>
                        )}
                    </div>
                </TabsContent>
            </Tabs>
        </BaseModal>
    );
};
