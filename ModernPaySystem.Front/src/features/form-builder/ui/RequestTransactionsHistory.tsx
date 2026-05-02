import { useRequestTransactionsHistory, formEndpoints } from '@/features/form-builder/api/formEndpoints';
import { UserDisplay } from '@/features/users/ui/UserDisplay';
import { useAttachments } from '@/features/form-builder/model/useAttachments';
import { AttachmentsGallery } from '@/features/form-builder/ui/AttachmentsGallery';
import { Loader2, GitPullRequest, Clock, MessageSquare } from 'lucide-react';
import type { RequestTransactionDto } from '@/entities/form/model/types';

interface TransactionItemProps {
    transaction: RequestTransactionDto;
}

const TransactionItem = ({ transaction }: TransactionItemProps) => {
    const {
        zipImages,
        isLoading: isLoadingImages,
        isAllImages,
        totalFiles
    } = useAttachments(
        transaction.requestTransactionAttachments && transaction.requestTransactionAttachments.length > 0
            ? () => formEndpoints.fetchTransactionAttachmentsBlob(transaction.id)
            : null,
        [transaction.id, transaction.requestTransactionAttachments]
    );

    return (
        <div className="relative pl-8 pb-8 last:pb-0 border-l-2 border-primary/20 ml-3">
            {/* Timeline Dot */}
            <div className="absolute left-[-9px] top-0 w-4 h-4 rounded-full bg-primary border-2 border-background shadow-sm" />

            <div className="bg-background/50 rounded-xl p-4 border border-primary/10 shadow-sm hover:shadow-md transition-shadow">
                <div className="flex items-center justify-between gap-4 mb-3">
                    <div className="flex items-center gap-2">
                        <UserDisplay userId={transaction.createdByUserId} className="text-xs font-bold" showIcon />
                    </div>
                    <div className="flex items-center gap-1.5 text-[10px] text-muted-foreground bg-muted/30 px-2 py-1 rounded-full">
                        <Clock className="w-3 h-3" />
                        {new Date(transaction.createdAt!).toLocaleString('ar-EG')}
                    </div>
                </div>

                {transaction.notes && (
                    <div className="bg-muted/20 p-3 rounded-lg border border-border/50 mb-4">
                        <div className="flex items-start gap-2">
                            <MessageSquare className="w-3.5 h-3.5 text-primary/60 mt-1 shrink-0" />
                            <p className="text-xs leading-relaxed text-foreground/80">{transaction.notes}</p>
                        </div>
                    </div>
                )}

                {transaction.requestTransactionAttachments && transaction.requestTransactionAttachments.length > 0 && (
                    <div className="mt-4">
                        <AttachmentsGallery
                            images={zipImages}
                            isLoading={isLoadingImages}
                            isAllImages={isAllImages}
                            totalFiles={totalFiles}
                            requestId={transaction.id}
                            onDownloadAll={() => formEndpoints.downloadTransactionAttachments(transaction.id)}
                            initialExpanded={false}
                        />
                    </div>
                )}
            </div>
        </div>
    );
};

interface RequestTransactionsHistoryProps {
    requestId: string;
}

export const RequestTransactionsHistory = ({ requestId }: RequestTransactionsHistoryProps) => {
    const { data: transactions, isLoading } = useRequestTransactionsHistory(requestId);

    if (isLoading) {
        return (
            <div className="flex flex-col items-center justify-center py-12">
                <Loader2 className="w-8 h-8 animate-spin text-primary opacity-20" />
                <p className="text-xs text-muted-foreground mt-4">جاري تحميل تاريخ الإحالات...</p>
            </div>
        );
    }

    if (!transactions || transactions.length === 0) {
        return (
            <div className="flex flex-col items-center justify-center py-12 text-center">
                <div className="p-4 bg-muted/20 rounded-full mb-4">
                    <GitPullRequest className="w-8 h-8 text-muted-foreground opacity-20" />
                </div>
                <p className="text-sm font-bold text-muted-foreground">لا توجد إحالات سابقة</p>
                <p className="text-xs text-muted-foreground/60 mt-2">سيتم عرض كافة الإحالات التي تمت على هذا الطلب هنا</p>
            </div>
        );
    }

    return (
        <div className="py-2">
            <div className="flex items-center gap-2 mb-8 bg-primary/5 p-3 rounded-xl border border-primary/10">
                <GitPullRequest className="w-4 h-4 text-primary" />
                <h4 className="text-sm font-bold text-primary">تاريخ الإحالات والتنقلات</h4>
                <span className="mr-auto text-[10px] font-bold bg-primary/20 text-primary px-2 py-0.5 rounded-full">
                    {transactions.length} إحالة
                </span>
            </div>

            <div className="space-y-2">
                {[...transactions]
                    .sort((a, b) => new Date(a.createdAt!).getTime() - new Date(b.createdAt!).getTime())
                    .reverse()
                    .map((transaction) => (
                        <TransactionItem key={transaction.id} transaction={transaction} />
                    ))}
            </div>
        </div>
    );
};
