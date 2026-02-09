import React from 'react';
import { Button } from '@/shared/ui/button';
import { Scan, Replace } from 'lucide-react';
import { cn } from '@/shared/lib/utils';

interface OcrToolbarProps {
    onExtract: () => void;
    onInsertUnder: () => void;
    onReplaceSelection: () => void;
    isLoading: boolean;
    disabled?: boolean;
}

export const OcrToolbar: React.FC<OcrToolbarProps> = ({
    onExtract,
    onInsertUnder,
    onReplaceSelection,
    isLoading,
    disabled = false,
}) => {
    return (
        <div className="grid grid-cols-2 lg:grid-cols-3 gap-2">
            <Button
                onClick={onExtract}
                disabled={isLoading || disabled}
                className="rounded-xl text-xs h-10 gap-2"
            >
                <Scan className={cn("h-4 w-4", isLoading && "animate-spin")} />
                {isLoading ? 'جاري الاستخراج...' : 'استخراج جديد'}
            </Button>
            <Button
                variant="secondary"
                onClick={onInsertUnder}
                disabled={isLoading || disabled}
                className="rounded-xl text-xs h-10"
            >
                إدراج تحت النص
            </Button>
            <Button
                variant="outline"
                onClick={onReplaceSelection}
                disabled={isLoading || disabled}
                className="rounded-xl text-xs h-10 gap-2 col-span-2 lg:col-span-1 border-2"
            >
                <Replace className="h-4 w-4" />
                استبدال المحدد
            </Button>
        </div>
    );
};
