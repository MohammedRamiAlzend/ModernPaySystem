import { ArchiveRecordFormInputValue, PhysicalFile } from '@/features/archiving/model/types';

export enum EditArchiveRequestStatus {
    Pending = 0,
    Approved = 1,
    Rejected = 2
}

export interface EditArchiveRequest {
    id: string;
    departmentId: string;
    archiveRecordId: string;
    status: EditArchiveRequestStatus;
    requesterId: string;
    requesterName?: string;
    approverId?: string;
    approverName?: string;
    justification: string;
    requestedChanges: ArchiveRecordFormInputValue[];
    requestedRecordName?: string;
    originalSnapshotJson?: string;
    rejectionReason?: string;
    approvalNotes?: string;
    approvedByUserId?: string;
    approvedAt?: string;
    rejectedByUserId?: string;
    rejectedAt?: string;
    createdByUserId?: string;
    createdAt?: string;
    attachedFiles?: PhysicalFile[];
    fileIdsToDelete?: string[];
}

export interface CreateEditArchiveRequestDto {
    archiveRecordId: string;
    justification: string;
    requestedRecordName?: string;
    requestedChanges: ArchiveRecordFormInputValue[];
    files: File[];
    fileIdsToDelete?: string[];
}

export interface EditArchiveRequestDecisionDto {
    notes?: string;
}

export interface EditArchiveRequestRejectDto {
    reason: string;
}
