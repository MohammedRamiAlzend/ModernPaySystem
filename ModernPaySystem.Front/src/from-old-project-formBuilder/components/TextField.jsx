import React from 'react';
import Input from '../../components/common/Input';

const TextField = ({ field, value, onChange, error, disabled, readOnly }) => {
    const handleChange = (e) => {
        onChange(e.target.value);
    };

    const getInputType = () => {
        switch (field.type) {
            case 'email':
                return 'email';
            case 'password':
                return 'password';
            case 'number':
                return 'number';
            default:
                return 'text';
        }
    };

    return (
        <Input
            label={field.label}
            id={field.id}
            type={getInputType()}
            value={value || ''}
            onChange={handleChange}
            placeholder={field.placeholder}
            error={error}
            disabled={disabled || readOnly}
            isReadOnly={readOnly}
            required={field.validation && field.validation.some(v => v.rule === 'required')}
        />
    );
};

export default TextField;
