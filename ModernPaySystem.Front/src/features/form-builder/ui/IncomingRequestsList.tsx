import { Skeleton } from '@/shared/ui/common/skeleton';
import { Card } from '@/shared/ui/card';
import { Clock, FileText, Eye, ChevronRight, Paperclip, MessageSquare } from 'lucide-react';
import { Button } from '@/shared/ui/button';
import { UserDisplay } from '@/features/users/ui/UserDisplay';
import { RequestFieldsPreview } from './RequestFieldsPreview';
import { Pagination } from '@/shared/ui/common/pagination';
import { useTemplateById } from '../api/formEndpoints';
import type { FormSchema, TemplateRequest } from '@/entities/form/model/types';

interface IncomingRequestsListProps {
    requests: (TemplateRequest & { isNew?: boolean })[];
    isLoading: boolean;
    templates: FormSchema[];
    selectedRequestId: string;
    onSelectRequest: (id: string) => void;
    onViewRequest: (request: TemplateRequest) => void;
    page: number;
    totalPages: number;
    onPageChange: (page: number) => void;
}

const TemplateTitle = ({ templateId, fallbackTitle }: { templateId: string, fallbackTitle: string }) => {
    const { data: template } = useTemplateById(templateId);
    return <>{template?.title || fallbackTitle}</>;
};

export const IncomingRequestsList = ({
    requests,
    isLoading,
    templates,
    selectedRequestId,
    onSelectRequest,
    onViewRequest,
    page,
    totalPages,
    onPageChange
}: IncomingRequestsListProps) => {
    return (
        <Card className="p-6 overflow-hidden flex flex-col h-full shadow-lg border-primary/5">
            <div className="flex items-center justify-between mb-4">
                <h2 className="text-xl font-semibold flex items-center gap-2 text-primary">
                    <Clock className="w-5 h-5" />
                    الطلبات الواردة
                </h2>
                <Pagination
                    currentPage={page}
                    totalPages={totalPages}
                    onPageChange={onPageChange}
                    className="mt-4 hidden md:flex gap-1"

                />
            </div>

            <div className="flex-1 overflow-y-auto space-y-4 pr-2 custom-scrollbar">
                {isLoading ? (
                    Array.from({ length: 4 }).map((_, i) => (
                        <Skeleton key={i} className="h-32 w-full rounded-xl" />
                    ))
                ) : requests.length > 0 ? (
                    requests.map((request) => (
                        <div
                            key={request.id}
                            onClick={() => onSelectRequest(request.id)}
                            className={`p-4 rounded-xl border-2 transition-all cursor-pointer group ${selectedRequestId === request.id
                                ? 'border-primary bg-primary/5'
                                : request.isNew
                                    ? 'border-primary/30 bg-primary/5 hover:border-primary/50'
                                    : 'border-border hover:border-primary/50 hover:bg-muted/30'
                                }`}
                        >
                            <div className="flex justify-between items-start mb-3">
                                <div className="flex items-center gap-2">
                                    <div className={`p-2 rounded-lg ${request.isNew ? 'bg-primary/20' : 'bg-primary/10'}`}>
                                        <FileText className={`w-4 h-4 ${request.isNew ? 'text-primary animate-pulse' : 'text-primary'}`} />
                                    </div>
                                    <div>
                                        <div className="font-bold text-sm truncate max-w-[200px] flex items-center gap-2">
                                            <TemplateTitle 
                                                templateId={request.templateId} 
                                                fallbackTitle={templates.find(t => t.id === request.templateId)?.title || `طلب #${request.requestNumber}`} 
                                            />
                                            <span className="px-1.5 py-0.5 bg-primary/10 text-[10px] text-primary rounded-md whitespace-nowrap">
                                                #{request.requestNumber}
                                            </span>
                                            {request.isNew && (
                                                <span className="px-1.5 py-0.5 bg-primary text-[10px] text-white rounded-md">جديد</span>
                                            )}
                                        </div>
                                        <div className="text-[10px] mt-0.5">
                                            <UserDisplay
                                                userId={request.requesterId}
                                                showIcon={true}
                                                iconClassName="w-3 h-3"
                                                className="text-muted-foreground"
                                            />
                                        </div>
                                    </div>
                                </div>
                                <div className="flex items-center gap-2">
                                    <Button
                                        variant="ghost"
                                        size="icon"
                                        className="h-8 w-8 rounded-full hover:bg-primary/20 hover:text-primary transition-colors"
                                        onClick={(e) => {
                                            e.stopPropagation();
                                            onViewRequest(request);
                                        }}
                                    >
                                        <Eye className="w-4 h-4" />
                                    </Button>
                                    <ChevronRight className={`w-4 h-4 transition-transform ${selectedRequestId === request.id ? 'translate-x-[-4px] text-primary' : 'text-muted-foreground group-hover:translate-x-[-2px]'}`} />
                                </div>
                            </div>

                            <RequestFieldsPreview
                                content={request.content}
                                fields={templates.find(t => t.id === request.templateId)?.fields || []}
                                templateId={request.templateId}
                                variant="card"
                            />

                            <div className="mt-3 flex items-center justify-between gap-4 text-[10px] text-muted-foreground">
                                <div className="flex items-center gap-4">
                                    {request.requestAttachmentDtos && request.requestAttachmentDtos.length > 0 && (
                                        <span className="flex items-center gap-1">
                                            <Paperclip className="w-3 h-3" />
                                            {request.requestAttachmentDtos.length} ملفات مرفقة
                                        </span>
                                    )}
                                </div>
                                {request.createdAt && (
                                    <span className="flex items-center gap-1 bg-muted/40 px-2 py-0.5 rounded-full">
                                        <Clock className="w-2.5 h-2.5" />
                                        {new Date(request.createdAt).toLocaleString('ar-EG', {
                                            day: 'numeric',
                                            month: 'short',
                                            year: 'numeric',
                                            hour: '2-digit',
                                            minute: '2-digit'
                                        })}
                                    </span>
                                )}
                            </div>
                        </div>
                    ))
                ) : (
                    <div className="h-64 flex flex-col items-center justify-center text-muted-foreground border-2 border-dashed rounded-xl">
                        <MessageSquare className="w-12 h-12 mb-2 opacity-20" />
                        <p>لا توجد طلبات واردة حالياً</p>
                    </div>
                )}
            </div>

            {totalPages > 1 && (
                <div className="mt-4 pt-4 border-t">
                    <Pagination
                        currentPage={page}
                        totalPages={totalPages}
                        onPageChange={onPageChange}
                    />
                </div>
            )}
        </Card>
    );
};
