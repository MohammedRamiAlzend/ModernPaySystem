import React, { useState } from 'react';
import { EditArchiveRequest } from '../model/types';
import { useApproveEditRequest, useRejectEditRequest } from '../model/mutations';
import { useUIStore } from '@/app/store/uiStore';
import { Button } from '@/shared/ui/button';
import { Input } from '@/shared/ui/input';
import { Check, X, Eye, FileText, User } from 'lucide-react';
import { Label } from '@radix-ui/react-label';

interface EditRequestsListProps {
    requests: EditArchiveRequest[];
    isLoading: boolean;
    onViewDetails: (request: EditArchiveRequest) => void;
}

export function EditRequestsList({ requests, isLoading, onViewDetails }: EditRequestsListProps) {
    const { showStatus } = useUIStore();
    const approveMutation = useApproveEditRequest();
    const rejectMutation = useRejectEditRequest();

    const [rejectionNotes, setRejectionNotes] = useState<Record<string, string>>({});
    const [approvalNotes, setApprovalNotes] = useState<Record<string, string>>({});
    const [activeActionId, setActiveActionId] = useState<string | null>(null);
    const [actionType, setActionType] = useState<'approve' | 'reject' | null>(null);

    const handleApprove = (id: string) => {
        const notes = approvalNotes[id] || '';
        approveMutation.mutate(
            { id, notes },
            {
                onSuccess: () => {
                    showStatus({
                        type: 'success',
                        title: 'تمت الموافقة',
                        message: 'تمت الموافقة على طلب التعديل بنجاح. يمكنك الآن تعديل السجل يدوياً.'
                    });
                    setActiveActionId(null);
                    setActionType(null);
                },

                onError: (error: any) => {
                    if (error.response?.data?.errors?.[0]?.arabicDescription) {
                        showStatus({ type: 'error', title: 'خطأ', message: error.response.data.errors[0].arabicDescription });
                    }
                    else {
                        showStatus({
                            type: 'error',
                            title: 'خطأ',
                            message: error?.response?.data?.message || 'حدث خطأ أثناء معالجة الطلب.'
                        });
                    }
                }
            }
        );
    };

    const handleReject = (id: string) => {
        const reason = rejectionNotes[id] || '';
        if (!reason.trim()) {
            showStatus({
                type: 'warning',
                title: 'تنبيه',
                message: 'يرجى كتابة سبب الرفض.'
            });
            return;
        }

        rejectMutation.mutate(
            { id, reason },
            {
                onSuccess: () => {
                    showStatus({
                        type: 'success',
                        title: 'تم الرفض',
                        message: 'تم رفض طلب التعديل بنجاح.'
                    });
                    setActiveActionId(null);
                    setActionType(null);
                },
                onError: (error: any) => {
                    if (error.response?.data?.errors?.[0]?.arabicDescription) {
                        showStatus({ type: 'error', title: 'خطأ', message: error.response.data.errors[0].arabicDescription });
                    }
                    else {
                        showStatus({
                            type: 'error',
                            title: 'خطأ',
                            message: error?.response?.data?.message || 'حدث خطأ أثناء رفض الطلب.'
                        });
                    }
                }
            }
        );
    };

    if (isLoading) {
        return (
            <div className="flex flex-col items-center justify-center p-12 gap-3 text-muted-foreground">
                <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
                <span className="text-xs font-bold">جاري تحميل طلبات التعديل...</span>
            </div>
        );
    }

    if (requests.length === 0) {
        return (
            <div className="flex flex-col items-center justify-center p-16 gap-3 text-muted-foreground border border-dashed border-border rounded-3xl bg-muted/5">
                <FileText className="h-10 w-10 text-muted-foreground/50" />
                <span className="text-xs font-bold">لا توجد طلبات تعديل معلقة حالياً</span>
            </div>
        );
    }

    return (
        <div className="flex flex-col gap-4 text-right">
            <div className="overflow-x-auto border border-border rounded-2xl bg-card">
                <table className="w-full text-sm text-right text-foreground">
                    <thead className="text-xs text-muted-foreground font-bold bg-muted/30 border-b border-border">
                        <tr>
                            <th className="px-6 py-4">رقم الأرشيف</th>
                            <th className="px-6 py-4">مقدم الطلب</th>
                            <th className="px-6 py-4">سبب طلب التعديل</th>
                            <th className="px-6 py-4">تاريخ الطلب</th>
                            <th className="px-6 py-4 text-left">الإجراءات</th>
                        </tr>
                    </thead>
                    <tbody className="divide-y divide-border font-medium">
                        {requests.map((req) => (
                            <React.Fragment key={req.id}>
                                <tr className="hover:bg-muted/10 transition-colors">
                                    <td className="px-6 py-4 font-bold text-foreground">
                                        {req.archiveRecordArchivalNumber || 'بدون رقم'}
                                    </td>
                                    <td className="px-6 py-4 flex items-center gap-2 justify-end">
                                        <span>{req.requesterName}</span>
                                        <User className="h-4 w-4 text-muted-foreground" />
                                    </td>
                                    <td className="px-6 py-4 max-w-xs truncate">
                                        {req.justification}
                                    </td>
                                    <td className="px-6 py-4 text-muted-foreground text-xs">
                                        {req.createdAt ? new Date(req.createdAt).toLocaleDateString('ar-EG') : '-'}
                                    </td>
                                    <td className="px-6 py-4 text-left flex items-center gap-2 justify-start">
                                        <Button
                                            size="sm"
                                            variant="outline"
                                            onClick={() => onViewDetails(req)}
                                            className="rounded-xl h-8 px-3 font-bold border-border text-foreground hover:bg-muted"
                                        >
                                            <Eye className="h-3.5 w-3.5 ml-1" />
                                            <span>تفاصيل المقارنة</span>
                                        </Button>

                                        <Button
                                            size="sm"
                                            onClick={() => {
                                                setActiveActionId(req.id);
                                                setActionType('approve');
                                            }}
                                            className="rounded-xl h-8 px-3 bg-success hover:bg-success/90 text-white font-bold"
                                        >
                                            <Check className="h-3.5 w-3.5 ml-1" />
                                            <span>موافقة</span>
                                        </Button>

                                        <Button
                                            size="sm"
                                            variant="outline"
                                            onClick={() => {
                                                setActiveActionId(req.id);
                                                setActionType('reject');
                                            }}
                                            className="rounded-xl h-8 px-3 border-destructive/30 hover:bg-destructive/10 text-destructive font-bold"
                                        >
                                            <X className="h-3.5 w-3.5 ml-1" />
                                            <span>رفض</span>
                                        </Button>
                                    </td>
                                </tr>

                                {/* Inline Actions Panel */}
                                {activeActionId === req.id && (
                                    <tr>
                                        <td colSpan={5} className="bg-muted/10 p-4 border-t border-border">
                                            <div className="flex flex-col gap-3 max-w-lg mr-auto text-right">
                                                <Label className="text-xs font-bold text-foreground">
                                                    {actionType === 'approve'
                                                        ? 'ملاحظات الموافقة (اختياري)'
                                                        : 'سبب الرفض (مطلوب) *'}
                                                </Label>
                                                <div className="flex gap-2">
                                                    <Button
                                                        onClick={() => {
                                                            setActiveActionId(null);
                                                            setActionType(null);
                                                        }}
                                                        variant="outline"
                                                        className="rounded-xl h-10 px-4"
                                                    >
                                                        إلغاء
                                                    </Button>
                                                    <Button
                                                        onClick={() => actionType === 'approve' ? handleApprove(req.id) : handleReject(req.id)}
                                                        disabled={approveMutation.isPending || rejectMutation.isPending}
                                                        className={`rounded-xl h-10 px-5 font-bold ${actionType === 'approve' ? 'bg-success hover:bg-success/90 text-white' : 'bg-destructive hover:bg-destructive/90 text-white'}`}
                                                    >
                                                        {actionType === 'approve' ? 'تأكيد الموافقة' : 'تأكيد الرفض'}
                                                    </Button>
                                                    <Input
                                                        value={actionType === 'approve' ? (approvalNotes[req.id] || '') : (rejectionNotes[req.id] || '')}
                                                        onChange={(e) => {
                                                            if (actionType === 'approve') {
                                                                setApprovalNotes(prev => ({ ...prev, [req.id]: e.target.value }));
                                                            } else {
                                                                setRejectionNotes(prev => ({ ...prev, [req.id]: e.target.value }));
                                                            }
                                                        }}
                                                        placeholder={actionType === 'approve' ? 'أضف أي ملاحظات للموافقة هنا...' : 'اكتب سبب الرفض هنا...'}
                                                        className="rounded-xl h-10 bg-background border-border flex-1"
                                                    />
                                                </div>
                                            </div>
                                        </td>
                                    </tr>
                                )}
                            </React.Fragment>
                        ))}
                    </tbody>
                </table>
            </div>
        </div>
    );
}
