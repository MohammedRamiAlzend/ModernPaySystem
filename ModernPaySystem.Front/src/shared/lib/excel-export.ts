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

function formatBytes(bytes: number): string {
    if (bytes === 0) return '0 B';
    const k = 1024;
    const sizes = ['B', 'KB', 'MB', 'GB', 'TB'];
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
    workbook.creator = 'ModernPaySystem';
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
// Dashboard Export
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
    const ws = wb.addWorksheet('لوحة المعلومات', { properties: { tabColor: { argb: 'FF1E3A5F' } } });

    const colCount = 3;
    addTitleRow(ws, `لوحة معلومات الأرشيف - ${dashboard.departmentName}`, colCount);
    addSubtitleRow(ws, colCount);
    addBlankRow(ws);

    const summaryHeaders = ['البيان', 'القيمة', ''];
    addHeaderRow(ws, summaryHeaders);

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

    const aligns = mixedAlignments([{ index: 1, align: NUMBER_ALIGNMENT }], colCount, TEXT_ALIGNMENT);
    summaryData.forEach(([label, value]) => addDataRow(ws, [label, value, ''], aligns));

    addBlankRow(ws);

    if (Object.keys(dashboard.actionTypeBreakdown).length > 0) {
        const actionRow = addDataRow(ws, ['توزيع الإجراءات', '', ''], TEXT_ALIGNMENT);
        actionRow.eachCell((cell) => {
            cell.font = { name: 'Calibri', size: 12, bold: true, color: { argb: 'FF1E3A5F' } };
        });

        const actionHeaders = ['الإجراء', 'العدد', ''];
        addHeaderRow(ws, actionHeaders);

        Object.entries(dashboard.actionTypeBreakdown).forEach(([action, count]) => {
            addDataRow(ws, [action, count, ''], aligns);
        });
    }

    ws.getColumn(1).width = 35;
    ws.getColumn(2).width = 20;
    ws.getColumn(3).width = 10;

    // Summary row at bottom
    addBlankRow(ws);
    const summaryRow = addDataRow(ws, [
        `إجمالي الإجراءات: ${Object.values(dashboard.actionTypeBreakdown).reduce((a, b) => a + b, 0)}`,
        '', '',
    ], TEXT_ALIGNMENT);
    summaryRow.eachCell((cell) => {
        cell.font = { name: 'Calibri', size: 11, bold: true, italic: true, color: { argb: 'FF666666' } };
    });

    const buffer = await wb.xlsx.writeBuffer();
    const filename = `لوحة_معلومات_الأرشيف_${new Date().toISOString().split('T')[0]}.xlsx`;
    await triggerDownload(buffer, filename);
}

// -----------------------------------------------------------------------
// Daily Report Export
// -----------------------------------------------------------------------
export async function exportDailyReportToExcel(report: {
    date: string;
    recordsCreated: number;
    recordsDeleted: number;
    filesAdded: number;
    filesDownloaded: number;
    printActions: number;
    views: number;
    activeUsers: number;
    hourlyBreakdown: { hour: number; recordsCreated: number; actions: number }[];
}) {
    const wb = await createWorkbook();
    const ws = wb.addWorksheet('التقرير اليومي', { properties: { tabColor: { argb: 'FF0D9488' } } });

    const dateLabel = formatDate(report.date);
    const colCount = 7;
    addTitleRow(ws, `التقرير اليومي - ${dateLabel}`, colCount);
    addSubtitleRow(ws, colCount);
    addBlankRow(ws);

    const summaryHeaders = ['البيان', 'القيمة', '', '', '', '', ''];
    addHeaderRow(ws, summaryHeaders);

    const items: [string, CellValue][] = [
        ['سجلات منشأة', report.recordsCreated],
        ['سجلات محذوفة', report.recordsDeleted],
        ['ملفات مضافة', report.filesAdded],
        ['تنزيلات', report.filesDownloaded],
        ['طباعات', report.printActions],
        ['مشاهدات', report.views],
        ['مستخدمون نشطون', report.activeUsers],
    ];

    const summaryAligns = mixedAlignments([{ index: 1, align: NUMBER_ALIGNMENT }], colCount, TEXT_ALIGNMENT);
    items.forEach(([label, value]) => addDataRow(ws, [label, value, '', '', '', '', ''], summaryAligns));

    if (report.hourlyBreakdown.length > 0) {
        addBlankRow(ws);
        const sectionRow = addDataRow(ws, ['التوزيع الساعي', '', '', '', '', '', ''], TEXT_ALIGNMENT);
        sectionRow.eachCell((cell) => {
            cell.font = { name: 'Calibri', size: 12, bold: true, color: { argb: 'FF0D9488' } };
        });

        const hourlyHeaders = ['الساعة', 'سجلات منشأة', 'إجراءات', '', '', '', ''];
        addHeaderRow(ws, hourlyHeaders);

        const hourlyAligns = mixedAlignments(
            [{ index: 0, align: TEXT_ALIGNMENT }, { index: 1, align: NUMBER_ALIGNMENT }, { index: 2, align: NUMBER_ALIGNMENT }],
            colCount,
        );
        report.hourlyBreakdown.forEach((h) => {
            addDataRow(ws, [`${h.hour}:00`, h.recordsCreated, h.actions, '', '', '', ''], hourlyAligns);
        });

        const totalRow = addDataRow(ws, [
            'الإجمالي',
            report.hourlyBreakdown.reduce((s, h) => s + h.recordsCreated, 0),
            report.hourlyBreakdown.reduce((s, h) => s + h.actions, 0),
            '', '', '', '',
        ], hourlyAligns);
        totalRow.eachCell((cell) => {
            cell.font = { name: 'Calibri', size: 11, bold: true };
        });
    }

    ws.getColumn(1).width = 25;
    ws.getColumn(2).width = 18;
    for (let i = 3; i <= 7; i++) ws.getColumn(i).width = 12;

    const buffer = await wb.xlsx.writeBuffer();
    const filename = `تقرير_يومي_${report.date.split('T')[0] || report.date}.xlsx`;
    await triggerDownload(buffer, filename);
}

// -----------------------------------------------------------------------
// Period Report Export (Weekly / Monthly)
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
    const ws = wb.addWorksheet(sheetLabel, { properties: { tabColor: { argb: 'FF2563EB' } } });

    const colCount = 7;
    addTitleRow(ws, `${sheetLabel} - ${report.periodLabel}`, colCount);
    addSubtitleRow(ws, colCount);

    const subtitleRow = addDataRow(ws, [
        `من ${formatDate(report.periodStart)} إلى ${formatDate(report.periodEnd)}`,
        '', '', '', '', '', '',
    ], TEXT_ALIGNMENT);
    subtitleRow.eachCell((cell) => {
        cell.font = { ...SUBTITLE_FONT, italic: true };
    });

    addBlankRow(ws);

    const summaryHeaders = ['البيان', 'القيمة', '', '', '', '', ''];
    addHeaderRow(ws, summaryHeaders);

    const summaryAligns = mixedAlignments([{ index: 1, align: NUMBER_ALIGNMENT }], colCount, TEXT_ALIGNMENT);
    const summaryItems: [string, CellValue][] = [
        ['سجلات منشأة', report.totalRecordsCreated],
        ['سجلات محذوفة', report.totalRecordsDeleted],
        ['ملفات مضافة', report.totalFilesAdded],
        ['تنزيلات', report.totalDownloads],
        ['طباعات', report.totalPrints],
        ['مشاهدات', report.totalViews],
        ['مستخدمون فريدون', report.uniqueActiveUsers],
    ];
    summaryItems.forEach(([label, value]) => addDataRow(ws, [label, value, '', '', '', '', ''], summaryAligns));

    if (report.dailyBreakdown.length > 0) {
        addBlankRow(ws);
        const sectionRow = addDataRow(ws, ['التوزيع اليومي', '', '', '', '', '', ''], TEXT_ALIGNMENT);
        sectionRow.eachCell((cell) => {
            cell.font = { name: 'Calibri', size: 12, bold: true, color: { argb: 'FF2563EB' } };
        });

        const dailyHeaders = ['التاريخ', 'سجلات منشأة', 'إجراءات', 'مستخدمون نشطون', '', '', ''];
        addHeaderRow(ws, dailyHeaders);

        const dailyAligns = mixedAlignments(
            [
                { index: 0, align: TEXT_ALIGNMENT },
                { index: 1, align: NUMBER_ALIGNMENT },
                { index: 2, align: NUMBER_ALIGNMENT },
                { index: 3, align: NUMBER_ALIGNMENT },
            ],
            colCount,
        );
        report.dailyBreakdown.forEach((d) => {
            addDataRow(ws, [formatDate(d.date), d.recordsCreated, d.actions, d.activeUsers, '', '', ''], dailyAligns);
        });
    }

    if (report.topUsers.length > 0) {
        addBlankRow(ws);
        const sectionRow = addDataRow(ws, ['أكثر المستخدمين نشاطاً', '', '', '', '', '', ''], TEXT_ALIGNMENT);
        sectionRow.eachCell((cell) => {
            cell.font = { name: 'Calibri', size: 12, bold: true, color: { argb: 'FF2563EB' } };
        });

        const usersHeaders = ['المستخدم', 'سجلات منشأة', 'مشاهدات', 'تنزيلات', 'طباعات', 'إجمالي الإجراءات', ''];
        addHeaderRow(ws, usersHeaders);

        const usersAligns = mixedAlignments(
            [
                { index: 0, align: TEXT_ALIGNMENT },
                { index: 1, align: NUMBER_ALIGNMENT },
                { index: 2, align: NUMBER_ALIGNMENT },
                { index: 3, align: NUMBER_ALIGNMENT },
                { index: 4, align: NUMBER_ALIGNMENT },
                { index: 5, align: NUMBER_ALIGNMENT },
            ],
            colCount,
        );
        report.topUsers.forEach((u) => {
            addDataRow(ws, [u.userName, u.recordsCreated, u.recordsViewed, u.filesDownloaded, u.printActions, u.totalActions, ''], usersAligns);
        });
    }

    ws.getColumn(1).width = 28;
    ws.getColumn(2).width = 18;
    ws.getColumn(3).width = 14;
    ws.getColumn(4).width = 18;
    ws.getColumn(5).width = 14;
    ws.getColumn(6).width = 20;
    ws.getColumn(7).width = 12;

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
            u.actionsPerformed.join('، ') || '-',
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
// Storage Report Export
// -----------------------------------------------------------------------
export async function exportStorageReportToExcel(report: {
    totalStorageBytes: number;
    totalFiles: number;
    perUser: { userName: string; totalFiles: number; totalBytes: number; percentageOfTotal: number; lastFileAddedAt: string | null }[];
    fileTypeBreakdown: { extension: string; count: number; totalBytes: number; percentageOfTotal: number }[];
}) {
    const wb = await createWorkbook();
    const ws = wb.addWorksheet('التخزين', { properties: { tabColor: { argb: 'FFD97706' } } });

    const colCount = 5;
    addTitleRow(ws, 'تقرير استهلاك التخزين', colCount);
    addSubtitleRow(ws, colCount);
    addBlankRow(ws);

    // Summary
    const summaryHeaders = ['البيان', 'القيمة', '', '', ''];
    addHeaderRow(ws, summaryHeaders);

    const summaryAligns = mixedAlignments([{ index: 1, align: NUMBER_ALIGNMENT }], colCount, TEXT_ALIGNMENT);
    [['إجمالي مساحة التخزين', formatBytes(report.totalStorageBytes)],
     ['إجمالي الملفات', report.totalFiles]].forEach(([label, value]) => {
        addDataRow(ws, [label, value, '', '', ''], summaryAligns);
    });

    // File type breakdown
    if (report.fileTypeBreakdown.length > 0) {
        addBlankRow(ws);
        const sectionRow = addDataRow(ws, ['توزيع الملفات حسب الامتداد', '', '', '', ''], TEXT_ALIGNMENT);
        sectionRow.eachCell((cell) => {
            cell.font = { name: 'Calibri', size: 12, bold: true, color: { argb: 'FFD97706' } };
        });

        const typeHeaders = ['الامتداد', 'العدد', 'المساحة', 'النسبة المئوية', ''];
        addHeaderRow(ws, typeHeaders);

        const typeAligns = mixedAlignments(
            [
                { index: 0, align: TEXT_ALIGNMENT },
                { index: 1, align: NUMBER_ALIGNMENT },
                { index: 2, align: NUMBER_ALIGNMENT },
                { index: 3, align: NUMBER_ALIGNMENT },
            ],
            colCount,
        );
        report.fileTypeBreakdown.forEach((t) => {
            addDataRow(ws, [`.${t.extension}`, t.count, formatBytes(t.totalBytes), `${t.percentageOfTotal.toFixed(1)}%`, ''], typeAligns);
        });
    }

    // Per user
    if (report.perUser.length > 0) {
        addBlankRow(ws);
        const sectionRow = addDataRow(ws, ['التخزين لكل مستخدم', '', '', '', ''], TEXT_ALIGNMENT);
        sectionRow.eachCell((cell) => {
            cell.font = { name: 'Calibri', size: 12, bold: true, color: { argb: 'FFD97706' } };
        });

        const userHeaders = ['المستخدم', 'الملفات', 'المساحة', 'النسبة المئوية', 'آخر إضافة'];
        addHeaderRow(ws, userHeaders);

        const userAligns = mixedAlignments(
            [
                { index: 0, align: TEXT_ALIGNMENT },
                { index: 1, align: NUMBER_ALIGNMENT },
                { index: 2, align: NUMBER_ALIGNMENT },
                { index: 3, align: NUMBER_ALIGNMENT },
                { index: 4, align: TEXT_ALIGNMENT },
            ],
            colCount,
        );
        report.perUser.forEach((u) => {
            addDataRow(ws, [u.userName, u.totalFiles, formatBytes(u.totalBytes), `${u.percentageOfTotal.toFixed(1)}%`, formatDate(u.lastFileAddedAt)], userAligns);
        });
    }

    ws.getColumn(1).width = 28;
    ws.getColumn(2).width = 18;
    ws.getColumn(3).width = 20;
    ws.getColumn(4).width = 16;
    ws.getColumn(5).width = 18;

    const buffer = await wb.xlsx.writeBuffer();
    const filename = `تقرير_التخزين_${new Date().toISOString().split('T')[0]}.xlsx`;
    await triggerDownload(buffer, filename);
}
