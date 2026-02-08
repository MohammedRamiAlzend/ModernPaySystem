import React, { useState } from 'react';
import { BaseModal } from '@/shared/ui/modals/base-modal';
import type { FormSchema } from '@/entities/form/model/types';
import type { FormResponse } from '@/shared/lib/form-engine/responses';
import { Button } from '@/shared/ui/button';
import { Printer, Download, Loader2 } from 'lucide-react';
import { printFormResponse, generateFormPDF } from '@/shared/lib/pdf-generator';
import { getVisibleFields, prepareFieldsForPrint } from '@/shared/lib/form-engine/response-evaluator';

interface ResponseDetailsModalProps {
    isOpen: boolean;
    onClose: () => void;
    schema: FormSchema;
    response: FormResponse | null;
}

export const ResponseDetailsModal: React.FC<ResponseDetailsModalProps> = ({
    isOpen,
    onClose,
    schema: fallbackSchema,
    response
}) => {
    const [isGeneratingPDF, setIsGeneratingPDF] = useState(false);

    if (!response) return null;

    // Use the saved schema from the response (captures form structure at submission time)
    // Fall back to the passed schema for backwards compatibility with older responses
    const schema = response.schema || fallbackSchema;

    // Get only visible fields based on logic evaluation
    const visibleFields = getVisibleFields({
        ...response,
        schema // ensure we're using the correct schema
    });

    const getPrintFields = () => {
        return prepareFieldsForPrint({
            ...response,
            schema
        });
    };

    const handlePrint = () => {
        const printFields = getPrintFields();
        printFormResponse(
            schema.title,
            new Date(response.submittedAt).toLocaleString('ar-EG'),
            printFields,
            'rtl'
        );
    };

    const handleDownloadPDF = async () => {
        setIsGeneratingPDF(true);
        try {
            const printFields = getPrintFields();
            await generateFormPDF(
                schema.title,
                new Date(response.submittedAt).toLocaleString('ar-EG'),
                printFields,
                'rtl'
            );
        } catch (error) {
            console.error('Error generating PDF:', error);
        } finally {
            setIsGeneratingPDF(false);
        }
    };

    const footer = (
        <div className="flex gap-3 w-full">
            <Button
                onClick={handleDownloadPDF}
                disabled={isGeneratingPDF}
                className="flex-1 h-12 rounded-xl shadow-lg shadow-primary/20 gap-2 font-bold"
            >
                {isGeneratingPDF ? (
                    <>
                        <Loader2 className="w-5 h-5 animate-spin" /> جاري التحميل...
                    </>
                ) : (
                    <>
                        <Download className="w-5 h-5" /> تحميل PDF
                    </>
                )}
            </Button>
            <Button
                onClick={handlePrint}
                variant="outline"
                className="flex-1 h-12 rounded-xl font-bold gap-2"
            >
                <Printer className="w-5 h-5" /> طباعة
            </Button>
            <Button variant="ghost" onClick={onClose} className="h-12 rounded-xl font-bold gap-2 px-6">
                إغلاق
            </Button>
        </div>
    );

    return (
        <BaseModal
            isOpen={isOpen}
            onClose={onClose}
            title={schema.title}
            maxWidth="xl"
            footer={footer}
        >
            <div className="space-y-8 text-right" dir="rtl">
                {/* Header */}
                <div className="text-center border-b-2 border-gray-100 pb-6 mb-8">
                    <h1 className="text-2xl font-bold mb-2 ">{schema.title}</h1>
                    <p className="text-gray-500 font-medium">تاريخ التقديم: {new Date(response.submittedAt).toLocaleString('ar-EG')}</p>
                    <p className="text-xs  mt-2">
                        يتم عرض {visibleFields.length} حقل من أصل {schema.fields.length} (بناءً على الشروط المنطقية)
                    </p>
                </div>

                {/* Data Display - Only showing visible fields based on logic */}
                <div className="grid grid-cols-12 gap-x-8 gap-y-6">
                    {visibleFields.map(({ field, displayValue }) => {
                        const colSpan = field.layout?.colSpan || 12;

                        const spanMap = {
                            12: 'col-span-12',
                            6: 'col-span-12 md:col-span-6',
                            4: 'col-span-12 md:col-span-4',
                            3: 'col-span-12 md:col-span-3',
                        };

                        const spanClass = spanMap[colSpan as keyof typeof spanMap] || 'col-span-12';

                        return (
                            <div key={field.id} className={`${spanClass} border-b border-gray-100 pb-2`}>
                                <div className="flex flex-row items-baseline gap-3">
                                    <span className="text-base font-bold  whitespace-nowrap min-w-fit">
                                        {field.label}:
                                    </span>
                                    <span className="text-base font-normal  break-words">
                                        {displayValue}
                                    </span>
                                </div>
                            </div>
                        );
                    })}
                </div>

                {visibleFields.length === 0 && (
                    <div className="text-center py-8 text-gray-400">
                        لا توجد بيانات لعرضها
                    </div>
                )}
            </div>
        </BaseModal>
    );
};
