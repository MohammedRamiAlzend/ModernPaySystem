import jsPDF from 'jspdf';
import html2canvas from 'html2canvas';

export interface PrintResponseItem {
    userName: string;
    content: string;
    date: string;
    type?: 'response' | 'referral';
}

export interface PrintOptions {
    responses?: PrintResponseItem[];
    referrals?: PrintResponseItem[];
    finalResponse?: PrintResponseItem;
    direction?: 'rtl' | 'ltr';
}

interface PrintField {
    label: string;
    value: string;
    colSpan: number;
    type?: string;
}

/**
 * Generates a PDF document from form response data.
 * Optimized for Arabic text and multi-page rendering.
 */
export const generateFormPDF = async (
    title: string,
    submittedAt: string,
    fields: PrintField[],
    options: PrintOptions = {}
): Promise<void> => {
    const direction = options.direction || 'rtl';
    const PAGE_WIDTH_PX = 794;

    const container = document.createElement('div');
    container.style.cssText = `
        position: fixed;
        left: -9999px;
        top: 0;
        width: ${PAGE_WIDTH_PX}px;
        background: #ffffff;
        padding: 40px;
        font-family: Cairo, Arial, sans-serif;
        direction: ${direction};
        color: #000000;
        box-sizing: border-box;
    `;

    const fieldsHtml = fields.map(field => {
        const widthPercent = (field.colSpan / 12) * 100;
        if (field.type === 'label') {
            return `
                <div style="width: 100%; padding: 20px 8px 10px 8px; margin-top: 15px; margin-bottom: 5px; box-sizing: border-box;">
                    <h2 style="margin: 0; font-size: 16px; font-weight: 800; color: #000000;">${field.label}</h2>
                    ${field.value ? `<p style="font-size: 12px; color: #333; margin: 5px 0 0 0;">${field.value}</p>` : ''}
                </div>
            `;
        }
        return `
            <div style="width: ${widthPercent}%; display: inline-block; vertical-align: top; padding: 10px 8px; box-sizing: border-box;">
                <span style="font-weight: 700; color: #000000; font-size: 13px; margin-left: 8px;">${field.label}:</span>
                <span style="color: #000000; font-size: 13px;">${field.value}</span>
            </div>
        `;
    }).join('');

    const generateTableHtml = (tableTitle: string, items: PrintResponseItem[]) => {
        if (!items || items.length === 0) return '';
        return `
            <div style="margin-top: 30px; width: 100%; clear: both;">
                <h3 style="font-size: 14px; font-weight: 800; color: #000000; padding: 10px 0; margin: 0; text-align: right; border-bottom: 2px solid #000000;">
                    ${tableTitle} عددها ${items.length}
                </h3>
                <table style="width: 100%; border-collapse: collapse; font-size: 11px; margin-top: 10px;">
                    <tr style="border-bottom: 2px solid #000000;">
                        <td style="padding: 12px 10px; text-align: right; font-weight: 900; width: 25%;">الاسم</td>
                        <td style="padding: 12px 10px; text-align: right; font-weight: 900; width: 55%;">المحتوى / الملاحظات</td>
                        <td style="padding: 12px 10px; text-align: center; font-weight: 900; width: 20%;">التاريخ</td>
                    </tr>
                    ${items.map(item => `
                        <tr style="border-bottom: 1px solid #eeeeee;">
                            <td style="padding: 10px 10px; font-weight: 700; vertical-align: top;">${item.userName}</td>
                            <td style="padding: 10px 10px; vertical-align: top;">${item.content}</td>
                            <td style="padding: 10px 10px; text-align: center; vertical-align: top;">${item.date}</td>
                        </tr>
                    `).join('')}
                </table>
            </div>
        `;
    };

    const finalResponseHtml = options.finalResponse ? `
        <div style="margin-top: 25px; padding: 15px; border: 1px solid #eeeeee; background: #ffffff;">
            <div style="margin-bottom: 8px; border-bottom: 1px solid #eeeeee; padding-bottom: 5px;">
                <span style="color: #000000; font-size: 14px; font-weight: 800;">الرد النهائي بواسطة ${options.finalResponse.userName}</span>
               
            </div>
            <div style="font-size: 13px; line-height: 1.6; color: #000000; margin-bottom: 8px;">
                ${options.finalResponse.content}
            </div>
            <div style="font-size: 10px; color: #000000; text-align: ${direction === 'rtl' ? 'left' : 'right'}; border-top: 1px dashed #cccccc; padding-top: 5px;">
                تاريخ الرد: ${options.finalResponse.date}
            </div>
        </div>
    ` : '';

    container.innerHTML = `
        <div style="background: #ffffff;">
            <div style="text-align: center; padding-bottom: 10px; margin-bottom: 20px; border-bottom: 2px solid #000000;">
                <img src="/ECSC.png" style="height: 50px; object-fit: contain; display: block; margin: 0 auto 10px;" />
                <h1 style="font-size: 18px; font-weight: 900; color: #000000; margin: 5px 0;">${title}</h1>
                <div style="font-size: 12px; color: #000000; font-weight: 600;">تاريخ التقديم: ${submittedAt}</div>
            </div>
            <div style="width: 100%;">${fieldsHtml}</div>
            <div style="clear: both;"></div>
            ${finalResponseHtml}
            ${generateTableHtml('تاريخ الإحالات والتنقلات', options.referrals || [])}
            <div style="margin-top: 20px; padding: 10px 0; border-top: 1px solid #000000; text-align: center; font-size: 10px;">
                طُبع بتاريخ: ${new Date().toLocaleDateString('ar-SA')}
            </div>
        </div>
    `;

    document.body.appendChild(container);

    try {
        await document.fonts.ready;
        const images = container.getElementsByTagName('img');
        await Promise.all(Array.from(images).map(img =>
            img.complete ? Promise.resolve() : new Promise(r => { img.onload = r; img.onerror = r; })
        ));

        await new Promise(resolve => setTimeout(resolve, 800));

        const canvas = await html2canvas(container, {
            scale: 2,
            useCORS: true,
            backgroundColor: '#ffffff',
            logging: false,
            windowWidth: PAGE_WIDTH_PX
        });

        const pdf = new jsPDF({
            orientation: 'portrait',
            unit: 'px',
            format: 'a4',
            hotfixes: ['px_scaling']
        });

        const imgData = canvas.toDataURL('image/jpeg', 0.95);
        const pdfWidth = pdf.internal.pageSize.getWidth();
        const pdfHeight = (canvas.height * pdfWidth) / canvas.width;
        const pageHeight = pdf.internal.pageSize.getHeight();

        let heightLeft = pdfHeight;
        let position = 0;

        pdf.addImage(imgData, 'JPEG', 0, position, pdfWidth, pdfHeight, undefined, 'FAST');
        heightLeft -= pageHeight;

        while (heightLeft > 0) {
            position = heightLeft - pdfHeight;
            pdf.addPage();
            pdf.addImage(imgData, 'JPEG', 0, position, pdfWidth, pdfHeight, undefined, 'FAST');
            heightLeft -= pageHeight;
        }

        const fileName = `${title.replace(/\s+/g, '_')}_${new Date().toISOString().split('T')[0]}.pdf`;
        pdf.save(fileName);
    } finally {
        if (document.body.contains(container)) {
            document.body.removeChild(container);
        }
    }
};

/**
 * Opens a print dialog for the form response.
 * Fallback for browsers where PDF download is not preferred.
 */
export const printFormResponse = (
    title: string,
    submittedAt: string,
    fields: PrintField[],
    options: PrintOptions = {}
): void => {
    const direction = options.direction || 'rtl';
    const iframe = document.createElement('iframe');
    iframe.style.position = 'fixed';
    iframe.style.right = '-9999px';
    iframe.style.width = '210mm';
    iframe.style.height = '297mm';
    iframe.style.border = 'none';
    document.body.appendChild(iframe);

    const doc = iframe.contentWindow?.document;
    if (!doc) return;

    const fieldsHtml = fields.map(field => {
        const widthPercent = (field.colSpan / 12) * 100;

        if (field.type === 'label') {
            return `
                <div class="field-item label-item" style="width:100%;border-bottom:none;background:#fff;padding-top:20px;padding-bottom:10px;">
                    <div style="display:flex;flex-direction:column;width:100%;">
                        <span class="field-label" style="font-size:14pt;color:#000;border-bottom:none;">${field.label}</span>
                        ${field.value ? `<span class="field-value" style="font-size:10pt;color:#000;font-style:italic;">${field.value}</span>` : ''}
                    </div>
                </div>
            `;
        }

        return `
            <div class="field-item" style="width:${widthPercent}%;border-bottom:none;padding:10px;">
                <span class="field-label" style="font-weight:700;color:#000;">${field.label}:</span>
                <span class="field-value" style="color:#000;">${field.value}</span>
            </div>
        `;
    }).join('');

    const generateTableHtml = (tableTitle: string, items: PrintResponseItem[]) => {
        if (!items || items.length === 0) return '';
        return `
            <div style="margin-top:30px;">
                <h3 style="font-size:14pt;font-weight:900;color:#000;padding:10px 0;margin:0;">
                    ${tableTitle} عددها ${items.length}
                </h3>
                <table style="width:100%;border-collapse:collapse;font-size:10pt;">
                    <thead>
                        <tr style="border-bottom:2px solid #000;">
                            <td style="padding:12px 10px;text-align:${direction === 'rtl' ? 'right' : 'left'};font-weight:800;">الاسم</td>
                            <td style="padding:12px 10px;text-align:${direction === 'rtl' ? 'right' : 'left'};font-weight:800;">المحتوى / الملاحظات</td>
                            <td style="padding:12px 10px;text-align:center;width:120px;font-weight:800;">التاريخ</td>
                        </tr>
                    </thead>
                    <tbody>
                        ${items.map(item => `
                            <tr style="border-bottom:1px solid #eee;">
                                <td style="padding:12px 10px;font-weight:700;">${item.userName}</td>
                                <td style="padding:12px 10px;">${item.content}</td>
                                <td style="padding:12px 10px;text-align:center;">${item.date}</td>
                            </tr>
                        `).join('')}
                    </tbody>
                </table>
            </div>
        `;
    };

    const finalResponseHtml = options.finalResponse ? `
        <div style="margin-top:30px;padding:20px;background:#fff;border:1px solid #eee;">
            <div style="margin-bottom:12px;">
                <span style="font-size:14pt;font-weight:900;">الرد النهائي</span>
                <span style="font-size:12pt;font-weight:700;margin-right:15px;">بواسطة: ${options.finalResponse.userName}</span>
            </div>
            <div style="font-size:11pt;line-height:1.6;margin-bottom:10px;">${options.finalResponse.content}</div>
            <div style="font-size:9pt;text-align:${direction === 'rtl' ? 'left' : 'right'};border-top:1px solid #eee;padding-top:5px;">
                تاريخ الرد: ${options.finalResponse.date}
            </div>
        </div>
    ` : '';

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
        * { margin:0; padding:0; box-sizing:border-box; }
        body { font-family:'Cairo',sans-serif; background:white; color:#1a1a1a; padding:15mm; direction:${direction}; font-size:12pt; line-height:1.6; }
        .document { max-width:180mm; margin:0 auto; }
        .header { text-align:center; padding-bottom:20px; margin-bottom:25px; border-bottom:2px solid #000; }
        .header h1 { font-size:22pt; font-weight:900; color:#000; margin-bottom:8px; }
        .header .date { font-size:11pt; color:#000; font-weight:600; }
        .fields-container { display:flex; flex-wrap:wrap; }
        .field-item { display:flex; align-items:baseline; gap:8px; padding:10px; min-height:40px; }
        .field-label { font-weight:700; white-space:nowrap; font-size:11pt; }
        .field-value { font-weight:500; font-size:11pt; }
        @media print {
            @page { size:auto; margin:0; }
            body { padding:15mm; -webkit-print-color-adjust:exact; print-color-adjust:exact; }
            .document { max-width:100%; margin:0; }
        }
    </style>
</head>
<body>
    <div class="document">
        <div class="header">
            <img src="/ECSC.png" style="height:60px;object-fit:contain;display:block;margin:0 auto 10px;" />
            <h1>${title}</h1>
            <div class="date">تاريخ التقديم: ${submittedAt}</div>
        </div>
        <div class="fields-container">${fieldsHtml}</div>
        ${finalResponseHtml}
        ${generateTableHtml('تاريخ الإحالات والتنقلات', options.referrals || [])}
    </div>
</body>
</html>`;

    doc.open();
    doc.write(htmlContent);
    doc.close();

    setTimeout(() => {
        iframe.contentWindow?.focus();
        iframe.contentWindow?.print();
        setTimeout(() => {
            if (document.body.contains(iframe)) document.body.removeChild(iframe);
        }, 2000);
    }, 800);
};