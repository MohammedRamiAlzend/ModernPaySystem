import { useState } from 'react';
import type { FormSchema } from '@/entities/form/model/types';
import { FormEditor } from '@/widgets/form-editor/ui/FormEditor';
import { FormsList } from '@/widgets/forms-list/ui/FormsList';
import { AnimatedContainer } from '@/shared/ui/common/animated-container';

const FormBuilderPage = () => {
  const [view, setView] = useState<'list' | 'editor'>('list');
  const [currentForm, setCurrentForm] = useState<FormSchema | undefined>();

  const handleCreate = () => {
    setCurrentForm(undefined);
    setView('editor');
  };

  const handleEdit = (form: FormSchema) => {
    setCurrentForm(form);
    setView('editor');
  };

  const handleSaveComplete = () => {
    setView('list');
  };

  return (
    <AnimatedContainer className="min-h-screen pb-12">
      {view === 'editor' ? (
        <div className="container mx-auto p-6">
          <FormEditor
            initialForm={currentForm}
            onSave={handleSaveComplete}
            onCancel={() => setView('list')}
          />
        </div>
      ) : (
        <FormsList
          onEdit={handleEdit}
          onCreate={handleCreate}
        />
      )}
    </AnimatedContainer>
  );
};

export default FormBuilderPage;
