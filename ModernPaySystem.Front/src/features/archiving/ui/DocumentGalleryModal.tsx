import { useState } from 'react';
import { DocumentGallery } from './DocumentGallery';
import { archivingService } from '../api/archivingService';
import type { ArchiveRecord, DynamicFormTemplate } from '../model/types';
import { FileText } from 'lucide-react';

interface DocumentGalleryModalProps {
    record: ArchiveRecord | null;
    dynamicTemplates: DynamicFormTemplate[];
    onClose: () => void;
    onFilesChanged?: () => Promise<void>;
}

export function DocumentGalleryModal({ record, dynamicTemplates, onClose, onFilesChanged }: DocumentGalleryModalProps) {
    const [previewingRecord, setPreviewingRecord] = useState<ArchiveRecord | null>(record);

    if (!record) return null;

    const formName = dynamicTemplates.find(t => t.id === record.formId)?.templateFormName;

    const handleFilesChanged = async () => {
        try {
            const updated = await archivingService.getArchiveRecordById(record.id);
            setPreviewingRecord(updated);
            await onFilesChanged?.();
        } catch (e) {
            console.error(e);
        }
    };

    return (
        <div className="fixed inset-0 bg-black/60 backdrop-blur-sm flex items-center justify-center p-4 z-50 animate-fade-in animate-duration-300">
            <div className="bg-card border border-border rounded-3xl w-full max-w-7xl h-[90vh] shadow-2xl flex flex-col overflow-hidden text-right">
                <div className="p-6 border-b border-border flex items-center justify-between">
                    <button
                        onClick={onClose}
                        className="text-muted-foreground hover:text-foreground font-bold p-1 rounded-lg hover:bg-muted transition-all"
                    >
                        إغلاق المعاينة
                    </button>
                    <h2 className="text-base font-bold text-foreground flex items-center gap-2">
                        <FileText className="h-5 w-5 text-primary" />
                        <span>تفاصيل المستند: {record.name || record.id.slice(0, 8)}</span>
                    </h2>
                </div>
                <div className="flex-1 overflow-hidden p-6">
                    <DocumentGallery
                        recordId={record.id}
                        files={(previewingRecord?.physicalFiles || record.physicalFiles) || []}
                        record={previewingRecord || record}
                        formName={formName}
                        onFilesChanged={handleFilesChanged}
                    />
                </div>
            </div>
        </div>
    );
}

export default DocumentGalleryModal;
