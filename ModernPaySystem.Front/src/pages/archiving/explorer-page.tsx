import React, { useState, useEffect, useRef } from 'react';
import { Folder, ArchiveRecord, DynamicFormTemplate, PhysicalFile } from '@/features/archiving/model/types';
import { archivingService } from '@/features/archiving/api/archivingService';
import { ExplorerView } from '@/features/archiving/ui/ExplorerView';
import { ListView } from '@/features/archiving/ui/ListView';
import { DocumentGallery } from '@/features/archiving/ui/DocumentGallery';
import { QRPreviewTemplate } from '@/features/archiving/ui/QRPreviewTemplate';
import { useUIStore } from '@/app/store/uiStore';
import { Button } from '@/shared/ui/button';
import { Input } from '@/shared/ui/input';
import { Label } from '@/shared/ui/label';
import { Switch } from '@/shared/ui/switch';
import {
    Plus,
    FolderPlus,
    LayoutGrid,
    List,
    Loader2,
    Search,
    FileText,
    Upload,
    ScanLine,
    ChevronLeft,
    Trash2,
    X
} from 'lucide-react';
import * as htmlToImage from 'html-to-image';
import { ScannerModal } from '@/features/document-scanner';
import { ImageMeta } from '@/features/document-scanner';

export default function ExplorerPage() {
    const { showStatus, showConfirm } = useUIStore();

    // ---------------------------------------------------------
    // States
    // ---------------------------------------------------------
    const [folders, setFolders] = useState<Folder[]>([]);
    const [records, setRecords] = useState<ArchiveRecord[]>([]);
    const [dynamicTemplates, setDynamicTemplates] = useState<DynamicFormTemplate[]>([]);
    const [currentFolder, setCurrentFolder] = useState<Folder | null>(null);
    const [breadcrumbs, setBreadcrumbs] = useState<Folder[]>([]);

    const [viewMode, setViewMode] = useState<'explorer' | 'list'>('explorer');
    const [searchTerm, setSearchTerm] = useState('');

    // Folder Modal States
    const [showFolderModal, setShowFolderModal] = useState(false);
    const [folderModalMode, setFolderModalMode] = useState<'create' | 'edit'>('create');
    const [folderName, setFolderName] = useState('');
    const [selectedFolder, setSelectedFolder] = useState<Folder | null>(null);
    const [isSavingFolder, setIsSavingFolder] = useState(false);
    const [loadingFolders, setLoadingFolders] = useState(false);

    // Record Modal States
    const [showRecordModal, setShowRecordModal] = useState(false);
    const [recordModalMode, setRecordModalMode] = useState<'create' | 'edit'>('create');
    const [selectedRecord, setSelectedRecord] = useState<ArchiveRecord | null>(null);
    const [archivalNumber, setArchivalNumber] = useState('');
    const [selectedTemplateId, setSelectedTemplateId] = useState('');
    const [templateInputs, setTemplateInputs] = useState<Record<string, string>>({});
    const [selectedFiles, setSelectedFiles] = useState<File[]>([]);
    const [existingFiles, setExistingFiles] = useState<PhysicalFile[]>([]);
    const [fileIdsToRemove, setFileIdsToRemove] = useState<string[]>([]);
    const [isSavingRecord, setIsSavingRecord] = useState(false);
    const [generateQrCover, setGenerateQrCover] = useState(true);
    const [loadingRecords, setLoadingRecords] = useState(false);
    const [recordsPage, setRecordsPage] = useState(1);
    const [hasMoreRecords, setHasMoreRecords] = useState(false);

    // Document Gallery State
    const [previewingRecord, setPreviewingRecord] = useState<ArchiveRecord | null>(null);

    // Scanner / OCR States
    const [showScannerModal, setShowScannerModal] = useState(false);
    const [scannerFiles, setScannerFiles] = useState<ImageMeta[]>([]);

    // QR Cover Generation Refs
    const qrCoverRef = useRef<HTMLDivElement>(null);
    const [qrCoverGuid, setQrCoverGuid] = useState<string>('');

    // ---------------------------------------------------------
    // Initial Load & Navigation
    // ---------------------------------------------------------
    const flattenFolders = (nodes: Folder[]): Folder[] => {
        let result: Folder[] = [];
        for (const node of nodes) {
            result.push(node);
            if (node.folderDtos && node.folderDtos.length > 0) {
                result = result.concat(flattenFolders(node.folderDtos));
            }
        }
        return result;
    };

    const loadFolders = async () => {
        setLoadingFolders(true);
        try {
            const data = await archivingService.getAllFolders();
            setFolders(flattenFolders(data));
        } catch (error) {
            console.error('Failed to load folders', error);
            showStatus({
                type: 'error',
                title: 'خطأ في تحميل المجلدات',
                message: 'تعذر تحميل المجلدات من الخادم.'
            });
        } finally {
            setLoadingFolders(false);
        }
    };

    const loadDynamicTemplates = async () => {
        try {
            const templates = await archivingService.getAllDynamicForms();
            setDynamicTemplates(templates);
        } catch (error) {
            console.error('Failed to load templates', error);
        }
    };

    const loadRecords = async (folderId: string, page = 1) => {
        if (page === 1) {
            setLoadingRecords(true);
        }
        try {
            const res = await archivingService.getArchiveRecordsByFolder(folderId, page, 10);
            if (page === 1) {
                setRecords(res.items);
            } else {
                setRecords(prev => [...prev, ...res.items]);
            }
            setHasMoreRecords(res.items.length === 10);
            setRecordsPage(page);
        } catch (error) {
            console.error('Failed to load records', error);
            showStatus({
                type: 'error',
                title: 'خطأ في تحميل المستندات',
                message: 'تعذر تحميل المستندات المؤرشفة لهذا المجلد.'
            });
        } finally {
            setLoadingRecords(false);
        }
    };

    const loadMoreRecords = () => {
        if (currentFolder) {
            loadRecords(currentFolder.id, recordsPage + 1);
        }
    };

    // Load folders & templates on mount
    useEffect(() => {
        loadFolders();
        loadDynamicTemplates();
    }, []);

    // Load records when current folder changes
    useEffect(() => {
        if (currentFolder) {
            loadRecords(currentFolder.id, 1);
        } else {
            setRecords([]);
            setHasMoreRecords(false);
        }
    }, [currentFolder]);

    // Keep current folder in sync when folder tree updates
    useEffect(() => {
        if (currentFolder) {
            const fresh = folders.find(f => f.id === currentFolder.id);
            if (fresh) {
                setCurrentFolder(fresh);
            }
        }
    }, [folders]);

    // Update breadcrumbs
    useEffect(() => {
        if (!currentFolder) {
            setBreadcrumbs([]);
            return;
        }
        const crumbs: Folder[] = [];
        let curr: Folder | undefined = currentFolder;
        while (curr) {
            crumbs.unshift(curr);
            const parentId: string | null = curr.parentId;
            curr = parentId ? folders.find(f => f.id === parentId) : undefined;
        }
        setBreadcrumbs(crumbs);
    }, [currentFolder, folders]);

    // Close preview on Esc key press
    useEffect(() => {
        const handleKeyDown = (e: KeyboardEvent) => {
            if (e.key === 'Escape') {
                setPreviewingRecord(null);
            }
        };

        if (previewingRecord) {
            window.addEventListener('keydown', handleKeyDown);
        }

        return () => {
            window.removeEventListener('keydown', handleKeyDown);
        };
    }, [previewingRecord]);

    // Close record modal on Esc key press
    useEffect(() => {
        const handleKeyDown = (e: KeyboardEvent) => {
            if (e.key === 'Escape') {
                setShowRecordModal(false);
            }
        };

        if (showRecordModal) {
            window.addEventListener('keydown', handleKeyDown);
        }

        return () => {
            window.removeEventListener('keydown', handleKeyDown);
        };
    }, [showRecordModal]);

    const navigateToFolder = (folder: Folder | null) => {
        setCurrentFolder(folder);
        setSearchTerm('');
    };

    // ---------------------------------------------------------
    // Folder Actions
    // ---------------------------------------------------------
    const handleOpenCreateFolder = () => {
        setFolderName('');
        setFolderModalMode('create');
        setShowFolderModal(true);
    };

    const handleOpenEditFolder = (folder: Folder) => {
        setSelectedFolder(folder);
        setFolderName(folder.name);
        setFolderModalMode('edit');
        setShowFolderModal(true);
    };

    const handleSaveFolder = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!folderName.trim()) return;
        setIsSavingFolder(true);
        try {
            if (folderModalMode === 'create') {
                await archivingService.createFolder({
                    name: folderName,
                    parentId: currentFolder ? currentFolder.id : null
                });
                showStatus({
                    type: 'success',
                    title: 'تم إنشاء المجلد',
                    message: `تم إنشاء المجلد "${folderName}" بنجاح.`
                });
            } else if (folderModalMode === 'edit' && selectedFolder) {
                await archivingService.updateFolder(selectedFolder.id, folderName);
                showStatus({
                    type: 'success',
                    title: 'تم تعديل اسم المجلد',
                    message: `تم تعديل الاسم إلى "${folderName}" بنجاح.`
                });
            }
            setShowFolderModal(false);
            setFolderName('');
            await loadFolders();
        } catch (error) {
            console.error('Failed to save folder', error);
            showStatus({
                type: 'error',
                title: 'خطأ في الحفظ',
                message: 'حدث خطأ أثناء حفظ المجلد. يرجى المحاولة لاحقاً.'
            });
        } finally {
            setIsSavingFolder(false);
        }
    };

    const handleDeleteFolder = (folder: Folder) => {
        showConfirm({
            title: 'حذف المجلد',
            message: `هل أنت متأكد من حذف المجلد "${folder.name}" وجميع محتوياته؟ لا يمكن التراجع عن هذا الإجراء.`,
            variant: 'destructive',
            confirmLabel: 'حذف المجلد',
            onConfirm: async () => {
                try {
                    await archivingService.deleteFolder(folder.id);
                    showStatus({
                        type: 'success',
                        title: 'تم حذف المجلد',
                        message: 'تم حذف المجلد وكل محتوياته بنجاح.'
                    });
                    if (currentFolder && (currentFolder.id === folder.id || breadcrumbs.some(c => c.id === folder.id))) {
                        setCurrentFolder(null);
                    }
                    await loadFolders();
                } catch (error) {
                    console.error('Failed to delete folder', error);
                    showStatus({
                        type: 'error',
                        title: 'فشل حذف المجلد',
                        message: 'تعذر إتمام عملية الحذف. يرجى التحقق من محتويات المجلد.'
                    });
                }
            }
        });
    };

    // ---------------------------------------------------------
    // Record Actions
    // ---------------------------------------------------------
    const handleOpenCreateRecord = () => {
        setSelectedRecord(null);
        setArchivalNumber(`ARC-${new Date().getFullYear()}-${Math.floor(1000 + Math.random() * 9000)}`);
        setSelectedTemplateId('');
        setTemplateInputs({});
        setSelectedFiles([]);
        setExistingFiles([]);
        setFileIdsToRemove([]);
        setQrCoverGuid(crypto.randomUUID());
        setGenerateQrCover(true);
        setRecordModalMode('create');
        setShowRecordModal(true);
    };

    const handleOpenEditRecord = (record: ArchiveRecord) => {
        setSelectedRecord(record);
        setArchivalNumber(record.archivalNumber);
        setSelectedTemplateId(record.formId || '');
        setExistingFiles(record.physicalFiles || []);
        setSelectedFiles([]);
        setFileIdsToRemove([]);
        setQrCoverGuid(record.id);
        
        // Load existing template values
        const inputs: Record<string, string> = {};
        if (record.archiveRecordTemplateValues?.archiveRecordFormInputValues) {
            record.archiveRecordTemplateValues.archiveRecordFormInputValues.forEach(val => {
                inputs[val.key] = val.value || '';
            });
        }
        setTemplateInputs(inputs);
        setRecordModalMode('edit');
        setShowRecordModal(true);
    };

    // Load default inputs when template changes in create mode
    useEffect(() => {
        if (recordModalMode === 'create') {
            const template = dynamicTemplates.find(t => t.id === selectedTemplateId);
            if (template) {
                try {
                    const fields = JSON.parse(template.contentAsJson);
                    if (Array.isArray(fields)) {
                        const defaultInputs: Record<string, string> = {};
                        fields.forEach(f => {
                            defaultInputs[f.label] = '';
                        });
                        setTemplateInputs(defaultInputs);
                    }
                } catch (e) {
                    console.error(e);
                    setTemplateInputs({});
                }
            } else {
                setTemplateInputs({});
            }
        }
    }, [selectedTemplateId, dynamicTemplates, recordModalMode]);

    const handleTemplateInputChange = (label: string, value: string) => {
        setTemplateInputs(prev => ({
            ...prev,
            [label]: value
        }));
    };

    const handleSaveRecord = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!archivalNumber.trim() || !currentFolder) return;
        setIsSavingRecord(true);

        try {
            const fieldsArray = Object.keys(templateInputs).map(key => ({
                key,
                value: templateInputs[key]
            }));

            let filesToUpload = [...selectedFiles];

            // Generate QR Cover if required
            if (recordModalMode === 'create' && generateQrCover) {
                const recordId = qrCoverGuid || crypto.randomUUID();
                setQrCoverGuid(recordId);

                // Give template a moment to render with the correct guid
                await new Promise(resolve => setTimeout(resolve, 300));

                if (qrCoverRef.current) {
                    try {
                        const blob = await htmlToImage.toBlob(qrCoverRef.current, {
                            pixelRatio: 2,
                            backgroundColor: '#ffffff'
                        });
                        if (blob) {
                            const qrFile = new File([blob], `QR_Cover_${archivalNumber}.png`, { type: 'image/png' });
                            filesToUpload.unshift(qrFile);
                        }
                    } catch (err) {
                        console.error('Failed to generate QR cover using html-to-image', err);
                    }
                }
            }

            if (recordModalMode === 'create') {
                const recordId = qrCoverGuid || crypto.randomUUID();
                await archivingService.createArchiveRecord({
                    id: recordId,
                    folderId: currentFolder.id,
                    formId: selectedTemplateId || null,
                    archivalNumber,
                    files: filesToUpload,
                    content: fieldsArray
                });
                showStatus({
                    type: 'success',
                    title: 'تم الأرشفة بنجاح',
                    message: `تم حفظ مستند الأرشفة رقم "${archivalNumber}" بنجاح.`
                });
            } else if (recordModalMode === 'edit' && selectedRecord) {
                await archivingService.updateArchiveRecord(selectedRecord.id, {
                    folderId: currentFolder.id,
                    formId: selectedTemplateId,
                    archivalNumber,
                    files: selectedFiles,
                    content: fieldsArray,
                    fileIdsToRemove,
                    replaceFiles: false
                });
                showStatus({
                    type: 'success',
                    title: 'تم تحديث البيانات',
                    message: 'تم تعديل بيانات المستند وحفظ التغييرات.'
                });
            }

            setShowRecordModal(false);
            await loadRecords(currentFolder.id, 1);
        } catch (error) {
            console.error('Failed to save record', error);
            showStatus({
                type: 'error',
                title: 'خطأ في الأرشفة',
                message: 'حدث خطأ أثناء حفظ السجل، يرجى مراجعة البيانات المرفقة.'
            });
        } finally {
            setIsSavingRecord(false);
        }
    };

    const handleDeleteRecord = (record: ArchiveRecord) => {
        showConfirm({
            title: 'حذف مستند',
            message: `هل أنت متأكد من حذف المستند رقم "${record.archivalNumber}" نهائياً؟ لا يمكن التراجع عن هذا الإجراء.`,
            variant: 'destructive',
            confirmLabel: 'حذف المستند',
            onConfirm: async () => {
                try {
                    await archivingService.deleteArchiveRecord(record.id);
                    showStatus({
                        type: 'success',
                        title: 'تم حذف المستند',
                        message: 'تم حذف المستند المؤرشف بنجاح.'
                    });
                    if (currentFolder) {
                        await loadRecords(currentFolder.id, 1);
                    }
                } catch (error) {
                    console.error('Failed to delete record', error);
                    showStatus({
                        type: 'error',
                        title: 'خطأ في الحذف',
                        message: 'تعذر حذف المستند المؤرشف من الخادم.'
                    });
                }
            }
        });
    };

    const handleDownloadRecordZip = async (record: ArchiveRecord) => {
        try {
            showStatus({
                type: 'info',
                title: 'تحضير الملفات',
                message: 'جاري تجميع الملفات في ملف ZIP مضغوط وتنزيله...'
            });
            const blob = await archivingService.downloadZip(record.id);
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = `Record_${record.archivalNumber}.zip`;
            document.body.appendChild(a);
            a.click();
            a.remove();
            window.URL.revokeObjectURL(url);
        } catch (error) {
            console.error('Failed to download zip', error);
            showStatus({
                type: 'error',
                title: 'فشل التنزيل',
                message: 'تعذر تجميع وتنزيل الملفات المضغوطة.'
            });
        }
    };

    // ---------------------------------------------------------
    // Scanner / OCR handlers
    // ---------------------------------------------------------
    const handleApplyScanner = (ocrText: string, files: ImageMeta[]) => {
        const fileObjects = files.map(f => f.file);
        setSelectedFiles(prev => [...prev, ...fileObjects]);

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

    // ---------------------------------------------------------
    // Filters & Render
    // ---------------------------------------------------------
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
                            onClick={() => navigateToFolder(currentFolder.parentId ? folders.find(f => f.id === currentFolder.parentId) || null : null)}
                            className="rounded-xl border border-border h-10 w-10 text-muted-foreground hover:text-foreground"
                            title="المجلد الأعلى"
                        >
                            <ChevronLeft className="h-5 w-5 transform rotate-180" />
                        </Button>
                    )}
                </div>
            </div>

            {/* Toolbar & Filter Bar */}
            <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 bg-muted/20 border border-border/80 p-4 rounded-3xl">
                {/* Search Input */}
                <div className="relative flex-1 max-w-md">
                    <Search className="absolute right-3.5 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                    <Input
                        value={searchTerm}
                        onChange={(e) => setSearchTerm(e.target.value)}
                        placeholder="ابحث عن مجلد أو رقم أرشيف..."
                        className="pr-10 rounded-2xl h-11 bg-background border-border"
                    />
                    {searchTerm && (
                        <button
                            onClick={() => setSearchTerm('')}
                            className="absolute left-3.5 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground"
                        >
                            <X className="h-4 w-4" />
                        </button>
                    )}
                </div>

                {/* View Mode Toggle */}
                <div className="flex items-center gap-2 justify-end">
                    <div className="flex bg-muted/80 p-1.5 rounded-2xl border border-border/50">
                        <button
                            onClick={() => setViewMode('explorer')}
                            className={`flex items-center gap-1.5 px-4 py-2 rounded-xl text-xs font-bold transition-all ${
                                viewMode === 'explorer' 
                                    ? 'bg-card text-primary shadow-sm' 
                                    : 'text-muted-foreground hover:text-foreground'
                            }`}
                            title="عرض شبكي مستكشف"
                        >
                            <LayoutGrid className="h-4 w-4" />
                            <span>شبكة</span>
                        </button>
                        <button
                            onClick={() => setViewMode('list')}
                            className={`flex items-center gap-1.5 px-4 py-2 rounded-xl text-xs font-bold transition-all ${
                                viewMode === 'list' 
                                    ? 'bg-card text-primary shadow-sm' 
                                    : 'text-muted-foreground hover:text-foreground'
                            }`}
                            title="عرض كقائمة جدولية"
                        >
                            <List className="h-4 w-4" />
                            <span>قائمة</span>
                        </button>
                    </div>
                </div>
            </div>

            {/* Breadcrumbs */}
            <div className="flex items-center gap-1.5 text-sm bg-muted/50 p-3 rounded-2xl border border-border">
                <button
                    onClick={() => navigateToFolder(null)}
                    className="text-muted-foreground hover:text-primary font-medium transition-colors font-bold"
                >
                    الأرشيف الرئيسي
                </button>
                {breadcrumbs.map((crumb, idx) => (
                    <React.Fragment key={crumb.id}>
                        <ChevronLeft className="h-4 w-4 text-muted-foreground/60" />
                        <button
                            onClick={() => navigateToFolder(crumb)}
                            className={`font-semibold transition-colors ${
                                idx === breadcrumbs.length - 1 ? 'text-primary font-bold' : 'text-muted-foreground hover:text-primary'
                            }`}
                        >
                            {crumb.name}
                        </button>
                    </React.Fragment>
                ))}
            </div>

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
                        onFolderDoubleClick={navigateToFolder}
                        onRecordClick={setPreviewingRecord}
                        onFolderEdit={handleOpenEditFolder}
                        onFolderDelete={handleDeleteFolder}
                        onRecordEdit={handleOpenEditRecord}
                        onRecordDelete={handleDeleteRecord}
                        onRecordDownloadZip={handleDownloadRecordZip}
                    />
                ) : (
                    <ListView 
                        folders={filteredFolders}
                        records={filteredRecords}
                        onFolderClick={navigateToFolder}
                        onFolderEdit={handleOpenEditFolder}
                        onFolderDelete={handleDeleteFolder}
                        onView={setPreviewingRecord}
                        onEdit={handleOpenEditRecord}
                        onDelete={handleDeleteRecord}
                        onDownloadZip={handleDownloadRecordZip}
                        isLoading={loadingRecords}
                        hasMore={hasMoreRecords}
                        onLoadMore={loadMoreRecords}
                    />
                )}
            </div>

            {/* 1. Modal: Folder Create / Edit */}
            {showFolderModal && (
                <div className="fixed inset-0 bg-black/60 backdrop-blur-sm flex items-center justify-center p-4 z-50 animate-fade-in">
                    <div className="bg-card border border-border rounded-3xl p-6 max-w-md w-full shadow-2xl flex flex-col gap-6 text-right">
                        <div className="flex flex-col gap-1 border-b border-border pb-4">
                            <h2 className="text-base font-bold text-foreground">
                                {folderModalMode === 'create' ? 'إنشاء مجلد جديد' : 'تعديل اسم المجلد'}
                            </h2>
                            <p className="text-xs text-muted-foreground font-medium">
                                {folderModalMode === 'create' 
                                    ? 'أدخل اسم المجلد الذي ترغب بإنشائه في المسار الحالي' 
                                    : 'أدخل الاسم الجديد للمجلد'}
                            </p>
                        </div>

                        <form onSubmit={handleSaveFolder} className="flex flex-col gap-4">
                            <div className="flex flex-col gap-2">
                                <Label className="text-xs font-semibold text-muted-foreground">اسم المجلد</Label>
                                <Input
                                    value={folderName}
                                    onChange={(e) => setFolderName(e.target.value)}
                                    placeholder="مثال: الفواتير الواردة 2026"
                                    className="rounded-2xl h-11 bg-background border-border"
                                    autoFocus
                                />
                            </div>

                            <div className="flex justify-end gap-3 pt-2">
                                <Button
                                    type="button"
                                    variant="ghost"
                                    onClick={() => setShowFolderModal(false)}
                                    className="rounded-xl px-5"
                                    disabled={isSavingFolder}
                                >
                                    إلغاء
                                </Button>
                                <Button
                                    type="submit"
                                    className="rounded-xl px-8 font-bold shadow-lg shadow-primary/20"
                                    disabled={isSavingFolder || !folderName.trim()}
                                >
                                    {isSavingFolder ? 'جاري الحفظ...' : 'حفظ'}
                                </Button>
                            </div>
                        </form>
                    </div>
                </div>
            )}

            {/* 2. Modal: Record Create / Edit */}
            {showRecordModal && (
                <div className="fixed inset-0 bg-black/60 backdrop-blur-sm flex items-center justify-center p-4 z-50 animate-fade-in">
                    <div className="bg-card border border-border rounded-3xl p-6 max-w-2xl w-full max-h-[90vh] shadow-2xl flex flex-col gap-6 text-right overflow-hidden">
                        <div className="flex flex-col gap-1 border-b border-border pb-4 flex-shrink-0">
                            <h2 className="text-base font-bold text-foreground">
                                {recordModalMode === 'create' ? 'أرشفة مستند جديد' : 'تعديل بيانات المستند المؤرشف'}
                            </h2>
                            <p className="text-xs text-muted-foreground font-medium">
                                املأ تفاصيل الأرشفة وأرفق الملفات الخاصة بالمستند
                            </p>
                        </div>

                        <form onSubmit={handleSaveRecord} className="flex flex-col gap-5 flex-1 overflow-hidden">
                            <div className="flex-1 overflow-y-auto flex flex-col gap-5 pr-1.5 pl-0.5">
                                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                                    <div className="flex flex-col gap-2">
                                        <Label className="text-xs font-semibold text-muted-foreground">رقم الأرشفة</Label>
                                        <Input
                                            value={archivalNumber}
                                            onChange={(e) => setArchivalNumber(e.target.value)}
                                            placeholder="مثال: ARC-2026-0001"
                                            className="rounded-2xl h-11 bg-background border-border"
                                            required
                                        />
                                    </div>

                                    <div className="flex flex-col gap-2">
                                        <Label className="text-xs font-semibold text-muted-foreground">نوع نموذج البيانات</Label>
                                        <select
                                            value={selectedTemplateId}
                                            onChange={(e) => setSelectedTemplateId(e.target.value)}
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
                                                        value={templateInputs[label]}
                                                        onChange={(e) => handleTemplateInputChange(label, e.target.value)}
                                                        placeholder={`أدخل ${label}...`}
                                                        className="rounded-lg h-9 bg-background"
                                                    />
                                                </div>
                                            ))}
                                        </div>
                                    </div>
                                )}

                                {/* QR Cover Page Generation Toggle (only in create mode) */}
                                {recordModalMode === 'create' && (
                                    <div className="flex items-center justify-between bg-muted/40 p-4 rounded-2xl border border-border">
                                        <div className="flex flex-col gap-0.5 text-right">
                                            <span className="text-xs font-bold text-foreground">توليد صفحة غلاف الـ QR</span>
                                            <span className="text-[10px] text-muted-foreground">سيتم تلقائياً تصميم وتوليد صفحة غلاف تحتوي على باركود للوصول الفوري للملف</span>
                                        </div>
                                        <Switch
                                            checked={generateQrCover}
                                            onCheckedChange={setGenerateQrCover}
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
                                {recordModalMode === 'edit' && existingFiles.length > 0 && (
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
                                                            className={`p-1.5 rounded-lg transition-colors ${
                                                                isRemoved ? 'text-primary hover:bg-primary/10' : 'text-destructive hover:bg-destructive/10'
                                                            }`}
                                                            onClick={() => {
                                                                if (isRemoved) {
                                                                    setFileIdsToRemove(prev => prev.filter(id => id !== f.id));
                                                                } else {
                                                                    setFileIdsToRemove(prev => [...prev, f.id]);
                                                                }
                                                            }}
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
                                                setSelectedFiles(prev => [...prev, ...files]);
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
                                                        onClick={() => setSelectedFiles(prev => prev.filter((_, i) => i !== index))}
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
                            <div className="border-t border-border pt-4 flex justify-end gap-3 flex-shrink-0">
                                <Button
                                    type="button"
                                    variant="ghost"
                                    onClick={() => setShowRecordModal(false)}
                                    className="rounded-xl px-5"
                                    disabled={isSavingRecord}
                                >
                                    إلغاء
                                </Button>
                                <Button
                                    type="submit"
                                    className="rounded-xl px-8 font-bold shadow-lg shadow-primary/20 flex items-center gap-2"
                                    disabled={isSavingRecord || !archivalNumber.trim()}
                                >
                                    <Upload className="h-4 w-4" />
                                    <span>{isSavingRecord ? 'جاري الحفظ والأرشفة...' : 'حفظ المستند'}</span>
                                </Button>
                            </div>
                        </form>
                    </div>
                </div>
            )}

            {/* 3. Modal: Document Gallery Preview */}
            {previewingRecord && (
                <div className="fixed inset-0 bg-black/60 backdrop-blur-sm flex items-center justify-center p-4 z-50 animate-fade-in animate-duration-300">
                    <div className="bg-card border border-border rounded-3xl w-full max-w-5xl h-[90vh] shadow-2xl flex flex-col overflow-hidden text-right">
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
                                        // تحديث قائمة السجلات لتحديث الحجم/العدادات
                                        await loadRecords(currentFolder.id, recordsPage);
                                        // تحديث السجل المفتوح حالياً لعرض التحديثات في المعاينة فوراً
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

            {/* 4. Scanner & OCR Modal */}
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
        </div>
    );
}
