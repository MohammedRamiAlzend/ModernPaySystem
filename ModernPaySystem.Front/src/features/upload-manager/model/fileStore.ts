/**
 * تخزين كائنات File الفعلية خارج Zustand.
 * Zustand يخزن metadata فقط (اسم، حجم، حالة) بينما كائنات File
 * غير قابلة للتسلسل ويجب تخزينها في Map منفصل.
 */
const fileStore = new Map<string, File>();

/** تخزين ملف جديد */
export const storeFile = (fileItemId: string, file: File): void => {
    fileStore.set(fileItemId, file);
};

/** استرجاع ملف حسب معرفه */
export const getFile = (fileItemId: string): File | undefined => {
    return fileStore.get(fileItemId);
};

/** حذف ملف من المخزن */
export const removeFile = (fileItemId: string): void => {
    fileStore.delete(fileItemId);
};

/** تخزين مجموعة ملفات دفعة واحدة */
export const storeFiles = (entries: { id: string; file: File }[]): void => {
    entries.forEach(({ id, file }) => fileStore.set(id, file));
};

/** حذف جميع ملفات جلسة معينة */
export const removeSessionFiles = (fileItemIds: string[]): void => {
    fileItemIds.forEach((id) => fileStore.delete(id));
};

/** مسح جميع الملفات المخزنة */
export const clearFileStore = (): void => {
    fileStore.clear();
};
