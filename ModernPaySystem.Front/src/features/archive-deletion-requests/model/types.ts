export enum ArchiveDeletionTargetType {
    Folder = 0,
    Record = 1
}

export enum DeleteArchiveRequestStatus {
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Executed = 3
}

export interface ArchiveDeletionTargetSnapshotDto {
    targetType: ArchiveDeletionTargetType;
    targetId: string;
    departmentId: string;
    displayName: string;
    parentPath?: string;
    childFolderCount: number;
    descendantFolderCount: number;
    recordCount: number;
    fileCount: number;
    metadataJson?: string;
}

export interface ArchiveDeletionDependencyDto {
    kind: string;
    id: string;
    displayName?: string;
    details?: string;
}

export interface DeleteArchiveRequest {
    id: string;
    departmentId: string;
    targetType: ArchiveDeletionTargetType;
    targetId: string;
    status: DeleteArchiveRequestStatus;
    requesterId: string;
    requesterName?: string;
    approverId: string;
    approverName?: string;
    justification: string;
    rejectionReason?: string;
    approvalNotes?: string;
    targetDisplayName?: string;
    targetSnapshot?: ArchiveDeletionTargetSnapshotDto;
    dependencies: ArchiveDeletionDependencyDto[];
    activitySnapshotJson?: string;
    approvedByUserId?: string;
    approvedAt?: string;
    executedByUserId?: string;
    executedAt?: string;
    rejectedByUserId?: string;
    rejectedAt?: string;
    requesterNotificationMessage?: string;
    requesterNotifiedAt?: string;
    createdByUserId?: string;
    createdAt?: string;
}

export interface CreateDeleteArchiveRequestDto {
    targetType: ArchiveDeletionTargetType;
    targetId: string;
    justification: string;
}

export interface DeleteArchiveRequestDecisionDto {
    notes?: string;
}

export interface DeleteArchiveRequestRejectDto {
    reason: string;
}
