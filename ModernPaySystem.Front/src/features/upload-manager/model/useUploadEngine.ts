import { useEffect, useRef, useCallback } from 'react';
import { useUploadStore } from './uploadStore';
import { getFile, removeFile } from './fileStore';
import { archivingService } from '@/features/archiving/api/archivingService';
import type { UploadSession } from './types';

/** الحد الأقصى لعدد الجلسات التي ترفع بالتوازي */
const MAX_CONCURRENT_SESSIONS = 2;

/**
 * محرك الرفع — يعمل في الخلفية على مستوى التطبيق.
 * يراقب Zustand Store ويرفع الملفات تلقائياً ملف بملف لكل جلسة.
 * يُستدعى مرة واحدة في Layout الرئيسي.
 */
export function useUploadEngine() {
    /** تتبع الجلسات التي يتم الرفع منها حالياً (لمنع التكرار) */
    const activeSessionIds = useRef<Set<string>>(new Set());

    const sessions = useUploadStore((s) => s.sessions);
    const updateFileProgress = useUploadStore((s) => s.updateFileProgress);
    const updateFileStatus = useUploadStore((s) => s.updateFileStatus);

    /** رفع ملف واحد لجلسة معينة */
    const uploadSingleFile = useCallback(
        async (session: UploadSession, fileId: string) => {
            const file = getFile(fileId);
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
                removeFile(fileId);
            } catch (error: any) {
                const message =
                    error?.response?.data?.message ||
                    error?.message ||
                    'فشل رفع الملف';
                updateFileStatus(session.id, fileId, 'failed', message);
            }
        },
        [updateFileProgress, updateFileStatus]
    );

    /** معالجة جلسة واحدة — يرفع ملفاتها واحداً تلو الآخر */
    const processSession = useCallback(
        async (session: UploadSession) => {
            if (activeSessionIds.current.has(session.id)) return;
            activeSessionIds.current.add(session.id);

            try {
                // جمع الملفات المعلقة (pending) بالترتيب
                const pendingFiles = session.files.filter(
                    (f) => f.status === 'pending'
                );

                for (const fileItem of pendingFiles) {
                    // إعادة قراءة الحالة من الـ store لأنها قد تتغير (مثل retry)
                    const freshSession =
                        useUploadStore.getState().sessions[session.id];
                    if (!freshSession) break; // الجلسة حُذفت

                    const freshFile = freshSession.files.find(
                        (f) => f.id === fileItem.id
                    );
                    if (!freshFile || freshFile.status !== 'pending') continue;

                    await uploadSingleFile(freshSession, fileItem.id);
                }
            } finally {
                activeSessionIds.current.delete(session.id);
            }
        },
        [uploadSingleFile]
    );

    /** المراقب الرئيسي — يفحص الجلسات ويبدأ الرفع */
    useEffect(() => {
        const sessionsArray = Object.values(sessions);

        // الجلسات التي تحتوي على ملفات pending وليست نشطة حالياً
        const sessionsToProcess = sessionsArray.filter(
            (s) =>
                s.files.some((f) => f.status === 'pending') &&
                !activeSessionIds.current.has(s.id)
        );

        // تحديد عدد الجلسات التي يمكن تشغيلها بالتوازي
        const availableSlots =
            MAX_CONCURRENT_SESSIONS - activeSessionIds.current.size;

        if (availableSlots <= 0 || sessionsToProcess.length === 0) return;

        const toStart = sessionsToProcess.slice(0, availableSlots);
        toStart.forEach((session) => {
            processSession(session);
        });
    }, [sessions, processSession]);
}
