import React from 'react';
import { FieldRenderer } from './FieldRenderer';
import { Button } from '@/shared/ui/button';
import type { FormSchema } from '@/entities/form/model/types';
import { useFormRenderer } from '../model/useFormRenderer';
import { Save } from 'lucide-react';

interface FormRendererProps {
  schema: FormSchema;
  onSubmit: (data: Record<string, any>) => void;
  initialData?: Record<string, any>;
  readOnly?: boolean;
}

export const FormRenderer: React.FC<FormRendererProps> = ({ schema, onSubmit, initialData, readOnly }) => {
  const {
    formData,
    errors,
    fieldStates,
    isSubmitted,
    handleChange,
    handleSubmit
  } = useFormRenderer(schema, onSubmit, initialData);

  const renderFields = () => {
    return schema.fields
      .filter(field => fieldStates[field.name]?.visible !== false)
      .map(field => {
        const fieldError = errors[field.name];
        const isEnabled = (fieldStates[field.name]?.enabled !== false) && !field.readOnly;
        const colSpan = field.layout?.colSpan || 12;

        const colSpanClasses: Record<number, string> = {
          12: 'md:col-span-12',
          6: 'md:col-span-6',
          4: 'md:col-span-4',
          3: 'md:col-span-3',
        };

        const spanClass = colSpanClasses[colSpan] || 'md:col-span-12';

        return (
          <div key={field.id} className={`col-span-12 ${spanClass} animate-in fade-in slide-in-from-bottom-2 duration-300`}>
            <FieldRenderer
              field={field}
              value={formData[field.name]}
              onChange={(value) => handleChange(field.name, value)}
              error={fieldError}
              disabled={!isEnabled || readOnly}
              readOnly={field.readOnly || readOnly}
            />
          </div>
        );
      });
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      <div className="grid grid-cols-12 gap-x-6 gap-y-4">
        {renderFields()}
      </div>

      {!readOnly && (
        <div className="flex items-center justify-end gap-4 pt-6 border-t mt-8">
          <Button
            type="submit"
            disabled={isSubmitted}
            className={`gap-2 px-8 py-6 text-lg font-bold shadow-lg rounded-xl transition-all hover:scale-[1.02] active:scale-[0.98] ${isSubmitted ? 'bg-muted hover:bg-muted font-normal' : 'shadow-primary/20'}`}
          >
            {isSubmitted ? (
              <>جاري المعالجة...</>
            ) : (
              <>
                <Save className="w-5 h-5" /> حفظ البيانات
              </>
            )}
          </Button>
        </div>
      )}
    </form>
  );
};
