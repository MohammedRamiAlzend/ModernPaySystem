import React from 'react';
import { Languages, ChevronDown, Loader2 } from 'lucide-react';
import { Language } from '../model/types';

interface LanguageSelectorProps {
    value: string;
    onChange: (value: string) => void;
    languages: Language[];
    isLoading?: boolean;
}

export const LanguageSelector: React.FC<LanguageSelectorProps> = ({
    value,
    onChange,
    languages,
    isLoading
}) => {
    return (
        <div className='flex items-center gap-4 bg-muted/20 p-3 rounded-2xl border'>
            <div className="flex items-center gap-2">
                {isLoading ? (
                    <Loader2 className="h-4 w-4 animate-spin text-primary" />
                ) : (
                    <Languages className="h-4 w-4 text-primary" />
                )}
                <span className="text-xs font-bold">لغة الاستخراج:</span>
            </div>
            <div className="relative group">
                <select
                    className="appearance-none bg-background border-2 rounded-xl px-4 py-1.5 pr-10 text-xs font-bold focus:ring-2 ring-primary transition-all outline-none cursor-pointer disabled:opacity-50"
                    value={value}
                    onChange={(e) => onChange(e.target.value)}
                    disabled={isLoading || languages.length === 0}
                >
                    {languages.map((lang) => (
                        <option key={lang.code} value={lang.code}>
                            {lang.name}
                        </option>
                    ))}
                    {!isLoading && languages.length === 0 && (
                        <option value="">لا توجد لغات</option>
                    )}
                </select>
                <ChevronDown className="absolute left-3 top-1/2 -translate-y-1/2 h-3 w-3 pointer-events-none opacity-50" />
            </div>
        </div>
    );
};
