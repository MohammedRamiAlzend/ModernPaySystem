import ExcelJS from 'exceljs';

type CellValue = string | number | boolean | Date | null | undefined;

function formatDate(dateStr: string | null | undefined): string {
    if (!dateStr) return '-';
    return new Date(dateStr).toLocaleDateString('ar-SY', {
        year: 'numeric', month: '2-digit', day: '2-digit',
    });
}

function formatDateTime(dateStr: string | null | undefined): string {
    if (!dateStr) return '-';
    return new Date(dateStr).toLocaleString('ar-SY', {
        year: 'numeric', month: '2-digit', day: '2-digit',
        hour: '2-digit', minute: '2-digit',
    });
}

const ACTION_TRANSLATIONS: Record<string, string> = {
    'View': 'عرض',
    'Print': 'طباعة',
    'Download': 'تحميل',
    'Create': 'إنشاء',
    'Update': 'تحديث',
    'Delete': 'حذف',
    'Export': 'تصدير',
    'Upload': 'رفع'
};

function formatBytes(bytes: number): string {
    if (bytes === 0) return '0 بايت';
    const k = 1024;
    const sizes = ['بايت', 'كيلوبايت', 'ميغابايت', 'جيجابايت', 'تيرابايت'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
}

const HEADER_FILL: ExcelJS.Fill = {
    type: 'pattern',
    pattern: 'solid',
    fgColor: { argb: 'FF1E3A5F' },
};

const HEADER_FONT: Partial<ExcelJS.Font> = {
    name: 'Calibri',
    size: 12,
    bold: true,
    color: { argb: 'FFFFFFFF' },
};

const TITLE_FONT: Partial<ExcelJS.Font> = {
    name: 'Calibri',
    size: 16,
    bold: true,
    color: { argb: 'FF1E3A5F' },
};

const SUBTITLE_FONT: Partial<ExcelJS.Font> = {
    name: 'Calibri',
    size: 10,
    color: { argb: 'FF666666' },
};

const BORDER: Partial<ExcelJS.Borders> = {
    top: { style: 'thin', color: { argb: 'FFD0D0D0' } },
    left: { style: 'thin', color: { argb: 'FFD0D0D0' } },
    bottom: { style: 'thin', color: { argb: 'FFD0D0D0' } },
    right: { style: 'thin', color: { argb: 'FFD0D0D0' } },
};

const DATA_FONT: Partial<ExcelJS.Font> = {
    name: 'Calibri',
    size: 11,
};

const HEADER_ALIGNMENT: Partial<ExcelJS.Alignment> = {
    horizontal: 'center',
    vertical: 'middle',
};

const TEXT_ALIGNMENT: Partial<ExcelJS.Alignment> = {
    horizontal: 'right',
    vertical: 'middle',
};

const NUMBER_ALIGNMENT: Partial<ExcelJS.Alignment> = {
    horizontal: 'center',
    vertical: 'middle',
};

async function createWorkbook(): Promise<ExcelJS.Workbook> {
    const workbook = new ExcelJS.Workbook();
    workbook.creator = 'نظام إدارة الأرشفة';
    workbook.created = new Date();
    return workbook;
}

function addTitleRow(ws: ExcelJS.Worksheet, title: string, columnCount: number) {
    const row = ws.addRow([title]);
    row.font = TITLE_FONT;
    row.alignment = { horizontal: 'right', vertical: 'middle' };
    row.height = 35;
    ws.mergeCells(1, 1, 1, columnCount);
}

function addSubtitleRow(ws: ExcelJS.Worksheet, columns: number) {
    const now = new Date().toLocaleString('ar-SY');
    const row = ws.addRow([`تاريخ التقرير: ${now}`]);
    row.font = SUBTITLE_FONT;
    row.alignment = { horizontal: 'right', vertical: 'middle' };
    row.height = 22;
    ws.mergeCells(2, 1, 2, columns);
}

function addBlankRow(ws: ExcelJS.Worksheet) {
    const row = ws.addRow([]);
    row.height = 8;
}

function addHeaderRow(ws: ExcelJS.Worksheet, headers: string[]) {
    const row = ws.addRow(headers);
    row.eachCell((cell) => {
        cell.fill = HEADER_FILL;
        cell.font = HEADER_FONT;
        cell.alignment = HEADER_ALIGNMENT;
        cell.border = BORDER;
    });
    row.height = 28;
    return row;
}

type AlignInput = Partial<ExcelJS.Alignment> | Partial<ExcelJS.Alignment>[];

function addDataRow(ws: ExcelJS.Worksheet, values: CellValue[], alignments?: AlignInput) {
    const aligns = Array.isArray(alignments) ? alignments : values.map(() => alignments ?? TEXT_ALIGNMENT);
    const row = ws.addRow(values.map(v => (v ?? '-')));
    row.eachCell((cell, colNumber) => {
        cell.font = DATA_FONT;
        cell.border = BORDER;
        cell.alignment = aligns[colNumber - 1] ?? TEXT_ALIGNMENT;
    });
    return row;
}



function mixedAlignments(
    specs: { index: number; align: Partial<ExcelJS.Alignment> }[],
    total: number,
    defaultAlign: Partial<ExcelJS.Alignment> = TEXT_ALIGNMENT,
): Partial<ExcelJS.Alignment>[] {
    const result = Array(total).fill(null).map(() => defaultAlign);
    specs.forEach(({ index, align }) => {
        if (index >= 0 && index < total) result[index] = align;
    });
    return result;
}

async function triggerDownload(buffer: ArrayBuffer, filename: string) {
    const blob = new Blob([buffer], {
        type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
    });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
}

// -----------------------------------------------------------------------
// Dashboard Export (multi-sheet)
// -----------------------------------------------------------------------
export async function exportDashboardToExcel(dashboard: {
    departmentName: string;
    totalArchiveRecords: number;
    totalUsers: number;
    totalFolders: number;
    totalPhysicalFiles: number;
    totalStorageBytes: number;
    recordsCreatedToday: number;
    recordsCreatedThisWeek: number;
    recordsCreatedThisMonth: number;
    activeUsersToday: number;
    activeUsersThisWeek: number;
    activeUsersThisMonth: number;
    actionTypeBreakdown: Record<string, number>;
}) {
    const wb = await createWorkbook();
    const deptName = dashboard.departmentName;
    const dateLabel = new Date().toLocaleDateString('ar-SY', { year: 'numeric', month: 'long', day: 'numeric' });

    // Sheet 1: Summary
    const summarySheet = wb.addWorksheet('ملخص', { properties: { tabColor: { argb: 'FF1E3A5F' } } });
    const sumCols = 3;
    addTitleRow(summarySheet, `لوحة معلومات الأرشيف - ${deptName}`, sumCols);
    addSubtitleRow(summarySheet, sumCols);
    addBlankRow(summarySheet);

    const summaryHeaders = ['البيان', 'القيمة', ''];
    addHeaderRow(summarySheet, summaryHeaders);

    const summaryData: [string, CellValue, string][] = [
        ['إجمالي السجلات', dashboard.totalArchiveRecords, ''],
        ['إجمالي المجلدات', dashboard.totalFolders, ''],
        ['الملفات المرفوعة', dashboard.totalPhysicalFiles, ''],
        ['مساحة التخزين', formatBytes(dashboard.totalStorageBytes), ''],
        ['إجمالي المستخدمين', dashboard.totalUsers, ''],
        ['سجلات اليوم', dashboard.recordsCreatedToday, ''],
        ['سجلات هذا الأسبوع', dashboard.recordsCreatedThisWeek, ''],
        ['سجلات هذا الشهر', dashboard.recordsCreatedThisMonth, ''],
        ['المستخدمون النشطون اليوم', dashboard.activeUsersToday, ''],
        ['المستخدمون النشطون هذا الأسبوع', dashboard.activeUsersThisWeek, ''],
        ['المستخدمون النشطون هذا الشهر', dashboard.activeUsersThisMonth, ''],
    ];

    const aligns = mixedAlignments([{ index: 1, align: NUMBER_ALIGNMENT }], sumCols, TEXT_ALIGNMENT);
    summaryData.forEach(([label, value]) => addDataRow(summarySheet, [label, value, ''], aligns));

    summarySheet.getColumn(1).width = 35;
    summarySheet.getColumn(2).width = 20;
    summarySheet.getColumn(3).width = 10;

    // Sheet 2: Action Breakdown
    if (Object.keys(dashboard.actionTypeBreakdown).length > 0) {
        const actionSheet = wb.addWorksheet('الإجراءات', { views: [{ rightToLeft: true }] });
        const actCols = 2;
        addTitleRow(actionSheet, `توزيع الإجراءات - ${deptName}`, actCols);
        const subRow = actionSheet.addRow([dateLabel]);
        subRow.font = SUBTITLE_FONT;
        subRow.alignment = { horizontal: 'right', vertical: 'middle' };
        actionSheet.mergeCells(2, 1, 2, actCols);
        addBlankRow(actionSheet);

        const actionHeaders = ['الإجراء', 'العدد'];
        addHeaderRow(actionSheet, actionHeaders);

        const actAligns = mixedAlignments([{ index: 1, align: NUMBER_ALIGNMENT }], actCols, TEXT_ALIGNMENT);
        Object.entries(dashboard.actionTypeBreakdown).forEach(([action, count]) => {
            const translatedAction = ACTION_TRANSLATIONS[action] || action;
            addDataRow(actionSheet, [translatedAction, count], actAligns);
        });

        addBlankRow(actionSheet);
        const totalRow = addDataRow(actionSheet, [
            `الإجمالي`,
            Object.values(dashboard.actionTypeBreakdown).reduce((a, b) => a + b, 0),
        ], actAligns);
        totalRow.eachCell((cell) => {
            cell.font = { name: 'Calibri', size: 11, bold: true };
        });

        actionSheet.getColumn(1).width = 30;
        actionSheet.getColumn(2).width = 18;
    }

    const buffer = await wb.xlsx.writeBuffer();
    const filename = `لوحة_معلومات_الأرشيف_${new Date().toISOString().split('T')[0]}.xlsx`;
    await triggerDownload(buffer, filename);
}

// -----------------------------------------------------------------------
// Daily Report Export (multi-sheet + optional chart image)
// -----------------------------------------------------------------------
export async function exportDailyReportToExcel(
    report: {
        date: string;
        recordsCreated: number;
        recordsDeleted: number;
        filesAdded: number;
        filesDownloaded: number;
        printActions: number;
        views: number;
        activeUsers: number;
        hourlyBreakdown: { hour: number; recordsCreated: number; actions: number }[];
    },
    chartImageUrl?: string,
) {
    const wb = await createWorkbook();
    const dateLabel = formatDate(report.date);

    // Sheet 1: Summary
    const summarySheet = wb.addWorksheet('ملخص', { properties: { tabColor: { argb: 'FF0D9488' } } });
    const sumCols = 2;
    addTitleRow(summarySheet, `التقرير اليومي - ${dateLabel}`, sumCols);
    addSubtitleRow(summarySheet, sumCols);
    addBlankRow(summarySheet);

    const summaryHeaders = ['البيان', 'القيمة'];
    addHeaderRow(summarySheet, summaryHeaders);

    const items: [string, CellValue][] = [
        ['سجلات منشأة', report.recordsCreated],
        ['سجلات محذوفة', report.recordsDeleted],
        ['ملفات مضافة', report.filesAdded],
        ['تنزيلات', report.filesDownloaded],
        ['طباعات', report.printActions],
        ['مشاهدات', report.views],
        ['مستخدمون نشطون', report.activeUsers],
    ];

    const aligns = mixedAlignments([{ index: 1, align: NUMBER_ALIGNMENT }], sumCols, TEXT_ALIGNMENT);
    items.forEach(([label, value]) => addDataRow(summarySheet, [label, value], aligns));

    // Embed chart image if provided
    if (chartImageUrl) {
        try {
            const response = await fetch(chartImageUrl);
            const blob = await response.blob();
            const arrayBuffer = await blob.arrayBuffer();
            const imageId = wb.addImage({ buffer: arrayBuffer as any, extension: 'png' } as any);
            summarySheet.addImage(imageId, {
                tl: { col: 0, row: summarySheet.rowCount + 2 },
                ext: { width: 600, height: 300 },
            } as any);
        } catch { /* chart image embed failed silently */ }
    }

    summarySheet.getColumn(1).width = 25;
    summarySheet.getColumn(2).width = 18;

    // Sheet 2: Hourly Breakdown
    if (report.hourlyBreakdown.length > 0) {
        const hourlySheet = wb.addWorksheet('التوزيع الساعي', { views: [{ rightToLeft: true }] });
        const hrCols = 3;
        addTitleRow(hourlySheet, `التوزيع الساعي - ${dateLabel}`, hrCols);
        const subRow = hourlySheet.addRow([dateLabel]);
        subRow.font = SUBTITLE_FONT;
        subRow.alignment = { horizontal: 'right', vertical: 'middle' };
        hourlySheet.mergeCells(2, 1, 2, hrCols);
        addBlankRow(hourlySheet);

        const hourlyHeaders = ['الساعة', 'سجلات منشأة', 'إجراءات'];
        addHeaderRow(hourlySheet, hourlyHeaders);

        const hourlyAligns = mixedAlignments(
            [{ index: 0, align: TEXT_ALIGNMENT }, { index: 1, align: NUMBER_ALIGNMENT }, { index: 2, align: NUMBER_ALIGNMENT }],
            hrCols,
        );
        report.hourlyBreakdown.forEach((h) => {
            addDataRow(hourlySheet, [`${h.hour}:00`, h.recordsCreated, h.actions], hourlyAligns);
        });

        const totalRow = addDataRow(hourlySheet, [
            'الإجمالي',
            report.hourlyBreakdown.reduce((s, h) => s + h.recordsCreated, 0),
            report.hourlyBreakdown.reduce((s, h) => s + h.actions, 0),
        ], hourlyAligns);
        totalRow.eachCell((cell) => {
            cell.font = { name: 'Calibri', size: 11, bold: true };
        });

        hourlySheet.getColumn(1).width = 18;
        hourlySheet.getColumn(2).width = 18;
        hourlySheet.getColumn(3).width = 18;
    }

    const buffer = await wb.xlsx.writeBuffer();
    const filename = `تقرير_يومي_${report.date.split('T')[0] || report.date}.xlsx`;
    await triggerDownload(buffer, filename);
}

// -----------------------------------------------------------------------
// Period Report Export (Weekly / Monthly) - multi-sheet
// -----------------------------------------------------------------------
export async function exportPeriodReportToExcel(
    report: {
        periodLabel: string;
        periodStart: string;
        periodEnd: string;
        totalRecordsCreated: number;
        totalRecordsDeleted: number;
        totalFilesAdded: number;
        totalDownloads: number;
        totalPrints: number;
        totalViews: number;
        uniqueActiveUsers: number;
        dailyBreakdown: { date: string; recordsCreated: number; actions: number; activeUsers: number }[];
        topUsers: { userName: string; recordsCreated: number; recordsViewed: number; filesDownloaded: number; printActions: number; totalActions: number }[];
    },
    sheetLabel: string,
) {
    const wb = await createWorkbook();
    const dateLabel = `${formatDate(report.periodStart)} - ${formatDate(report.periodEnd)}`;

    // Sheet 1: Summary
    const summarySheet = wb.addWorksheet('ملخص', { properties: { tabColor: { argb: 'FF2563EB' } } });
    const sumCols = 2;
    addTitleRow(summarySheet, `${sheetLabel} - ${report.periodLabel}`, sumCols);
    const subRow = summarySheet.addRow([dateLabel]);
    subRow.font = SUBTITLE_FONT;
    subRow.alignment = { horizontal: 'right', vertical: 'middle' };
    summarySheet.mergeCells(2, 1, 2, sumCols);
    addBlankRow(summarySheet);

    const summaryHeaders = ['البيان', 'القيمة'];
    addHeaderRow(summarySheet, summaryHeaders);

    const summaryAligns = mixedAlignments([{ index: 1, align: NUMBER_ALIGNMENT }], sumCols, TEXT_ALIGNMENT);
    const summaryItems: [string, CellValue][] = [
        ['سجلات منشأة', report.totalRecordsCreated],
        ['سجلات محذوفة', report.totalRecordsDeleted],
        ['ملفات مضافة', report.totalFilesAdded],
        ['تنزيلات', report.totalDownloads],
        ['طباعات', report.totalPrints],
        ['مشاهدات', report.totalViews],
        ['مستخدمون فريدون', report.uniqueActiveUsers],
    ];
    summaryItems.forEach(([label, value]) => addDataRow(summarySheet, [label, value], summaryAligns));
    summarySheet.getColumn(1).width = 28;
    summarySheet.getColumn(2).width = 20;

    // Sheet 2: Daily Breakdown
    if (report.dailyBreakdown.length > 0) {
        const dailySheet = wb.addWorksheet('التوزيع اليومي', { views: [{ rightToLeft: true }] });
        const dailyCols = 4;
        addTitleRow(dailySheet, `التوزيع اليومي - ${report.periodLabel}`, dailyCols);
        const dSub = dailySheet.addRow([dateLabel]);
        dSub.font = SUBTITLE_FONT;
        dSub.alignment = { horizontal: 'right', vertical: 'middle' };
        dailySheet.mergeCells(2, 1, 2, dailyCols);
        addBlankRow(dailySheet);

        const dailyHeaders = ['التاريخ', 'سجلات منشأة', 'إجراءات', 'مستخدمون نشطون'];
        addHeaderRow(dailySheet, dailyHeaders);

        const dailyAligns = mixedAlignments(
            [
                { index: 0, align: TEXT_ALIGNMENT },
                { index: 1, align: NUMBER_ALIGNMENT },
                { index: 2, align: NUMBER_ALIGNMENT },
                { index: 3, align: NUMBER_ALIGNMENT },
            ],
            dailyCols,
        );
        report.dailyBreakdown.forEach((d) => {
            addDataRow(dailySheet, [formatDate(d.date), d.recordsCreated, d.actions, d.activeUsers], dailyAligns);
        });

        dailySheet.getColumn(1).width = 20;
        dailySheet.getColumn(2).width = 18;
        dailySheet.getColumn(3).width = 14;
        dailySheet.getColumn(4).width = 20;
    }

    // Sheet 3: Top Users
    if (report.topUsers.length > 0) {
        const usersSheet = wb.addWorksheet('المستخدمون', { views: [{ rightToLeft: true }] });
        const userCols = 6;
        addTitleRow(usersSheet, `أكثر المستخدمين نشاطاً - ${report.periodLabel}`, userCols);
        const uSub = usersSheet.addRow([dateLabel]);
        uSub.font = SUBTITLE_FONT;
        uSub.alignment = { horizontal: 'right', vertical: 'middle' };
        usersSheet.mergeCells(2, 1, 2, userCols);
        addBlankRow(usersSheet);

        const usersHeaders = ['المستخدم', 'سجلات منشأة', 'مشاهدات', 'تنزيلات', 'طباعات', 'إجمالي الإجراءات'];
        addHeaderRow(usersSheet, usersHeaders);

        const usersAligns = mixedAlignments(
            [
                { index: 0, align: TEXT_ALIGNMENT },
                { index: 1, align: NUMBER_ALIGNMENT },
                { index: 2, align: NUMBER_ALIGNMENT },
                { index: 3, align: NUMBER_ALIGNMENT },
                { index: 4, align: NUMBER_ALIGNMENT },
                { index: 5, align: NUMBER_ALIGNMENT },
            ],
            userCols,
        );
        report.topUsers.forEach((u) => {
            addDataRow(usersSheet, [u.userName, u.recordsCreated, u.recordsViewed, u.filesDownloaded, u.printActions, u.totalActions], usersAligns);
        });

        usersSheet.getColumn(1).width = 28;
        usersSheet.getColumn(2).width = 18;
        usersSheet.getColumn(3).width = 14;
        usersSheet.getColumn(4).width = 14;
        usersSheet.getColumn(5).width = 14;
        usersSheet.getColumn(6).width = 20;
    }

    const buffer = await wb.xlsx.writeBuffer();
    const filename = `${sheetLabel.replace(/\s+/g, '_')}_${new Date().toISOString().split('T')[0]}.xlsx`;
    await triggerDownload(buffer, filename);
}

// -----------------------------------------------------------------------
// User Activity Export
// -----------------------------------------------------------------------
export async function exportUserActivityToExcel(
    data: { userName: string; recordsCreated: number; recordsViewed: number; filesDownloaded: number; printActions: number; totalActions: number; lastActivityDate: string | null }[],
    fromDate?: string,
    toDate?: string,
) {
    const wb = await createWorkbook();
    const ws = wb.addWorksheet('نشاط المستخدمين', { properties: { tabColor: { argb: 'FF7C3AED' } } });

    const colCount = 7;
    const dateRange = fromDate || toDate
        ? ` - من ${formatDate(fromDate || null)} إلى ${formatDate(toDate || null)}`
        : '';
    addTitleRow(ws, `تقرير نشاط المستخدمين${dateRange}`, colCount);
    addSubtitleRow(ws, colCount);
    addBlankRow(ws);

    const headers = ['المستخدم', 'سجلات منشأة', 'مشاهدات', 'تنزيلات', 'طباعات', 'إجمالي الإجراءات', 'آخر نشاط'];
    addHeaderRow(ws, headers);

    const aligns = mixedAlignments(
        [
            { index: 0, align: TEXT_ALIGNMENT },
            { index: 1, align: NUMBER_ALIGNMENT },
            { index: 2, align: NUMBER_ALIGNMENT },
            { index: 3, align: NUMBER_ALIGNMENT },
            { index: 4, align: NUMBER_ALIGNMENT },
            { index: 5, align: NUMBER_ALIGNMENT },
            { index: 6, align: TEXT_ALIGNMENT },
        ],
        colCount,
    );

    data.forEach((u) => {
        addDataRow(ws, [u.userName, u.recordsCreated, u.recordsViewed, u.filesDownloaded, u.printActions, u.totalActions, formatDateTime(u.lastActivityDate)], aligns);
    });

    // Summary row
    addBlankRow(ws);
    const totalRow = addDataRow(ws, [
        'الإجمالي',
        data.reduce((s, u) => s + u.recordsCreated, 0),
        data.reduce((s, u) => s + u.recordsViewed, 0),
        data.reduce((s, u) => s + u.filesDownloaded, 0),
        data.reduce((s, u) => s + u.printActions, 0),
        data.reduce((s, u) => s + u.totalActions, 0),
        '',
    ], aligns);
    totalRow.eachCell((cell) => {
        cell.font = { name: 'Calibri', size: 11, bold: true };
        cell.fill = {
            type: 'pattern',
            pattern: 'solid',
            fgColor: { argb: 'FFF0F0F0' },
        };
    });

    ws.getColumn(1).width = 25;
    ws.getColumn(2).width = 16;
    ws.getColumn(3).width = 14;
    ws.getColumn(4).width = 14;
    ws.getColumn(5).width = 14;
    ws.getColumn(6).width = 18;
    ws.getColumn(7).width = 22;

    const buffer = await wb.xlsx.writeBuffer();
    const filename = `نشاط_المستخدمين_${new Date().toISOString().split('T')[0]}.xlsx`;
    await triggerDownload(buffer, filename);
}

// -----------------------------------------------------------------------
// Active Users Export
// -----------------------------------------------------------------------
export async function exportActiveUsersToExcel(
    data: { userName: string; departmentName: string | null; totalActions: number; firstActionDate: string | null; lastActionDate: string | null; actionsPerformed: string[] }[],
    fromDate?: string,
    toDate?: string,
) {
    const wb = await createWorkbook();
    const ws = wb.addWorksheet('المستخدمون النشطون', { properties: { tabColor: { argb: 'FF059669' } } });

    const colCount = 6;
    const dateRange = fromDate || toDate
        ? ` - من ${formatDate(fromDate || null)} إلى ${formatDate(toDate || null)}`
        : '';
    addTitleRow(ws, `تقرير المستخدمين النشطين${dateRange}`, colCount);
    addSubtitleRow(ws, colCount);
    addBlankRow(ws);

    const headers = ['المستخدم', 'القسم', 'إجمالي الإجراءات', 'أول نشاط', 'آخر نشاط', 'الإجراءات'];
    addHeaderRow(ws, headers);

    const aligns = mixedAlignments(
        [
            { index: 0, align: TEXT_ALIGNMENT },
            { index: 1, align: TEXT_ALIGNMENT },
            { index: 2, align: NUMBER_ALIGNMENT },
            { index: 3, align: TEXT_ALIGNMENT },
            { index: 4, align: TEXT_ALIGNMENT },
            { index: 5, align: TEXT_ALIGNMENT },
        ],
        colCount,
    );

    data.forEach((u) => {
        addDataRow(ws, [
            u.userName,
            u.departmentName || '-',
            u.totalActions,
            formatDate(u.firstActionDate),
            formatDate(u.lastActionDate),
            u.actionsPerformed.map(act => ACTION_TRANSLATIONS[act] || act).join('، ') || '-',
        ], aligns);
    });

    ws.getColumn(1).width = 25;
    ws.getColumn(2).width = 22;
    ws.getColumn(3).width = 18;
    ws.getColumn(4).width = 18;
    ws.getColumn(5).width = 18;
    ws.getColumn(6).width = 35;

    const buffer = await wb.xlsx.writeBuffer();
    const filename = `المستخدمون_النشطون_${new Date().toISOString().split('T')[0]}.xlsx`;
    await triggerDownload(buffer, filename);
}

// -----------------------------------------------------------------------
// Storage Report Export (multi-sheet)
// -----------------------------------------------------------------------
export async function exportStorageReportToExcel(report: {
    totalStorageBytes: number;
    totalFiles: number;
    perUser: { userName: string; totalFiles: number; totalBytes: number; percentageOfTotal: number; lastFileAddedAt: string | null }[];
    fileTypeBreakdown: { extension: string; count: number; totalBytes: number; percentageOfTotal: number }[];
}) {
    const wb = await createWorkbook();
    const now = new Date().toISOString().split('T')[0];
    const dateLabelArabic = new Date().toLocaleDateString('ar-SY', { year: 'numeric', month: 'long', day: 'numeric' });

    // Sheet 1: Summary
    const summarySheet = wb.addWorksheet('ملخص', { properties: { tabColor: { argb: 'FFD97706' } } });
    const sumCols = 2;
    addTitleRow(summarySheet, 'تقرير استهلاك التخزين', sumCols);
    const subRow = summarySheet.addRow([dateLabelArabic]);
    subRow.font = SUBTITLE_FONT;
    subRow.alignment = { horizontal: 'right', vertical: 'middle' };
    summarySheet.mergeCells(2, 1, 2, sumCols);
    addBlankRow(summarySheet);

    const summaryHeaders = ['البيان', 'القيمة'];
    addHeaderRow(summarySheet, summaryHeaders);

    const summaryAligns = mixedAlignments([{ index: 1, align: NUMBER_ALIGNMENT }], sumCols, TEXT_ALIGNMENT);
    [['إجمالي مساحة التخزين', formatBytes(report.totalStorageBytes)],
     ['إجمالي الملفات', report.totalFiles]].forEach(([label, value]) => {
        addDataRow(summarySheet, [label, value], summaryAligns);
    });
    summarySheet.getColumn(1).width = 28;
    summarySheet.getColumn(2).width = 20;

    // Sheet 2: File Extensions
    if (report.fileTypeBreakdown.length > 0) {
        const extSheet = wb.addWorksheet('الامتدادات', { views: [{ rightToLeft: true }] });
        const extCols = 4;
        addTitleRow(extSheet, 'توزيع الملفات حسب الامتداد', extCols);
        const eSub = extSheet.addRow([dateLabelArabic]);
        eSub.font = SUBTITLE_FONT;
        eSub.alignment = { horizontal: 'right', vertical: 'middle' };
        extSheet.mergeCells(2, 1, 2, extCols);
        addBlankRow(extSheet);

        const typeHeaders = ['الامتداد', 'العدد', 'المساحة', 'النسبة المئوية'];
        addHeaderRow(extSheet, typeHeaders);

        const typeAligns = mixedAlignments(
            [
                { index: 0, align: TEXT_ALIGNMENT },
                { index: 1, align: NUMBER_ALIGNMENT },
                { index: 2, align: NUMBER_ALIGNMENT },
                { index: 3, align: NUMBER_ALIGNMENT },
            ],
            extCols,
        );
        report.fileTypeBreakdown.forEach((t) => {
            addDataRow(extSheet, [`.${t.extension}`, t.count, formatBytes(t.totalBytes), `${t.percentageOfTotal.toFixed(1)}%`], typeAligns);
        });

        extSheet.getColumn(1).width = 18;
        extSheet.getColumn(2).width = 14;
        extSheet.getColumn(3).width = 20;
        extSheet.getColumn(4).width = 16;
    }

    // Sheet 3: Per User
    if (report.perUser.length > 0) {
        const userSheet = wb.addWorksheet('المستخدمون', { views: [{ rightToLeft: true }] });
        const userCols = 5;
        addTitleRow(userSheet, 'التخزين لكل مستخدم', userCols);
        const uSub = userSheet.addRow([dateLabelArabic]);
        uSub.font = SUBTITLE_FONT;
        uSub.alignment = { horizontal: 'right', vertical: 'middle' };
        userSheet.mergeCells(2, 1, 2, userCols);
        addBlankRow(userSheet);

        const userHeaders = ['المستخدم', 'الملفات', 'المساحة', 'النسبة المئوية', 'آخر إضافة'];
        addHeaderRow(userSheet, userHeaders);

        const userAligns = mixedAlignments(
            [
                { index: 0, align: TEXT_ALIGNMENT },
                { index: 1, align: NUMBER_ALIGNMENT },
                { index: 2, align: NUMBER_ALIGNMENT },
                { index: 3, align: NUMBER_ALIGNMENT },
                { index: 4, align: TEXT_ALIGNMENT },
            ],
            userCols,
        );
        report.perUser.forEach((u) => {
            addDataRow(userSheet, [u.userName, u.totalFiles, formatBytes(u.totalBytes), `${u.percentageOfTotal.toFixed(1)}%`, formatDate(u.lastFileAddedAt)], userAligns);
        });

        userSheet.getColumn(1).width = 28;
        userSheet.getColumn(2).width = 14;
        userSheet.getColumn(3).width = 20;
        userSheet.getColumn(4).width = 16;
        userSheet.getColumn(5).width = 18;
    }

    const buffer = await wb.xlsx.writeBuffer();
    const filename = `تقرير_التخزين_${now}.xlsx`;
    await triggerDownload(buffer, filename);
}

// ---------------------------------------------------------
// Multi-Sheet Daily Work Report
// ---------------------------------------------------------
export async function exportDailyWorkReportToExcel(report: import('@/features/archiving/model/types').DailyWorkReportDto) {
    const wb = await createWorkbook();
    const dateLabel = formatDate(report.date);
    const deptName = report.departmentName;

    // ---- Sheet 1: Audit Logs ----
    const auditSheet = wb.addWorksheet('سجل النشاطات', { views: [{ rightToLeft: true }] });
    const auditCols = 6;

    addTitleRow(auditSheet, `سجل النشاطات - ${deptName}`, auditCols);
    const auditSubtitle = auditSheet.addRow([`تاريخ التقرير: ${dateLabel}`]);
    auditSubtitle.font = SUBTITLE_FONT;
    auditSubtitle.alignment = { horizontal: 'right', vertical: 'middle' };
    auditSheet.mergeCells(2, 1, 2, auditCols);

    const auditHeaders = ['الرقم', 'رقم السجل', 'المستخدم', 'الإجراء', 'التفاصيل', 'الوقت'];
    addHeaderRow(auditSheet, auditHeaders);

    const auditAligns = mixedAlignments(
        [
            { index: 0, align: NUMBER_ALIGNMENT },
            { index: 1, align: TEXT_ALIGNMENT },
            { index: 2, align: TEXT_ALIGNMENT },
            { index: 3, align: TEXT_ALIGNMENT },
            { index: 4, align: TEXT_ALIGNMENT },
            { index: 5, align: TEXT_ALIGNMENT },
        ],
        auditCols,
    );

    report.auditLogs.forEach((log, idx) => {
        addDataRow(auditSheet, [
            idx + 1,
            log.id,
            log.userName,
            ACTION_TRANSLATIONS[log.action] || log.action,
            log.details || '-',
            formatDateTime(log.timestamp),
        ], auditAligns);
    });

    auditSheet.getColumn(1).width = 8;
    auditSheet.getColumn(2).width = 22;
    auditSheet.getColumn(3).width = 22;
    auditSheet.getColumn(4).width = 16;
    auditSheet.getColumn(5).width = 40;
    auditSheet.getColumn(6).width = 20;

    // ---- Sheet 2: Archive Records ----
    const recordSheet = wb.addWorksheet('سجلات الأرشيف', { views: [{ rightToLeft: true }] });
    const recordCols = 8;

    addTitleRow(recordSheet, `سجلات الأرشيف - ${deptName}`, recordCols);
    const recordSubtitle = recordSheet.addRow([`تاريخ التقرير: ${dateLabel}`]);
    recordSubtitle.font = SUBTITLE_FONT;
    recordSubtitle.alignment = { horizontal: 'right', vertical: 'middle' };
    recordSheet.mergeCells(2, 1, 2, recordCols);

    const recordHeaders = ['الرقم', 'رقم السجل', 'المسار', 'النموذج', 'المستخدم', 'تاريخ الإنشاء', 'آخر تحديث', 'بيانات الحقول'];
    addHeaderRow(recordSheet, recordHeaders);

    const recordAligns = mixedAlignments(
        [
            { index: 0, align: NUMBER_ALIGNMENT },
            { index: 1, align: TEXT_ALIGNMENT },
            { index: 2, align: TEXT_ALIGNMENT },
            { index: 3, align: TEXT_ALIGNMENT },
            { index: 4, align: TEXT_ALIGNMENT },
            { index: 5, align: TEXT_ALIGNMENT },
            { index: 6, align: TEXT_ALIGNMENT },
            { index: 7, align: TEXT_ALIGNMENT },
        ],
        recordCols,
    );

    report.archiveRecords.forEach((rec, idx) => {
        const formValuesStr = rec.formValues.length > 0
            ? rec.formValues.map(fv => `${fv.key}: ${fv.value ?? '-'}`).join(' | ')
            : '-';

        addDataRow(recordSheet, [
            idx + 1,
            rec.id,
            rec.folderPath || '-',
            rec.formName || '-',
            rec.createdByUserName || '-',
            formatDateTime(rec.createdAt),
            formatDateTime(rec.updatedAt),
            formValuesStr,
        ], recordAligns);
    });

    recordSheet.getColumn(1).width = 8;
    recordSheet.getColumn(2).width = 22;
    recordSheet.getColumn(3).width = 40;
    recordSheet.getColumn(4).width = 22;
    recordSheet.getColumn(5).width = 22;
    recordSheet.getColumn(6).width = 20;
    recordSheet.getColumn(7).width = 20;
    recordSheet.getColumn(8).width = 60;

    const buffer = await wb.xlsx.writeBuffer();
    const filename = `التقرير_اليومي_${report.date.split('T')[0]}.xlsx`;
    await triggerDownload(buffer, filename);
}

// -----------------------------------------------------------------------
// Charts Export (multi-sheet with per-sheet chart images)
// -----------------------------------------------------------------------
export async function exportChartsToExcel(
    data: import('@/features/archiving/model/types').DepartmentChartsData,
    chartImages?: Partial<Record<string, string>>,
) {
    const wb = await createWorkbook();
    const dateLabelArabic = new Date().toLocaleDateString('ar-SY', { year: 'numeric', month: 'long', day: 'numeric' });

    const sheets: { key: string; name: string; title: string; points: { label: string; value: number }[]; cols: number; headers: string[]; colWidths: number[] }[] = [
        {
            key: 'dailyActivity',
            name: 'النشاط اليومي',
            title: 'النشاط اليومي',
            points: data.dailyActivity,
            cols: 2,
            headers: ['اليوم', 'القيمة'],
            colWidths: [20, 18],
        },
        {
            key: 'actionTypeBreakdown',
            name: 'توزيع الإجراءات',
            title: 'توزيع الإجراءات',
            points: data.actionTypeBreakdown,
            cols: 2,
            headers: ['الإجراء', 'العدد'],
            colWidths: [20, 18],
        },
        {
            key: 'hourlyDistribution',
            name: 'التوزيع الساعي',
            title: 'التوزيع الساعي',
            points: data.hourlyDistribution,
            cols: 2,
            headers: ['الساعة', 'النشاط'],
            colWidths: [18, 18],
        },
        {
            key: 'topActiveUsers',
            name: 'المستخدمون النشطاء',
            title: 'أكثر المستخدمين نشاطاً',
            points: data.topActiveUsers,
            cols: 2,
            headers: ['المستخدم', 'عدد الإجراءات'],
            colWidths: [28, 18],
        },
        {
            key: 'topStorageUsers',
            name: 'المستخدمون للتخزين',
            title: 'أكثر المستخدمين استخداماً للتخزين',
            points: data.topStorageUsers,
            cols: 2,
            headers: ['المستخدم', 'المساحة'],
            colWidths: [28, 20],
        },
        {
            key: 'trend7Days',
            name: 'اتجاه 7 أيام',
            title: 'اتجاه آخر 7 أيام',
            points: data.trend7Days,
            cols: 2,
            headers: ['التاريخ', 'القيمة'],
            colWidths: [20, 18],
        },
    ];

    for (const sheetDef of sheets) {
        if (sheetDef.points.length === 0) continue;

        const ws = wb.addWorksheet(sheetDef.name, { views: [{ rightToLeft: true }] });
        const colCount = sheetDef.cols;
        addTitleRow(ws, sheetDef.title, colCount);
        const subRow = ws.addRow([dateLabelArabic]);
        subRow.font = SUBTITLE_FONT;
        subRow.alignment = { horizontal: 'right', vertical: 'middle' };
        ws.mergeCells(2, 1, 2, colCount);
        addBlankRow(ws);

        addHeaderRow(ws, sheetDef.headers);

        const aligns = mixedAlignments([{ index: 1, align: NUMBER_ALIGNMENT }], colCount, TEXT_ALIGNMENT);
        sheetDef.points.forEach((pt) => {
            const val = sheetDef.key === 'topStorageUsers' ? formatBytes(pt.value) : pt.value;
            addDataRow(ws, [pt.label, val], aligns);
        });

        ws.getColumn(1).width = sheetDef.colWidths[0];
        ws.getColumn(2).width = sheetDef.colWidths[1];

        // Embed per-sheet chart image if provided
        const imgUrl = chartImages?.[sheetDef.key];
        if (imgUrl) {
            try {
                const response = await fetch(imgUrl);
                const blob = await response.blob();
                const arrayBuffer = await blob.arrayBuffer();
                const imageId = wb.addImage({ buffer: arrayBuffer as any, extension: 'png' } as any);
                ws.addImage(imageId, {
                    tl: { col: 0, row: ws.rowCount + 2 },
                    ext: { width: 600, height: 300 },
                } as any);
            } catch { /* chart image embed failed silently */ }
        }
    }

    const buffer = await wb.xlsx.writeBuffer();
    const filename = `الرسوم_البيانية_${new Date().toISOString().split('T')[0]}.xlsx`;
    await triggerDownload(buffer, filename);
}
