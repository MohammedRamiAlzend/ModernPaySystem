import { useState, useRef, type KeyboardEvent } from 'react';
import { X } from 'lucide-react';
import { cn } from '@/shared/lib/utils';

interface TagInputProps {
    value: string[];
    onChange: (tags: string[]) => void;
    disabled?: boolean;
    placeholder?: string;
}

export function TagInput({ value, onChange, disabled = false, placeholder = 'أضف لاحقة...' }: TagInputProps) {
    const [inputValue, setInputValue] = useState('');
    const inputRef = useRef<HTMLInputElement>(null);

    const normalizeTag = (raw: string): string | null => {
        let tag = raw.trim().toLowerCase();
        if (!tag) return null;
        if (!tag.startsWith('.')) tag = '.' + tag;
        // Validate: dot followed by alphanumeric only
        if (!/^\.[a-z0-9]+$/.test(tag)) return null;
        return tag;
    };

    const addTag = (raw: string) => {
        const tag = normalizeTag(raw);
        if (!tag) return;
        if (value.includes(tag)) return; // prevent duplicates
        onChange([...value, tag]);
        setInputValue('');
    };

    const removeTag = (index: number) => {
        onChange(value.filter((_, i) => i !== index));
    };

    const handleKeyDown = (e: KeyboardEvent<HTMLInputElement>) => {
        if (e.key === 'Enter' || e.key === ',') {
            e.preventDefault();
            addTag(inputValue);
        } else if (e.key === 'Backspace' && !inputValue && value.length > 0) {
            removeTag(value.length - 1);
        }
    };

    const handleBlur = () => {
        if (inputValue.trim()) {
            addTag(inputValue);
        }
    };

    return (
        <div
            className={cn(
                'flex flex-wrap items-center gap-1.5 min-h-[44px] w-full rounded-xl border border-input bg-background px-3 py-2',
                'ring-offset-background transition-all duration-200',
                'focus-within:ring-2 focus-within:ring-ring focus-within:ring-offset-2',
                disabled && 'cursor-not-allowed opacity-50'
            )}
            onClick={() => inputRef.current?.focus()}
        >
            {value.map((tag, index) => (
                <span
                    key={tag}
                    className={cn(
                        'inline-flex items-center gap-1 rounded-lg px-2.5 py-1 text-xs font-semibold',
                        'bg-primary/10 text-primary border border-primary/20',
                        'animate-in fade-in zoom-in-95 duration-200',
                        'transition-all hover:bg-primary/15'
                    )}
                >
                    <span dir="ltr" className="font-mono">{tag}</span>
                    {!disabled && (
                        <button
                            type="button"
                            onClick={(e) => {
                                e.stopPropagation();
                                removeTag(index);
                            }}
                            className="rounded-full p-0.5 hover:bg-primary/20 transition-colors"
                        >
                            <X className="h-3 w-3" />
                        </button>
                    )}
                </span>
            ))}
            {!disabled && (
                <input
                    ref={inputRef}
                    type="text"
                    value={inputValue}
                    onChange={(e) => setInputValue(e.target.value)}
                    onKeyDown={handleKeyDown}
                    onBlur={handleBlur}
                    placeholder={value.length === 0 ? placeholder : ''}
                    className="flex-1 min-w-[80px] bg-transparent text-sm outline-none placeholder:text-muted-foreground"
                    disabled={disabled}
                    dir="ltr"
                />
            )}
        </div>
    );
}
