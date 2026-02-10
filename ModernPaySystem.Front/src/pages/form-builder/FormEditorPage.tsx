
import { FormEditor } from '@/widgets/form-editor/ui/FormEditor';
import { AnimatedContainer } from '@/shared/ui/common/animated-container';
import { useNavigate, useParams } from 'react-router-dom';
import { useForms } from '@/features/form-builder/model/useForms';
import { useMemo } from 'react';

export const FormEditorPage = () => {
    const navigate = useNavigate();
    const { id } = useParams();
    const { data: forms = [] } = useForms();

    const currentForm = useMemo(() => {
        if (!id || id === 'new') return undefined;
        return forms.find(f => f.id === id);
    }, [id, forms]);

    const handleSaveComplete = () => {
        navigate('/form-builder/templates');
    };

    return (
        <AnimatedContainer className="min-h-screen pb-12">
            <div className="container mx-auto p-6">
                <FormEditor
                    key={currentForm?.id || 'new'} // Force re-mount when form changes
                    initialForm={currentForm}
                    onSave={handleSaveComplete}
                    onCancel={() => navigate('/form-builder/templates')}
                />
            </div>
        </AnimatedContainer>
    );
};
