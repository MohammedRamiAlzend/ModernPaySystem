import JSZip from 'jszip';
import { jsPDF } from 'jspdf';

export interface ZipImage {
    name: string;
    url: string;
}

export interface ZipContent {
    images: ZipImage[];
    isAllImages: boolean;
    totalFiles: number;
}

const IMAGE_EXTENSIONS = ['.jpg', '.jpeg', '.png', '.gif', '.webp', '.bmp'];

/**
 * Parses a ZIP blob and extracts all images as Object URLs
 */
export const extractImagesFromZip = async (zipBlob: Blob): Promise<ZipContent> => {
    const zip = new JSZip();
    const contents = await zip.loadAsync(zipBlob);
    const images: ZipImage[] = [];

    // Filter out directories and get only real files
    const fileNames = Object.keys(contents.files).filter(name => !contents.files[name].dir);
    let imageCount = 0;

    const filePromises = fileNames.map(async (filename) => {
        const file = contents.files[filename];
        const lowerFilename = filename.toLowerCase();

        if (IMAGE_EXTENSIONS.some(ext => lowerFilename.endsWith(ext))) {
            imageCount++;
            const blob = await file.async('blob');
            const url = URL.createObjectURL(blob);
            images.push({
                name: filename,
                url
            });
        }
    });

    await Promise.all(filePromises);

    return {
        images,
        isAllImages: imageCount === fileNames.length && fileNames.length > 0,
        totalFiles: fileNames.length
    };
};

/**
 * Converts an array of image URLs to a single PDF using the scaling logic from the previous project
 */
export const imagesToPdf = async (images: ZipImage[], title: string = 'Attachments'): Promise<void> => {
    if (images.length === 0) return;

    const pdf = new jsPDF();
    const pageWidth = pdf.internal.pageSize.getWidth();
    const pageHeight = pdf.internal.pageSize.getHeight();
    let pagesAdded = 0;

    for (let i = 0; i < images.length; i++) {
        const imgItem = images[i];

        try {
            // Fetch the blob from the Object URL to convert it to DataURL (as in previous project)
            const blob = await fetch(imgItem.url).then(r => r.blob());

            const dataUrl = await new Promise<string>((resolve, reject) => {
                const reader = new FileReader();
                reader.onload = (e) => resolve(e.target?.result as string);
                reader.onerror = reject;
                reader.readAsDataURL(blob);
            });

            if (!dataUrl || !dataUrl.startsWith('data:')) continue;

            if (pagesAdded > 0) pdf.addPage();

            // Load image to get original dimensions
            const img = new Image();
            await new Promise((resolve, reject) => {
                img.onload = resolve;
                img.onerror = reject;
                img.src = dataUrl;
            });

            const imgWidth = img.width;
            const imgHeight = img.height;

            // Calculate ratio to fit image into page boundaries
            const ratio = Math.min(pageWidth / imgWidth, pageHeight / imgHeight);
            const finalWidth = imgWidth * ratio;
            const finalHeight = imgHeight * ratio;

            // Calculate centering offsets
            const x = (pageWidth - finalWidth) / 2;
            const y = (pageHeight - finalHeight) / 2;

            // Add the image to the PDF
            pdf.addImage(dataUrl, 'JPEG', x, y, finalWidth, finalHeight, undefined, 'FAST');
            pagesAdded++;
        } catch (err) {
            console.error(`Failed to process image ${imgItem.name}:`, err);
        }
    }

    if (pagesAdded > 0) {
        pdf.save(`${title}.pdf`);
    }
};

/**
 * Revokes object URLs to prevent memory leaks
 */
export const revokeZipImages = (images: ZipImage[]) => {
    images.forEach(img => URL.revokeObjectURL(img.url));
};
