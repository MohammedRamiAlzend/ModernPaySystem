import { useEffect, useMemo, useState } from 'react';
import { archivingService } from '@/features/archiving/api/archivingService';
import type { SubFolderTreeNodeDto } from '@/features/archiving/model/types';
import { Label } from '@/shared/ui/label';
import { Folder, ChevronLeft, ChevronDown, Loader2, Check, Minus } from 'lucide-react';
import { cn } from '@/shared/lib/utils';

interface SubfolderTreeViewProps {
    folderId: string;
    selectedFolderIds: string[];
    onSelectionChange: (ids: string[]) => void;
}

type SelectionMode = 'this-only' | 'all' | 'custom';

export const SubfolderTreeView = ({
    folderId,
    selectedFolderIds,
    onSelectionChange,
}: SubfolderTreeViewProps) => {
    const [tree, setTree] = useState<SubFolderTreeNodeDto[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [mode, setMode] = useState<SelectionMode>('this-only');
    const [expandedIds, setExpandedIds] = useState<Set<string>>(new Set());

    useEffect(() => {
        if (!folderId) return;
        archivingService.getSubFolderTree(folderId)
            .then(data => {
                setTree(data);
                if (data.length > 0) {
                    setExpandedIds(new Set(data.map(n => n.id)));
                }
            })
            .catch(() => setError('فشل تحميل المجلدات الفرعية'))
            .finally(() => setLoading(false));
    }, [folderId]);

    const parentMap = useMemo(() => {
        const map = new Map<string, string>();
        const walk = (nodes: SubFolderTreeNodeDto[], parentId?: string) => {
            for (const node of nodes) {
                if (parentId !== undefined) {
                    map.set(node.id, parentId);
                }
                walk(node.children, node.id);
            }
        };
        walk(tree);
        return map;
    }, [tree]);

    const ancestorNodeIds = useMemo(() => {
        const ancestors = new Set<string>();
        const selectedSet = new Set(selectedFolderIds);

        const walk = (node: SubFolderTreeNodeDto): boolean => {
            let hasSelectedDescendant = false;
            for (const child of node.children) {
                if (walk(child)) {
                    hasSelectedDescendant = true;
                }
            }
            if (node.children.some(c => selectedSet.has(c.id))) {
                hasSelectedDescendant = true;
            }
            if (selectedSet.has(node.id) && hasSelectedDescendant) {
                ancestors.add(node.id);
            }
            return selectedSet.has(node.id) || hasSelectedDescendant;
        };

        for (const node of tree) {
            walk(node);
        }
        return ancestors;
    }, [tree, selectedFolderIds]);

    const getAllDescendantIds = (nodes: SubFolderTreeNodeDto[]): string[] => {
        const ids: string[] = [];
        for (const node of nodes) {
            ids.push(node.id);
            ids.push(...getAllDescendantIds(node.children));
        }
        return ids;
    };

    const handleModeChange = (newMode: SelectionMode) => {
        setMode(newMode);
        if (newMode === 'this-only') {
            onSelectionChange([]);
        } else if (newMode === 'all') {
            const allIds = getAllDescendantIds(tree);
            onSelectionChange(allIds);
        }
    };

    const toggleNode = (nodeId: string) => {
        setMode('custom');
        const isCurrentlyChecked = selectedFolderIds.includes(nodeId);

        if (isCurrentlyChecked) {
            onSelectionChange(selectedFolderIds.filter(id => id !== nodeId));
        } else {
            const newSet = new Set(selectedFolderIds);
            newSet.add(nodeId);

            let currentId = nodeId;
            while (parentMap.has(currentId)) {
                const parentId = parentMap.get(currentId)!;
                newSet.add(parentId);
                currentId = parentId;
            }

            onSelectionChange([...newSet]);
        }
    };

    const toggleExpand = (nodeId: string) => {
        setExpandedIds(prev => {
            const next = new Set(prev);
            if (next.has(nodeId)) next.delete(nodeId);
            else next.add(nodeId);
            return next;
        });
    };

    const renderNode = (node: SubFolderTreeNodeDto, depth: number = 0) => {
        const hasChildren = node.children.length > 0;
        const isExpanded = expandedIds.has(node.id);
        const isChecked = selectedFolderIds.includes(node.id);
        const isAncestor = ancestorNodeIds.has(node.id);

        return (
            <div key={node.id}>
                <div
                    className={cn(
                        'flex items-center gap-2 py-1.5 px-2 rounded-lg hover:bg-accent/50 transition-colors',
                        depth > 0 && 'mr-4'
                    )}
                    style={{ marginRight: `${depth * 16}px` }}
                >
                    {hasChildren ? (
                        <button
                            type="button"
                            onClick={() => toggleExpand(node.id)}
                            className="p-0.5 hover:bg-muted rounded shrink-0"
                        >
                            {isExpanded
                                ? <ChevronDown className="w-3.5 h-3.5 text-muted-foreground" />
                                : <ChevronLeft className="w-3.5 h-3.5 text-muted-foreground" />
                            }
                        </button>
                    ) : (
                        <div className="w-4 shrink-0" />
                    )}
                    <button
                        type="button"
                        onClick={() => toggleNode(node.id)}
                        className={cn(
                            'w-4 h-4 rounded border-2 flex items-center justify-center shrink-0 transition-colors',
                            isChecked && !isAncestor && 'bg-primary border-primary',
                            isAncestor && 'bg-primary/50 border-primary/50',
                            !isChecked && !isAncestor && 'border-muted-foreground/30'
                        )}
                    >
                        {isChecked && !isAncestor && <Check className="w-3 h-3 text-primary-foreground" />}
                        {isAncestor && <Minus className="w-3 h-3 text-primary-foreground" />}
                    </button>
                    <Folder className={cn(
                        'w-4 h-4 shrink-0',
                        isAncestor ? 'text-amber-500/50' : 'text-amber-500/80'
                    )} />
                    <span className={cn(
                        'text-sm truncate',
                        isAncestor && 'text-muted-foreground'
                    )}>
                        {node.name}
                    </span>
                </div>
                {hasChildren && isExpanded && (
                    <div>
                        {node.children.map(child => renderNode(child, depth + 1))}
                    </div>
                )}
            </div>
        );
    };

    if (loading) {
        return (
            <div className="flex items-center justify-center py-6 text-muted-foreground">
                <Loader2 className="w-5 h-5 animate-spin ml-2" />
                <span className="text-sm">جاري تحميل المجلدات الفرعية...</span>
            </div>
        );
    }

    if (error) {
        return (
            <p className="text-xs text-destructive text-center py-4">{error}</p>
        );
    }

    return (
        <div className="space-y-3">
            <div className="space-y-2">
                <Label className="text-xs font-bold text-muted-foreground">
                    نطاق الصلاحية
                </Label>
                <div className="flex flex-col gap-1.5">
                    <label className="flex items-center gap-2 px-3 py-2 rounded-lg hover:bg-accent/50 cursor-pointer">
                        <input
                            type="radio"
                            name="scope"
                            checked={mode === 'this-only'}
                            onChange={() => handleModeChange('this-only')}
                            className="accent-primary"
                        />
                        <span className="text-sm">هذا المجلد فقط</span>
                    </label>
                    {tree.length > 0 && (
                        <>
                            <label className="flex items-center gap-2 px-3 py-2 rounded-lg hover:bg-accent/50 cursor-pointer">
                                <input
                                    type="radio"
                                    name="scope"
                                    checked={mode === 'all'}
                                    onChange={() => handleModeChange('all')}
                                    className="accent-primary"
                                />
                                <span className="text-sm">هذا المجلد وجميع المجلدات الفرعية</span>
                            </label>
                            <label className="flex items-center gap-2 px-3 py-2 rounded-lg hover:bg-accent/50 cursor-pointer">
                                <input
                                    type="radio"
                                    name="scope"
                                    checked={mode === 'custom'}
                                    onChange={() => handleModeChange('custom')}
                                    className="accent-primary"
                                />
                                <span className="text-sm">اختيار مجلدات محددة</span>
                            </label>

                            {mode === 'custom' && (
                                <div className="border border-border rounded-xl p-2 max-h-48 overflow-y-auto space-y-0.5 mt-1">
                                    {tree.map(node => renderNode(node))}
                                </div>
                            )}
                        </>
                    )}
                </div>
            </div>
        </div>
    );
};
