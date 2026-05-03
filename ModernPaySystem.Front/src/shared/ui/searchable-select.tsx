import { useState, useMemo } from 'react';
import { Check, ChevronsUpDown, X, Search, Loader2 } from 'lucide-react';
import { cn } from '@/shared/lib/utils';
import { Button } from '@/shared/ui/button';
import { Badge } from '@/shared/ui/badge';
import {
    Popover,
    PopoverContent,
    PopoverTrigger,
} from '@/shared/ui/popover';
import {
    Command,
    CommandGroup,
    CommandItem,
    CommandList,
} from '@/shared/ui/command';

// ─── Types ───────────────────────────────────────────────────────────────────

export interface SearchableSelectOption {
    /** Unique identifier */
    value: string;
    /** Display label (used for text search) */
    label: string;
    /** Optional numeric order/index (used for numeric search) */
    order?: number;
    /** Optional icon to show beside the item */
    icon?: React.ReactNode;
    /** Any extra data the consumer might need */
    meta?: Record<string, unknown>;
}

interface SearchableSelectBaseProps {
    /** Items to display */
    options: SearchableSelectOption[];
    /** Placeholder text shown when nothing is selected */
    placeholder?: string;
    /** Placeholder for the search input */
    searchPlaceholder?: string;
    /** Message shown when search returns no results */
    emptyMessage?: string;
    /** Whether the component is in a loading state */
    isLoading?: boolean;
    /** Whether the component is disabled */
    disabled?: boolean;
    /** Additional class names */
    className?: string;
}

interface SingleSelectProps extends SearchableSelectBaseProps {
    multiple?: false;
    /** Currently selected value (single mode) */
    value?: string;
    /** Callback when a value is selected (single mode) */
    onValueChange: (value: string) => void;
    /** Not used in single mode */
    values?: never;
    /** Not used in single mode */
    onValuesChange?: never;
}

interface MultiSelectProps extends SearchableSelectBaseProps {
    multiple: true;
    /** Currently selected values (multi mode) */
    values: string[];
    /** Callback when values change (multi mode) */
    onValuesChange: (values: string[]) => void;
    /** Not used in multi mode */
    value?: never;
    /** Not used in multi mode */
    onValueChange?: never;
}

export type SearchableSelectProps = SingleSelectProps | MultiSelectProps;

// ─── Custom filter ───────────────────────────────────────────────────────────
// If the user types a number, filter by `order`. Otherwise filter by `label`.

function filterOption(
    option: SearchableSelectOption,
    search: string
): boolean {
    if (!search) return true;
    const trimmed = search.trim();
    const isNumeric = /^\d+$/.test(trimmed);

    if (isNumeric && option.order !== undefined) {
        return String(option.order).startsWith(trimmed);
    }

    return option.label.toLowerCase().includes(trimmed.toLowerCase());
}

// ─── Component ───────────────────────────────────────────────────────────────

export const SearchableSelect = (props: SearchableSelectProps) => {
    const {
        options,
        placeholder = 'اختر...',
        searchPlaceholder = 'ابحث بالاسم أو الرقم...',
        emptyMessage = 'لا توجد نتائج.',
        isLoading = false,
        disabled = false,
        className,
    } = props;

    const [open, setOpen] = useState(false);
    const [search, setSearch] = useState('');

    const isMulti = props.multiple === true;

    // Derive selected set for quick lookups
    const selectedSet = useMemo(() => {
        if (isMulti && props.values) return new Set(props.values);
        if (!isMulti && props.value) return new Set([props.value]);
        return new Set<string>();
    }, [isMulti, props.values, props.value]);

    // Filter options based on search
    const filteredOptions = useMemo(
        () => options.filter((o) => filterOption(o, search)),
        [options, search]
    );

    // Derive trigger label
    const triggerLabel = useMemo(() => {
        if (isMulti) {
            if (props.values.length === 0) return null;
            return `${props.values.length} محدد`;
        }
        if (!props.value) return null;
        return options.find((o) => o.value === props.value)?.label;
    }, [isMulti, options, props.values, props.value]);

    // Resolve labels for selected items (multi)
    const selectedItems = useMemo(() => {
        if (!isMulti || !props.values) return [];
        return props.values
            .map((v) => options.find((o) => o.value === v))
            .filter(Boolean) as SearchableSelectOption[];
    }, [isMulti, props.values, options]);

    // ── Handlers ──

    const handleSelect = (value: string) => {
        if (isMulti) {
            const current = props.values;
            if (current.includes(value)) {
                props.onValuesChange(current.filter((v) => v !== value));
            } else {
                props.onValuesChange([...current, value]);
            }
        } else {
            const newValue = value === props.value ? '' : value;
            props.onValueChange(newValue);
            setOpen(false);
        }
        setSearch('');
    };

    const handleRemove = (value: string) => {
        if (isMulti) {
            props.onValuesChange(props.values.filter((v) => v !== value));
        }
    };

    // ── Render ──

    return (
        <div className={cn('space-y-2', className)}>
            <Popover open={open} onOpenChange={setOpen}>
                <PopoverTrigger asChild>
                    <Button
                        variant="outline"
                        role="combobox"
                        aria-expanded={open}
                        disabled={disabled || isLoading}
                        className={cn(
                            'w-full justify-between h-10 rounded-xl bg-background/50 backdrop-blur-sm border-primary/10 hover:bg-background/80 font-normal text-right',
                            !triggerLabel && 'text-muted-foreground'
                        )}
                    >
                        <span className="truncate" title={typeof triggerLabel === 'string' ? triggerLabel : undefined}>
                            {isLoading ? (
                                <span className="flex items-center gap-2">
                                    <Loader2 className="w-4 h-4 animate-spin" />
                                    جاري التحميل...
                                </span>
                            ) : (
                                triggerLabel || placeholder
                            )}
                        </span>
                        <ChevronsUpDown className="h-4 w-4 shrink-0 opacity-50" />
                    </Button>
                </PopoverTrigger>

                <PopoverContent
                    className="w-[--radix-popover-trigger-width] p-0 rounded-xl border-primary/10"
                    align="start"
                    dir="rtl"
                >
                    <Command shouldFilter={false}>
                        <div className="flex items-center border-b px-3" cmdk-input-wrapper="">
                            <Search className="ml-2 h-4 w-4 shrink-0 opacity-50" />
                            <input
                                value={search}
                                onChange={(e) => setSearch(e.target.value)}
                                placeholder={searchPlaceholder}
                                className="flex h-10 w-full rounded-md bg-transparent py-3 text-sm outline-none placeholder:text-muted-foreground disabled:cursor-not-allowed disabled:opacity-50 text-right"
                                dir="rtl"
                            />
                        </div>
                        <CommandList>
                            {!isLoading && filteredOptions.length === 0 && (
                                <div className="py-6 text-center text-sm text-muted-foreground">
                                    {emptyMessage}
                                </div>
                            )}
                            {isLoading && (
                                <div className="py-6 flex items-center justify-center gap-2 text-sm text-muted-foreground">
                                    <Loader2 className="w-4 h-4 animate-spin" />
                                    جاري التحميل...
                                </div>
                            )}
                            <CommandGroup>
                                {filteredOptions.map((option) => {
                                    const isSelected = selectedSet.has(option.value);
                                    return (
                                        <CommandItem
                                            key={option.value}
                                            value={option.value}
                                            onSelect={() => handleSelect(option.value)}
                                            className="flex items-center gap-2 text-right cursor-pointer"
                                        >
                                            <Check
                                                className={cn(
                                                    'h-4 w-4 shrink-0',
                                                    isSelected ? 'opacity-100 text-primary' : 'opacity-0'
                                                )}
                                            />
                                            {option.icon && (
                                                <span className="shrink-0">{option.icon}</span>
                                            )}
                                            <span className="flex-1 truncate" title={option.label}>{option.label}</span>
                                            {option.order !== undefined && (
                                                <span className="text-[10px] text-muted-foreground font-mono bg-muted/50 px-1.5 py-0.5 rounded">
                                                    #{option.order}
                                                </span>
                                            )}
                                        </CommandItem>
                                    );
                                })}
                            </CommandGroup>
                        </CommandList>
                    </Command>
                </PopoverContent>
            </Popover>

            {/* Multi-select badges */}
            {isMulti && selectedItems.length > 0 && (
                <div className="flex flex-wrap gap-1.5 p-2 rounded-xl bg-muted/30 border border-dashed border-primary/10 animate-in fade-in-50">
                    {selectedItems.map((item) => (
                        <Badge
                            key={item.value}
                            variant="secondary"
                            className="pl-1.5 pr-2 py-1 gap-1.5 flex items-center bg-background/80 backdrop-blur-sm border-primary/10 hover:bg-background transition-colors text-xs"
                        >
                            {item.icon && <span className="shrink-0">{item.icon}</span>}
                            <span className="font-medium">{item.label}</span>
                            {item.order !== undefined && (
                                <span className="text-[9px] text-muted-foreground font-mono">#{item.order}</span>
                            )}
                            <button
                                onClick={(e) => {
                                    e.stopPropagation();
                                    handleRemove(item.value);
                                }}
                                className="hover:text-destructive transition-colors p-0.5 rounded-full hover:bg-destructive/10"
                            >
                                <X className="w-3 h-3" />
                            </button>
                        </Badge>
                    ))}
                </div>
            )}
        </div>
    );
};
