import React, { useState } from 'react';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription } from '@/shared/ui/dialog';
import { Button } from '@/shared/ui/button';
import { RefreshCw, FileStack, Plus, Trash2, ShieldAlert } from 'lucide-react';
import { useTemplatesByUserDirect, useAddUserTemplateOwnership, useRemoveUserTemplateOwnership } from '@/features/form-builder/model/useTemplateOwnerships';
import { useForms } from '@/features/form-builder/model/useForms';
import { SearchableSelect } from '@/shared/ui/searchable-select';
import { useUIStore } from '@/app/store/uiStore';
import { User } from '../api/usersApi';

interface UserTemplatesDialogProps {
    user: User | null;
    isOpen: boolean;
    onClose: () => void;
}

export const UserTemplatesDialog: React.FC<UserTemplatesDialogProps> = ({ user, isOpen, onClose }) => {
    const { showConfirm } = useUIStore();
    
    // Fetch assigned templates directly to user
    const { data: assignedTemplates = [], isLoading } = useTemplatesByUserDirect(user?.id);
    
    // Fetch all templates for assignment
    const { data: allTemplates = [] } = useForms();
    
    const addMut = useAddUserTemplateOwnership();
    const removeMut = useRemoveUserTemplateOwnership();
    
    const [selectedTemplate, setSelectedTemplate] = useState<string>('');
    
    // Unassigned templates are those not already in assignedTemplates
    const unassignedTemplates = allTemplates.filter(t => !assignedTemplates.some(at => at.id === t.id));
    const templateOptions = unassignedTemplates.map(t => ({ value: t.id, label: t.title }));

    const handleAssign = () => {
        if (!selectedTemplate || !user) return;
        addMut.mutate({ templateId: selectedTemplate, userId: user.id }, {
            onSuccess: () => setSelectedTemplate('')
        });
    };

    const handleRemove = (templateId: string, templateTitle: string) => {
        if (!user) return;
        showConfirm({
            title: 'إزالة الصلاحية المباشرة',
            message: `هل أنت متأكد من سحب صلاحية النموذج (${templateTitle}) من المستخدم ${user.userName}؟`,
            variant: 'destructive',
            confirmLabel: 'إزالة',
            onConfirm: () => removeMut.mutate({ templateId, userId: user.id })
        });
    };

    if (!user) return null;

    return (
        <Dialog open={isOpen} onOpenChange={(open) => !open && onClose()}>
            <DialogContent className="sm:max-w-2xl" style={{ direction: 'rtl' }}>
                <DialogHeader className="relative">
                    <button
                        onClick={onClose}
                        className="absolute left-0 top-0 p-2 rounded-xl hover:bg-muted/80 transition-colors text-muted-foreground hover:text-foreground"
                        aria-label="إغلاق"
                    >
                        <X className="w-5 h-5" />
                    </button>
                    <DialogTitle className="flex items-center gap-2 ml-8">
                        <ShieldAlert className="w-5 h-5 text-primary" />
                        الصلاحيات المباشرة للمستخدم
                    </DialogTitle>
                    <DialogDescription>
                        إدارة النماذج التي يمتلك <strong className="text-foreground">{user.userName}</strong> صلاحية مباشرة عليها استثنائياً.
                    </DialogDescription>
                </DialogHeader>

                <div className="space-y-6 py-4">
                    <div className="flex gap-2 items-end">
                        <div className="flex-1">
                            <label className="text-sm font-medium mb-1 block">منح صلاحية وصول لنموذج جديد</label>
                            <SearchableSelect
                                options={templateOptions}
                                value={selectedTemplate}
                                onValueChange={setSelectedTemplate}
                                placeholder="ابحث عن نموذج..."
                            />
                        </div>
                        <Button 
                            onClick={handleAssign} 
                            disabled={!selectedTemplate || addMut.isPending}
                            className="gap-2"
                        >
                            <Plus className="w-4 h-4" /> إضافة
                        </Button>
                    </div>

                    <div className="border rounded-md min-h-[250px]">
                        {isLoading ? (
                            <div className="flex flex-col items-center justify-center py-10 gap-4">
                                <RefreshCw className="w-6 h-6 animate-spin text-primary/50" />
                                <p className="text-sm text-muted-foreground">جاري تحميل النماذج...</p>
                            </div>
                        ) : assignedTemplates.length > 0 ? (
                            <ul className="divide-y max-h-[300px] overflow-y-auto custom-scrollbar">
                                {assignedTemplates.map(template => (
                                    <li key={template.id} className="flex items-center justify-between p-3 hover:bg-muted/50">
                                        <div className="flex items-center gap-3">
                                            <div className="w-8 h-8 rounded-full bg-primary/10 flex items-center justify-center shrink-0">
                                                <FileStack className="w-4 h-4 text-primary" />
                                            </div>
                                            <span className="font-medium line-clamp-1">{template.title}</span>
                                        </div>
                                        <Button 
                                            variant="ghost" 
                                            size="sm" 
                                            className="text-destructive hover:text-destructive/90 hover:bg-destructive/10 shrink-0"
                                            onClick={() => handleRemove(template.id, template.title)}
                                        >
                                            <Trash2 className="w-4 h-4" />
                                        </Button>
                                    </li>
                                ))}
                            </ul>
                        ) : (
                            <div className="flex flex-col items-center justify-center py-12 text-center text-muted-foreground">
                                <ShieldAlert className="w-12 h-12 mb-3 opacity-20" />
                                <p>لا يمتلك هذا المستخدم أي صلاحيات مباشرة لنماذج حالياً.</p>
                                <p className="text-sm mt-1 opacity-70">علماً بأنه قد يمتلك صلاحيات مستمدة من قسمه الحالي.</p>
                            </div>
                        )}
                    </div>
                </div>
            </DialogContent>
        </Dialog>
    );
};
