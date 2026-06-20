import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import type { UploadSession, FileUploadStatus, SessionStatus, FileUploadItem } from './types';
import { removeSessionFiles } from './fileStore';

interface UploadManagerState {
    /** جميع جلسات الرفع النشطة */
    sessions: Record<string, UploadSession>;
    /** هل اللوحة العائمة مفتوحة */
    isPanelOpen: boolean;
    /** هل اللوحة مصغرة */
    isPanelMinimized: boolean;

    // ─── إجراءات إدارة الجلسات ───
    createSession: (session: UploadSession) => void;
    removeSession: (sessionId: string) => void;
    clearCompletedSessions: () => void;

    // ─── إجراءات تحديث الملفات ───
    updateFileProgress: (sessionId: string, fileId: string, progress: number) => void;
    updateFileStatus: (sessionId: string, fileId: string, status: FileUploadStatus, errorMessage?: string) => void;
    retryFile: (sessionId: string, fileId: string) => void;
    retryAllFailed: (sessionId: string) => void;
    requeueFile: (sessionId: string, fileId: string) => void;
    removeFileFromSession: (sessionId: string, fileId: string) => void;

    // ─── إجراءات واجهة المستخدم ───
    togglePanel: () => void;
    toggleMinimize: () => void;
    openPanel: () => void;
}

/** حساب حالة الجلسة الإجمالية بناءً على حالة ملفاتها */
const computeSessionStatus = (files: FileUploadItem[]): SessionStatus => {
    const allSuccess = files.every((f) => f.status === 'success');
    if (allSuccess) return 'completed';

    const hasFailed = files.some((f) => f.status === 'failed');
    const hasUploading = files.some((f) => f.status === 'uploading');
    const hasPending = files.some((f) => f.status === 'pending');

    if (hasUploading || hasPending) return 'uploading';
    if (hasFailed) return 'partial';
    return 'completed';
};

/** تحديث ملف معين داخل جلسة مع إعادة حساب حالة الجلسة */
const updateFileInSession = (
    session: UploadSession,
    fileId: string,
    updater: (file: FileUploadItem) => FileUploadItem
): UploadSession => {
    const updatedFiles = session.files.map((f) =>
        f.id === fileId ? updater(f) : f
    );
    return {
        ...session,
        files: updatedFiles,
        status: computeSessionStatus(updatedFiles),
    };
};

export const useUploadStore = create<UploadManagerState>()(
    persist(
        (set) => ({
            sessions: {},
            isPanelOpen: false,
            isPanelMinimized: false,

            createSession: (session) =>
                set((state) => ({
                    sessions: { ...state.sessions, [session.id]: session },
                    isPanelOpen: true,
                    isPanelMinimized: false,
                })),

            removeSession: (sessionId) =>
                set((state) => {
                    const session = state.sessions[sessionId];
                    if (session) {
                        removeSessionFiles(session.files.map((f) => f.id));
                    }
                    const { [sessionId]: _, ...rest } = state.sessions;
                    return { sessions: rest };
                }),

            clearCompletedSessions: () =>
                set((state) => {
                    const remaining: Record<string, UploadSession> = {};
                    Object.entries(state.sessions).forEach(([id, session]) => {
                        if (session.status !== 'completed') {
                            remaining[id] = session;
                        } else {
                            removeSessionFiles(session.files.map((f) => f.id));
                        }
                    });
                    return { sessions: remaining };
                }),

            updateFileProgress: (sessionId, fileId, progress) =>
                set((state) => {
                    const session = state.sessions[sessionId];
                    if (!session) return state;
                    return {
                        sessions: {
                            ...state.sessions,
                            [sessionId]: updateFileInSession(session, fileId, (f) => ({
                                ...f,
                                progress,
                                status: 'uploading',
                            })),
                        },
                    };
                }),

            updateFileStatus: (sessionId, fileId, status, errorMessage) =>
                set((state) => {
                    const session = state.sessions[sessionId];
                    if (!session) return state;
                    return {
                        sessions: {
                            ...state.sessions,
                            [sessionId]: updateFileInSession(session, fileId, (f) => ({
                                ...f,
                                status,
                                progress: status === 'success' ? 100 : f.progress,
                                errorMessage: status === 'failed' ? errorMessage : undefined,
                            })),
                        },
                    };
                }),

            retryFile: (sessionId, fileId) =>
                set((state) => {
                    const session = state.sessions[sessionId];
                    if (!session) return state;
                    return {
                        sessions: {
                            ...state.sessions,
                            [sessionId]: updateFileInSession(session, fileId, (f) => ({
                                ...f,
                                status: 'pending',
                                progress: 0,
                                errorMessage: undefined,
                                retryCount: f.retryCount + 1,
                            })),
                        },
                    };
                }),

            retryAllFailed: (sessionId) =>
                set((state) => {
                    const session = state.sessions[sessionId];
                    if (!session) return state;
                    const updatedFiles = session.files.map((f) =>
                        f.status === 'failed'
                            ? { ...f, status: 'pending' as FileUploadStatus, progress: 0, errorMessage: undefined, retryCount: f.retryCount + 1 }
                            : f
                    );
                    return {
                        sessions: {
                            ...state.sessions,
                            [sessionId]: {
                                ...session,
                                files: updatedFiles,
                                status: computeSessionStatus(updatedFiles),
                            },
                        },
                    };
                }),

            requeueFile: (sessionId, fileId) =>
                set((state) => {
                    const session = state.sessions[sessionId];
                    if (!session) return state;

                    const targetFile = session.files.find((f) => f.id === fileId);
                    if (!targetFile) return state;

                    // Remove file from current position and reset progress
                    const remainingFiles = session.files.filter((f) => f.id !== fileId);
                    const resetFile: FileUploadItem = {
                        ...targetFile,
                        status: 'pending',
                        progress: 0,
                        errorMessage: undefined,
                    };

                    const updatedFiles = [...remainingFiles, resetFile];

                    return {
                        sessions: {
                            ...state.sessions,
                            [sessionId]: {
                                ...session,
                                files: updatedFiles,
                                status: computeSessionStatus(updatedFiles),
                            },
                        },
                    };
                }),

            removeFileFromSession: (sessionId, fileId) =>
                set((state) => {
                    const session = state.sessions[sessionId];
                    if (!session) return state;

                    const updatedFiles = session.files.filter((f) => f.id !== fileId);
                    removeSessionFiles([fileId]);

                    if (updatedFiles.length === 0) {
                        const { [sessionId]: _, ...rest } = state.sessions;
                        return { sessions: rest };
                    }

                    return {
                        sessions: {
                            ...state.sessions,
                            [sessionId]: {
                                ...session,
                                files: updatedFiles,
                                status: computeSessionStatus(updatedFiles),
                            },
                        },
                    };
                }),

            togglePanel: () => set((state) => ({ isPanelOpen: !state.isPanelOpen })),
            toggleMinimize: () => set((state) => ({ isPanelMinimized: !state.isPanelMinimized })),
            openPanel: () => set({ isPanelOpen: true, isPanelMinimized: false }),
        }),
        {
            name: 'upload-manager-store',
        }
    )
);
