import React from 'react';

const RadioButtonField = ({ field, value, onChange, error, disabled, readOnly }) => {
    const options = field.dataSource?.options || [];

    const handleChange = (val) => {
        if (disabled || readOnly) return;
        onChange(val);
    };

    return (
        <div className="flex flex-col space-y-3">
            {field.label && (
                <label className="block text-sm font-medium text-text">
                    {field.label}
                    {field.validation && field.validation.some(v => v.rule === 'required') && (
                        <span className="text-error mr-1">*</span>
                    )}
                </label>
            )}

            <div className={`flex flex-wrap gap-4 ${field.layout === 'vertical' ? 'flex-col' : 'flex-row'}`}>
                {options.map((option) => (
                    <div
                        key={option.value}
                        onClick={() => handleChange(option.value)}
                        className={`
              flex items-center space-x-2 space-x-reverse cursor-pointer group
              ${disabled || readOnly ? 'opacity-50 cursor-not-allowed' : ''}
            `}
                    >
                        <div className={`
              relative w-5 h-5 rounded-full border-2 transition-all duration-200 flex items-center justify-center
              ${value === option.value
                                ? 'border-secondary'
                                : 'border-border group-hover:border-primary'
                            }
            `}>
                            {value === option.value && (
                                <div className="w-2.5 h-2.5 rounded-full bg-secondary animate-scale-in" />
                            )}
                        </div>
                        <span className={`text-sm transition-colors ${value === option.value ? 'text-secondary font-bold' : 'text-text group-hover:text-primary'
                            }`}>
                            {option.label}
                        </span>
                    </div>
                ))}
            </div>

            {error && <p className="text-sm text-error">{error}</p>}
        </div>
    );
};

export default RadioButtonField;
