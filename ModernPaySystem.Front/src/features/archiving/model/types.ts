export interface Folder {
    id: string;
    name: string;
    level: number;
    parentId: string | null;
    folderDtos: Folder[];
    createdByUserId?: string | null;
    createdAt?: string | null;
    updatedByUserId?: string | null;
    updatedAt?: string | null;
    canManagePermissions?: boolean;
}

export interface PhysicalFile {
    id: string;
    archiveRecordId: string;
    fileName: string;
    fileExtension: string;
    storagePath: string;
    fileSize: number;
    contentType: string;
    isDeleted: boolean;
    isQrPage?: boolean;
    deletedAt?: string | null;
    createdByUserId?: string | null;
    createdAt?: string | null;
}

export interface ArchiveRecordFormInputValue {
    key: string;
    value: string | null;
}

export interface ArchiveRecordTemplateValues {
    id: string;
    archiveRecordId: string;
    archiveFormTemplateId: string;
    archiveRecordFormInputValues: ArchiveRecordFormInputValue[];
}

export interface ArchiveRecord {
    id: string;
    folderId: string;
    formId: string | null;
    archivalNumber: string;
    archiveRecordTemplateValues?: ArchiveRecordTemplateValues | null;
    physicalFiles: PhysicalFile[];
    createdByUserId?: string | null;
    createdAt?: string | null;
    updatedByUserId?: string | null;
    updatedAt?: string | null;
}

export interface DynamicFormTemplate {
    id: string;
    templateFormName: string;
    contentAsJson: string;
    createdByUserId?: string | null;
    createdAt?: string | null;
}

export interface InitialFolderPermissionDto {
    userId: string;
    accessLevel: number;
}

export interface CreateFolderDto {
    name: string;
    parentId: string | null;
    initialPermissions?: InitialFolderPermissionDto[];
}

export interface CreateArchiveRecordDto {
    id?: string;
    folderId: string;
    formId: string | null;
    archivalNumber: string;
    files: File[];
    content: ArchiveRecordFormInputValue[];
}

export interface CreateDynamicFormTemplateDto {
    templateFormName: string;
    contentAsJson: string;
}

export interface UpdateDynamicFormTemplateDto {
    templateFormName: string;
    contentAsJson: string;
}

// Semantic Search
export interface SemanticSearchRequest {
    query: string;
    topK: number;
    minScore: number;
    sourceType: number | null;
    archiveRecordId: string | null;
    folderId: string | null;
}

export interface FolderPermissionDto {
    id: string;
    folderId: string;
    userId: string;
    userName?: string | null;
    accessLevel: number;
    isInherited: boolean;
    createdByUserId?: string | null;
    createdAt?: string | null;
    updatedByUserId?: string | null;
    updatedAt?: string | null;
}

export interface CreateFolderPermissionDto {
    folderId: string;
    userId: string;
    accessLevel: number;
    isInherited: boolean;
}

export interface SemanticSearchResultItem {
    documentId: string;
    chunkId: string;
    sourceType: number;
    physicalFileId: string;
    archiveRecordId: string;
    archiveRecordNumber: string;
    fileName: string;
    chunkIndex: number;
    content: string;
    score: number;
}
