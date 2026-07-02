import { useState } from 'react';
import { useAuthStore } from '@/app/store/authStore';
import { useQuery } from '@tanstack/react-query';
import api from '@/shared/api/baseApi';
import { usePendingDeletionRequests } from '@/features/archive-deletion-requests/model/queries';
import { DeletionRequestsList } from '@/features/archive-deletion-requests/ui/DeletionRequestsList';
import { DeletionRequestDetails } from '@/features/archive-deletion-requests/ui/DeletionRequestDetails';
import { DeleteArchiveRequest } from '@/features/archive-deletion-requests/model/types';
import { Card } from '@/shared/ui/card';
import { ShieldAlert } from 'lucide-react';
import { departmentApi } from '@/entities/department/api/departmentApi';
import { AnimatedContainer } from '@/shared/ui/common/animated-container';

export default function ArchiveDeletionRequestsPage() {
    const user = useAuthStore((state) => state.user);
    const [selectedRequest, setSelectedRequest] = useState<DeleteArchiveRequest | null>(null);
    const [showDetailsModal, setShowDetailsModal] = useState(false);

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

    const { data: archiveLeaders } = useQuery({
        queryKey: ['department-archive-leaders', userDeptId],
        queryFn: () => userDeptId ? departmentApi.getArchiveLeaders(userDeptId) : [],
        enabled: !!userDeptId
    });

    const isDepartmentHead = !!user?.roles?.includes('SuperAdmin') ||
        !!user?.permissions?.includes('archiving.delete-requests.approve') ||
        !!(userProfile && archiveLeaders && archiveLeaders.some((leader: any) => leader.userId === userProfile.id));

    const [pendingPage] = useState(1);

    const { data: pendingData, isLoading: isLoadingPending } = usePendingDeletionRequests(
        isDepartmentHead ? userDeptId : null,
        pendingPage,
        10
    );

    const handleViewDetails = (req: DeleteArchiveRequest) => {
        setSelectedRequest(req);
        setShowDetailsModal(true);
    };

    return (
        <AnimatedContainer className="p-6 md:p-8 flex flex-col gap-6 text-right" dir="rtl">

            <div className="flex flex-col md:flex-row md:justify-between md:items-center gap-4 border-b border-border pb-6">
                <div className="flex flex-col gap-1.5">
                    <h1 className="text-xl font-black text-primary">طلبات حذف الأرشيف</h1>
                    <p className="text-xs text-muted-foreground font-semibold">
                        مراجعة وإدارة طلبات حذف السجلات والمستندات المؤرشفة
                    </p>
                </div>
            </div>

            {isDepartmentHead && userProfile && (
                <div className="flex items-center gap-3 bg-amber-500/10 border border-amber-500/20 p-4 rounded-2xl text-amber-500 text-xs font-bold">
                    <ShieldAlert className="h-5 w-5 flex-shrink-0" />
                    <span>
                        أنت مسجل كمدير أرشيف لقسم ({userProfile.departmentName}). يمكنك الموافقة أو الرفض على طلبات حذف الأرشيف المقدمة من موظفي قسمك.
                    </span>
                </div>
            )}

            <div className="flex-1">
                {isDepartmentHead && (
                    <Card className="p-6 border-border rounded-3xl">
                        <div className="flex flex-col gap-4">
                            <span className="text-xs font-black text-muted-foreground">قائمة طلبات الحذف المعلقة للقسم</span>
                            <DeletionRequestsList
                                requests={pendingData?.items || []}
                                isLoading={isLoadingPending || isLoadingProfile}
                                onViewDetails={handleViewDetails}
                            />
                        </div>
                    </Card>
                )}
            </div>

            <DeletionRequestDetails
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
