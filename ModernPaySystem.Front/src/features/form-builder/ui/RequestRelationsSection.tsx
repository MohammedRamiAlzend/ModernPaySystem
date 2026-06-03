import { useState, useMemo } from 'react';
import { Link2, AlertCircle, Eye } from 'lucide-react';
// import { Link2, Plus, Trash2, AlertCircle, Eye } from 'lucide-react';
import { Button } from '@/shared/ui/button';
import { BaseModal } from '@/shared/ui/modals/base-modal';
import { Label } from '@/shared/ui/label';
import { Input } from '@/shared/ui/input';
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from '@/shared/ui/select';
import { useUIStore } from '@/app/store/uiStore';
import {
    useRequestRelations,
    useCreateRelation,
    // useDeleteRelation,
    useRequestById,
    useTemplateById
} from '../api/formEndpoints';
import { RequestPicker } from './RequestPicker';
import { ResponseDetailsModal } from '@/widgets/form-editor/ui/response-details-modal';
import { RequestRelationType, FormResponse } from '@/entities/form/model/types';

interface RequestRelationsSectionProps {
    requestId: string;
}

const RELATION_TYPE_OPTIONS = [
    { value: '0', label: 'مرجع فقط' },
    { value: '1', label: 'متابعة / استكمال' },
    { value: '2', label: 'استبدال لطلب سابق' },
    { value: '3', label: 'تكرار' }
];

const getRelationTypeLabel = (type: number) => {
    switch (type) {
        case 0: return 'مرجع فقط';
        case 1: return 'متابعة / استكمال';
        case 2: return 'استبدال لطلب سابق';
        case 3: return 'تكرار';
        default: return 'غير معروف';
    }
};

export const RequestRelationsSection = ({ requestId }: RequestRelationsSectionProps) => {
    const { showStatus } = useUIStore();
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [viewingRequestId, setViewingRequestId] = useState<string | null>(null);

    // Form fields for adding relation
    const [targetRequestIds, setTargetRequestIds] = useState<string[]>([]);
    const [relationType, setRelationType] = useState<RequestRelationType>(RequestRelationType.Reference);
    const [notes, setNotes] = useState('');

    // Fetch relations
    const { data: relations = [], isLoading: isLoadingRelations } = useRequestRelations(requestId);

    // Mutations
    const createRelationMutation = useCreateRelation();
    // const deleteRelationMutation = useDeleteRelation();

    const handleAddRelation = async () => {
        if (targetRequestIds.length === 0) return;

        // Filter out those that already exist
        const idsToAdd = targetRequestIds.filter(id => !relations.some(r => r.targetRequestId === id));

        if (idsToAdd.length === 0) {
            showStatus({
                type: 'warning',
                title: 'تنبيه',
                message: 'الارتباطات المحددة موجودة بالفعل.'
            });
            return;
        }

        try {
            const addPromises = idsToAdd.map(targetId =>
                createRelationMutation.mutateAsync({
                    sourceRequestId: requestId,
                    targetRequestId: targetId,
                    relationType,
                    notes: notes || undefined
                })
            );

            await Promise.all(addPromises);

            showStatus({
                type: 'success',
                title: 'نجاح',
                message: 'تم إضافة الارتباطات بنجاح.'
            });

            // Reset
            setTargetRequestIds([]);
            setRelationType(RequestRelationType.Reference);
            setNotes('');
            setIsModalOpen(false);
        } catch {
            showStatus({
                type: 'error',
                title: 'خطأ',
                message: 'فشل إضافة بعض الارتباطات. تأكد من صلاحية الإجراء.'
            });
        }
    };

    // const handleDeleteClick = (relationId: string) => {
    //     showConfirm({
    //         title: 'تأكيد حذف الارتباط',
    //         message: 'هل أنت متأكد من حذف هذا الارتباط؟ لا يمكن التراجع عن هذا الإجراء.',
    //         variant: 'destructive',
    //         confirmLabel: 'حذف',
    //         onConfirm: async () => {
    //             try {
    //                 await deleteRelationMutation.mutateAsync({
    //                     id: relationId,
    //                     sourceRequestId: requestId
    //                 });
    //                 showStatus({
    //                     type: 'success',
    //                     title: 'نجاح',
    //                     message: 'تم حذف الارتباط بنجاح.'
    //                 });
    //             } catch {
    //                 showStatus({
    //                     type: 'error',
    //                     title: 'خطأ',
    //                     message: 'فشل حذف الارتباط.'
    //                 });
    //             }
    //         }
    //     });
    // };

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <h3 className="text-lg font-bold flex items-center gap-2 text-primary">
                    <Link2 className="w-5 h-5" />
                    ارتباطات الطلبات القائمة
                </h3>
                {/* <Button
                    size="sm"
                    onClick={() => setIsModalOpen(true)}
                    className="gap-2 rounded-xl text-xs font-bold"
                >
                    <Plus className="w-4 h-4" />
                    إضافة ارتباط
                </Button> */}
            </div>

            {isLoadingRelations ? (
                <div className="text-center py-8 text-muted-foreground text-sm">جاري تحميل الارتباطات...</div>
            ) : relations.length > 0 ? (
                <div className="overflow-x-auto border border-primary/10 rounded-2xl bg-background/50 backdrop-blur-sm">
                    <table className="w-full text-right border-collapse text-xs md:text-sm" dir="rtl">
                        <thead className="bg-muted/40">
                            <tr>
                                <th className="px-4 py-3 font-bold text-muted-foreground text-right border-b">الطلب المرتبط</th>
                                <th className="px-4 py-3 font-bold text-muted-foreground text-right border-b">نوع الارتباط</th>
                                <th className="px-4 py-3 font-bold text-muted-foreground text-right border-b">ملاحظات</th>
                                <th className="px-4 py-3 font-bold text-muted-foreground text-center border-b">الإجراءات</th>
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-border/50">
                            {relations.map((rel) => (
                                <tr key={rel.id} className="hover:bg-muted/10 transition-colors">
                                    <td className="px-4 py-3 font-semibold">
                                        طلب #{rel.targetRequestNumber}
                                    </td>
                                    <td className="px-4 py-3">
                                        <span className="px-2 py-0.5 bg-primary/10 text-primary text-[10px] md:text-xs font-bold rounded-md">
                                            {getRelationTypeLabel(rel.relationType)}
                                        </span>
                                    </td>
                                    <td className="px-4 py-3 text-muted-foreground text-xs">
                                        {rel.notes || '---'}
                                    </td>
                                    <td className="px-4 py-3 text-center">
                                        <div className="flex items-center justify-center gap-1.5">
                                            <button
                                                onClick={() => setViewingRequestId(rel.targetRequestId)}
                                                className="p-1.5 text-muted-foreground hover:text-primary hover:bg-primary/10 rounded-lg transition-all"
                                                title="عرض تفاصيل الطلب"
                                            >
                                                <Eye className="w-4 h-4" />
                                            </button>
                                            {/* <button
                                                onClick={() => handleDeleteClick(rel.id)}
                                                className="p-1.5 text-muted-foreground hover:text-destructive hover:bg-destructive/10 rounded-lg transition-all"
                                                title="حذف الارتباط"
                                            >
                                                <Trash2 className="w-4 h-4" />
                                            </button> */}
                                        </div>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            ) : (
                <div className="flex flex-col items-center justify-center p-8 border border-dashed border-primary/10 rounded-2xl bg-muted/5 text-muted-foreground gap-2">
                    <AlertCircle className="w-8 h-8 opacity-30" />
                    <p className="text-sm font-medium">لا توجد طلبات مرتبطة حالياً</p>
                </div>
            )}

            {/* Add Relation Modal */}
            <BaseModal
                isOpen={isModalOpen}
                onClose={() => setIsModalOpen(false)}
                title="إضافة ارتباط جديد بالطلب"
                maxWidth="xl"
                maxHeight="xl"
                footer={
                    <div className="flex justify-end gap-3 w-full" dir="rtl">
                        <Button
                            variant="outline"
                            onClick={() => setIsModalOpen(false)}
                            className="rounded-xl"
                        >
                            إلغاء
                        </Button>
                        <Button
                            onClick={handleAddRelation}
                            disabled={targetRequestIds.length === 0 || createRelationMutation.isPending}
                            className="rounded-xl px-6"
                        >
                            إضافة الارتباط
                        </Button>
                    </div>
                }
            >
                <div className="space-y-4 text-right" dir="rtl">
                    <div className="space-y-2">
                        <Label className="text-xs font-bold text-muted-foreground">الطلب المراد الارتباط به</Label>
                        <RequestPicker
                            multiple
                            values={targetRequestIds}
                            onValuesChange={setTargetRequestIds}
                            excludeRequestId={requestId}
                            placeholder="اختر الطلب..."
                        />
                    </div>

                    <div className="space-y-2">
                        <Label className="text-xs font-bold text-muted-foreground">نوع الارتباط</Label>
                        <Select
                            value={String(relationType)}
                            onValueChange={(val) => setRelationType(Number(val) as RequestRelationType)}
                        >
                            <SelectTrigger className="h-10 rounded-xl bg-background/50 backdrop-blur-sm border-primary/10">
                                <SelectValue placeholder="اختر نوع الارتباط..." />
                            </SelectTrigger>
                            <SelectContent className="rounded-xl border-primary/10">
                                {RELATION_TYPE_OPTIONS.map(opt => (
                                    <SelectItem key={opt.value} value={opt.value}>
                                        {opt.label}
                                    </SelectItem>
                                ))}
                            </SelectContent>
                        </Select>
                    </div>

                    <div className="space-y-2">
                        <Label className="text-xs font-bold text-muted-foreground">ملاحظات (اختياري)</Label>
                        <Input
                            value={notes}
                            onChange={(e) => setNotes(e.target.value)}
                            placeholder="اكتب ملاحظة حول سبب الارتباط..."
                            className="h-10 rounded-xl bg-background/50 border-primary/10"
                        />
                    </div>
                </div>
            </BaseModal>

            {/* Request Details Modal */}
            {viewingRequestId && (
                <RequestDetailDialog
                    id={viewingRequestId}
                    onClose={() => setViewingRequestId(null)}
                />
            )}
        </div>
    );
};

interface RequestDetailDialogProps {
    id: string;
    onClose: () => void;
}

const RequestDetailDialog = ({ id, onClose }: RequestDetailDialogProps) => {
    const { data: request, isLoading: isLoadingRequest } = useRequestById(id);
    const { data: template, isLoading: isLoadingTemplate } = useTemplateById(request?.templateId || null);

    const viewingResponse = useMemo(() => {
        if (!request || !template) return null;
        try {
            return {
                id: request.id,
                formId: request.templateId,
                submittedAt: request.createdAt || new Date().toISOString(),
                data: (request.content || []).reduce((acc, curr) => {
                    acc[curr.key] = curr.value;
                    return acc;
                }, {} as Record<string, any>),
                schema: template,
                attachments: request.requestAttachmentDtos,
                requestNumber: request.requestNumber
            } as FormResponse;
        } catch {
            return null;
        }
    }, [request, template]);

    if (isLoadingRequest || isLoadingTemplate || !request || !template || !viewingResponse) {
        return (
            <BaseModal
                isOpen={true}
                onClose={onClose}
                title="جاري تحميل تفاصيل الطلب..."
                maxWidth="sm"
            >
                <div className="flex justify-center items-center py-12">
                    <span className="w-8 h-8 rounded-full border-4 border-primary border-t-transparent animate-spin" />
                </div>
            </BaseModal>
        );
    }

    return (
        <ResponseDetailsModal
            isOpen={true}
            onClose={onClose}
            response={viewingResponse}
            schema={template}
        />
    );
};
