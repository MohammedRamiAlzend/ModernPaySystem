import jsPDF from 'jspdf';
import html2canvas from 'html2canvas';

interface PrintField {
    label: string;
    value: string;
    colSpan: number;
    type?: string;
}

/**
 * Generates a PDF document from form response data.
 * Uses html2canvas to render Arabic text correctly.
 */
export const generateFormPDF = async (
    title: string,
    submittedAt: string,
    fields: PrintField[],
    direction: 'rtl' | 'ltr' = 'rtl'
): Promise<void> => {
    // Create a temporary container for rendering
    const container = document.createElement('div');
    container.style.position = 'fixed';
    container.style.left = '-9999px';
    container.style.top = '0';
    container.style.width = '794px'; // A4 width in pixels at 96 DPI
    container.style.background = 'white';
    container.style.padding = '40px';
    container.style.fontFamily = 'Cairo, Arial, sans-serif';
    container.style.direction = direction;

    // Generate fields HTML
    const fieldsHtml = fields.map(field => {
        const widthPercent = (field.colSpan / 12) * 100;

        if (field.type === 'label') {
            return `
                <div style="
                    width: 100%;
                    padding: 20px 8px 10px 8px;
                    border-bottom: 2px solid #16a34a;
                    margin-top: 15px;
                    margin-bottom: 10px;
                    box-sizing: border-box;
                ">
                    <h2 style="
                        margin: 0;
                        font-size: 18px;
                        font-weight: 800;
                        color: #16a34a;
                    ">${field.label}</h2>
                    ${field.value ? `<p style="font-size: 13px; color: #666; margin: 5px 0 0 0;">${field.value}</p>` : ''}
                </div>
            `;
        }

        return `
            <div style="
                width: ${widthPercent}%;
                display: inline-block;
                vertical-align: top;
                padding: 12px 8px;
                border-bottom: 1px solid #e5e7eb;
                box-sizing: border-box;
            ">
                <span style="
                    font-weight: 700;
                    color: #000000;
                    font-size: 13px;
                    margin-left: 8px;
                ">${field.label}:</span>
                <span style="
                    color: #000000;
                    font-size: 13px;
                ">${field.value}</span>
            </div>
        `;
    }).join('');

    container.innerHTML = `
        <div style="max-width: 100%; margin: 0 auto;">
            <div style="
                text-align: center;
                padding-bottom: 20px;
                margin-bottom: 25px;
                border-bottom: 2px solid #000000;
            ">
                <h1 style="
                    font-size: 24px;
                    font-weight: 900;
                    color: #000000;
                    margin: 0 0 10px 0;
                ">${title}</h1>
                <div style="
                    font-size: 14px;
                    color: #000000;
                    font-weight: 600;
                ">تاريخ التقديم: ${submittedAt}</div>
            </div>
            
            <div style="
                display: flex;
                flex-wrap: wrap;
            ">
                ${fieldsHtml}
            </div>
            
            
        </div>
    `;

    document.body.appendChild(container);

    try {
        // Wait for fonts to load
        await document.fonts.ready;

        // Use html2canvas to capture the rendered HTML
        const canvas = await html2canvas(container, {
            scale: 2, // Higher resolution
            useCORS: true,
            backgroundColor: '#ffffff',
            logging: false,
        });

        // Create PDF
        const pdf = new jsPDF({
            orientation: 'portrait',
            unit: 'mm',
            format: 'a4',
        });

        const imgWidth = 210; // A4 width in mm
        const imgHeight = (canvas.height * imgWidth) / canvas.width;

        // Handle multi-page if content is too long
        const pageHeight = 297; // A4 height in mm
        let heightLeft = imgHeight;
        let position = 0;

        const imgData = canvas.toDataURL('image/png');

        // First page
        pdf.addImage(imgData, 'PNG', 0, position, imgWidth, imgHeight);
        heightLeft -= pageHeight;

        // Additional pages if needed
        while (heightLeft > 0) {
            position = heightLeft - imgHeight;
            pdf.addPage();
            pdf.addImage(imgData, 'PNG', 0, position, imgWidth, imgHeight);
            heightLeft -= pageHeight;
        }

        // Download the PDF
        const fileName = `${title.replace(/\s+/g, '_')}_${new Date().toISOString().split('T')[0]}.pdf`;
        pdf.save(fileName);

    } finally {
        // Cleanup
        if (document.body.contains(container)) {
            document.body.removeChild(container);
        }
    }
};

/**
 * Opens a print dialog for the form response.
 * This is a fallback for browsers that don't support PDF download well.
 */
export const printFormResponse = (
    title: string,
    submittedAt: string,
    fields: PrintField[],
    direction: 'rtl' | 'ltr' = 'rtl'
): void => {
    // Create a hidden iframe for isolated printing
    const iframe = document.createElement('iframe');
    iframe.style.position = 'fixed';
    iframe.style.right = '-9999px';
    iframe.style.width = '210mm';
    iframe.style.height = '297mm';
    iframe.style.border = 'none';
    document.body.appendChild(iframe);

    const doc = iframe.contentWindow?.document;
    if (!doc) return;

    // Generate fields HTML
    const fieldsHtml = fields.map(field => {
        const widthPercent = (field.colSpan / 12) * 100;
        
        if (field.type === 'label') {
            return `
                <div class="field-item label-item" style="width: 100%; border-bottom: 2px solid #16a34a; background-color: #f8fafc; padding-top: 20px;">
                    <div style="display: flex; flex-direction: column; width: 100%;">
                        <span class="field-label" style="font-size: 14pt; color: #16a34a;">${field.label}</span>
                        ${field.value ? `<span class="field-value" style="font-size: 10pt; color: #64748b; font-style: italic;">${field.value}</span>` : ''}
                    </div>
                </div>
            `;
        }

        return `
            <div class="field-item" style="width: ${widthPercent}%;">
                <span class="field-label">${field.label}:</span>
                <span class="field-value">${field.value}</span>
            </div>
        `;
    }).join('');

    const htmlContent = `
<!DOCTYPE html>
<html lang="ar" dir="${direction}">
<head>
    <meta charset="UTF-8">
    <title>${title}</title>
    <link rel="preconnect" href="https://fonts.googleapis.com">
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
    <link href="https://fonts.googleapis.com/css2?family=Cairo:wght@400;600;700;900&display=swap" rel="stylesheet">
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }
        
        body {
            font-family: 'Cairo', sans-serif;
            background: white;
            color: #1a1a1a;
            padding: 15mm;
            direction: ${direction};
            font-size: 12pt;
            line-height: 1.6;
        }
        
        .document {
            max-width: 180mm;
            margin: 0 auto;
        }
        
        .header {
            text-align: center;
            padding-bottom: 20px;
            margin-bottom: 25px;
            border-bottom: 2px solid #000000;
        }
        
        .header h1 {
            font-size: 22pt;
            font-weight: 900;
            color: #000000;
            margin-bottom: 8px;
        }
        
        .header .date {
            font-size: 11pt;
            color: #000000;
            font-weight: 600;
        }
        
        .fields-container {
            display: flex;
            flex-wrap: wrap;
            gap: 0;
        }
        
        .field-item {
            display: flex;
            align-items: baseline;
            gap: 8px;
            padding: 12px 10px;
            border-bottom: 1px solid #e2e8f0;
            min-height: 45px;
        }
        
        .field-label {
            font-weight: 700;
            white-space: nowrap;
            font-size: 11pt;
        }
        
        .field-value {
            font-weight: 500;
            font-size: 11pt;
        }
        
        .footer {
            margin-top: 40px;
            padding-top: 15px;
            border-top: 1px solid #cbd5e1;
            text-align: center;
            font-size: 9pt;
            color: #94a3b8;
        }
        
        @media print {
            @page {
                size: auto;
                margin: 0;
            }
            body {
                padding: 15mm;
                -webkit-print-color-adjust: exact;
                print-color-adjust: exact;
            }
            .document {
                max-width: 100%;
                margin: 0;
            }
        }
    </style>
</head>
<body>
    <div class="document">
        <div class="header">
            <h1>${title}</h1>
            <div class="date">تاريخ التقديم: ${submittedAt}</div>
        </div>
        
        <div class="fields-container">
            ${fieldsHtml}
        </div>
        
        </div>
        </body>
        </html>
        `;

    // <div class="footer">
    //     تم توليد هذا التقرير تلقائياً من نظام إدارة النماذج
    // </div>
    doc.open();
    doc.write(htmlContent);
    doc.close();

    // Wait for fonts to load, then print
    setTimeout(() => {
        iframe.contentWindow?.focus();
        iframe.contentWindow?.print();

        // Cleanup after print dialog closes
        setTimeout(() => {
            if (document.body.contains(iframe)) {
                document.body.removeChild(iframe);
            }
        }, 2000);
    }, 800);
};
