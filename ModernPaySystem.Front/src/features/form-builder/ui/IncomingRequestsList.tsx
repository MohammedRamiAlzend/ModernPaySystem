import { Skeleton } from '@/shared/ui/common/skeleton';
import { Card } from '@/shared/ui/card';
import { Clock, FileText, Eye, ChevronRight, Paperclip, MessageSquare } from 'lucide-react';
import { Button } from '@/shared/ui/button';
import { UserDisplay } from '@/features/users/ui/UserDisplay';
import type { FormSchema, TemplateRequest } from '@/entities/form/model/types';

interface IncomingRequestsListProps {
    requests: TemplateRequest[];
    isLoading: boolean;
    templates: FormSchema[];
    selectedRequestId: string;
    onSelectRequest: (id: string) => void;
    onViewRequest: (request: TemplateRequest) => void;
}

export const IncomingRequestsList = ({
    requests,
    isLoading,
    templates,
    selectedRequestId,
    onSelectRequest,
    onViewRequest
}: IncomingRequestsListProps) => {
    return (
        <Card className="lg:col-span-2 p-6 overflow-hidden flex flex-col h-[700px]">
            <h2 className="text-xl font-semibold mb-4 flex items-center gap-2 text-primary">
                <Clock className="w-5 h-5" />
                الطلبات الواردة
            </h2>

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
                                : 'border-border hover:border-primary/50 hover:bg-muted/30'
                                }`}
                        >
                            <div className="flex justify-between items-start mb-3">
                                <div className="flex items-center gap-2">
                                    <div className="p-2 bg-primary/10 rounded-lg">
                                        <FileText className="w-4 h-4 text-primary" />
                                    </div>
                                    <div>
                                        <div className="font-bold text-sm truncate max-w-[200px]">
                                            {templates.find(t => t.id === request.templateId)?.title || `${request.id.split('-')[0].toUpperCase()} ... ID`}
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

                            {/* Display first 3 fields */}
                            <div className="space-y-2">
                                {(() => {
                                    try {
                                        const data = JSON.parse(request.content);
                                        const schema = templates.find(t => t.id === request.templateId);
                                        const fields = schema?.fields || [];

                                        // Get first 3 fields that have values
                                        const displayFields = fields
                                            .filter(field => data[field.name] !== undefined && data[field.name] !== null && data[field.name] !== '')
                                            .slice(0, 3);

                                        if (displayFields.length === 0) {
                                            return (
                                                <div className="text-xs text-muted-foreground bg-muted/20 p-2 rounded-md">
                                                    لا توجد بيانات لعرضها
                                                </div>
                                            );
                                        }

                                        return displayFields.map((field, idx) => (
                                            <div key={idx} className="flex items-start gap-2 text-xs bg-muted/20 p-2 rounded-md">
                                                <span className="font-bold text-muted-foreground min-w-[80px]">
                                                    {field.label}:
                                                </span>
                                                <span className="text-foreground font-medium truncate">
                                                    {String(data[field.name])}
                                                </span>
                                            </div>
                                        ));
                                    } catch (e) {
                                        return (
                                            <div className="text-xs text-muted-foreground bg-muted/20 p-2 rounded-md font-mono truncate">
                                                {request.content}
                                            </div>
                                        );
                                    }
                                })()}
                            </div>

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
        </Card>
    );
};
