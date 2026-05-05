import { useTemplateById } from '../api/formEndpoints';

interface TemplateTitleProps {
    templateId: string;
    fallbackTitle: string;
}

export const TemplateTitle = ({ templateId, fallbackTitle }: TemplateTitleProps) => {
    const { data: template } = useTemplateById(templateId);
    return <>{template?.title || fallbackTitle}</>;
};
