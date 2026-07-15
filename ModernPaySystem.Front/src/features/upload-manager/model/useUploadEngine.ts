import { useEffect, useRef, useCallback } from 'react';
import { useUploadStore } from './uploadStore';
import { getFile, removeFile } from './fileStore';
import { archivingService } from '@/features/archiving/api/archivingService';
import { useAuthStore } from '@/app/store/authStore';
import { extractErrorMessage } from '@/shared/lib/error-utils';
import type { UploadSession } from './types';

/** الحد الأقصى لعدد الجلسات التي ترفع بالتوازي */
const MAX_CONCURRENT_SESSIONS = 2;

/**
 * محرك الرفع — يعمل في الخلفية على مستوى التطبيق.
 * يراقب Zustand Store ويرفع الملفات تلقائياً ملف بملف لكل جلسة.
 * يُستدعى مرة واحدة في Layout الرئيسي.
 * يعالج فقط الجلسات التابعة للمستخدم المسجّل حالياً.
 */
export function useUploadEngine() {
    /** تتبع الجلسات التي يتم الرفع منها حالياً (لمنع التكرار) */
    const activeSessionIds = useRef<Set<string>>(new Set());

    const sessions = useUploadStore((s) => s.sessions);
    const updateFileProgress = useUploadStore((s) => s.updateFileProgress);
    const updateFileStatus = useUploadStore((s) => s.updateFileStatus);
    const currentUserId = useAuthStore((s) => s.user?.id);

    /** رفع ملف واحد لجلسة معينة */
    const uploadSingleFile = useCallback(
        async (session: UploadSession, fileId: string) => {
            const file = await getFile(fileId);
            if (!file) {
                updateFileStatus(session.id, fileId, 'failed', 'الملف غير موجود في المخزن المؤقت');
                return;
            }

            try {
                updateFileStatus(session.id, fileId, 'uploading');

                await archivingService.addFilesToArchiveRecord(
                    session.recordId,
                    [file],
                    (progressEvent) => {
                        if (progressEvent.total) {
                            const percent = Math.round(
                                (progressEvent.loaded * 100) / progressEvent.total
                            );
                            updateFileProgress(session.id, fileId, percent);
                        }
                    }
                );

                updateFileStatus(session.id, fileId, 'success');
                await removeFile(fileId);
            } catch (error: any) {
                const message = extractErrorMessage(error, 'فشل رفع الملف');
                updateFileStatus(session.id, fileId, 'failed', message);
            }
        },
        [updateFileProgress, updateFileStatus]
    );

    /** معالجة جلسة واحدة — يرفع ملفاتها واحداً تلو الآخر */
    const processSession = useCallback(
        async (session: UploadSession) => {
            if (activeSessionIds.current.has(session.id)) {
                return;
            }
            activeSessionIds.current.add(session.id);

            try {
                const pendingFiles = session.files.filter(
                    (f) => f.status === 'pending'
                );

                for (const fileItem of pendingFiles) {
                    const freshSession =
                        useUploadStore.getState().sessions[session.id];
                    if (!freshSession) {
                        break;
                    }

                    const freshFile = freshSession.files.find(
                        (f) => f.id === fileItem.id
                    );
                    if (!freshFile || freshFile.status !== 'pending') {
                        continue;
                    }

                    await uploadSingleFile(freshSession, fileItem.id);
                }
            } finally {
                activeSessionIds.current.delete(session.id);
            }
        },
        [uploadSingleFile]
    );

    // عند تحميل التطبيق، التحقق من الملفات التي علقت في حالة 'uploading'
    const hasCleanedUp = useRef(false);
    useEffect(() => {
        if (hasCleanedUp.current || !currentUserId) return;

        // فلترة الجلسات التابعة للمستخدم الحالي فقط
        const sessionsArray = Object.values(sessions).filter(
            (s) => s.userId === currentUserId
        );
        if (sessionsArray.length === 0) {
            return;
        }

        const runCleanup = async () => {
            const store = useUploadStore.getState();
            
            for (const session of sessionsArray) {
                const uploadingFiles = session.files.filter(f => f.status === 'uploading');
                if (uploadingFiles.length === 0) continue;

                try {
                    const record = await archivingService.getArchiveRecordById(session.recordId);
                    
                    for (const file of uploadingFiles) {
                        const fileExistsOnServer = record.physicalFiles?.some(
                            (pf) => pf.fileName === file.fileName
                        );

                        if (fileExistsOnServer) {
                            store.updateFileStatus(session.id, file.id, 'success');
                            await removeFile(file.id);
                        } else {
                            store.requeueFile(session.id, file.id);
                        }
                    }
                } catch {
                    uploadingFiles.forEach(file => {
                        store.requeueFile(session.id, file.id);
                    });
                }
            }
        };

        runCleanup();
        hasCleanedUp.current = true;
    }, [sessions, currentUserId]);

    /** المراقب الرئيسي — يفحص الجلسات ويبدأ الرفع */
    useEffect(() => {
        if (!currentUserId) return;

        const sessionsArray = Object.values(sessions);

        // فقط الجلسات التابعة للمستخدم الحالي والتي تحتوي على ملفات pending
        const sessionsToProcess = sessionsArray.filter(
            (s) =>
                s.userId === currentUserId &&
                s.files.some((f) => f.status === 'pending') &&
                !activeSessionIds.current.has(s.id)
        );

        const availableSlots =
            MAX_CONCURRENT_SESSIONS - activeSessionIds.current.size;

        if (availableSlots <= 0 || sessionsToProcess.length === 0) {
            return;
        }

        const toStart = sessionsToProcess.slice(0, availableSlots);
        toStart.forEach((session) => {
            processSession(session);
        });
    }, [sessions, processSession, currentUserId]);
}

