import React from 'react';

const CheckboxField = ({ field, value, onChange, error, disabled, readOnly }) => {
    const handleChange = (e) => {
        onChange(e.target.checked);
    };

    return (
        <div className="flex flex-col space-y-2">
            <div className="flex items-center space-x-3 space-x-reverse cursor-pointer group">
                <div className="relative flex items-center">
                    <input
                        id={field.id}
                        type="checkbox"
                        checked={!!value}
                        onChange={handleChange}
                        disabled={disabled || readOnly}
                        className={`
              h-5 w-5 rounded border-2 transition-all duration-200
              ${value
                                ? 'bg-secondary border-secondary text-white'
                                : 'bg-background border-border hover:border-primary'
                            }
              ${disabled || readOnly ? 'opacity-50 cursor-not-allowed' : 'cursor-pointer'}
              focus:ring-2 focus:ring-secondary/50 focus:ring-offset-0
            `}
                    />
                </div>
                {field.label && (
                    <label
                        htmlFor={field.id}
                        className={`text-sm font-medium transition-colors ${disabled || readOnly ? 'text-secondary/50' : 'text-text group-hover:text-primary cursor-pointer'
                            }`}
                    >
                        {field.label}
                        {field.validation && field.validation.some(v => v.rule === 'required') && (
                            <span className="text-error mr-1">*</span>
                        )}
                    </label>
                )}
            </div>
            {error && <p className="text-sm text-error mr-8">{error}</p>}
        </div>
    );
};

export default CheckboxField;
