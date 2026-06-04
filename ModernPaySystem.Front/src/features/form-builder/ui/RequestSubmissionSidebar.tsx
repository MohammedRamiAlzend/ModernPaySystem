import { useRef, useState, useMemo } from 'react';
import { Shield, ImagePlus, FileText, X, Scan, Building2, Link2, Plus } from 'lucide-react';
// import { Shield, ImagePlus, FileText, X, Scan, Building2, Link2, Plus, Trash2 } from 'lucide-react';
import { Button } from '@/shared/ui/button';
import { SidebarSection } from '@/shared/ui/sidebar-section';
import { cn } from '@/shared/lib/utils';
import { ScannerModal } from '@/features/document-scanner';
import type { ImageMeta } from '@/features/document-scanner';
import { useDepartmentTree } from '@/features/department-management';
import { SearchableSelect } from '@/shared/ui/searchable-select';
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
import { RequestPicker } from './RequestPicker';
import { useRequestsPaged } from '@/features/form-builder/api/formEndpoints';
import { CreateRequestRelatedRequestDto, RequestRelationType, TemplateRequest } from '@/entities/form/model/types';

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
        default: return '';
    }
};


interface RequestSubmissionSidebarProps {
    departmentId: string;
    onDepartmentSelect: (id: string) => void;
    readOnlyUsers: string[];
    onReadOnlyUsersChange: (ids: string[]) => void;
    files: File[];
    onFilesChange: (files: File[]) => void;
    showFiles?: boolean;
    className?: string;
    departmentLabel?: string;
    relatedRequests?: CreateRequestRelatedRequestDto[];
    onRelatedRequestsChange?: (relations: CreateRequestRelatedRequestDto[]) => void;
    showRelations?: boolean;
}


export const RequestSubmissionSidebar = ({
    departmentId,
    onDepartmentSelect,
    files,
    onFilesChange,
    showFiles = true,
    className,
    departmentLabel = "القسم المستلم",
    relatedRequests = [],
    onRelatedRequestsChange,
    showRelations = true,
}: RequestSubmissionSidebarProps) => {

    const fileInputRef = useRef<HTMLInputElement>(null);
    const [isScannerOpen, setIsScannerOpen] = useState(false);
    const [scannedFiles, setScannedFiles] = useState<ImageMeta[]>([]);

    const [isRelationModalOpen, setIsRelationModalOpen] = useState(false);
    const [tempTargetRequestIds, setTempTargetRequestIds] = useState<string[]>([]);
    const [tempRelationType, setTempRelationType] = useState<RequestRelationType>(RequestRelationType.Reference);
    const [tempNotes, setTempNotes] = useState('');

    // Setup relation state and fetch requests
    const { data: requestsPaged } = useRequestsPaged({ page: 1, pageSize: 100 });
    const requestsItems = useMemo(() => requestsPaged?.items || [], [requestsPaged]);

    const handleAddRelation = () => {
        if (tempTargetRequestIds.length === 0) return;

        const newRelations: CreateRequestRelatedRequestDto[] = [];
        tempTargetRequestIds.forEach(id => {
            if (relatedRequests.some(r => r.targetRequestId === id)) return;
            newRelations.push({
                targetRequestId: id,
                relationType: tempRelationType,
                notes: tempNotes || undefined
            });
        });

        if (onRelatedRequestsChange) {
            onRelatedRequestsChange([...relatedRequests, ...newRelations]);
        }

        // Reset
        setTempTargetRequestIds([]);
        setTempRelationType(RequestRelationType.Reference);
        setTempNotes('');
        setIsRelationModalOpen(false);
    };

    // const handleRemoveRelation = (targetId: string) => {
    //     if (onRelatedRequestsChange) {
    //         onRelatedRequestsChange(relatedRequests.filter(r => r.targetRequestId !== targetId));
    //     }
    // };

    const { data: departmentTree = [], isLoading: isDeptsLoading } = useDepartmentTree();

    const departmentOptions = useMemo(() => {
        const options: any[] = [];
        const flatten = (nodes: any[]) => {
            nodes.forEach(node => {
                options.push({
                    value: node.id,
                    label: node.name,
                    icon: <Building2 className="w-3.5 h-3.5 text-primary/60" />
                });
                if (node.children) flatten(node.children);
            });
        };
        flatten(departmentTree);
        return options;
    }, [departmentTree]);


    const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        if (e.target.files) {
            const newFiles = Array.from(e.target.files);
            onFilesChange([...files, ...newFiles]);
        }
    };

    const handleScannerApply = (_ocrText: string, images: ImageMeta[]) => {
        const newFiles = images.map(img => img.file);
        onFilesChange([...files, ...newFiles]);
        setIsScannerOpen(false);
        setScannedFiles([]); // Clear local temporary files after applying
    };

    const removeFile = (index: number) => {
        onFilesChange(files.filter((_, i) => i !== index));
    };

    return (
        <div className={cn("space-y-6 sticky top-8", className)}>
            {/* Department Selection (single) */}
            <SidebarSection title={departmentLabel} icon={Shield}>
                <SearchableSelect
                    options={departmentOptions}
                    value={departmentId}
                    onValueChange={onDepartmentSelect}
                    placeholder="اختر القسم..."
                    isLoading={isDeptsLoading}
                />
            </SidebarSection>


            {/* ReadOnly (CC) Users Selection (multi) */}
            {/* تم تعليق حاليا حتى يتم اكمال المنطق البرمجي في باك ايند */}
            {/* <SidebarSection title="للاطلاع فقط" icon={Eye}>
                <UserPicker
                    multiple
                    selectedUserIds={readOnlyUsers}
                    onUsersChange={onReadOnlyUsersChange}
                    className="!grid-cols-1"
                    label="المراقبين (CC)"
                    placeholder="اختر للاطلاع..."
                    showCurrentUser={false}
                />
            </SidebarSection> */}

            {/* File Upload & Scanner */}
            {showFiles && (
                <SidebarSection title="المرفقات" icon={ImagePlus}>
                    <div className="space-y-3">
                        <div className="flex gap-2">
                            <Button
                                variant="outline"
                                className="flex-1 h-12 gap-2 border-dashed border-primary/20 hover:border-primary/50 hover:bg-primary/5 hover:text-primary transition-all rounded-xl"
                                onClick={() => fileInputRef.current?.click()}
                                title="إرفاق ملفات من الكمبيوتر"
                            >
                                <ImagePlus className="w-5 h-5" />
                                <span className="text-xs">إرفاق ({files.length})</span>
                            </Button>

                            <Button
                                variant="outline"
                                className="flex-1 h-12 gap-2 border-dashed border-sky-200 hover:border-sky-500 hover:bg-sky-50 hover:text-sky-600 transition-all rounded-xl"
                                onClick={() => setIsScannerOpen(true)}
                                title="مسح ضوئي من الماسح (Scanner)"
                            >
                                <Scan className="w-5 h-5" />
                                <span className="text-xs">مسح ضوئي</span>
                            </Button>
                        </div>

                        <input
                            type="file"
                            multiple
                            accept="image/*,.pdf,.doc,.docx"
                            className="hidden"
                            ref={fileInputRef}
                            onChange={handleFileChange}
                        />

                        {files.length > 0 && (
                            <div className="space-y-2 mt-4 max-h-[150px] overflow-y-auto pr-1 custom-scrollbar">
                                {files.map((file, idx) => (
                                    <div key={idx} className="flex items-center justify-between p-2 rounded-lg bg-muted/50 border text-xs group/item hover:bg-muted/80 transition-colors">
                                        <div className="flex items-center gap-2 truncate">
                                            {file.type.startsWith('image/') ? <ImagePlus className="w-3 h-3 text-primary/70" /> : <FileText className="w-3 h-3 text-primary/70" />}
                                            <span className="truncate max-w-[140px]">{file.name}</span>
                                        </div>
                                        <button
                                            onClick={(e) => { e.stopPropagation(); removeFile(idx); }}
                                            className="text-muted-foreground hover:text-destructive transition-colors p-1"
                                            title="حذف الملف"
                                        >
                                            <X className="w-3.5 h-3.5" />
                                        </button>
                                    </div>
                                ))}
                            </div>
                        )}
                    </div>
                </SidebarSection>
            )}

            {/* Request Relations */}
            {showRelations && onRelatedRequestsChange && (
                <SidebarSection title="الارتباط بطلبات أخرى" icon={Link2}>
                    <div className="space-y-3">
                        <Button
                            variant="outline"
                            className="w-full h-12 gap-2 border-dashed border-primary/20 hover:border-primary/50 hover:bg-primary/5 hover:text-primary transition-all rounded-xl"
                            onClick={() => setIsRelationModalOpen(true)}
                            title="إضافة ارتباط بطلب آخر"
                        >
                            <Plus className="w-5 h-5" />
                            <span className="text-xs">إضافة ارتباط ({relatedRequests.length})</span>
                        </Button>

                        {relatedRequests.length > 0 && (
                            <div className="space-y-2 mt-4 max-h-[200px] overflow-y-auto pr-1 custom-scrollbar">
                                {relatedRequests.map((rel, idx) => {
                                    const matchedReq = (requestsItems as TemplateRequest[]).find((r: TemplateRequest) => r.id === rel.targetRequestId);
                                    const reqLabel = matchedReq
                                        ? `طلب #${matchedReq.requestNumber} - ${matchedReq.template?.templateName || ''}`
                                        : 'طلب غير معروف';

                                    return (
                                        <div key={idx} className="flex flex-col gap-1 p-2.5 rounded-lg bg-muted/50 border border-primary/10 text-xs group/item hover:bg-muted/80 transition-colors" dir="rtl">
                                            <div className="flex items-center justify-between">
                                                <span className="font-bold truncate max-w-[170px]" title={reqLabel}>
                                                    {reqLabel}
                                                </span>
                                                {/* <button
                                                    onClick={(e) => { e.stopPropagation(); handleRemoveRelation(rel.targetRequestId); }}
                                                    className="text-muted-foreground hover:text-destructive transition-colors p-1"
                                                    title="حذف الارتباط"
                                                >
                                                    <Trash2 className="w-3.5 h-3.5" />
                                                </button> */}
                                            </div>
                                            <div className="flex items-center justify-between text-[10px] text-muted-foreground mt-1">
                                                <span className="bg-primary/10 text-primary px-1.5 py-0.5 rounded">
                                                    {getRelationTypeLabel(rel.relationType)}
                                                </span>
                                                {rel.notes && (
                                                    <span className="truncate max-w-[100px]" title={rel.notes}>
                                                        {rel.notes}
                                                    </span>
                                                )}
                                            </div>
                                        </div>
                                    );
                                })}
                            </div>
                        )}
                    </div>
                </SidebarSection>
            )}

            {/* Document Scanner Modal */}
            <ScannerModal
                isOpen={isScannerOpen}
                onClose={() => setIsScannerOpen(false)}
                onApply={handleScannerApply}
                imageFiles={scannedFiles}
                setImageFiles={setScannedFiles}
                acceptAllFiles={true}
            />

            {/* Add Relation Modal */}
            <BaseModal
                isOpen={isRelationModalOpen}
                onClose={() => setIsRelationModalOpen(false)}
                title="إضافة ارتباط بطلب آخر"
                maxWidth="xl"
                maxHeight="xl"
                footer={
                    <div className="flex justify-end gap-3 w-full" dir="rtl">
                        <Button
                            variant="outline"
                            onClick={() => setIsRelationModalOpen(false)}
                            className="rounded-xl"
                        >
                            إلغاء
                        </Button>
                        <Button
                            onClick={handleAddRelation}
                            disabled={tempTargetRequestIds.length === 0}
                            className="rounded-xl px-6"
                        >
                            إضافة
                        </Button>
                    </div>
                }
            >
                <div className="space-y-4 text-right" dir="rtl">
                    <div className="space-y-2">
                        <Label className="text-xs font-bold text-muted-foreground">الطلب المراد الارتباط به</Label>
                        <RequestPicker
                            multiple
                            values={tempTargetRequestIds}
                            onValuesChange={setTempTargetRequestIds}
                            placeholder="اختر الطلب..."
                        />
                    </div>

                    <div className="space-y-2">
                        <Label className="text-xs font-bold text-muted-foreground">نوع الارتباط</Label>
                        <Select
                            value={String(tempRelationType)}
                            onValueChange={(val) => setTempRelationType(Number(val) as RequestRelationType)}
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
                            value={tempNotes}
                            onChange={(e) => setTempNotes(e.target.value)}
                            placeholder="اكتب ملاحظة حول سبب الارتباط..."
                            className="h-10 rounded-xl bg-background/50 border-primary/10"
                        />
                    </div>
                </div>
            </BaseModal>
        </div>
    );
};
