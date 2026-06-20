/**
 * تخزين كائنات File الفعلية خارج Zustand في IndexedDB.
 * Zustand يخزن metadata فقط (اسم، حجم، حالة) بينما كائنات File
 * غير قابلة للتسلسل ويتم حفظها في قاعدة بيانات IndexedDB للاستمرار بعد تحديث الصفحة.
 */
const DB_NAME = 'UploadManagerFiles';
const STORE_NAME = 'files';
const DB_VERSION = 1;

let dbPromise: Promise<IDBDatabase> | null = null;

const getDB = (): Promise<IDBDatabase> => {
    if (dbPromise) return dbPromise;

    dbPromise = new Promise((resolve, reject) => {
        const request = indexedDB.open(DB_NAME, DB_VERSION);

        request.onupgradeneeded = () => {
            const db = request.result;
            if (!db.objectStoreNames.contains(STORE_NAME)) {
                db.createObjectStore(STORE_NAME);
            }
        };

        request.onsuccess = () => {
            resolve(request.result);
        };

        request.onerror = () => {
            reject(request.error);
        };
    });

    return dbPromise;
};

/** تخزين ملف جديد */
export const storeFile = async (fileItemId: string, file: File): Promise<void> => {
    const db = await getDB();
    return new Promise((resolve, reject) => {
        const transaction = db.transaction(STORE_NAME, 'readwrite');
        const store = transaction.objectStore(STORE_NAME);
        const request = store.put(file, fileItemId);

        request.onsuccess = () => resolve();
        request.onerror = () => reject(request.error);
    });
};

/** استرجاع ملف حسب معرفه */
export const getFile = async (fileItemId: string): Promise<File | undefined> => {
    const db = await getDB();
    return new Promise((resolve, reject) => {
        const transaction = db.transaction(STORE_NAME, 'readonly');
        const store = transaction.objectStore(STORE_NAME);
        const request = store.get(fileItemId);

        request.onsuccess = () => resolve(request.result);
        request.onerror = () => reject(request.error);
    });
};

/** حذف ملف من المخزن */
export const removeFile = async (fileItemId: string): Promise<void> => {
    const db = await getDB();
    return new Promise((resolve, reject) => {
        const transaction = db.transaction(STORE_NAME, 'readwrite');
        const store = transaction.objectStore(STORE_NAME);
        const request = store.delete(fileItemId);

        request.onsuccess = () => resolve();
        request.onerror = () => reject(request.error);
    });
};

/** تخزين مجموعة ملفات دفعة واحدة */
export const storeFiles = async (entries: { id: string; file: File }[]): Promise<void> => {
    const db = await getDB();
    return new Promise((resolve, reject) => {
        const transaction = db.transaction(STORE_NAME, 'readwrite');
        const store = transaction.objectStore(STORE_NAME);

        entries.forEach(({ id, file }) => {
            store.put(file, id);
        });

        transaction.oncomplete = () => resolve();
        transaction.onerror = () => reject(transaction.error);
    });
};

/** حذف جميع ملفات جلسة معينة */
export const removeSessionFiles = async (fileItemIds: string[]): Promise<void> => {
    const db = await getDB();
    return new Promise((resolve, reject) => {
        const transaction = db.transaction(STORE_NAME, 'readwrite');
        const store = transaction.objectStore(STORE_NAME);

        fileItemIds.forEach((id) => {
            store.delete(id);
        });

        transaction.oncomplete = () => resolve();
        transaction.onerror = () => reject(transaction.error);
    });
};

/** مسح جميع الملفات المخزنة */
export const clearFileStore = async (): Promise<void> => {
    const db = await getDB();
    return new Promise((resolve, reject) => {
        const transaction = db.transaction(STORE_NAME, 'readwrite');
        const store = transaction.objectStore(STORE_NAME);
        const request = store.clear();

        request.onsuccess = () => resolve();
        request.onerror = () => reject(request.error);
    });
};
