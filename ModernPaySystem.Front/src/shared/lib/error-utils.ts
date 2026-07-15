export const extractErrorMessage = (error: any, fallback?: string): string => {
    return (
        error?.response?.data?.errors?.[0]?.arabicDescription
        ?? error?.response?.data?.errors?.[0]?.description
        ?? error?.response?.data?.message
        ?? error?.data?.message
        ?? error?.message
        ?? fallback
        ?? 'حدث خطأ غير متوقع'
    );
};
