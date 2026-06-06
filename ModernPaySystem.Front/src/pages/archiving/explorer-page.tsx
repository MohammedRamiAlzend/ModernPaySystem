import { useState, useEffect, useRef } from 'react';
import { Folder, ArchiveRecord } from '@/features/archiving/model/types';
import { archivingService } from '@/features/archiving/api/archivingService';
import { useArchivingFolders } from '@/features/archiving/hooks/useArchivingFolders';
import { useArchivingRecords } from '@/features/archiving/hooks/useArchivingRecords';
import { FolderModal } from '@/features/archiving/ui/FolderModal';
import { RecordModal } from '@/features/archiving/ui/RecordModal';
import { SubmitEditRequestModal } from '@/features/archive-edit-requests/ui/SubmitEditRequestModal';
import { ExplorerToolbar } from '@/features/archiving/ui/ExplorerToolbar';
import { ExplorerView } from '@/features/archiving/ui/ExplorerView';
import { ListView } from '@/features/archiving/ui/ListView';
import { DocumentGallery } from '@/features/archiving/ui/DocumentGallery';
import { QRPreviewTemplate } from '@/features/archiving/ui/QRPreviewTemplate';
import { Button } from '@/shared/ui/button';
import { Progress } from '@/shared/ui/progress';
import {
    Plus,
    FolderPlus,
    Loader2,
    ChevronLeft,
    FileText
} from 'lucide-react';

export default function ExplorerPage() {
    const [searchTerm, setSearchTerm] = useState('');
    const [viewMode, setViewMode] = useState<'explorer' | 'list'>('explorer');
    const [previewingRecord, setPreviewingRecord] = useState<ArchiveRecord | null>(null);

    const qrCoverRef = useRef<HTMLDivElement>(null);

    const [showSubmitEditModal, setShowSubmitEditModal] = useState(false);
    const [editRequestRecord, setEditRequestRecord] = useState<ArchiveRecord | null>(null);

    const handleOpenRequestEdit = (record: ArchiveRecord) => {
        setEditRequestRecord(record);
        setShowSubmitEditModal(true);
    };

    const {
        folders,
        currentFolder,
        loadingFolders,
        loadFolders,
        breadcrumbs,
        showFolderModal,
        setShowFolderModal,
        folderModalMode,
        folderName,
        setFolderName,
        isSavingFolder,
        handleOpenCreateFolder,
        handleOpenEditFolder,
        handleSaveFolder,
        handleDeleteFolder,
        navigateToFolder
    } = useArchivingFolders();

    const {
        records,
        dynamicTemplates,
        loadingRecords,
        recordsPage,
        hasMoreRecords,
        loadRecords,
        loadMoreRecords,
        showRecordModal,
        setShowRecordModal,
        recordModalMode,
        archivalNumber,
        setArchivalNumber,
        selectedTemplateId,
        handleTemplateIdChange,
        templateInputs,
        setTemplateInputs,
        selectedFiles,
        setSelectedFiles,
        existingFiles,
        fileIdsToRemove,
        setFileIdsToRemove,
        isSavingRecord,
        uploadProgress,
        downloadingZipId,
        downloadProgress,
        generateQrCover,
        setGenerateQrCover,
        qrCoverGuid,
        handleTemplateInputChange,
        handleOpenCreateRecord,
        handleOpenEditRecord,
        handleSaveRecord,
        handleDeleteRecord,
        handleDownloadRecordZip
    } = useArchivingRecords(currentFolder?.id);

    // Initial folder load
    useEffect(() => {
        loadFolders();
    }, [loadFolders]);

    // Esc key bindings to close previews and modals
    useEffect(() => {
        const handleKeyDown = (e: KeyboardEvent) => {
            if (e.key === 'Escape') {
                setPreviewingRecord(null);
                setShowRecordModal(false);
                setShowFolderModal(false);
            }
        };
        window.addEventListener('keydown', handleKeyDown);
        return () => window.removeEventListener('keydown', handleKeyDown);
    }, [setShowRecordModal, setShowFolderModal]);

    const handleNavigate = (folder: Folder | null) => {
        navigateToFolder(folder);
        setSearchTerm('');
    };

    // Filter folder and records based on search term
    const filteredFolders = folders.filter(f =>
        f.parentId === (currentFolder ? currentFolder.id : null) &&
        (searchTerm.trim() === '' || f.name.toLowerCase().includes(searchTerm.toLowerCase()))
    );

    const filteredRecords = currentFolder
        ? records.filter(r =>
            searchTerm.trim() === '' || r.archivalNumber.toLowerCase().includes(searchTerm.toLowerCase())
        )
        : [];

    return (
        <div className="flex flex-col gap-6 p-6" dir="rtl">
            {/* Top Header */}
            <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
                <div className="flex flex-col gap-1 text-right">
                    <h1 className="text-xl font-bold text-foreground">مستكشف ونظام الأرشفة</h1>
                    <p className="text-xs text-muted-foreground font-medium">إدارة وتصنيف المجلدات والمستندات المؤرشفة والملفات المرفقة بها</p>
                </div>

                <div className="flex items-center gap-3">
                    {currentFolder && (
                        <Button
                            onClick={handleOpenCreateRecord}
                            className="rounded-xl px-5 font-bold shadow-lg shadow-primary/20 flex items-center gap-2"
                        >
                            <Plus className="h-4 w-4" />
                            <span>أرشفة مستند</span>
                        </Button>
                    )}

                    <Button
                        onClick={handleOpenCreateFolder}
                        variant="outline"
                        className="rounded-xl px-5 border-border text-foreground hover:bg-muted font-bold flex items-center gap-2"
                    >
                        <FolderPlus className="h-4 w-4 text-amber-500" />
                        <span>مجلد جديد</span>
                    </Button>

                    {currentFolder && (
                        <Button
                            variant="ghost"
                            size="icon"
                            onClick={() => handleNavigate(currentFolder.parentId ? folders.find(f => f.id === currentFolder.parentId) || null : null)}
                            className="rounded-xl border border-border h-10 w-10 text-muted-foreground hover:text-foreground"
                            title="المجلد الأعلى"
                        >
                            <ChevronLeft className="h-5 w-5 transform rotate-180" />
                        </Button>
                    )}
                </div>
            </div>

            {/* Toolbar, Search & Breadcrumbs */}
            <ExplorerToolbar
                searchTerm={searchTerm}
                onSearchTermChange={setSearchTerm}
                viewMode={viewMode}
                onViewModeChange={setViewMode}
                breadcrumbs={breadcrumbs}
                onNavigateToFolder={handleNavigate}
            />

            {/* Main Content Area */}
            <div className="bg-card border border-border rounded-3xl p-6 shadow-sm min-h-[400px]">
                {loadingFolders || (loadingRecords && recordsPage === 1) ? (
                    <div className="flex flex-col items-center justify-center py-24 gap-3 text-muted-foreground">
                        <Loader2 className="h-8 w-8 animate-spin text-primary" />
                        <span className="text-sm font-medium">جاري تحميل مستندات الأرشيف...</span>
                    </div>
                ) : viewMode === 'explorer' ? (
                    <ExplorerView
                        folders={filteredFolders}
                        records={filteredRecords}
                        onFolderDoubleClick={handleNavigate}
                        onRecordClick={setPreviewingRecord}
                        onFolderEdit={handleOpenEditFolder}
                        onFolderDelete={handleDeleteFolder}
                        onRecordEdit={handleOpenEditRecord}
                        onRecordDelete={handleDeleteRecord}
                        onRecordDownloadZip={handleDownloadRecordZip}
                        onRecordRequestEdit={handleOpenRequestEdit}
                    />
                ) : (
                    <ListView
                        folders={filteredFolders}
                        records={filteredRecords}
                        onFolderClick={handleNavigate}
                        onFolderEdit={handleOpenEditFolder}
                        onFolderDelete={handleDeleteFolder}
                        onView={setPreviewingRecord}
                        onEdit={handleOpenEditRecord}
                        onDelete={handleDeleteRecord}
                        onDownloadZip={handleDownloadRecordZip}
                        onRecordRequestEdit={handleOpenRequestEdit}
                        isLoading={loadingRecords}
                        hasMore={hasMoreRecords}
                        onLoadMore={loadMoreRecords}
                    />
                )}
            </div>

            {/* 1. Modal: Folder Create / Edit */}
            <FolderModal
                isOpen={showFolderModal}
                mode={folderModalMode}
                folderName={folderName}
                onFolderNameChange={setFolderName}
                onClose={() => setShowFolderModal(false)}
                onSubmit={handleSaveFolder}
                isSaving={isSavingFolder}
            />

            {/* 2. Modal: Record Create / Edit */}
            <RecordModal
                isOpen={showRecordModal}
                mode={recordModalMode}
                archivalNumber={archivalNumber}
                onArchivalNumberChange={setArchivalNumber}
                selectedTemplateId={selectedTemplateId}
                onSelectedTemplateIdChange={handleTemplateIdChange}
                dynamicTemplates={dynamicTemplates}
                templateInputs={templateInputs}
                onTemplateInputChange={handleTemplateInputChange}
                setTemplateInputs={setTemplateInputs}
                generateQrCover={generateQrCover}
                onGenerateQrCoverChange={setGenerateQrCover}
                existingFiles={existingFiles}
                fileIdsToRemove={fileIdsToRemove}
                onToggleRemoveExistingFile={(id) => {
                    if (fileIdsToRemove.includes(id)) {
                        setFileIdsToRemove(prev => prev.filter(x => x !== id));
                    } else {
                        setFileIdsToRemove(prev => [...prev, id]);
                    }
                }}
                onSubmit={(e) => handleSaveRecord(e, qrCoverRef)}
                onClose={() => setShowRecordModal(false)}
                selectedFiles={selectedFiles}
                onAddSelectedFiles={(files) => setSelectedFiles(prev => [...prev, ...files])}
                onRemoveSelectedFile={(index) => setSelectedFiles(prev => prev.filter((_, i) => i !== index))}
                isSaving={isSavingRecord}
                uploadProgress={uploadProgress}
            />

            {/* 3. Modal: Document Gallery Preview */}
            {previewingRecord && (
                <div className="fixed inset-0 bg-black/60 backdrop-blur-sm flex items-center justify-center p-4 z-50 animate-fade-in animate-duration-300">
                    <div className="bg-card border border-border rounded-3xl w-full max-w-7xl h-[90vh] shadow-2xl flex flex-col overflow-hidden text-right">
                        <div className="p-6 border-b border-border flex items-center justify-between">
                            <button
                                onClick={() => setPreviewingRecord(null)}
                                className="text-muted-foreground hover:text-foreground font-bold p-1 rounded-lg hover:bg-muted transition-all"
                            >
                                إغلاق المعاينة
                            </button>
                            <h2 className="text-base font-bold text-foreground flex items-center gap-2">
                                <FileText className="h-5 w-5 text-primary" />
                                <span>تفاصيل المستند ورقم الأرشفة: {previewingRecord.archivalNumber}</span>
                            </h2>
                        </div>
                        <div className="flex-1 overflow-hidden p-6">
                            <DocumentGallery
                                recordId={previewingRecord.id}
                                files={previewingRecord.physicalFiles || []}
                                record={previewingRecord}
                                formName={dynamicTemplates.find(t => t.id === previewingRecord.formId)?.templateFormName}
                                onFilesChanged={async () => {
                                    if (currentFolder) {
                                        // Update record lists to refresh counters and sizes
                                        await loadRecords(currentFolder.id, recordsPage);
                                        // Refresh the open record to display updates immediately in the gallery view
                                        try {
                                            const updated = await archivingService.getArchiveRecordById(previewingRecord.id);
                                            setPreviewingRecord(updated);
                                        } catch (e) {
                                            console.error(e);
                                        }
                                    }
                                }}
                            />
                        </div>
                    </div>
                </div>
            )}

            {/* Off-screen QR Preview Template for canvas generation */}
            <div style={{ position: 'absolute', left: '-9999px', top: '-9999px' }}>
                <QRPreviewTemplate
                    ref={qrCoverRef}
                    guid={qrCoverGuid}
                    archivalNumber={archivalNumber}
                    formName={dynamicTemplates.find(t => t.id === selectedTemplateId)?.templateFormName}
                    content={Object.keys(templateInputs).map(k => ({ key: k, value: templateInputs[k] }))}
                />
            </div>

            {/* Global Progress Overlay for ZIP Downloads */}
            {downloadingZipId && (
                <div className="fixed inset-0 bg-black/40 backdrop-blur-[2px] flex items-center justify-center z-[100] animate-in fade-in duration-300">
                    <div className="bg-card border border-border rounded-3xl p-8 max-w-sm w-full shadow-2xl flex flex-col items-center gap-6 text-center">
                        <div className="w-16 h-16 rounded-2xl bg-primary/10 text-primary flex items-center justify-center">
                            <Loader2 className="h-8 w-8 animate-spin" />
                        </div>
                        <div className="flex flex-col gap-2">
                            <h3 className="text-base font-bold text-foreground">جاري تحميل الملفات</h3>
                            <p className="text-xs text-muted-foreground font-medium">يتم الآن تجميع وضغط الملفات وتنزيلها كملف ZIP واحد...</p>
                        </div>
                        <div className="w-full flex flex-col gap-2">
                            <div className="flex justify-between text-xs font-bold text-primary">
                                <span>التقدم:</span>
                                <span>{downloadProgress}%</span>
                            </div>
                            <Progress value={downloadProgress} className="h-2" />
                        </div>
                    </div>
                </div>
            )}
            {/* 4. Modal: Submit Archive Edit Request */}
            <SubmitEditRequestModal
                isOpen={showSubmitEditModal}
                record={editRequestRecord}
                onClose={() => {
                    setShowSubmitEditModal(false);
                    setEditRequestRecord(null);
                }}
            />
        </div>
    );
}

