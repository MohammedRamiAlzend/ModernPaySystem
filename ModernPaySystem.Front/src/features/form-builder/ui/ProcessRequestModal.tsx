import React from 'react';
import { BaseModal } from '@/shared/ui/modals/base-modal';
import { SelectedRequestPreview } from './SelectedRequestPreview';
import { ResponseForm } from './ResponseForm';
import type { FormSchema, TemplateRequest } from '@/entities/form/model/types';
import { Button } from '@/shared/ui/button';

interface ProcessRequestModalProps {
    isOpen: boolean;
    onClose: () => void;
    request: TemplateRequest | null;
    template: FormSchema | null;
    // Form Props
    comment: string;
    files: File[];
    isPending: boolean;
    onCommentChange: (value: string) => void;
    onFileChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
    onFilesAdd: (files: File[]) => void;
    onRemoveFile: (index: number) => void;
    submissionMode: 'submit' | 'referral';
    onSubmissionModeChange: (mode: 'submit' | 'referral') => void;
    targetUserId: string;
    onTargetUserChange: (userId: string) => void;
    onSubmit: () => void;
}

export const ProcessRequestModal = ({
    isOpen,
    onClose,
    request,
    template,
    comment,
    files,
    isPending,
    onCommentChange,
    onFileChange,
    onFilesAdd,
    onRemoveFile,
    submissionMode,
    onSubmissionModeChange,
    targetUserId,
    onTargetUserChange,
    onSubmit
}: ProcessRequestModalProps) => {
    if (!request || !template) return null;

    return (
        <BaseModal
            isOpen={isOpen}
            onClose={onClose}
            title={`معالجة الطلب: ${template.title}`}
            maxWidth="4xl"
            maxHeight="4xl"
            footer={
                <div className="flex justify-end gap-3 w-full">
                    <Button variant="outline" onClick={onClose} className="px-8 rounded-xl font-bold">إغلاق</Button>
                </div>
            }
        >
            <div className="flex flex-col lg:grid lg:grid-cols-12 gap-6 lg:h-[75vh]" dir="rtl">
                {/* Details Section (60%) */}
                <div className="lg:col-span-7 h-[600px] lg:h-full">
                    <SelectedRequestPreview request={request} template={template} />
                </div>

                {/* Response Form Section (40%) */}
                <div className="lg:col-span-5 h-[500px] lg:h-full pt-4 lg:pt-0 border-t lg:border-t-0">
                    <ResponseForm
                        requestId={request.id}
                        comment={comment}
                        files={files}
                        isPending={isPending}
                        onCommentChange={onCommentChange}
                        onFileChange={onFileChange}
                        onFilesAdd={onFilesAdd}
                        onRemoveFile={onRemoveFile}
                        submissionMode={submissionMode}
                        onSubmissionModeChange={onSubmissionModeChange}
                        targetUserId={targetUserId}
                        onTargetUserChange={onTargetUserChange}
                        onSubmit={onSubmit}
                    />
                </div>
            </div>
        </BaseModal>
    );
};
