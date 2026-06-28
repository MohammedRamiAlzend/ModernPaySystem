import { useState } from 'react';
import { Button } from '@/shared/ui/button';
import { FileSpreadsheet, Loader2 } from 'lucide-react';

interface ExportButtonProps {
    onExport: () => Promise<void>;
    label?: string;
}

export function ExportButton({ onExport, label = 'تصدير Excel' }: ExportButtonProps) {
    const [exporting, setExporting] = useState(false);

    const handleExport = async () => {
        setExporting(true);
        try {
            await onExport();
        } catch (error) {
            console.error('Export failed:', error);
        } finally {
            setExporting(false);
        }
    };

    return (
        <Button
            variant="outline"
            size="sm"
            onClick={handleExport}
            disabled={exporting}
            className="border-emerald-500/30 hover:bg-emerald-500/10 hover:text-emerald-600 hover:border-emerald-500/50 text-emerald-600 dark:text-emerald-400 transition-all"
        >
            {exporting ? (
                <Loader2 className="w-4 h-4 ml-2 animate-spin" />
            ) : (
                <FileSpreadsheet className="w-4 h-4 ml-2" />
            )}
            <span>{exporting ? 'جاري التصدير...' : label}</span>
        </Button>
    );
}
