import React from 'react';
import { OcrModal } from '@/shared/ui/modals/ocr-modal';
import { OcrScannerContent, OcrScannerContentProps } from './ocr-scanner-content';

interface ScannerModalProps extends OcrScannerContentProps {
    isOpen: boolean;
}

export const ScannerModal: React.FC<ScannerModalProps> = ({
    isOpen,
    onClose,
    ...contentProps
}) => {
    return (
        <OcrModal isOpen={isOpen} onClose={onClose || (() => { })}>
            <OcrScannerContent {...contentProps} onClose={onClose} />
        </OcrModal>
    );
};
