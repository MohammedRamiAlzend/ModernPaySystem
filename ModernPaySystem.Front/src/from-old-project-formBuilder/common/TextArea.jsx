import { memo, forwardRef } from 'react';

const TextArea = memo(forwardRef(({
    label,
    error,
    helperText,
    size = 'md',
    variant = 'default',
    className = '',
    containerClassName = '',
    rows = 4,
    isReadOnly = false,
    ...props
}, ref) => {

    // تحديد الأحجام
    const sizes = {
        sm: 'px-3 py-1.5 text-sm',
        md: 'px-4 py-2 text-base',
        lg: 'px-6 py-3 text-lg',
    };

    // تحديد الأنماط
    const variants = {
        default: 'border-border focus:border-primary focus:ring-primary/50',
        error: 'border-error focus:border-error focus:ring-error/50',
    };

    // الأنماط الأساسية
    const baseClasses = `
    w-full rounded-lg border-2
    bg-background text-text
    placeholder:text-secondary
    transition-colors duration-200
    focus:outline-none focus:ring-2 focus:ring-offset-0
    disabled:opacity-50 disabled:cursor-not-allowed
    resize-none
    ${sizes[size]}
    ${error ? variants.error : variants.default}
    ${className}
    ${isReadOnly ? 'cursor-not-allowed' : ''}
  `.trim();

    return (
        <div className={`space-y-2 ${containerClassName}`}>
            {label && (
                <label className="block text-sm font-medium text-text">
                    {label}
                </label>
            )}

            <div className="relative">
                <textarea
                    ref={ref}
                    rows={rows}
                    className={baseClasses}
                    readOnly={isReadOnly}
                    {...props}
                />
            </div>

            {error && (
                <p className="text-sm text-error">{error}</p>
            )}

            {helperText && !error && (
                <p className="text-sm text-secondary">{helperText}</p>
            )}
        </div>
    );
}));

TextArea.displayName = 'TextArea';

export default TextArea;
