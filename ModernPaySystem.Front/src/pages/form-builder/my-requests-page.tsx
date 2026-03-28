import { Card } from '@/shared/ui/card';
import { AnimatedContainer } from '@/shared/ui/common/animated-container';
import { FileText, Eye, History } from 'lucide-react';
import { Skeleton } from '@/shared/ui/common/skeleton';
import { ResponseDetailsModal } from '@/widgets/form-editor/ui/response-details-modal';
import { Button } from '@/shared/ui/button';
import { UserDisplay } from '@/features/users/ui/UserDisplay';
import { RequestFieldsPreview } from '@/features/form-builder/ui/RequestFieldsPreview';
import { useMyRequestsLogic } from '@/features/form-builder/model/useMyRequestsLogic';
import { Pagination } from '@/shared/ui/common/pagination';
import type { TemplateRequest } from '@/entities/form/model/types';

export const MyRequestsPage = () => {
    const {
        requests,
        isLoading,
        templates,
        totalItems,
        totalPages,
        page,
        setPage,
        isModalOpen,
        setIsModalOpen,
        viewingResponse,
        handleViewRequest
    } = useMyRequestsLogic();

    return (
        <AnimatedContainer className="container mx-auto p-6 space-y-6">
            <h1 className="text-3xl font-bold">طلباتي</h1>

            <Card className="p-6 overflow-hidden flex flex-col min-h-[600px]">
                <div className="flex items-center justify-between mb-6">
                    <h2 className="text-xl font-semibold flex items-center gap-2 text-primary">
                        <History className="w-5 h-5" />
                        سجل الطلبات المقدمة
                    </h2>
                    <div className="flex items-center gap-4">
                        <span className="px-3 py-1 bg-primary/10 text-primary text-xs font-bold rounded-full">
                            {totalItems} طلب مقدم
                        </span>
                        <Pagination
                            currentPage={page}
                            totalPages={totalPages}
                            onPageChange={setPage}
                            className="mt-4 hidden md:flex"
                        />
                    </div>
                </div>

                <div className="overflow-x-auto flex-1 custom-scrollbar">
                    <table className="w-full text-right border-collapse" dir="rtl">
                        <thead className="bg-muted/30 sticky top-0 z-10 backdrop-blur-md">
                            <tr>
                                <th className="px-6 py-4 font-bold text-sm text-muted-foreground border-b text-right">اسم النموذج</th>
                                <th className="px-6 py-4 font-bold text-sm text-muted-foreground border-b text-right">محتوى الطلب</th>
                                <th className="px-6 py-4 font-bold text-sm text-muted-foreground border-b text-right">الموافق/المسؤول</th>
                                <th className="px-6 py-4 font-bold text-sm text-muted-foreground border-b text-right">تاريخ التقديم</th>
                                <th className="px-6 py-4 font-bold text-sm text-muted-foreground border-b text-center">الإجراءات</th>
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-border/50">
                            {isLoading ? (
                                Array.from({ length: 5 }).map((_, i) => (
                                    <tr key={i} className="animate-pulse">
                                        <td colSpan={6} className="px-6 py-4">
                                            <Skeleton className="h-10 w-full rounded-lg" />
                                        </td>
                                    </tr>
                                ))
                            ) : requests.length > 0 ? (
                                requests.map((request: TemplateRequest) => (
                                    <tr key={request.id} className="hover:bg-muted/20 transition-colors group">
                                        <td className="px-6 py-4">
                                            <div className="flex items-center gap-3">
                                                <div className="p-2 bg-primary/10 rounded-lg group-hover:bg-primary/20 transition-colors">
                                                    <FileText className="w-4 h-4 text-primary" />
                                                </div>
                                                <span className="font-bold text-sm">
                                                    {templates.find(t => t.id === request.templateId)?.title || "نموذج غير معروف"}
                                                </span>
                                            </div>
                                        </td>
                                        <td className="px-6 py-4">
                                            <RequestFieldsPreview
                                                content={request.content}
                                                fields={templates.find(t => t.id === request.templateId)?.fields || []}
                                                variant="inline"
                                                className="space-y-1.5 max-w-xs"
                                            />
                                        </td>
                                        <td className="px-6 py-4">
                                            <UserDisplay
                                                userId={request.approverId}
                                                showIcon={true}
                                                iconClassName="w-3 h-3"
                                                className="text-sm"
                                            />
                                        </td>
                                        <td className="px-6 py-4 text-xs text-muted-foreground">
                                            {request.createdAt ? new Date(request.createdAt).toLocaleDateString('ar-EG') : '---'}
                                        </td>
                                        <td className="px-6 py-4 text-center">
                                            <Button
                                                variant="outline"
                                                size="sm"
                                                className="h-9 px-4 rounded-xl gap-2 text-xs font-bold hover:bg-primary hover:text-white transition-all shadow-sm"
                                                onClick={() => handleViewRequest(request)}
                                            >
                                                <Eye className="w-4 h-4" />
                                                عرض التفاصيل
                                            </Button>
                                        </td>
                                    </tr>
                                ))
                            ) : (
                                <tr>
                                    <td colSpan={6} className="py-20 text-center">
                                        <div className="flex flex-col items-center justify-center text-muted-foreground">
                                            <History className="w-12 h-12 mb-3 opacity-20" />
                                            <p className="font-medium">لا توجد طلبات مقدمة حالياً</p>
                                        </div>
                                    </td>
                                </tr>
                            )}
                        </tbody>
                    </table>
                </div>

                <div className="mt-6 pt-6 border-t">
                    <Pagination
                        currentPage={page}
                        totalPages={totalPages}
                        onPageChange={setPage}
                    />
                </div>
            </Card>

            {viewingResponse && (
                <ResponseDetailsModal
                    isOpen={isModalOpen}
                    onClose={() => setIsModalOpen(false)}
                    response={viewingResponse}
                    schema={viewingResponse.schema}
                />
            )}
        </AnimatedContainer>
    );
};

export default MyRequestsPage;
