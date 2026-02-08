import React from 'react';
import TextArea from '../common/TextArea';

const TextAreaField = ({ field, value, onChange, error, disabled, readOnly }) => {
    const handleChange = (e) => {
        onChange(e.target.value);
    };

    return (
        <TextArea
            label={field.label}
            id={field.id}
            value={value || ''}
            onChange={handleChange}
            placeholder={field.placeholder}
            error={error}
            disabled={disabled || readOnly}
            isReadOnly={readOnly}
            rows={field.rows || 4}
            required={field.validation && field.validation.some(v => v.rule === 'required')}
        />
    );
};

export default TextAreaField;
