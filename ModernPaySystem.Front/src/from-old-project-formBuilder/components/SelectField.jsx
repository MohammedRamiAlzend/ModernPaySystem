import React, { useState, useEffect } from 'react';
import Select from '../../components/common/Select';

const SelectField = ({ field, value, onChange, error, disabled, readOnly }) => {
    const [options, setOptions] = useState([]);

    // Load options based on data source
    useEffect(() => {
        if (field.dataSource) {
            if (field.dataSource.type === 'static' && field.dataSource.options) {
                setOptions(field.dataSource.options);
            } else if (field.dataSource.type === 'api' && field.dataSource.url) {
                // In a real implementation, we would fetch from the API
                const fetchOptions = async () => {
                    setTimeout(() => {
                        setOptions([]);
                    }, 0);
                };
                fetchOptions();
            }
        } else {
            setOptions([]);
        }
    }, [field.dataSource]);

    const handleChange = (e) => {
        onChange(e.target.value);
    };

    return (
        <Select
            label={field.label}
            id={field.id}
            options={options}
            value={value || ''}
            onChange={handleChange}
            placeholder={field.placeholder || "اختر..."}
            error={error}
            disabled={disabled || readOnly}
            readonly={readOnly}
            required={field.validation && field.validation.some(v => v.rule === 'required')}
        />
    );
};

export default SelectField;
