import React, { useState } from 'react';
import { useForms, useDeleteForm } from '@/features/form-builder/model/useForms';
import { Button } from '@/shared/ui/button';
import { Card } from '@/shared/ui/card';
import { Skeleton } from '@/shared/ui/common/skeleton';
import { DeleteConfirmModal } from '@/shared/ui/modals/delete-confirm-modal';
import type { FormSchema } from '@/entities/form/model/types';

interface FormsListProps {
    onEdit: (form: FormSchema) => void;
    onCreate: () => void;
}

export const FormsList: React.FC<FormsListProps> = ({ onEdit, onCreate }) => {
    const { data: forms = [], isLoading } = useForms();
    const deleteMutation = useDeleteForm();

    // State for Confirm Modal
    const [idToDelete, setIdToDelete] = useState<string | null>(null);

    const handleDeleteClick = (id: string, e: React.MouseEvent) => {
        e.stopPropagation();
        setIdToDelete(id);
    };

    const handleConfirmDelete = () => {
        if (idToDelete) {
            deleteMutation.mutate(idToDelete, {
                onSuccess: () => {
                    setIdToDelete(null);
                }
            });
        }
    };

    if (isLoading) {
        return <Skeleton cards={3} />;
    }

    return (
        <div className="container mx-auto p-6 space-y-6">
            <div className="flex justify-between items-center">
                <h1 className="text-3xl font-bold">بناء النماذج (Form Builder)</h1>
                <Button onClick={onCreate}>+ إنشاء خدمة جديدة</Button>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                {forms.length === 0 ? (
                    <div className="col-span-12 text-center py-10 text-gray-500">
                        لا توجد نماذج حالياً. قم بإنشاء واحد جديد.
                    </div>
                ) : (
                    forms.map(form => (
                        <Card
                            key={form.id}
                            className="p-6 hover:shadow-lg transition-shadow cursor-pointer border-t-4 border-t-primary"
                            onClick={() => onEdit(form)}
                        >
                            <div className="flex justify-between items-start mb-4">
                                <h3 className="text-xl font-semibold">{form.title}</h3>
                                <Button
                                    variant="destructive"
                                    size="sm"
                                    onClick={(e) => handleDeleteClick(form.id, e)}
                                    disabled={deleteMutation.isPending && idToDelete === form.id}
                                >
                                    {deleteMutation.isPending && idToDelete === form.id ? 'جاري...' : 'حذف'}
                                </Button>
                            </div>

                            <div className="text-sm text-gray-500 mb-4">
                                {form.fields.length} حقول
                            </div>

                            <div className="text-xs text-gray-400">
                                ID: {form.id}
                            </div>
                        </Card>
                    ))
                )}
            </div>

            {/* Global Delete Confirm Modal */}
            <DeleteConfirmModal
                isOpen={!!idToDelete}
                onClose={() => setIdToDelete(null)}
                onConfirm={handleConfirmDelete}
                isLoading={deleteMutation.isPending}
                title="حذف النموذج"
                description="هل أنت متأكد من حذف هذا النموذج؟ سيؤدي هذا الإجراء إلى مسح كافة الحقول والقواعد المرتبطة به نهائياً."
            />
        </div>
    );
};
