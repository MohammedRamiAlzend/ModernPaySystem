import React, { useState } from 'react';
import { DynamicFormTemplate, PhysicalFile } from '@/features/archiving/model/types';
import { ImageMeta, ScannerModal } from '@/features/document-scanner';
import { Button } from '@/shared/ui/button';
import { Input } from '@/shared/ui/input';
import { Label } from '@/shared/ui/label';
import { Switch } from '@/shared/ui/switch';
import { Progress } from '@/shared/ui/progress';
import { Upload, ScanLine, Plus, Trash2 } from 'lucide-react';

interface RecordModalProps {
    isOpen: boolean;
    mode: 'create' | 'edit';
    archivalNumber: string;
    onArchivalNumberChange: (val: string) => void;
    selectedTemplateId: string;
    onSelectedTemplateIdChange: (val: string) => void;
    dynamicTemplates: DynamicFormTemplate[];
    templateInputs: Record<string, string>;
    onTemplateInputChange: (label: string, value: string) => void;
    setTemplateInputs: React.Dispatch<React.SetStateAction<Record<string, string>>>;
    generateQrCover: boolean;
    onGenerateQrCoverChange: (val: boolean) => void;
    existingFiles: PhysicalFile[];
    fileIdsToRemove: string[];
    onToggleRemoveExistingFile: (id: string) => void;
    selectedFiles: File[];
    onAddSelectedFiles: (files: File[]) => void;
    onRemoveSelectedFile: (index: number) => void;
    isSaving: boolean;
    uploadProgress: number;
    onSubmit: (e: React.FormEvent) => void;
    onClose: () => void;
}

export function RecordModal({
    isOpen,
    mode,
    archivalNumber,
    onArchivalNumberChange,
    selectedTemplateId,
    onSelectedTemplateIdChange,
    dynamicTemplates,
    templateInputs,
    onTemplateInputChange,
    setTemplateInputs,
    generateQrCover,
    onGenerateQrCoverChange,
    existingFiles,
    fileIdsToRemove,
    onToggleRemoveExistingFile,
    selectedFiles,
    onAddSelectedFiles,
    onRemoveSelectedFile,
    isSaving,
    uploadProgress,
    onSubmit,
    onClose
}: RecordModalProps) {
    const [showScannerModal, setShowScannerModal] = useState(false);
    const [scannerFiles, setScannerFiles] = useState<ImageMeta[]>([]);

    if (!isOpen) return null;

    const handleApplyScanner = (ocrText: string, files: ImageMeta[]) => {
        const fileObjects = files.map(f => f.file);
        onAddSelectedFiles(fileObjects);

        // Fills the first matching textarea/text field in the form with OCR text
        const template = dynamicTemplates.find(t => t.id === selectedTemplateId);
        if (template) {
            try {
                const fields = JSON.parse(template.contentAsJson);
                if (Array.isArray(fields)) {
                    const targetField = fields.find(f =>
                        f.type === 'textarea' ||
                        f.label.includes('نص') ||
                        f.label.includes('محتوى') ||
                        f.label.includes('ملاحظات')
                    ) || fields[0];

                    if (targetField) {
                        setTemplateInputs(prev => ({
                            ...prev,
                            [targetField.label]: ocrText
                        }));
                    }
                }
            } catch (e) {
                console.error(e);
            }
        }
        setShowScannerModal(false);
        setScannerFiles([]);
    };

    return (
        <>
            <div className="fixed inset-0 bg-black/60 backdrop-blur-sm flex items-center justify-center p-4 z-50 animate-fade-in">
                <div className="bg-card border border-border rounded-3xl p-6 max-w-2xl w-full max-h-[90vh] shadow-2xl flex flex-col gap-6 text-right overflow-hidden">
                    <div className="flex flex-col gap-1 border-b border-border pb-4 flex-shrink-0">
                        <h2 className="text-base font-bold text-foreground">
                            {mode === 'create' ? 'أرشفة مستند جديد' : 'تعديل بيانات المستند المؤرشف'}
                        </h2>
                        <p className="text-xs text-muted-foreground font-medium">
                            املأ تفاصيل الأرشفة وأرفق الملفات الخاصة بالمستند
                        </p>
                    </div>

                    <form onSubmit={onSubmit} className="flex flex-col gap-5 flex-1 overflow-hidden">
                        <div className="flex-1 overflow-y-auto flex flex-col gap-5 pr-1.5 pl-0.5">
                            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                                <div className="flex flex-col gap-2">
                                    <Label className="text-xs font-semibold text-muted-foreground">رقم الأرشفة</Label>
                                    <Input
                                        value={archivalNumber}
                                        onChange={(e) => onArchivalNumberChange(e.target.value)}
                                        placeholder="مثال: ARC-2026-0001"
                                        className="rounded-2xl h-11 bg-background border-border"
                                        required
                                    />
                                </div>

                                <div className="flex flex-col gap-2">
                                    <Label className="text-xs font-semibold text-muted-foreground">نوع نموذج البيانات</Label>
                                    <select
                                        value={selectedTemplateId}
                                        onChange={(e) => onSelectedTemplateIdChange(e.target.value)}
                                        className="flex h-11 w-full rounded-2xl border border-border bg-background px-3 py-2 text-sm ring-offset-background file:border-0 file:bg-transparent file:text-sm file:font-medium placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50 font-medium"
                                    >
                                        <option value="">نموذج عام (بدون حقول إضافية)</option>
                                        {dynamicTemplates.map(t => (
                                            <option key={t.id} value={t.id}>{t.templateFormName}</option>
                                        ))}
                                    </select>
                                </div>
                            </div>

                            {/* Dynamic Form Input Fields */}
                            {selectedTemplateId && Object.keys(templateInputs).length > 0 && (
                                <div className="bg-muted/30 border border-border p-4 rounded-2xl flex flex-col gap-3">
                                    <span className="text-xs font-bold text-muted-foreground border-b pb-2 mb-1 border-border">بيانات حقول النموذج:</span>
                                    <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                                        {Object.keys(templateInputs).map((label) => (
                                            <div key={label} className="flex flex-col gap-1">
                                                <Label className="text-xs font-semibold text-muted-foreground">{label}</Label>
                                                <Input
                                                    value={templateInputs[label] || ''}
                                                    onChange={(e) => onTemplateInputChange(label, e.target.value)}
                                                    placeholder={`أدخل ${label}...`}
                                                    className="rounded-lg h-9 bg-background"
                                                />
                                            </div>
                                        ))}
                                    </div>
                                </div>
                            )}

                            {/* QR Cover Page Generation Toggle (only in create mode) */}
                            {mode === 'create' && (
                                <div className="flex items-center justify-between bg-muted/40 p-4 rounded-2xl border border-border">
                                    <div className="flex flex-col gap-0.5 text-right">
                                        <span className="text-xs font-bold text-foreground">توليد صفحة غلاف الـ QR</span>
                                        <span className="text-[10px] text-muted-foreground">سيتم تلقائياً تصميم وتوليد صفحة غلاف تحتوي على باركود للوصول الفوري للملف</span>
                                    </div>
                                    <Switch
                                        checked={generateQrCover}
                                        onCheckedChange={onGenerateQrCoverChange}
                                    />
                                </div>
                            )}

                            {/* Scanner / OCR Quick Action */}
                            <div className="flex justify-between items-center border border-dashed border-primary/20 bg-primary/5 p-4 rounded-2xl">
                                <div className="text-right">
                                    <span className="text-xs font-bold text-foreground block">سحب من الماسح الضوئي</span>
                                    <span className="text-[10px] text-muted-foreground">يمكنك استيراد الملفات مباشرة من الماسح الضوئي مع استخراج النصوص الذكي OCR</span>
                                </div>
                                <Button
                                    type="button"
                                    onClick={() => setShowScannerModal(true)}
                                    variant="outline"
                                    className="rounded-xl px-4 py-2 border-primary/30 text-primary hover:bg-primary/10 flex items-center gap-1.5 font-bold"
                                >
                                    <ScanLine className="h-4 w-4" />
                                    <span>البدء بالمسح والـ OCR</span>
                                </Button>
                            </div>

                            {/* Existing Files for Edit */}
                            {mode === 'edit' && existingFiles.length > 0 && (
                                <div className="flex flex-col gap-2">
                                    <Label className="text-xs font-semibold text-muted-foreground">الملفات الموجودة مسبقاً (انقر على الحذف لإزالتها):</Label>
                                    <div className="flex flex-col gap-1.5 max-h-[150px] overflow-y-auto border border-border rounded-2xl p-3 bg-muted/15">
                                        {existingFiles.map(f => {
                                            const isRemoved = fileIdsToRemove.includes(f.id);
                                            return (
                                                <div key={f.id} className="flex items-center justify-between text-xs p-1.5 rounded-xl hover:bg-muted/40 transition-colors">
                                                    <span className={`truncate flex-1 text-right ${isRemoved ? 'line-through text-destructive font-bold' : 'font-semibold text-foreground'}`}>
                                                        {f.fileName}
                                                    </span>
                                                    <button
                                                        type="button"
                                                        className={`p-1.5 rounded-lg transition-colors ${isRemoved ? 'text-primary hover:bg-primary/10' : 'text-destructive hover:bg-destructive/10'
                                                            }`}
                                                        onClick={() => onToggleRemoveExistingFile(f.id)}
                                                    >
                                                        {isRemoved ? <Plus className="h-4 w-4" /> : <Trash2 className="h-4 w-4" />}
                                                    </button>
                                                </div>
                                            );
                                        })}
                                    </div>
                                </div>
                            )}

                            {/* File Upload Section */}
                            <div className="flex flex-col gap-2">
                                <Label className="text-xs font-semibold text-muted-foreground">إرفاق الملفات</Label>
                                <div className="border-2 border-dashed border-border hover:border-primary/50 transition-all rounded-3xl p-6 flex flex-col items-center justify-center gap-2 cursor-pointer bg-muted/10 hover:bg-muted/30 relative">
                                    <input
                                        type="file"
                                        multiple
                                        className="absolute inset-0 opacity-0 cursor-pointer"
                                        onChange={(e) => {
                                            const files = Array.from(e.target.files || []);
                                            onAddSelectedFiles(files);
                                        }}
                                    />
                                    <Upload className="h-8 w-8 text-muted-foreground" />
                                    <span className="text-xs font-bold text-foreground">اسحب وأفلت الملفات هنا أو انقر للتصفح</span>
                                    <span className="text-[10px] text-muted-foreground">صيغ الملفات المدعومة: PDF, JPG, PNG, DOCX, XLSX ...</span>
                                </div>

                                {selectedFiles.length > 0 && (
                                    <div className="flex flex-col gap-1.5 max-h-[150px] overflow-y-auto mt-2">
                                        {selectedFiles.map((file, index) => (
                                            <div key={index} className="flex items-center justify-between text-xs p-2 rounded-2xl bg-primary/5 border border-primary/10">
                                                <span className="truncate font-semibold text-right flex-1">{file.name}</span>
                                                <button
                                                    type="button"
                                                    className="text-muted-foreground hover:text-destructive p-1 rounded-lg hover:bg-destructive/10 transition-colors"
                                                    onClick={() => onRemoveSelectedFile(index)}
                                                >
                                                    <Trash2 className="h-4 w-4" />
                                                </button>
                                            </div>
                                        ))}
                                    </div>
                                )}
                            </div>
                        </div>

                        {/* Save Actions */}
                        <div className="border-t border-border pt-4 flex flex-col gap-3 flex-shrink-0">
                            {isSaving && (
                                <div className="flex flex-col gap-1.5 px-1 animate-in fade-in slide-in-from-bottom-1 duration-200">
                                    <div className="flex justify-between text-[10px] font-bold text-primary">
                                        <span>جاري رفع البيانات والملفات...</span>
                                        <span>{uploadProgress}%</span>
                                    </div>
                                    <Progress value={uploadProgress} className="h-1.5" />
                                </div>
                            )}
                            <div className="flex justify-end gap-3">
                                <Button
                                    type="button"
                                    variant="ghost"
                                    onClick={onClose}
                                    className="rounded-xl px-5"
                                    disabled={isSaving}
                                >
                                    إلغاء
                                </Button>
                                <Button
                                    type="submit"
                                    className="rounded-xl px-8 font-bold shadow-lg shadow-primary/20 flex items-center gap-2"
                                    disabled={isSaving || !archivalNumber.trim()}
                                >
                                    <Upload className="h-4 w-4" />
                                    <span>{isSaving ? 'جاري الحفظ والأرشفة...' : 'حفظ المستند'}</span>
                                </Button>
                            </div>
                        </div>
                    </form>
                </div>
            </div>

            {/* Scanner & OCR Modal */}
            <ScannerModal
                isOpen={showScannerModal}
                onClose={() => {
                    setShowScannerModal(false);
                    setScannerFiles([]);
                }}
                imageFiles={scannerFiles}
                setImageFiles={setScannerFiles}
                onApply={handleApplyScanner}
            />
        </>
    );
}
