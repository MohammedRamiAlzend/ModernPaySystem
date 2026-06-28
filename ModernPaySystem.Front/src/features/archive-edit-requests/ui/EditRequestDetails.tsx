import { useState } from 'react';
import { EditArchiveRequest } from '../model/types';
import { X, Calendar, User, FileText, Download, ExternalLink } from 'lucide-react';
import { Button } from '@/shared/ui/button';
import { archivingService } from '@/features/archiving/api/archivingService';
import { useUIStore } from '@/app/store/uiStore';

interface EditRequestDetailsProps {
    isOpen: boolean;
    request: EditArchiveRequest | null;
    onClose: () => void;
}

export function EditRequestDetails({ isOpen, request, onClose }: EditRequestDetailsProps) {
    const [previewStates, setPreviewStates] = useState<Record<string, string | null>>({});
    const { showStatus } = useUIStore();

    if (!isOpen || !request) return null;

    let originalData: { 
        Name?: string | null;
        FormId: string | null; 
        Content: Array<{ key: string; value: string | null }>;
        PhysicalFiles: Array<{ id: string; fileName: string }>;
    } | null = null;
    try {
        if (request.originalSnapshotJson) {
            const parsed = JSON.parse(request.originalSnapshotJson);
            // Map keys in content to lowercase to match our standard form input values
            const content = Array.isArray(parsed.Content || parsed.content)
                ? (parsed.Content || parsed.content).map((c: any) => ({
                    key: c.Key || c.key || '',
                    value: c.Value !== undefined ? c.Value : c.value !== undefined ? c.value : null
                }))
                : [];

            originalData = {
                Name: parsed.Name || parsed.name || null,
                FormId: parsed.FormId || parsed.formId || null,
                Content: content,
                PhysicalFiles: parsed.PhysicalFiles || parsed.physicalFiles || []
            };
        }
    } catch (e) {
        console.error("Failed to parse original snapshot json", e);
    }

    // Build side by side comparison fields
    const fieldsComparison: Array<{ label: string; original: string; proposed: string; isDifferent: boolean }> = [];

    if (originalData) {
        // Compare form metadata
        request.requestedChanges.forEach((proposedItem) => {
            const originalItem = originalData?.Content.find((o) => o.key.toLowerCase() === proposedItem.key.toLowerCase());
            const originalValue = originalItem ? originalItem.value || '' : '';
            const proposedValue = proposedItem.value || '';
            fieldsComparison.push({
                label: proposedItem.key,
                original: originalValue,
                proposed: proposedValue,
                isDifferent: originalValue !== proposedValue
            });
        });
    }

    const handleDownloadFile = async (fileId: string, fileName: string) => {
        try {
            const blob = await archivingService.downloadFileById(fileId);
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = fileName;
            document.body.appendChild(a);
            a.click();
            a.remove();
            window.URL.revokeObjectURL(url);
        } catch (error) {
            console.error('Failed to download file', error);
        }
    };

    const handleViewFile = async (fileId: string, fileName: string, includeDeleted: boolean = false) => {
        try {
            if (previewStates[fileId]) {
                window.URL.revokeObjectURL(previewStates[fileId]!);
                setPreviewStates(prev => {
                    const newState = { ...prev };
                    delete newState[fileId];
                    return newState;
                });
                return;
            }

            const ext = fileName.split('.').pop()?.toLowerCase() || '';
            const isImage = ['jpg', 'jpeg', 'png', 'gif', 'webp', 'bmp'].includes(ext);

            const normalizedId = fileId.toLowerCase().replace(/[{}]/g, '');
            const blob = await archivingService.viewFileBlobById(normalizedId, includeDeleted);
            const url = window.URL.createObjectURL(blob);
            
            if (isImage) {
                setPreviewStates(prev => ({ ...prev, [fileId]: url }));
            } else {
                window.open(url, '_blank');
                setTimeout(() => window.URL.revokeObjectURL(url), 30000);
            }
        } catch (error: any) {
            const status = error?.response?.status;
            showStatus({
                type: 'error',
                title: 'تعذّر معاينة الملف',
                message: status === 404
                    ? 'الملف غير موجود أو ربما تم حذفه مسبقاً من المستند.'
                    : 'حدث خطأ أثناء تحميل الملف. يرجى المحاولة مرة أخرى.'
            });
        }
    };

    return (
        <div className="fixed inset-0 bg-black/60 backdrop-blur-sm flex items-center justify-center p-4 z-50 animate-fade-in">
            <div className="bg-card border border-border rounded-3xl p-6 max-w-3xl w-full max-h-[90vh] shadow-2xl flex flex-col gap-6 text-right overflow-hidden" dir="rtl">

                {/* Header */}
                <div className="flex justify-between items-start border-b border-border pb-4 flex-shrink-0">
                    <button onClick={onClose} className="text-muted-foreground hover:text-foreground transition-colors p-1 rounded-lg">
                        <X className="h-5 w-5" />
                    </button>
                    <div className="flex flex-col gap-1">
                        <h2 className="text-base font-bold text-foreground">
                            تفاصيل ومقارنة طلب التعديل
                        </h2>
                        <p className="text-xs text-muted-foreground font-medium">
                            طلب رقم: {request.id.slice(0, 8)}
                        </p>
                    </div>
                </div>

                {/* Body Content */}
                <div className="flex-grow overflow-y-auto flex flex-col gap-5 pr-1.5 pl-0.5">

                    {/* Meta info card */}
                    <div className="grid grid-cols-1 sm:grid-cols-3 gap-3 bg-muted/20 border border-border p-4 rounded-2xl">
                        <div className="flex flex-col gap-1 items-start">
                            <span className="text-[10px] font-bold text-muted-foreground">مقدم الطلب</span>
                            <div className="flex items-center gap-1.5 text-xs font-semibold text-foreground">
                                <User className="h-3.5 w-3.5 text-muted-foreground" />
                                <span>{request.requesterName}</span>
                            </div>
                        </div>
                        <div className="flex flex-col gap-1 items-start">
                            <span className="text-[10px] font-bold text-muted-foreground">تاريخ التقديم</span>
                            <div className="flex items-center gap-1.5 text-xs font-semibold text-foreground">
                                <Calendar className="h-3.5 w-3.5 text-muted-foreground" />
                                <span>{request.createdAt ? new Date(request.createdAt).toLocaleString('ar-EG') : '-'}</span>
                            </div>
                        </div>
                        <div className="flex flex-col gap-1 items-start">
                            <span className="text-[10px] font-bold text-muted-foreground">رقم المستند الأرشيفي</span>
                            <div className="flex items-center gap-1.5 text-xs font-bold text-primary">
                                <FileText className="h-3.5 w-3.5 text-primary" />
                                <span>{request.archiveRecordId.slice(0, 8)}</span>
                            </div>
                        </div>
                    </div>

                    {/* Justification */}
                    <div className="flex flex-col gap-1.5 bg-primary/5 border border-primary/10 p-4 rounded-2xl">
                        <span className="text-xs font-bold text-primary">سبب طلب التعديل والتغييرات المطلوبة:</span>
                        <p className="text-xs font-medium text-foreground leading-relaxed whitespace-pre-wrap">
                            {request.justification}
                        </p>
                    </div>

                    {/* Name Comparison */}
                    {request.requestedRecordName && originalData?.Name !== undefined && (
                        <div className="flex flex-col gap-2 border-t border-border pt-4">
                            <span className="text-xs font-bold text-foreground">تعديل اسم المستند:</span>
                            <div className="border border-border rounded-2xl overflow-hidden bg-background">
                                <table className="w-full text-xs text-right">
                                    <thead className="bg-muted/30 text-muted-foreground font-bold border-b border-border">
                                        <tr>
                                            <th className="px-4 py-3">الحقل</th>
                                            <th className="px-4 py-3 text-center">القيمة الحالية</th>
                                            <th className="px-4 py-3 text-center">القيمة المقترحة</th>
                                            <th className="px-4 py-3 text-center">حالة التغيير</th>
                                        </tr>
                                    </thead>
                                    <tbody className="divide-y divide-border font-medium">
                                        <tr className="hover:bg-muted/10 transition-colors bg-success/5">
                                            <td className="px-4 py-3 font-bold text-foreground">اسم المستند</td>
                                            <td className="px-4 py-3 text-center text-muted-foreground line-through bg-destructive/5 font-semibold">
                                                {originalData.Name || '(فارغ)'}
                                            </td>
                                            <td className="px-4 py-3 text-center text-success-foreground bg-success/10 font-bold">
                                                {request.requestedRecordName || '(فارغ)'}
                                            </td>
                                            <td className="px-4 py-3 text-center font-bold">
                                                <span className="inline-flex items-center px-2 py-0.5 rounded-full bg-success/20 text-success-foreground text-[10px]">
                                                    تم التعديل
                                                </span>
                                            </td>
                                        </tr>
                                    </tbody>
                                </table>
                            </div>
                        </div>
                    )}

                    {/* Diff Comparison Table */}
                    <div className="flex flex-col gap-2">
                        <span className="text-xs font-bold text-foreground">جدول مقارنة حقول نموذج البيانات:</span>

                        {fieldsComparison.length > 0 ? (
                            <div className="border border-border rounded-2xl overflow-hidden bg-background">
                                <table className="w-full text-xs text-right">
                                    <thead className="bg-muted/30 text-muted-foreground font-bold border-b border-border">
                                        <tr>
                                            <th className="px-4 py-3">اسم الحقل</th>
                                            <th className="px-4 py-3 text-center">القيمة الحالية</th>
                                            <th className="px-4 py-3 text-center">القيمة المقترحة</th>
                                            <th className="px-4 py-3 text-center">حالة التغيير</th>
                                        </tr>
                                    </thead>
                                    <tbody className="divide-y divide-border font-medium">
                                        {fieldsComparison.map((f, index) => (
                                            <tr key={index} className={`hover:bg-muted/10 transition-colors ${f.isDifferent ? 'bg-success/5' : ''}`}>
                                                <td className="px-4 py-3 font-bold text-foreground">{f.label}</td>
                                                <td className="px-4 py-3 text-center text-muted-foreground line-through bg-destructive/5 font-semibold">
                                                    {f.original || '(فارغ)'}
                                                </td>
                                                <td className="px-4 py-3 text-center text-success-foreground bg-success/10 font-bold">
                                                    {f.proposed || '(فارغ)'}
                                                </td>
                                                <td className="px-4 py-3 text-center font-bold">
                                                    {f.isDifferent ? (
                                                        <span className="inline-flex items-center px-2 py-0.5 rounded-full bg-success/20 text-success-foreground text-[10px]">
                                                            تم التعديل
                                                        </span>
                                                    ) : (
                                                        <span className="inline-flex items-center px-2 py-0.5 rounded-full bg-muted/50 text-muted-foreground text-[10px]">
                                                            لا يوجد تغيير
                                                        </span>
                                                    )}
                                                </td>
                                            </tr>
                                        ))}
                                    </tbody>
                                </table>
                            </div>
                        ) : (
                            <div className="p-6 text-center text-xs font-bold text-muted-foreground border border-dashed border-border rounded-2xl">
                                لا توجد حقول ديناميكية تم اقتراح تعديلها (طلب تعديل عام)
                            </div>
                        )}
                    </div>

                    {/* Attached Files Section */}
                    {request.attachedFiles && request.attachedFiles.length > 0 && (
                        <div className="flex flex-col gap-2 border-t border-border pt-4">
                            <span className="text-xs font-bold text-foreground">الملفات المرفقة الجديدة مع طلب التعديل:</span>
                            <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                                {request.attachedFiles.map((file) => (
                                    <div key={file.id} className="flex flex-col gap-2 w-full">
                                        <div className="flex items-center justify-between border border-border p-3 rounded-2xl bg-muted/10 hover:bg-muted/20 transition-colors">
                                            <div className="flex flex-col text-right truncate pl-2">
                                                <span className="text-xs font-bold text-foreground truncate animate-fade-in" title={file.fileName}>
                                                    {file.fileName}
                                                </span>
                                                <span className="text-[10px] text-muted-foreground font-semibold mt-0.5">
                                                    {Math.round(file.fileSize / 1024)} KB | {file.fileExtension.replace('.', '').toUpperCase()}
                                                </span>
                                            </div>
                                            <div className="flex items-center gap-2">
                                                <Button
                                                    type="button"
                                                    variant="outline"
                                                    size="sm"
                                                    onClick={() => handleViewFile(file.id, file.fileName)}
                                                    className="rounded-xl px-3 font-bold text-xs h-8 flex items-center gap-1 hover:bg-primary/10 transition-colors"
                                                >
                                                    <ExternalLink className="h-3.5 w-3.5" />
                                                    <span>معاينة</span>
                                                </Button>
                                                <Button
                                                    type="button"
                                                    variant="outline"
                                                    size="sm"
                                                    onClick={() => handleDownloadFile(file.id, file.fileName)}
                                                    className="rounded-xl px-3 font-bold text-xs h-8 flex items-center gap-1 hover:bg-primary hover:text-primary-foreground transition-colors"
                                                >
                                                    <Download className="h-3.5 w-3.5" />
                                                    <span>تحميل</span>
                                                </Button>
                                            </div>
                                        </div>
                                        {previewStates[file.id] && (
                                            <div className="relative mt-2">
                                                <button onClick={() => {
                                                    window.URL.revokeObjectURL(previewStates[file.id]!);
                                                    setPreviewStates(prev => {
                                                        const newState = { ...prev };
                                                        delete newState[file.id];
                                                        return newState;
                                                    });
                                                }} className="absolute top-2 right-2 bg-background border border-border rounded-full p-1 text-muted-foreground hover:text-foreground">
                                                    <X className="h-4 w-4" />
                                                </button>
                                                <img src={previewStates[file.id]!} alt="Preview" className="max-h-[200px] w-full rounded-xl object-contain border border-border" />
                                            </div>
                                        )}
                                    </div>
                                ))}
                            </div>
                        </div>
                    )}

                    {/* Files Requested for Deletion Section */}
                    {request.fileIdsToDelete && request.fileIdsToDelete.length > 0 && (
                        <div className="flex flex-col gap-2 border-t border-border pt-4">
                            <span className="text-xs font-bold text-destructive">الملفات المطلوب حذفها من المستند:</span>
                            <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                                {request.fileIdsToDelete.map((fileId) => {
                                    const originalFile = originalData?.PhysicalFiles?.find(f => f.id.toLowerCase() === fileId.toLowerCase() || f.id === fileId);
                                    const fileName = originalFile ? originalFile.fileName : 'ملف غير معروف (أو تم حذفه مسبقاً)';
                                    return (
                                        <div key={fileId} className="flex flex-col gap-2 w-full">
                                            <div className="flex items-center justify-between border border-destructive/20 p-3 rounded-2xl bg-destructive/5 hover:bg-destructive/10 transition-colors">
                                                <div className="flex flex-col text-right truncate pl-2">
                                                    <span className="text-xs font-bold text-destructive truncate animate-fade-in" title={fileName}>
                                                        {fileName}
                                                    </span>
                                                </div>
                                                <div className="flex items-center gap-2">
                                                    <Button
                                                        type="button"
                                                        variant="outline"
                                                        size="sm"
                                                        onClick={() => handleViewFile(fileId, fileName, true)}
                                                        className="rounded-xl px-3 font-bold text-xs h-8 flex items-center gap-1 hover:bg-destructive hover:text-destructive-foreground transition-colors border-destructive/30 text-destructive"
                                                    >
                                                        <ExternalLink className="h-3.5 w-3.5" />
                                                        <span>معاينة للتحقق</span>
                                                    </Button>
                                                </div>
                                            </div>
                                            {previewStates[fileId] && (
                                                <div className="relative mt-2">
                                                    <button onClick={() => {
                                                        window.URL.revokeObjectURL(previewStates[fileId]!);
                                                        setPreviewStates(prev => {
                                                            const newState = { ...prev };
                                                            delete newState[fileId];
                                                            return newState;
                                                        });
                                                    }} className="absolute top-2 right-2 bg-background border border-border rounded-full p-1 text-muted-foreground hover:text-foreground">
                                                        <X className="h-4 w-4" />
                                                    </button>
                                                    <img src={previewStates[fileId]!} alt="Preview" className="max-h-[200px] w-full rounded-xl object-contain border border-border" />
                                                </div>
                                            )}
                                        </div>
                                    );
                                })}
                            </div>
                        </div>
                    )}

                </div>

                {/* Footer */}
                <div className="flex gap-2 justify-start border-t border-border pt-4 flex-shrink-0">
                    <Button
                        type="button"
                        variant="outline"
                        onClick={onClose}
                        className="rounded-xl px-5 font-bold"
                    >
                        إغلاق النافذة
                    </Button>
                </div>

            </div>
        </div>
    );
}
