import React, { useState } from 'react';
import { ArchiveRecord } from '@/features/archiving/model/types';
import { archivingService } from '@/features/archiving/api/archivingService';
import { useSubmitEditRequest } from '../model/mutations';
import { useUIStore } from '@/app/store/uiStore';
import { Button } from '@/shared/ui/button';
import { Input } from '@/shared/ui/input';
import { Label } from '@/shared/ui/label';
import { Upload, Trash2, X, Eye } from 'lucide-react';

interface SubmitEditRequestModalProps {
    isOpen: boolean;
    record: ArchiveRecord | null;
    onClose: () => void;
}

export function SubmitEditRequestModal({ isOpen, record, onClose }: SubmitEditRequestModalProps) {
    const { showStatus } = useUIStore();
    const submitMutation = useSubmitEditRequest();

    const [justification, setJustification] = useState('');
    const [fields, setFields] = useState<Record<string, string>>({});
    const [selectedFiles, setSelectedFiles] = useState<File[]>([]);
    const [fileIdsToRemove, setFileIdsToRemove] = useState<string[]>([]);

    // Track previous props to synchronize state during render (avoids setState in useEffect)
    const [prevRecordId, setPrevRecordId] = useState<string | null>(null);
    const [prevIsOpen, setPrevIsOpen] = useState(false);

    if (isOpen !== prevIsOpen || (record && record.id !== prevRecordId)) {
        setPrevIsOpen(isOpen);
        setPrevRecordId(record ? record.id : null);
        setJustification('');
        setSelectedFiles([]);
        setFileIdsToRemove([]);

        // Populate fields from current template values
        const initialFields: Record<string, string> = {};
        if (record?.archiveRecordTemplateValues?.archiveRecordFormInputValues) {
            record.archiveRecordTemplateValues.archiveRecordFormInputValues.forEach(item => {
                initialFields[item.key] = item.value || '';
            });
        }
        setFields(initialFields);
    }

    if (!isOpen || !record) return null;

    const handleFieldChange = (key: string, value: string) => {
        setFields(prev => ({
            ...prev,
            [key]: value
        }));
    };

    const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        if (e.target.files) {
            const filesList = Array.from(e.target.files);
            setSelectedFiles(prev => [...prev, ...filesList]);
        }
    };

    const handleRemoveFile = (index: number) => {
        setSelectedFiles(prev => prev.filter((_, i) => i !== index));
    };

    const handleToggleExistingFile = (id: string) => {
        setFileIdsToRemove(prev => prev.includes(id)
            ? prev.filter(x => x !== id)
            : [...prev, id]
        );
    };

    const handleViewFile = async (fileId: string) => {
        try {
            const blob = await archivingService.viewFileBlobById(fileId);
            const url = window.URL.createObjectURL(blob);
            window.open(url, '_blank');
            setTimeout(() => window.URL.revokeObjectURL(url), 30000);
        } catch (err) {
            console.error('Failed to view file', err);
        }
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        if (!justification.trim()) {
            showStatus({
                type: 'warning',
                title: 'تنبيه',
                message: 'يرجى كتابة سبب طلب التعديل'
            });
            return;
        }

        const requestedChanges = Object.keys(fields).map(key => ({
            key,
            value: fields[key]
        }));

        submitMutation.mutate(
            {
                archiveRecordId: record.id,
                justification: justification.trim(),
                requestedChanges,
                files: selectedFiles,
                fileIdsToDelete: fileIdsToRemove
            },
            {
                onSuccess: () => {
                    showStatus({
                        type: 'success',
                        title: 'تم بنجاح',
                        message: 'تم إرسال طلب التعديل إلى مدير الأرشيف للمراجعة'
                    });
                    onClose();
                },
                onError: (error: any) => {
                    if (error.response?.data?.errors?.[0]?.arabicDescription) {
                        showStatus({ type: 'error', title: 'خطأ', message: error.response.data.errors[0].arabicDescription });
                    }
                    else {
                        showStatus({
                            type: 'error',
                            title: 'خطأ',
                            message: error?.response?.data?.message || 'فشل إرسال طلب التعديل. يرجى المحاولة لاحقاً.'
                        });
                    }
                }
            }
        );
    };

    return (
        <div className="fixed inset-0 bg-black/60 backdrop-blur-sm flex items-center justify-center p-4 z-50 animate-fade-in">
            <div className="bg-card border border-border rounded-3xl p-6 max-w-2xl w-full max-h-[90vh] shadow-2xl flex flex-col gap-6 text-right overflow-hidden">
                <div className="flex justify-between items-start border-b border-border pb-4 flex-shrink-0">
                    <button onClick={onClose} className="text-muted-foreground hover:text-foreground transition-colors p-1 rounded-lg">
                        <X className="h-5 w-5" />
                    </button>
                    <div className="flex flex-col gap-1">
                        <h2 className="text-base font-bold text-foreground">
                            تقديم طلب تعديل مستند مؤرشف
                        </h2>
                        <p className="text-xs text-muted-foreground font-medium">
                            رقم الأرشيف: {record.archivalNumber}
                        </p>
                    </div>
                </div>

                <form onSubmit={handleSubmit} className="flex flex-col gap-5 flex-1 overflow-hidden">
                    <div className="flex-1 overflow-y-auto flex flex-col gap-5 pr-1.5 pl-0.5">

                        {/* Cause of edit */}
                        <div className="flex flex-col gap-2">
                            <Label className="text-xs font-semibold text-muted-foreground">سبب التعديل المطلوب *</Label>
                            <textarea
                                value={justification}
                                onChange={(e) => setJustification(e.target.value)}
                                placeholder="يرجى كتابة سبب طلب التعديل والتغييرات المطلوبة بالتفصيل..."
                                className="flex min-h-[80px] w-full rounded-2xl border border-border bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50 text-right"
                                required
                            />
                        </div>

                        {/* Metadata edits */}
                        {Object.keys(fields).length > 0 && (
                            <div className="bg-muted/30 border border-border p-4 rounded-2xl flex flex-col gap-3">
                                <span className="text-xs font-bold text-muted-foreground border-b pb-2 mb-1 border-border">
                                    تعديل بيانات حقول النموذج المقترحة:
                                </span>
                                <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                                    {Object.keys(fields).map((key) => (
                                        <div key={key} className="flex flex-col gap-1">
                                            <Label className="text-xs font-semibold text-muted-foreground">{key}</Label>
                                            <Input
                                                value={fields[key]}
                                                onChange={(e) => handleFieldChange(key, e.target.value)}
                                                className="rounded-lg h-9 bg-background"
                                            />
                                        </div>
                                    ))}
                                </div>
                            </div>
                        )}

                        {/* Existing Files to Delete */}
                        {record.physicalFiles && record.physicalFiles.length > 0 && (
                            <div className="flex flex-col gap-2 bg-muted/20 border border-border p-4 rounded-2xl">
                                <Label className="text-xs font-semibold text-muted-foreground mb-1">
                                    الملفات الموجودة حالياً بالمستند (اختر لحذفها):
                                </Label>
                                <div className="flex flex-col gap-1.5 max-h-[150px] overflow-y-auto">
                                    {record.physicalFiles.map(f => {
                                        const isRemoved = fileIdsToRemove.includes(f.id);
                                        return (
                                            <div key={f.id} className="flex items-center justify-between text-xs p-2 rounded-xl hover:bg-muted/40 transition-colors border border-border bg-background">
                                                <span className={`truncate flex-1 text-right ${isRemoved ? 'line-through text-destructive font-bold' : 'font-semibold text-foreground'}`}>
                                                    {f.fileName}
                                                </span>
                                                <div className="flex items-center gap-1.5 mr-2">
                                                    <button
                                                        type="button"
                                                        className={`p-1.5 rounded-lg transition-colors border ${isRemoved ? 'border-primary text-primary hover:bg-primary/10' : 'border-destructive/30 text-destructive hover:bg-destructive/10'}`}
                                                        onClick={() => handleToggleExistingFile(f.id)}
                                                    >
                                                        {isRemoved ? <span className="font-bold text-[10px] px-1">تراجع</span> : <Trash2 className="h-4 w-4" />}
                                                    </button>
                                                    <button
                                                        type="button"
                                                        className="p-1.5 rounded-lg text-muted-foreground hover:bg-muted/60 hover:text-foreground transition-colors"
                                                        onClick={() => handleViewFile(f.id)}
                                                        title="معاينة الملف"
                                                    >
                                                        <Eye className="h-4 w-4" />
                                                    </button>
                                                </div>
                                            </div>
                                        );
                                    })}
                                </div>
                            </div>
                        )}

                        {/* File Upload Section */}
                        <div className="flex flex-col gap-2">
                            <Label className="text-xs font-semibold text-muted-foreground">إرفاق مستندات أو ملفات جديدة (اختياري)</Label>

                            <div className="flex flex-col items-center justify-center border-2 border-dashed border-border hover:border-primary/50 rounded-2xl p-6 bg-muted/10 transition-colors cursor-pointer relative">
                                <input
                                    type="file"
                                    multiple
                                    onChange={handleFileChange}
                                    className="absolute inset-0 w-full h-full opacity-0 cursor-pointer"
                                />
                                <Upload className="h-8 w-8 text-muted-foreground mb-2" />
                                <span className="text-xs font-bold text-foreground">اضغط هنا أو اسحب الملفات لتحميلها</span>
                                <span className="text-[10px] text-muted-foreground mt-1">تنسيقات الملفات المسموحة: PDF, PNG, JPG, Docx</span>
                            </div>

                            {/* Selected Files List */}
                            {selectedFiles.length > 0 && (
                                <div className="flex flex-col gap-1.5 mt-2 max-h-[150px] overflow-y-auto border border-border rounded-2xl p-3 bg-muted/15">
                                    <span className="text-[10px] font-bold text-muted-foreground border-b pb-1 mb-1 border-border">الملفات المختارة للتحميل:</span>
                                    {selectedFiles.map((file, idx) => (
                                        <div key={idx} className="flex items-center justify-between text-xs p-1.5 rounded-xl hover:bg-muted/40 transition-colors">
                                            <button
                                                type="button"
                                                onClick={() => handleRemoveFile(idx)}
                                                className="text-destructive hover:bg-destructive/10 p-1 rounded-lg transition-colors"
                                            >
                                                <Trash2 className="h-4 w-4" />
                                            </button>
                                            <span className="truncate flex-1 text-right font-medium text-foreground">
                                                {file.name} ({Math.round(file.size / 1024)} KB)
                                            </span>
                                        </div>
                                    ))}
                                </div>
                            )}
                        </div>

                    </div>

                    <div className="flex gap-3 justify-start border-t border-border pt-4 flex-shrink-0">
                        <Button
                            type="button"
                            variant="outline"
                            onClick={onClose}
                            className="rounded-xl px-5"
                        >
                            إلغاء
                        </Button>
                        <Button
                            type="submit"
                            disabled={submitMutation.isPending}
                            className="rounded-xl px-6 font-bold"
                        >
                            {submitMutation.isPending ? 'جاري الإرسال...' : 'إرسال الطلب'}
                        </Button>
                    </div>
                </form>
            </div>
        </div>
    );
}
