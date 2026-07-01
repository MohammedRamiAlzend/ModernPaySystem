import { useState } from 'react';
import { useAuthStore } from '@/app/store/authStore';
import { useQuery } from '@tanstack/react-query';
import api from '@/shared/api/baseApi';
import { usePendingEditRequests, useMyEditRequests } from '@/features/archive-edit-requests/model/queries';
import { EditRequestsList } from '@/features/archive-edit-requests/ui/EditRequestsList';
import { EditRequestDetails } from '@/features/archive-edit-requests/ui/EditRequestDetails';
import { EditArchiveRequest, EditArchiveRequestStatus } from '@/features/archive-edit-requests/model/types';
import { Button } from '@/shared/ui/button';
import { Card } from '@/shared/ui/card';
import { Clock, CheckCircle, XCircle, User, FileClock, ShieldAlert } from 'lucide-react';
import { departmentApi } from '@/entities/department/api/departmentApi';
import { AnimatedContainer } from '@/shared/ui/common/animated-container';

export default function ArchiveEditRequestsPage() {
    const user = useAuthStore((state) => state.user);
    const [activeTab, setActiveTab] = useState<'pending' | 'my'>('my');
    const [selectedRequest, setSelectedRequest] = useState<EditArchiveRequest | null>(null);
    const [showDetailsModal, setShowDetailsModal] = useState(false);

    // Fetch user details to get departmentId
    const { data: userProfile, isLoading: isLoadingProfile } = useQuery({
        queryKey: ['users', 'profile', user?.id],
        queryFn: async () => {
            if (!user?.id) return null;
            const res = await api.get<any>(`/Users/${user.id}`);
            return res.data.data;
        },
        enabled: !!user?.id
    });

    const userDeptId = userProfile?.departmentId || null;

    // Fetch department archive leaders to check if current user is leader
    const { data: archiveLeaders } = useQuery({
        queryKey: ['department-archive-leaders', userDeptId],
        queryFn: () => userDeptId ? departmentApi.getArchiveLeaders(userDeptId) : [],
        enabled: !!userDeptId
    });

    const isArchiveLeader = !!user?.roles?.includes('SuperAdmin') ||
        !!user?.permissions?.includes('archiving.edit-requests.approve') ||
        !!(userProfile && archiveLeaders && archiveLeaders.some((leader: any) => leader.userId === userProfile.id));

    const [pendingPage] = useState(1);
    const [myPage] = useState(1);

    const { data: pendingData, isLoading: isLoadingPending } = usePendingEditRequests(
        isArchiveLeader ? userDeptId : null,
        pendingPage,
        10
    );

    const { data: myData, isLoading: isLoadingMy } = useMyEditRequests(myPage, 10);

    const handleViewDetails = (req: EditArchiveRequest) => {
        setSelectedRequest(req);
        setShowDetailsModal(true);
    };

    const getStatusBadge = (status: EditArchiveRequestStatus) => {
        switch (status) {
            case EditArchiveRequestStatus.Pending:
                return (
                    <span className="inline-flex items-center gap-1 px-2.5 py-1 rounded-full bg-amber-500/10 text-amber-500 text-xs font-bold">
                        <Clock className="h-3.5 w-3.5" />
                        <span>قيد المراجعة</span>
                    </span>
                );
            case EditArchiveRequestStatus.Approved:
                return (
                    <span className="inline-flex items-center gap-1 px-2.5 py-1 rounded-full bg-success/10 text-success-foreground text-xs font-bold">
                        <CheckCircle className="h-3.5 w-3.5" />
                        <span>تمت الموافقة</span>
                    </span>
                );
            case EditArchiveRequestStatus.Rejected:
                return (
                    <span className="inline-flex items-center gap-1 px-2.5 py-1 rounded-full bg-destructive/10 text-destructive text-xs font-bold">
                        <XCircle className="h-3.5 w-3.5" />
                        <span>مرفوض</span>
                    </span>
                );
            default:
                return null;
        }
    };

    return (
        <AnimatedContainer className="p-6 md:p-8 flex flex-col gap-6 text-right" dir="rtl">

            {/* Page Header */}
            <div className="flex flex-col md:flex-row md:justify-between md:items-center gap-4 border-b border-border pb-6">
                <div className="flex flex-col gap-1.5">
                    <h1 className="text-xl font-black text-primary">طلبات تعديل الأرشيف</h1>
                    <p className="text-xs text-muted-foreground font-semibold">
                        مراجعة وإدارة طلبات التعديل على السجلات والمستندات المؤرشفة
                    </p>
                </div>
            </div>

            {/* Role Warning / Notice */}
            {isArchiveLeader && userProfile && (
                <div className="flex items-center gap-3 bg-amber-500/10 border border-amber-500/20 p-4 rounded-2xl text-amber-500 text-xs font-bold">
                    <ShieldAlert className="h-5 w-5 flex-shrink-0" />
                    <span>
                        أنت مسجل كمدير أرشيف لقسم ({userProfile.departmentName}). يمكنك الموافقة أو الرفض على طلبات تعديل الأرشيف المقدمة من موظفي قسمك.
                    </span>
                </div>
            )}

            {/* Tab Navigation */}
            <div className="flex border-b border-border pb-px gap-1">
                {isArchiveLeader && (
                    <button
                        onClick={() => setActiveTab('pending')}
                        className={`px-6 py-3 text-xs font-bold border-b-2 transition-all cursor-pointer ${activeTab === 'pending'
                            ? 'border-primary text-primary'
                            : 'border-transparent text-muted-foreground hover:text-foreground'
                            }`}
                    >
                        الطلبات المعلقة للقسم ({pendingData?.totalItems || 0})
                    </button>
                )}
                <button
                    onClick={() => setActiveTab('my')}
                    className={`px-6 py-3 text-xs font-bold border-b-2 transition-all cursor-pointer ${activeTab === 'my'
                        ? 'border-primary text-primary'
                        : 'border-transparent text-muted-foreground hover:text-foreground'
                        }`}
                >
                    طلبات التعديل الخاصة بي ({myData?.totalItems || 0})
                </button>
            </div>

            {/* Tab Contents */}
            <div className="flex-1">
                {activeTab === 'pending' && isArchiveLeader && (
                    <Card className="p-6 border-border rounded-3xl">
                        <div className="flex flex-col gap-4">
                            <span className="text-xs font-black text-muted-foreground">قائمة طلبات موظفي القسم المعلقة</span>
                            <EditRequestsList
                                requests={pendingData?.items || []}
                                isLoading={isLoadingPending || isLoadingProfile}
                                onViewDetails={handleViewDetails}
                            />
                        </div>
                    </Card>
                )}

                {activeTab === 'my' && (
                    <Card className="p-6 border-border rounded-3xl">
                        <div className="flex flex-col gap-4">
                            <span className="text-xs font-black text-muted-foreground">قائمة الطلبات التي قمت بتقديمها للتعديل</span>

                            {isLoadingMy ? (
                                <div className="flex flex-col items-center justify-center p-12 gap-3 text-muted-foreground">
                                    <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
                                    <span className="text-xs font-bold">جاري تحميل طلباتك...</span>
                                </div>
                            ) : myData?.items && myData.items.length > 0 ? (
                                <div className="overflow-x-auto border border-border rounded-2xl bg-card">
                                    <table className="w-full text-sm text-right text-foreground">
                                        <thead className="text-xs text-muted-foreground font-bold bg-muted/30 border-b border-border">
                                            <tr>
                                                <th className="px-6 py-4">رقم الأرشيف</th>
                                                <th className="px-6 py-4">السبب المقدم</th>
                                                <th className="px-6 py-4">تاريخ التقديم</th>
                                                <th className="px-6 py-4">المدير المستلم</th>
                                                <th className="px-6 py-4">حالة الطلب</th>
                                                <th className="px-6 py-4 text-left">الإجراءات</th>
                                            </tr>
                                        </thead>
                                        <tbody className="divide-y divide-border font-medium">
                                            {myData.items.map((req) => (
                                                <tr key={req.id} className="hover:bg-muted/10 transition-colors">
                                                    <td className="px-6 py-4 font-bold text-foreground">
                                                        {req.archiveRecordId.slice(0, 8)}
                                                    </td>
                                                    <td className="px-6 py-4 max-w-xs truncate">
                                                        {req.justification}
                                                    </td>
                                                    <td className="px-6 py-4 text-muted-foreground text-xs">
                                                        {req.createdAt ? new Date(req.createdAt).toLocaleDateString('ar-EG') : '-'}
                                                    </td>
                                                    <td className="px-6 py-4 flex items-center gap-2 justify-end">
                                                        <span>{req.approverName || 'لم يتم المراجعة بعد'}</span>
                                                        <User className="h-4 w-4 text-muted-foreground" />
                                                    </td>
                                                    <td className="px-6 py-4">
                                                        {getStatusBadge(req.status)}
                                                    </td>
                                                    <td className="px-6 py-4 text-left">
                                                        <Button
                                                            size="sm"
                                                            variant="outline"
                                                            onClick={() => handleViewDetails(req)}
                                                            className="rounded-xl h-8 px-3 font-bold border-border text-foreground hover:bg-muted"
                                                        >
                                                            <span>عرض التعديلات المقترحة</span>
                                                        </Button>
                                                    </td>
                                                </tr>
                                            ))}
                                        </tbody>
                                    </table>
                                </div>
                            ) : (
                                <div className="flex flex-col items-center justify-center p-16 gap-3 text-muted-foreground border border-dashed border-border rounded-3xl bg-muted/5">
                                    <FileClock className="h-10 w-10 text-muted-foreground/50" />
                                    <span className="text-xs font-bold">لم تقم بتقديم أي طلبات تعديل حتى الآن</span>
                                </div>
                            )}
                        </div>
                    </Card>
                )}
            </div>

            {/* Details Modal */}
            <EditRequestDetails
                isOpen={showDetailsModal}
                request={selectedRequest}
                onClose={() => {
                    setShowDetailsModal(false);
                    setSelectedRequest(null);
                }}
            />

        </AnimatedContainer>
    );
}
