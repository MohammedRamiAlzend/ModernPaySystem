import React from 'react';

interface ResponseDetailsHeaderProps {
    title: string;
    submittedAt: string;
    visibleCount: number;
    totalCount: number;
}

export const ResponseDetailsHeader: React.FC<ResponseDetailsHeaderProps> = ({
    title,
    // submittedAt,
    visibleCount,
    totalCount
}) => {
    return (
        <div className="text-center border-b-2 border-gray-100 pb-6 mb-8">
            <h1 className="text-2xl font-bold mb-2">{title}</h1>
            {/* <p className="text-gray-500 font-medium">
                تاريخ التقديم: {new Date(submittedAt).toLocaleString('ar-EG')}
            </p> */}
            <p className="text-xs mt-2">
                يتم عرض {visibleCount} حقل من أصل {totalCount} (بناءً على الشروط المنطقية)
            </p>
        </div>
    );
};
