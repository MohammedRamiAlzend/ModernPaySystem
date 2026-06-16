const DB_NAME = 'ModernPayArchiveCache';
const STORE_NAME = 'attachments';
const DB_VERSION = 1;

export interface CachedAttachment {
    fileId: string;
    blob: Blob;
    timestamp: number;
}

class AttachmentCache {
    private db: IDBDatabase | null = null;

    private getDB(): Promise<IDBDatabase> {
        if (this.db) return Promise.resolve(this.db);

        return new Promise((resolve, reject) => {
            const request = indexedDB.open(DB_NAME, DB_VERSION);

            request.onupgradeneeded = () => {
                const db = request.result;
                if (!db.objectStoreNames.contains(STORE_NAME)) {
                    db.createObjectStore(STORE_NAME, { keyPath: 'fileId' });
                }
            };

            request.onsuccess = () => {
                this.db = request.result;
                resolve(this.db);
            };

            request.onerror = () => {
                reject(request.error);
            };
        });
    }

    async get(fileId: string): Promise<CachedAttachment | null> {
        try {
            const db = await this.getDB();
            const record = await new Promise<CachedAttachment | null>((resolve, reject) => {
                const transaction = db.transaction(STORE_NAME, 'readonly');
                const store = transaction.objectStore(STORE_NAME);
                const request = store.get(fileId);

                request.onsuccess = () => {
                    resolve(request.result || null);
                };

                request.onerror = () => {
                    reject(request.error);
                };
            });

            if (record) {
                // Update timestamp to mark as recently accessed (LRU)
                record.timestamp = Date.now();
                await new Promise<void>((resolve, reject) => {
                    const transaction = db.transaction(STORE_NAME, 'readwrite');
                    const store = transaction.objectStore(STORE_NAME);
                    const request = store.put(record);

                    request.onsuccess = () => resolve();
                    request.onerror = () => reject(request.error);
                });
            }

            return record;
        } catch (e) {
            console.error('Failed to get from IndexedDB cache', e);
            return null;
        }
    }

    async set(fileId: string, blob: Blob): Promise<void> {
        try {
            const db = await this.getDB();
            const record: CachedAttachment = {
                fileId,
                blob,
                timestamp: Date.now()
            };

            await new Promise<void>((resolve, reject) => {
                const transaction = db.transaction(STORE_NAME, 'readwrite');
                const store = transaction.objectStore(STORE_NAME);
                const request = store.put(record);

                request.onsuccess = () => resolve();
                request.onerror = () => reject(request.error);
            });

            // Enforce capacity of 30 items
            await this.enforceCapacity(db);
        } catch (e) {
            console.error('Failed to set in IndexedDB cache', e);
        }
    }

    private async enforceCapacity(db: IDBDatabase): Promise<void> {
        const MAX_ITEMS = 30;
        try {
            const allRecords = await new Promise<CachedAttachment[]>((resolve, reject) => {
                const transaction = db.transaction(STORE_NAME, 'readonly');
                const store = transaction.objectStore(STORE_NAME);
                const request = store.getAll();

                request.onsuccess = () => resolve(request.result || []);
                request.onerror = () => reject(request.error);
            });

            if (allRecords.length > MAX_ITEMS) {
                // Sort by timestamp ascending (oldest first)
                allRecords.sort((a, b) => a.timestamp - b.timestamp);
                
                // Evict oldest items
                const itemsToDelete = allRecords.slice(0, allRecords.length - MAX_ITEMS);
                
                const transaction = db.transaction(STORE_NAME, 'readwrite');
                const store = transaction.objectStore(STORE_NAME);
                
                for (const item of itemsToDelete) {
                    store.delete(item.fileId);
                }

                await new Promise<void>((resolve, reject) => {
                    transaction.oncomplete = () => resolve();
                    transaction.onerror = () => reject(transaction.error);
                });
            }
        } catch (e) {
            console.error('Failed to enforce IndexedDB cache capacity limit', e);
        }
    }

    async delete(fileId: string): Promise<void> {
        try {
            const db = await this.getDB();
            return new Promise((resolve, reject) => {
                const transaction = db.transaction(STORE_NAME, 'readwrite');
                const store = transaction.objectStore(STORE_NAME);
                const request = store.delete(fileId);

                request.onsuccess = () => resolve();
                request.onerror = () => reject(request.error);
            });
        } catch (e) {
            console.error('Failed to delete from IndexedDB cache', e);
        }
    }

    async clear(): Promise<void> {
        try {
            const db = await this.getDB();
            return new Promise((resolve, reject) => {
                const transaction = db.transaction(STORE_NAME, 'readwrite');
                const store = transaction.objectStore(STORE_NAME);
                const request = store.clear();

                request.onsuccess = () => resolve();
                request.onerror = () => reject(request.error);
            });
        } catch (e) {
            console.error('Failed to clear IndexedDB cache', e);
        }
    }
}

export const attachmentCache = new AttachmentCache();
