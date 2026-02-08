import { ActionModal } from './base-modal';
import { Trash2 } from 'lucide-react';

interface DeleteConfirmModalProps {
    isOpen: boolean;
    onClose: () => void;
    onConfirm: () => void;
    title?: string;
    description?: string;
    isLoading?: boolean;
}

/**
 * DeleteConfirmModal: A specialized version of ActionModal for deletion.
 */
export const DeleteConfirmModal: React.FC<DeleteConfirmModalProps> = ({
    isOpen,
    onClose,
    onConfirm,
    title = 'هل أنت متأكد من حذف هذا العنصر؟',
    description = 'هذا الإجراء نهائي ولا يمكن التراجع عنه بعد الحذف.',
    isLoading = false
}) => {
    return (
        <ActionModal
            isOpen={isOpen}
            onClose={onClose}
            onConfirm={onConfirm}
            title={title}
            description={description}
            confirmLabel="تأكيد الحذف"
            variant="destructive"
            isLoading={isLoading}
            icon={<Trash2 className="w-6 h-6" />}
        />
    );
};
