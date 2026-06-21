// حالة ملف واحد في قائمة الرفع
export type FileUploadStatus = 'pending' | 'uploading' | 'success' | 'failed';

export interface FileUploadItem {
    /** معرف فريد (UUID) */
    id: string;
    /** اسم الملف الأصلي */
    fileName: string;
    /** حجم الملف بالبايت */
    fileSize: number;
    /** حالة الرفع الحالية */
    status: FileUploadStatus;
    /** نسبة التقدم (0-100) */
    progress: number;
    /** رسالة الخطأ عند الفشل */
    errorMessage?: string;
    /** عدد مرات إعادة المحاولة */
    retryCount: number;
}

// حالة جلسة رفع كاملة
export type SessionStatus = 'uploading' | 'completed' | 'partial' | 'paused';

export interface UploadSession {
    /** معرف الجلسة (UUID) */
    id: string;
    /** معرف المستخدم الذي أنشأ الجلسة */
    userId: string;
    /** معرف المستند الأرشيفي */
    recordId: string;
    /** عنوان/رقم المستند (للعرض في اللوحة) */
    recordTitle: string;
    /** معرف المجلد */
    folderId: string;
    /** قائمة الملفات المراد رفعها */
    files: FileUploadItem[];
    /** وقت إنشاء الجلسة */
    createdAt: string;
    /** حالة الجلسة الإجمالية */
    status: SessionStatus;
}
