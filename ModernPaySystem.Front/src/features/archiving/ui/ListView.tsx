import React from 'react';
import { Folder, ArchiveRecord } from '../model/types';
import { UserDisplay } from '@/features/users/ui/UserDisplay';
import { Button } from '@/shared/ui/button';
import {
    FileText,
    Download,
    Edit3,
    Trash2,
    Eye,
    Folder as FolderIcon,
    Move,
    FileX2,
    Building2
} from 'lucide-react';

interface ListViewProps {
    folders: Folder[];
    records: ArchiveRecord[];
    onFolderClick: (folder: Folder) => void;
    onFolderEdit?: (folder: Folder) => void;
    onFolderDelete?: (folder: Folder) => void;
    onView: (record: ArchiveRecord) => void;
    onEdit: (record: ArchiveRecord) => void;
    onDelete?: (record: ArchiveRecord) => void;
    onRecordDeleteRequest?: (record: ArchiveRecord) => void;
    onDownloadZip: (record: ArchiveRecord) => void;
    onRecordRequestEdit?: (record: ArchiveRecord) => void;
    onRecordMove?: (record: ArchiveRecord) => void;
    isLoading?: boolean;
    hasMore?: boolean;
    onLoadMore?: () => void;
}

export const ListView: React.FC<ListViewProps> = ({
    folders,
    records,
    onFolderClick,
    onFolderEdit,
    onFolderDelete,
    onView,
    // onEdit,
    onDelete,
    onRecordDeleteRequest,
    onDownloadZip,
    onRecordRequestEdit,
    onRecordMove,
    isLoading = false,
    hasMore = false,
    onLoadMore
}) => {
    return (
        <div className="flex flex-col gap-4">
            <div className="overflow-x-auto rounded-3xl border border-border bg-card shadow-sm hover:shadow-md transition-all duration-300">
                <table className="w-full text-right text-sm">
                    <thead className="bg-muted/40 text-muted-foreground border-b border-border text-xs">
                        <tr>
                            <th className="p-4 font-semibold text-right">الاسم / رقم الأرشيف</th>
                            <th className="p-4 font-semibold text-right">النوع / النموذج</th>
                            <th className="p-4 font-semibold text-right">المنشئ</th>
                            <th className="p-4 font-semibold text-right">تاريخ الإنشاء</th>
                            <th className="p-4 font-semibold text-right">القسم</th>
                            <th className="p-4 font-semibold text-center">الإجراءات</th>
                        </tr>
                    </thead>
                    <tbody className="divide-y divide-border">
                        {folders.length === 0 && records.length === 0 ? (
                            <tr>
                                <td colSpan={6} className="p-12 text-center text-muted-foreground font-semibold">
                                    المجلد فارغ تماماً
                                </td>
                            </tr>
                        ) : (
                            <>
                                {/* Render Folders */}
                                {folders.map((folder) => (
                                    <tr
                                        key={folder.id}
                                        className="hover:bg-amber-500/5 cursor-pointer transition-colors"
                                        onDoubleClick={() => onFolderClick(folder)}
                                    >
                                        <td className="p-4 flex items-center gap-3">
                                            <div className="p-2 rounded-xl bg-amber-500/10 text-amber-500">
                                                <FolderIcon className="h-4 w-4 fill-amber-500/10" />
                                            </div>
                                            <span className="font-bold text-foreground">{folder.name}</span>
                                        </td>
                                        <td className="p-4 text-muted-foreground font-semibold text-xs">مجلد</td>
                                        <td className="p-4 text-muted-foreground text-xs">
                                            {folder.createdByUserId ? <UserDisplay userId={folder.createdByUserId} showIcon={false} className="text-xs" /> : '-'}
                                        </td>
                                        <td className="p-4 text-muted-foreground text-xs whitespace-nowrap">
                                            {folder.createdAt ? new Date(folder.createdAt).toLocaleDateString('ar-SA') : '-'}
                                        </td>
                                        <td className="p-4 text-muted-foreground text-xs">
                                            <div className="flex items-center gap-1">
                                                {folder.departmentName && <Building2 className="w-3 h-3 shrink-0" />}
                                                {folder.departmentName || '-'}
                                            </div>
                                        </td>
                                        <td className="p-4">
                                            <div className="flex items-center justify-center gap-2" onClick={(e) => e.stopPropagation()}>
                                                {onFolderEdit && folder.canEdit && (
                                                    <Button
                                                        variant="ghost"
                                                        size="icon"
                                                        className="h-8 w-8 rounded-xl text-muted-foreground hover:text-foreground hover:bg-muted border border-transparent hover:border-border transition-all duration-200"
                                                        onClick={() => onFolderEdit(folder)}
                                                        title="تعديل المجلد"
                                                    >
                                                        <Edit3 className="h-4 w-4" />
                                                    </Button>
                                                )}
                                                {onFolderDelete && (
                                                    <Button
                                                        variant="ghost"
                                                        size="icon"
                                                        className="h-8 w-8 rounded-xl text-muted-foreground hover:text-destructive hover:bg-destructive/10 border border-transparent hover:border-destructive/20 transition-all duration-200"
                                                        onClick={() => onFolderDelete(folder)}
                                                        title="حذف المجلد"
                                                    >
                                                        <Trash2 className="h-4 w-4" />
                                                    </Button>
                                                )}
                                            </div>
                                        </td>
                                    </tr>
                                ))}

                                {/* Render Records */}
                                {records.map((record) => (
                                    <tr
                                        key={record.id}
                                        className="hover:bg-primary/5 cursor-pointer transition-colors"
                                        onClick={() => onView(record)}
                                    >
                                        <td className="p-4">
                                            <div className="flex items-center gap-3">
                                                <div className="p-2 rounded-xl bg-sky-500/10 text-sky-500">
                                                    <FileText className="h-4 w-4 fill-sky-500/10" />
                                                </div>
                                                <span className="font-bold text-foreground">{record.name || record.id.slice(0, 8)}</span>
                                            </div>
                                        </td>
                                        <td className="p-4 text-muted-foreground font-semibold text-xs">
                                            {record.formId ? 'نموذج مخصص' : 'نموذج عام'}
                                        </td>
                                        <td className="p-4 text-muted-foreground text-xs">
                                            {record.createdByUserId ? <UserDisplay userId={record.createdByUserId} showIcon={false} className="text-xs" /> : '-'}
                                        </td>
                                        <td className="p-4 text-muted-foreground text-xs whitespace-nowrap">
                                            {record.createdAt ? new Date(record.createdAt).toLocaleDateString('ar-SA') : '-'}
                                        </td>
                                        <td className="p-4 text-muted-foreground text-xs">
                                            <div className="flex items-center gap-1">
                                                {record.departmentName && <Building2 className="w-3 h-3 shrink-0" />}
                                                {record.departmentName || '-'}
                                            </div>
                                        </td>
                                        <td className="p-4">
                                            <div className="flex items-center justify-center gap-2" onClick={(e) => e.stopPropagation()}>
                                                <Button
                                                    variant="ghost"
                                                    size="icon"
                                                    className="h-8 w-8 rounded-xl text-muted-foreground hover:text-foreground hover:bg-muted border border-transparent hover:border-border transition-all duration-200"
                                                    onClick={() => onView(record)}
                                                    title="عرض السجل"
                                                >
                                                    <Eye className="h-4 w-4" />
                                                </Button>
                                                {/* {onEdit && (
                                                    <Button
                                                        variant="ghost"
                                                        size="icon"
                                                        className="h-8 w-8 rounded-xl text-muted-foreground hover:text-foreground hover:bg-muted border border-transparent hover:border-border transition-all duration-200"
                                                        onClick={() => onEdit(record)}
                                                        title="تعديل السجل"
                                                    >
                                                        <Edit3 className="h-4 w-4" />
                                                    </Button>
                                                )} */}
                                                {onRecordRequestEdit && (
                                                    <Button
                                                        variant="ghost"
                                                        size="icon"
                                                        className="h-8 w-8 rounded-xl text-muted-foreground hover:text-amber-500 hover:bg-amber-500/10 border border-transparent hover:border-amber-500/20 transition-all duration-200"
                                                        onClick={() => onRecordRequestEdit(record)}
                                                        title="طلب تعديل السجل"
                                                    >
                                                        <Edit3 className="h-4 w-4 text-amber-500" />
                                                    </Button>
                                                )}
                                                {onRecordMove && (
                                                    <Button
                                                        variant="ghost"
                                                        size="icon"
                                                        className="h-8 w-8 rounded-xl text-muted-foreground hover:text-primary hover:bg-primary/10 border border-transparent hover:border-primary/20 transition-all duration-200"
                                                        onClick={() => onRecordMove(record)}
                                                        title="نقل المستند"
                                                    >
                                                        <Move className="h-4 w-4" />
                                                    </Button>
                                                )}
                                                <Button
                                                    variant="ghost"
                                                    size="icon"
                                                    className="h-8 w-8 rounded-xl text-muted-foreground hover:text-foreground hover:bg-muted border border-transparent hover:border-border transition-all duration-200"
                                                    onClick={() => onDownloadZip(record)}
                                                    title="تحميل كملف ZIP"
                                                >
                                                    <Download className="h-4 w-4" />
                                                </Button>
                                                {onRecordDeleteRequest && (
                                                    <Button
                                                        variant="ghost"
                                                        size="icon"
                                                        className="h-8 w-8 rounded-xl text-muted-foreground hover:text-destructive hover:bg-destructive/10 border border-transparent hover:border-destructive/20 transition-all duration-200"
                                                        onClick={() => onRecordDeleteRequest(record)}
                                                        title="تقديم طلب حذف"
                                                    >
                                                        <FileX2 className="h-4 w-4" />
                                                    </Button>
                                                )}
                                                {onDelete && !onRecordDeleteRequest && (
                                                    <Button
                                                        variant="ghost"
                                                        size="icon"
                                                        className="h-8 w-8 rounded-xl text-muted-foreground hover:text-destructive hover:bg-destructive/10 border border-transparent hover:border-destructive/20 transition-all duration-200"
                                                        onClick={() => onDelete(record)}
                                                        title="حذف الأرشيف"
                                                    >
                                                        <Trash2 className="h-4 w-4" />
                                                    </Button>
                                                )}
                                            </div>
                                        </td>
                                    </tr>
                                ))}
                            </>
                        )}
                    </tbody>
                </table>
            </div>

            {hasMore && onLoadMore && (
                <div className="flex justify-center py-4">
                    <Button
                        variant="outline"
                        onClick={onLoadMore}
                        disabled={isLoading}
                        className="rounded-xl px-8 border-border text-foreground hover:bg-muted font-bold transition-all hover:scale-[1.02]"
                    >
                        {isLoading ? 'جاري التحميل...' : 'تحميل المزيد'}
                    </Button>
                </div>
            )}
        </div>
    );
};