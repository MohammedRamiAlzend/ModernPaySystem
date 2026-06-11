import { useState, useMemo } from 'react';
import { Card } from '@/shared/ui/card';
import { Button } from '@/shared/ui/button';
import { Input } from '@/shared/ui/input';
import { AnimatedContainer } from '@/shared/ui/common/animated-container';
import { Badge } from '@/shared/ui/badge';
import { archivingService } from '@/features/archiving/api/archivingService';
import { DocumentGalleryModal } from '@/features/archiving/ui/DocumentGalleryModal';
import type { SemanticSearchRequest, SemanticSearchResultItem } from '@/features/archiving/model/types';
import type { ArchiveRecord } from '@/features/archiving/model/types';
import { cn } from '@/shared/lib/utils';
import {
    Search,
    SlidersHorizontal,
    Loader2,
    X,
    FileText,
    Hash,
    Layers,
    Eye,
    Sparkles,
    Brain,
    ChevronDown,
    Library
} from 'lucide-react';

export function SemanticSearchPage() {
    const [query, setQuery] = useState('');
    const [topK, setTopK] = useState(10);
    const [minScore, setMinScore] = useState(0.5);
    const [sourceType, setSourceType] = useState<number | null>(null);
    const [archiveRecordId, setArchiveRecordId] = useState('');
    const [folderId, setFolderId] = useState('');

    const [results, setResults] = useState<SemanticSearchResultItem[] | null>(null);
    const [isSearching, setIsSearching] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [expandedRecords, setExpandedRecords] = useState<Set<string>>(new Set());
    const [previewRecord, setPreviewRecord] = useState<ArchiveRecord | null>(null);
    const [previewLoading, setPreviewLoading] = useState<string | null>(null);

    const handleSearch = async () => {
        if (!query.trim()) return;
        setIsSearching(true);
        setError(null);
        setExpandedRecords(new Set());
        try {
            const request: SemanticSearchRequest = {
                query: query.trim(),
                topK,
                minScore,
                sourceType,
                archiveRecordId: archiveRecordId.trim() || null,
                folderId: folderId.trim() || null,
            };
            const data = await archivingService.semanticSearch(request);
            setResults(data);
        } catch (e) {
            console.error('Semantic search failed:', e);
            setError('فشل البحث الدلالي. يرجى المحاولة مرة أخرى.');
        } finally {
            setIsSearching(false);
        }
    };

    const handleClear = () => {
        setQuery('');
        setTopK(10);
        setMinScore(0.5);
        setSourceType(null);
        setArchiveRecordId('');
        setFolderId('');
        setResults(null);
        setError(null);
        setExpandedRecords(new Set());
    };

    const handleKeyDown = (e: React.KeyboardEvent) => {
        if (e.key === 'Enter') handleSearch();
    };

    const getScoreColor = (score: number) => {
        if (score >= 0.7) return 'text-emerald-600 bg-emerald-50 border-emerald-200';
        if (score >= 0.5) return 'text-amber-600 bg-amber-50 border-amber-200';
        return 'text-red-600 bg-red-50 border-red-200';
    };

    const toggleExpandRecord = (recordId: string) => {
        setExpandedRecords(prev => {
            const next = new Set(prev);
            if (next.has(recordId)) next.delete(recordId);
            else next.add(recordId);
            return next;
        });
    };

    const handlePreviewRecord = async (recordId: string) => {
        setPreviewLoading(recordId);
        try {
            const record = await archivingService.getArchiveRecordById(recordId);
            setPreviewRecord(record);
        } catch (e) {
            console.error('Failed to load record details:', e);
        } finally {
            setPreviewLoading(null);
        }
    };

    // Group results by archiveRecordId, sort groups by max score descending
    const recordGroups = useMemo(() => {
        if (!results || results.length === 0) return [];

        const groupsMap = new Map<string, { items: SemanticSearchResultItem[]; maxScore: number }>();

        for (const item of results) {
            const key = item.archiveRecordId || 'unknown';
            const existing = groupsMap.get(key);
            if (existing) {
                existing.items.push(item);
                if (item.score > existing.maxScore) existing.maxScore = item.score;
            } else {
                groupsMap.set(key, { items: [item], maxScore: item.score });
            }
        }

        return Array.from(groupsMap.entries())
            .map(([recordId, group]) => ({
                recordId,
                items: group.items.sort((a, b) => b.score - a.score),
                maxScore: group.maxScore,
                bestItem: group.items.reduce((best, curr) => curr.score > best.score ? curr : best),
            }))
            .sort((a, b) => b.maxScore - a.maxScore);
    }, [results]);

    return (
        <AnimatedContainer className="container mx-auto p-6 space-y-6 text-right min-h-screen" dir="rtl">
            {/* Top Header */}
            <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 border-b border-border pb-6">
                <div className="flex flex-col gap-1">
                    <h1 className="text-3xl font-black text-primary flex items-center gap-2">
                        <Brain className="h-8 w-8 text-primary" />
                        البحث الدلالي في الأرشيف
                    </h1>
                    <p className="text-xs text-muted-foreground font-semibold">
                        ابحث في محتوى المستندات باستخدام الذكاء الاصطناعي للبحث بناءً على المعنى والسياق وليس فقط الكلمات المطابقة
                    </p>
                </div>
            </div>

            {/* Search Form Card */}
            <Card className="p-6 bg-card border border-border shadow-lg rounded-3xl space-y-6">
                <h3 className="text-base font-bold flex items-center gap-2 border-b pb-3 text-primary">
                    <Sparkles className="h-4 w-4" />
                    معايير البحث الدلالي
                </h3>

                <div className="space-y-4">
                    {/* Main Query */}
                    <div className="flex flex-col gap-2">
                        <label className="text-xs font-bold text-muted-foreground">نص البحث الدلالي</label>
                        <div className="relative">
                            <Input
                                value={query}
                                onChange={(e) => setQuery(e.target.value)}
                                onKeyDown={handleKeyDown}
                                placeholder="أدخل جملة أو كلمات مفتاحية للبحث في محتوى المستندات..."
                                className="pl-10 rounded-xl text-sm"
                            />
                            <Search className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                        </div>
                    </div>

                    <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
                        {/* Top K */}
                        <div className="flex flex-col gap-2">
                            <label className="text-xs font-bold text-muted-foreground">عدد النتائج (TopK)</label>
                            <div className="relative">
                                <Input
                                    type="number"
                                    min={1}
                                    max={100}
                                    value={topK}
                                    onChange={(e) => setTopK(Number(e.target.value))}
                                    className="pl-10 rounded-xl"
                                />
                                <Layers className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                            </div>
                        </div>

                        {/* Min Score */}
                        <div className="flex flex-col gap-2">
                            <label className="text-xs font-bold text-muted-foreground">الحد الأدنى للتطابق</label>
                            <div className="relative">
                                <Input
                                    type="number"
                                    min={0}
                                    max={1}
                                    step={0.05}
                                    value={minScore}
                                    onChange={(e) => setMinScore(Number(e.target.value))}
                                    className="pl-10 rounded-xl"
                                />
                                <SlidersHorizontal className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                            </div>
                        </div>

                        {/* Source Type */}
                        <div className="flex flex-col gap-2">
                            <label className="text-xs font-bold text-muted-foreground">نوع المصدر</label>
                            <select
                                value={sourceType ?? ''}
                                onChange={(e) => setSourceType(e.target.value ? Number(e.target.value) : null)}
                                className="flex h-10 w-full rounded-xl border border-input bg-background px-3 py-2 text-sm ring-offset-background focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
                            >
                                <option value="">-- الكل --</option>
                                <option value={1}>مستند</option>
                                <option value={2}>صورة</option>
                            </select>
                        </div>

                        {/* Archive Record ID */}
                        <div className="flex flex-col gap-2">
                            <label className="text-xs font-bold text-muted-foreground">معرف السجل (اختياري)</label>
                            <div className="relative">
                                <Input
                                    value={archiveRecordId}
                                    onChange={(e) => setArchiveRecordId(e.target.value)}
                                    placeholder="تحديد سجل معين..."
                                    className="pl-10 rounded-xl"
                                />
                                <Hash className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                            </div>
                        </div>
                    </div>

                    <div className="grid grid-cols-1 md:grid-cols-3 gap-6 items-end">
                        {/* Folder ID */}
                        <div className="flex flex-col gap-2">
                            <label className="text-xs font-bold text-muted-foreground">معرف المجلد (اختياري)</label>
                            <div className="relative">
                                <Input
                                    value={folderId}
                                    onChange={(e) => setFolderId(e.target.value)}
                                    placeholder="تحديد مجلد معين..."
                                    className="pl-10 rounded-xl"
                                />
                                <Hash className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                            </div>
                        </div>

                        {/* Action Buttons */}
                        <div className="flex gap-3 md:col-span-2">
                            <Button
                                onClick={handleSearch}
                                disabled={isSearching || !query.trim()}
                                className="flex-1 rounded-xl font-bold gap-2"
                            >
                                {isSearching ? <Loader2 className="h-4 w-4 animate-spin" /> : <Brain className="h-4 w-4" />}
                                <span>{isSearching ? 'جاري البحث...' : 'بحث دلالي'}</span>
                            </Button>
                            <Button
                                onClick={handleClear}
                                variant="outline"
                                className="rounded-xl font-bold gap-2 border-border"
                            >
                                <X className="h-4 w-4" />
                                <span>تفريغ</span>
                            </Button>
                        </div>
                    </div>
                </div>
            </Card>

            {/* Error Message */}
            {error && (
                <Card className="p-4 bg-red-50 border border-red-200 rounded-3xl text-red-700 text-sm font-bold text-center">
                    {error}
                </Card>
            )}

            {/* Results Section */}
            {results !== null && (
                <Card className="p-6 bg-card border border-border shadow-lg rounded-3xl">
                    <div className="flex justify-between items-center mb-6">
                        <h2 className="text-lg font-bold text-primary flex items-center gap-2">
                            <Sparkles className="h-5 w-5" />
                            نتائج البحث الدلالي — {recordGroups.length} سجل
                        </h2>
                        <span className="text-xs text-muted-foreground font-medium">
                            ({results.length} مقطع موزعة على {recordGroups.length} سجل)
                        </span>
                    </div>

                    <div className="space-y-6">
                        {recordGroups.length > 0 ? (
                            recordGroups.map((group, groupIndex) => {
                                const isExpanded = expandedRecords.has(group.recordId);
                                const best = group.bestItem;

                                return (
                                    <div
                                        key={group.recordId}
                                        className="border border-border rounded-2xl overflow-hidden transition-all"
                                    >
                                        {/* Record Header Card (always visible) */}
                                        <div
                                            className={cn(
                                                "p-5 cursor-pointer transition-colors",
                                                isExpanded ? "bg-primary/5" : "hover:bg-muted/10"
                                            )}
                                            onClick={() => toggleExpandRecord(group.recordId)}
                                        >
                                            <div className="flex items-start justify-between gap-4">
                                                <div className="flex items-center gap-2 flex-wrap">
                                                    <span className="text-[10px] text-muted-foreground font-mono font-bold shrink-0 bg-muted/30 px-2 py-0.5 rounded-lg">
                                                        #{groupIndex + 1}
                                                    </span>
                                                    <Badge
                                                        variant="outline"
                                                        className={cn(
                                                            "text-[10px] font-bold px-2 py-0.5 rounded-lg border",
                                                            getScoreColor(group.maxScore)
                                                        )}
                                                    >
                                                        {(group.maxScore * 100).toFixed(1)}% أعلى تطابق
                                                    </Badge>
                                                    <Badge variant="secondary" className="text-[10px] font-bold px-2 py-0.5 rounded-lg">
                                                        <FileText className="h-3 w-3 ml-1" />
                                                        {best.fileName}
                                                    </Badge>
                                                    {best.archiveRecordNumber && (
                                                        <Badge variant="outline" className="text-[10px] font-bold px-2 py-0.5 rounded-lg">
                                                            <Hash className="h-3 w-3 ml-1" />
                                                            {best.archiveRecordNumber}
                                                        </Badge>
                                                    )}
                                                    <Badge variant="outline" className="text-[10px] font-bold px-2 py-0.5 rounded-lg border-primary/30 text-primary">
                                                        <Library className="h-3 w-3 ml-1" />
                                                        {group.items.length} مقطع
                                                    </Badge>
                                                </div>
                                                <ChevronDown className={cn(
                                                    "h-4 w-4 text-muted-foreground transition-transform shrink-0",
                                                    isExpanded && "rotate-180"
                                                )} />
                                            </div>

                                            {/* Best content preview */}
                                            <p className="text-sm text-foreground/80 leading-relaxed line-clamp-2 bg-muted/20 rounded-xl p-3 mt-3 border border-border/50">
                                                {best.content}
                                            </p>

                                            {/* Record-level action */}
                                            <div className="flex items-center justify-between mt-3 pt-1">
                                                <div className="flex items-center gap-3 text-[10px] text-muted-foreground font-medium">
                                                    <span>السجل: <span className="font-mono font-bold text-foreground/60">{group.recordId.slice(0, 8)}...</span></span>
                                                    {best.documentId && (
                                                        <span>المستند: <span className="font-mono font-bold text-foreground/60">{best.documentId.slice(0, 8)}...</span></span>
                                                    )}
                                                </div>
                                                <button
                                                    onClick={(e) => {
                                                        e.stopPropagation();
                                                        handlePreviewRecord(group.recordId);
                                                    }}
                                                    disabled={previewLoading === group.recordId}
                                                    className="inline-flex items-center gap-1.5 text-xs font-bold text-primary hover:text-primary/80 transition-colors disabled:opacity-50"
                                                >
                                                    {previewLoading === group.recordId ? (
                                                        <Loader2 className="h-3.5 w-3.5 animate-spin" />
                                                    ) : (
                                                        <Eye className="h-3.5 w-3.5" />
                                                    )}
                                                    عرض المستند
                                                </button>
                                            </div>
                                        </div>

                                        {/* Expanded chunks inside record */}
                                        {isExpanded && (
                                            <div className="border-t border-border/50 bg-muted/5 animate-in slide-in-from-top-2 duration-200">
                                                <div className="p-4 space-y-3">
                                                    <h4 className="text-xs font-bold text-muted-foreground flex items-center gap-1.5">
                                                        <Layers className="h-3.5 w-3.5" />
                                                        جميع المقاطع المسترجعة ({group.items.length})
                                                    </h4>
                                                    {group.items.map((item) => (
                                                        <div
                                                            key={item.chunkId}
                                                            className="border border-border/60 rounded-xl p-4 bg-card hover:bg-muted/10 transition-colors space-y-2"
                                                        >
                                                            <div className="flex items-center justify-between">
                                                                <div className="flex items-center gap-2">
                                                                    <Badge
                                                                        variant="outline"
                                                                        className={cn(
                                                                            "text-[10px] font-bold px-2 py-0.5 rounded-lg border",
                                                                            getScoreColor(item.score)
                                                                        )}
                                                                    >
                                                                        {(item.score * 100).toFixed(1)}% تطابق
                                                                    </Badge>
                                                                    {item.chunkIndex > 0 && (
                                                                        <span className="text-[10px] font-bold text-muted-foreground px-2 py-0.5">
                                                                            المقطع #{item.chunkIndex}
                                                                        </span>
                                                                    )}
                                                                </div>
                                                                <button
                                                                    onClick={() => handlePreviewRecord(item.archiveRecordId)}
                                                                    className="inline-flex items-center gap-1 text-[10px] font-bold text-primary hover:text-primary/80"
                                                                >
                                                                    <Eye className="h-3 w-3" />
                                                                    عرض
                                                                </button>
                                                            </div>
                                                            <p className="text-xs text-foreground/70 leading-relaxed line-clamp-3">
                                                                {item.content}
                                                            </p>
                                                        </div>
                                                    ))}
                                                </div>
                                            </div>
                                        )}
                                    </div>
                                );
                            })
                        ) : (
                            <div className="py-16 text-center text-muted-foreground">
                                <div className="flex flex-col items-center justify-center gap-2">
                                    <FileText className="h-10 w-10 opacity-30 text-muted-foreground" />
                                    <p className="font-semibold text-sm">
                                        لم يتم العثور على نتائج تطابق معايير البحث الدلالي.
                                    </p>
                                    <p className="text-xs text-muted-foreground/60">
                                        حاول استخدام كلمات مختلفة أو خفض الحد الأدنى للتطابق
                                    </p>
                                </div>
                            </div>
                        )}
                    </div>
                </Card>
            )}

            {/* Document Gallery Preview Modal */}
            <DocumentGalleryModal
                record={previewRecord}
                dynamicTemplates={[]}
                onClose={() => setPreviewRecord(null)}
            />

            {/* Loading Overlay */}
            {isSearching && (
                <div className="fixed inset-0 bg-black/40 backdrop-blur-[2px] flex items-center justify-center z-[100] animate-in fade-in duration-300">
                    <div className="bg-card border border-border rounded-3xl p-8 max-w-sm w-full shadow-2xl flex flex-col items-center gap-6 text-center">
                        <div className="w-16 h-16 rounded-2xl bg-primary/10 text-primary flex items-center justify-center">
                            <Brain className="h-8 w-8 animate-pulse" />
                        </div>
                        <div className="flex flex-col gap-2">
                            <h3 className="text-base font-bold text-foreground">جاري البحث الدلالي</h3>
                            <p className="text-xs text-muted-foreground font-medium">
                                يتم الآن تحليل النص والبحث في محتوى المستندات...
                            </p>
                        </div>
                        <Loader2 className="h-6 w-6 animate-spin text-primary" />
                    </div>
                </div>
            )}
        </AnimatedContainer>
    );
}

export default SemanticSearchPage;
