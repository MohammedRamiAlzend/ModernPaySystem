import React from 'react';

interface ResponseDetailsHeaderProps {
    title: string;
    submittedAt: string;
    visibleCount: number;
    totalCount: number;
    requestNumber?: number;
}

export const ResponseDetailsHeader: React.FC<ResponseDetailsHeaderProps> = ({
    title,
    // submittedAt,
    visibleCount,
    totalCount,
    requestNumber
}) => {
    return (
        <div className="text-center border-b-2 border-gray-100 pb-6 mb-8">
            <h1 className="text-2xl font-bold mb-2 flex items-center justify-center gap-2">
                {title}
                {requestNumber !== undefined && (
                    <span className="px-2 py-0.5 bg-primary/10 text-xs text-primary rounded-md whitespace-nowrap">
                        #{requestNumber}
                    </span>
                )}
            </h1>
            {/* <p className="text-gray-500 font-medium">
                تاريخ التقديم: {new Date(submittedAt).toLocaleString('ar-EG')}
            </p> */}
            <p className="text-xs mt-2">
                يتم عرض {visibleCount} حقل من أصل {totalCount} (بناءً على الشروط المنطقية)
            </p>
        </div>
    );
};
