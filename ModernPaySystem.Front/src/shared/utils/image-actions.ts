/**
 * Image utility functions for printing and downloading
 */

/**
 * Opens a new window to print a single image
 * @param url The image URL (can be a blob URL or remote URL)
 * @param title Optional title for the print window
 */
export const printImage = (url: string, title: string = 'طباعة صورة') => {
    const printWindow = window.open('', '_blank');
    if (printWindow) {
        printWindow.document.write(`
            <html>
                <head>
                    <title>${title}</title>
                    <style>
                        body { margin: 0; display: flex; justify-content: center; align-items: center; min-height: 100vh; background: white; }
                        img { max-width: 100%; max-height: 100%; object-fit: contain; }
                        @media print {
                            @page { margin: 0; size: auto; }
                            body { margin: 1cm; background: white; }
                        }
                    </style>
                </head>
                <body>
                    <script>
                        function printImg() {
                            window.print();
                            setTimeout(() => window.close(), 500);
                        }
                    </script>
                    <img src="${url}" onload="printImg()" />
                </body>
            </html>
        `);
        printWindow.document.close();
    }
};

/**
 * Triggers a browser download for an image
 * @param url The image URL
 * @param fileName The name to save the file as
 */
export const downloadImage = (url: string, fileName: string) => {
    const link = document.createElement('a');
    link.href = url;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};
