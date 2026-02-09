import { useRef, useImperativeHandle, forwardRef } from 'react';

interface OcrTextAreaRef {
    getSelection: () => { start: number; end: number };
    focus: () => void;
    setCaretPosition: (pos: number) => void;
}

interface OcrTextAreaProps {
    value: string;
    onChange: (value: string) => void;
    isLoading?: boolean;
    placeholder?: string;
}

export const OcrTextArea = forwardRef<OcrTextAreaRef, OcrTextAreaProps>(({
    value,
    onChange,
    isLoading = false,
    placeholder = "سيظهر النص المستخرج هنا...",
}, ref) => {
    const textAreaRef = useRef<HTMLTextAreaElement>(null);

    useImperativeHandle(ref, () => ({
        getSelection: () => {
            const ta = textAreaRef.current;
            return { start: ta?.selectionStart ?? 0, end: ta?.selectionEnd ?? 0 };
        },
        focus: () => textAreaRef.current?.focus(),
        setCaretPosition: (pos: number) => {
            if (textAreaRef.current) {
                textAreaRef.current.focus();
                textAreaRef.current.setSelectionRange(pos, pos);
            }
        },
    }));

    return (
        <div className="flex flex-col gap-3">
            <span className="text-xs font-bold text-muted-foreground px-1">
                النص المستخلص (قابل للتحرير)
            </span>
            <div className="flex-1 relative flex flex-col">
                <textarea
                    className="flex-1 w-full bg-accent/10 rounded-3xl p-5 text-sm font-medium focus:ring-2 ring-primary/20 transition-all outline-none border-2 border-transparent focus:border-primary/20 resize-none leading-relaxed min-h-[300px]"
                    placeholder={placeholder}
                    value={value}
                    ref={textAreaRef}
                    onChange={(e) => onChange(e.target.value)}
                />
                {isLoading && (
                    <div className="absolute inset-0 bg-background/50 backdrop-blur-[1px] flex items-center justify-center rounded-3xl">
                        <div className="flex flex-col items-center gap-2">
                            <div className="w-8 h-8 rounded-full border-4 border-primary/20 border-t-primary animate-spin" />
                            <span className="text-[10px] font-bold text-primary animate-pulse">جاري المعالجة الرقمية...</span>
                        </div>
                    </div>
                )}
            </div>
        </div>
    );
});

OcrTextArea.displayName = 'OcrTextArea';

export type { OcrTextAreaRef };
