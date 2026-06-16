import React from 'react';
import { BaseModal } from './base-modal';
import { Scan, X } from 'lucide-react';
import { Button } from '../button';

interface OcrModalProps {
    isOpen: boolean;
    onClose: () => void;
    title?: string;
    description?: string;
    headerActions?: React.ReactNode;
    children: React.ReactNode;
}

/**
 * OcrModal: A specialized modal wrapper for OCR and Document Scanning.
 * Located in shared/ui/modals - acts as a pure shell/container.
 * 
 * This modal does NOT import any features. The feature content
 * should be passed as children from the Widget/Page layer.
 * 
 * Usage:
 * ```tsx
 * <OcrModal isOpen={isOpen} onClose={handleClose}>
 *   <OcrScannerContent {...props} />
 * </OcrModal>
 * ```
 */
export const OcrModal: React.FC<OcrModalProps> = ({
    isOpen,
    onClose,
    title = 'استخراج النص وتحرير الصور',
    description = 'قم بمسح، قص واستخراج النصوص بسهولة',
    headerActions,
    children
}) => {
    return (
        <BaseModal
            isOpen={isOpen}
            onClose={onClose}
            maxWidth="3xl"
            maxHeight="3xl"
            title={
                <div className="flex items-center justify-between w-full">
                    <div className="flex items-center gap-3">
                        <div className="w-8 h-8 rounded-lg bg-primary/10 flex items-center justify-center text-primary">
                            <Scan className="h-5 w-5" />
                        </div>
                        <div>
                            <h3 className='text-lg font-bold'>{title}</h3>
                            <p className="text-[10px] text-muted-foreground font-medium">{description}</p>
                        </div>
                    </div>
                    {headerActions && (
                        <div className="mr-auto pl-8">
                            {headerActions}
                        </div>
                    )}
                </div>
            }
        >
            <div className="relative">
                <Button
                    variant='ghost'
                    size="icon"
                    onClick={onClose}
                    className="absolute -top-16 left-0 rounded-full hover:bg-muted"
                >
                    <X className="h-5 w-5" />
                </Button>
                {children}
            </div>
        </BaseModal>
    );
};
