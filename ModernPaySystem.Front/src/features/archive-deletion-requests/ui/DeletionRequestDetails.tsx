import { DeleteArchiveRequest, DeleteArchiveRequestStatus, ArchiveDeletionTargetType } from '../model/types';
import { X, Calendar, User, FileText, Folder, Clock, CheckCircle, XCircle, HardDrive, File, FolderOpen } from 'lucide-react';
import { UserDisplay } from '@/features/users/ui/UserDisplay';
import { Button } from '@/shared/ui/button';

interface DeletionRequestDetailsProps {
    isOpen: boolean;
    request: DeleteArchiveRequest | null;
    onClose: () => void;
}

export function DeletionRequestDetails({ isOpen, request, onClose }: DeletionRequestDetailsProps) {
    if (!isOpen || !request) return null;

    const isRecord = request.targetType === ArchiveDeletionTargetType.Record;

    const getStatusBadge = (status: DeleteArchiveRequestStatus) => {
        switch (status) {
            case DeleteArchiveRequestStatus.Pending:
                return (
                    <span className="inline-flex items-center gap-1 px-2.5 py-1 rounded-full bg-amber-500/10 text-amber-500 text-xs font-bold">
                        <Clock className="h-3.5 w-3.5" />
                        <span>قيد المراجعة</span>
                    </span>
                );
            case DeleteArchiveRequestStatus.Approved:
                return (
                    <span className="inline-flex items-center gap-1 px-2.5 py-1 rounded-full bg-success/10 text-success-foreground text-xs font-bold">
                        <CheckCircle className="h-3.5 w-3.5" />
                        <span>تمت الموافقة</span>
                    </span>
                );
            case DeleteArchiveRequestStatus.Rejected:
                return (
                    <span className="inline-flex items-center gap-1 px-2.5 py-1 rounded-full bg-destructive/10 text-destructive text-xs font-bold">
                        <XCircle className="h-3.5 w-3.5" />
                        <span>مرفوض</span>
                    </span>
                );
            case DeleteArchiveRequestStatus.Executed:
                return (
                    <span className="inline-flex items-center gap-1 px-2.5 py-1 rounded-full bg-primary/10 text-primary text-xs font-bold">
                        <HardDrive className="h-3.5 w-3.5" />
                        <span>تم التنفيذ</span>
                    </span>
                );
            default:
                return null;
        }
    };

    return (
        <div className="fixed inset-0 bg-black/60 backdrop-blur-sm flex items-center justify-center p-4 z-50 animate-fade-in">
            <div className="bg-card border border-border rounded-3xl p-6 max-w-3xl w-full max-h-[90vh] shadow-2xl flex flex-col gap-6 text-right overflow-hidden" dir="rtl">

                <div className="flex justify-between items-start border-b border-border pb-4 flex-shrink-0">
                    <button onClick={onClose} className="text-muted-foreground hover:text-foreground transition-colors p-1 rounded-lg">
                        <X className="h-5 w-5" />
                    </button>
                    <div className="flex flex-col gap-1">
                        <h2 className="text-base font-bold text-foreground">
                            تفاصيل طلب حذف {isRecord ? 'مستند' : 'مجلد'}
                        </h2>
                        <p className="text-xs text-muted-foreground font-medium">
                            طلب رقم: {request.id.slice(0, 8)}
                        </p>
                    </div>
                </div>

                <div className="flex-grow overflow-y-auto flex flex-col gap-5 pr-1.5 pl-0.5">

                    <div className="grid grid-cols-1 sm:grid-cols-2 gap-3 bg-muted/20 border border-border p-4 rounded-2xl">
                        <div className="flex flex-col gap-1 items-start">
                            <span className="text-[10px] font-bold text-muted-foreground">مقدم الطلب</span>
                            <div className="flex items-center gap-1.5 text-xs font-semibold text-foreground">
                                <User className="h-3.5 w-3.5 text-muted-foreground" />
                                <UserDisplay userId={request.requesterId} showIcon={false} />
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
                            <span className="text-[10px] font-bold text-muted-foreground">نوع الهدف</span>
                            <div className="flex items-center gap-1.5 text-xs font-bold text-foreground">
                                {isRecord ? (
                                    <FileText className="h-3.5 w-3.5 text-muted-foreground" />
                                ) : (
                                    <Folder className="h-3.5 w-3.5 text-muted-foreground" />
                                )}
                                <span>{isRecord ? 'مستند أرشيفي' : 'مجلد'}</span>
                            </div>
                        </div>
                        <div className="flex flex-col gap-1 items-start">
                            <span className="text-[10px] font-bold text-muted-foreground">حالة الطلب</span>
                            {getStatusBadge(request.status)}
                        </div>
                    </div>

                    <div className="flex flex-col gap-1.5 bg-destructive/5 border border-destructive/10 p-4 rounded-2xl">
                        <span className="text-xs font-bold text-destructive">سبب طلب الحذف:</span>
                        <p className="text-xs font-medium text-foreground leading-relaxed whitespace-pre-wrap">
                            {request.justification}
                        </p>
                    </div>

                    {request.targetSnapshot && (
                        <div className="flex flex-col gap-2">
                            <span className="text-xs font-bold text-foreground">معلومات الهدف:</span>
                            <div className="grid grid-cols-2 sm:grid-cols-3 gap-3 border border-border p-4 rounded-2xl bg-muted/10">
                                {request.targetSnapshot.displayName && (
                                    <div className="flex flex-col gap-1">
                                        <span className="text-[10px] font-bold text-muted-foreground">الاسم</span>
                                        <span className="text-xs font-semibold text-foreground">{request.targetSnapshot.displayName}</span>
                                    </div>
                                )}
                                <div className="flex flex-col gap-1">
                                    <span className="text-[10px] font-bold text-muted-foreground">عدد الملفات</span>
                                    <span className="text-xs font-bold text-foreground">{request.targetSnapshot.fileCount}</span>
                                </div>
                                {isRecord && (
                                    <div className="flex flex-col gap-1">
                                        <span className="text-[10px] font-bold text-muted-foreground">عدد السجلات</span>
                                        <span className="text-xs font-bold text-foreground">{request.targetSnapshot.recordCount}</span>
                                    </div>
                                )}
                            </div>
                        </div>
                    )}

                    {request.dependencies && request.dependencies.length > 0 && (
                        <div className="flex flex-col gap-2">
                            <span className="text-xs font-bold text-muted-foreground">المتعلقات التي سيتم حذفها:</span>
                            <div className="flex flex-col gap-1.5 max-h-[150px] overflow-y-auto border border-border rounded-2xl p-3 bg-muted/5">
                                {request.dependencies.map((dep, idx) => (
                                    <div key={idx} className="flex items-center gap-2 text-xs p-2 rounded-xl bg-background border border-border">
                                        {dep.kind === 'physical-file' ? (
                                            <File className="h-3.5 w-3.5 text-destructive" />
                                        ) : (
                                            <FolderOpen className="h-3.5 w-3.5 text-amber-500" />
                                        )}
                                        <span className="font-semibold text-foreground">{dep.displayName || dep.id.slice(0, 8)}</span>
                                        {dep.details && (
                                            <span className="text-muted-foreground mr-auto">{dep.details}</span>
                                        )}
                                    </div>
                                ))}
                            </div>
                        </div>
                    )}

                    {request.status === DeleteArchiveRequestStatus.Rejected && request.rejectionReason && (
                        <div className="flex flex-col gap-1.5 bg-destructive/10 border border-destructive/20 p-4 rounded-2xl">
                            <span className="text-xs font-bold text-destructive">سبب الرفض:</span>
                            <p className="text-xs font-medium text-foreground leading-relaxed whitespace-pre-wrap">
                                {request.rejectionReason}
                            </p>
                            {request.rejectedAt && (
                                <span className="text-[10px] text-muted-foreground mt-1">
                                    تم الرفض في: {new Date(request.rejectedAt).toLocaleString('ar-EG')}
                                </span>
                            )}
                        </div>
                    )}

                    {request.status === DeleteArchiveRequestStatus.Approved && request.approvalNotes && (
                        <div className="flex flex-col gap-1.5 bg-success/10 border border-success/20 p-4 rounded-2xl">
                            <span className="text-xs font-bold text-success-foreground">ملاحظات الموافقة:</span>
                            <p className="text-xs font-medium text-foreground leading-relaxed whitespace-pre-wrap">
                                {request.approvalNotes}
                            </p>
                        </div>
                    )}

                    {request.approverId && (
                        <div className="flex items-center gap-2 text-xs border-t border-border pt-4">
                            <User className="h-4 w-4 text-muted-foreground" />
                            <span className="font-bold text-muted-foreground">المدير المعني:</span>
                            <span className="font-semibold text-foreground"><UserDisplay userId={request.approverId} showIcon={false} /></span>
                        </div>
                    )}

                </div>

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
