import { forwardRef } from 'react';
import { QRCodeSVG } from 'qrcode.react';
import { ArchiveRecordFormInputValue } from '../model/types';

interface QRPreviewTemplateProps {
    guid: string;
    archivalNumber: string;
    formName?: string;
    content: ArchiveRecordFormInputValue[];
    createdAt?: string;
}

export const QRPreviewTemplate = forwardRef<HTMLDivElement, QRPreviewTemplateProps>(({
    guid,
    archivalNumber,
    formName,
    content,
    createdAt = new Date().toLocaleString('ar-SY')
}, ref) => {
    return (
        <div
            ref={ref}
            className="bg-white text-slate-800 p-8 flex flex-col justify-between border-[12px] border-double border-primary/20 shadow-2xl relative"
            style={{
                width: '595px',
                height: '842px',
                fontFamily: 'Tajawal, system-ui, -apple-system, sans-serif',
                direction: 'rtl'
            }}
        >
            {/* الخلفية المائية */}
            {/* <div className="absolute inset-0 flex items-center justify-center opacity-[0.03] pointer-events-none select-none">
                <svg className="w-96 h-96 text-primary" fill="currentColor" viewBox="0 0 24 24">
                    <path d="M20 6h-8l-2-2H4c-1.11 0-1.99.89-1.99 2L2 18c0 1.11.89 2 2 2h16c1.11 0 2-.89 2-2V8c0-1.11-.89-2-2-2zm-1 11H5V8h14v9z" />
                </svg>
            </div> */}

            {/* الهيدر */}
            <div className="border-b-2 border-primary/20 pb-6 flex items-center justify-between">
                <div className="flex flex-col">
                    <h1 className="text-xl font-bold text-primary tracking-wide">نظام الأرشفة الإلكتروني الحديث</h1>
                    <span className="text-xs text-muted-foreground mt-1">وثيقة غلاف أرشيفية معتمدة</span>
                </div>
                {/* <div className="w-10 h-10 rounded-xl bg-primary/10 flex items-center justify-center text-primary">
                    <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                    </svg>
                </div> */}
            </div>

            {/* الجزء الأوسط: معلومات المستند */}
            <div className="flex-1 my-8 flex flex-col gap-6">
                <div>
                    <h2 className="text-sm font-semibold text-slate-500 mb-2">معلومات الملف الأساسية:</h2>
                    <div className="grid grid-cols-2 gap-4 bg-slate-50 p-4 rounded-xl border border-slate-100">
                        <div>
                            <span className="text-xs text-slate-400 block">رقم الأرشيف:</span>
                            <span className="text-sm font-bold text-slate-700">{archivalNumber}</span>
                        </div>
                        <div>
                            <span className="text-xs text-slate-400 block">نوع النموذج:</span>
                            <span className="text-sm font-bold text-slate-700">{formName || 'نموذج عام'}</span>
                        </div>
                        {/* <div className="col-span-2">
                            <span className="text-xs text-slate-400 block">المعرّف الفريد للمستند (GUID):</span>
                            <span className="text-xs font-mono text-slate-600 block break-all">{guid}</span>
                        </div> */}
                    </div>
                </div>

                {content && content.length > 0 && (
                    <div>
                        <h2 className="text-sm font-semibold text-slate-500 mb-2">بيانات :</h2>
                        <div className="max-h-[300px] overflow-hidden border border-slate-100 rounded-xl">
                            <table className="w-full text-right text-xs">
                                <thead className="bg-slate-50 text-slate-500 border-b border-slate-100">
                                    {/* <tr>
                                        <th className="p-3 font-semibold"></th>
                                        <th className="p-3 font-semibold"> </th>
                                    </tr> */}
                                </thead>
                                <tbody className="divide-y divide-slate-100">
                                    {content.map((item, idx) => (
                                        <tr key={idx} className="hover:bg-slate-50/50">
                                            <td className="p-3 font-medium text-slate-600">{item.key}</td>
                                            <td className="p-3 text-slate-700 font-bold">{item.value || '-'}</td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        </div>
                    </div>
                )}
            </div>

            {/* الفوتر: الباركود الـ QR والتحقق */}
            <div className="border-t-2 border-primary/10 pt-6 flex items-center justify-between">
                <div className="flex flex-col gap-1 text-[11px] text-slate-400">
                    <div>تاريخ الأرشفة والتوليد:</div>
                    <div className="font-semibold text-slate-600">{createdAt}</div>
                    {/* <div className="mt-4 text-[10px] italic text-primary">
                        * رمز الـ QR المرفق يحتوي على المعرّف الفريد للتحقق الرقمي الفوري من صحة المستند وأصالته.
                    </div> */}
                </div>
                <div className="flex flex-col items-center gap-2 bg-slate-50 p-3 rounded-2xl border border-slate-100 shadow-inner">
                    {guid ? (
                        <div className="w-[140px] h-[140px] flex items-center justify-center bg-white rounded-lg border border-slate-200 p-1">
                            <QRCodeSVG
                                value={guid}
                                size={128}
                                level="M"
                                includeMargin={false}
                            />
                        </div>
                    ) : (
                        <div className="w-[140px] h-[140px] rounded-lg border border-slate-200 bg-white p-1 flex items-center justify-center text-slate-300 text-xs">
                            لا يوجد رمز
                        </div>
                    )}
                    {/* <span className="text-[10px] font-mono font-bold text-slate-400">GUID VERIFIED</span> */}
                </div>
            </div>
        </div>
    );
});

QRPreviewTemplate.displayName = 'QRPreviewTemplate';
