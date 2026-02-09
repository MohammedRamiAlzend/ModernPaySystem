import React from 'react';
import { BaseModal } from './base-modal';
import { Scan } from 'lucide-react';
import { Button } from '../button';
import { X } from 'lucide-react';

interface OcrModalWrapperProps {
    isOpen: boolean;
    onClose: () => void;
    title?: string;
    description?: string;
    children: React.ReactNode;
}

/**
 * OcrModal: A specialized modal wrapper for OCR and Document Scanning.
 * Located in shared/ui/modals as requested, to be used by various features.
 */
export const OcrModal: React.FC<OcrModalWrapperProps> = ({
    isOpen,
    onClose,
    title = 'استخراج النص وتحرير الصور',
    description = 'قم بمسح، قص واستخراج النصوص بسهولة',
    children
}) => {
    return (
        <BaseModal
            isOpen={isOpen}
            onClose={onClose}
            maxWidth="4xl"
            title={
                <div className="flex items-center gap-3">
                    <div className="w-10 h-10 rounded-xl bg-primary/10 flex items-center justify-center text-primary">
                        <Scan className="h-6 w-6" />
                    </div>
                    <div>
                        <h3 className='text-xl font-bold'>{title}</h3>
                        <p className="text-xs text-muted-foreground font-medium">{description}</p>
                    </div>
                </div>
            }
        // Custom Close button in header since BaseModal uses AlertDialog logic
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
