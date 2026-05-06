
import { useState, useEffect, useRef } from 'react';
import { extractImagesFromZip, revokeZipImages, type ZipImage } from '@/shared/utils/zip-handler';

/**
 * Hook to handle fetching and extracting images from a ZIP blob
 * @param blobFetcher Function that returns a Promise resolving to a Blob
 * @param dependencies Array of dependencies that trigger a refetch
 */
export const useAttachments = (
    blobFetcher: (() => Promise<Blob>) | null,
    dependencies: any[] = []
) => {
    const [zipImages, setZipImages] = useState<ZipImage[]>([]);
    const [isLoading, setIsLoading] = useState(false);
    const [isAllImages, setIsAllImages] = useState(false);
    const [totalFiles, setTotalFiles] = useState(0);
    
    const zipImagesRef = useRef<ZipImage[]>([]);

    // Reset state if blobFetcher becomes null - sync during render
    if (!blobFetcher && (zipImages.length > 0 || totalFiles > 0)) {
        setZipImages([]);
        setIsAllImages(false);
        setTotalFiles(0);
    }

    useEffect(() => {
        if (!blobFetcher) return;

        const loadAttachments = async () => {
            setIsLoading(true);
            try {
                const blob = await blobFetcher();
                
                // Safety check: if blob is JSON, it's likely an error message from the server
                if (blob.type === 'application/json') {
                    const text = await blob.text();
                    const errorData = JSON.parse(text);
                    console.error('Server returned an error instead of a ZIP:', errorData);
                    throw new Error(errorData.message || 'فشل تحميل الملفات من الخادم');
                }

                const data = await extractImagesFromZip(blob);
                setZipImages(data.images);
                zipImagesRef.current = data.images;
                setIsAllImages(data.isAllImages);
                setTotalFiles(data.totalFiles);
            } catch (error: any) {
                console.error('Failed to load attachments from ZIP', error);
            } finally {
                setIsLoading(false);
            }
        };


        loadAttachments();

        return () => {
            if (zipImagesRef.current.length > 0) {
                revokeZipImages(zipImagesRef.current);
                zipImagesRef.current = [];
                setZipImages([]);
            }
        };
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [...dependencies]);

    return {
        zipImages,
        isLoading,
        isAllImages,
        totalFiles
    };
};
