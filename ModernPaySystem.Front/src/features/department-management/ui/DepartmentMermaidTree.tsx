import React, { useEffect, useRef, useState, useCallback } from 'react';
import mermaid from 'mermaid';
import { DepartmentTree } from '@/entities/department/model/types';
import { convertToMermaid } from '../model/useDepartmentTree';
import { useTheme } from '@/app/providers/theme-context';
import { TransformWrapper, TransformComponent } from "react-zoom-pan-pinch";
import { ZoomIn, ZoomOut, RotateCcw, Maximize2, Minimize2 } from 'lucide-react';
import { Button } from '@/shared/ui/button';
import { cn } from '@/shared/lib/utils';

const Skeleton = ({ className }: { className?: string }) => (
    <div className={cn("animate-pulse rounded-md bg-muted/20", className)} />
);

interface DepartmentMermaidTreeProps {
    data: DepartmentTree[];
    highlightId?: string;
    isLoading?: boolean;
    onNodeClick?: (id: string) => void;
}

interface ControlsProps {
    zoomIn: (step?: number, speed?: number) => void;
    zoomOut: (step?: number, speed?: number) => void;
    resetTransform: (speed?: number) => void;
    scale: number;
    isFullscreen: boolean;
    onToggleFullscreen: () => void;
}

const Controls: React.FC<ControlsProps> = ({ 
    zoomIn, 
    zoomOut, 
    resetTransform, 
    scale,
    isFullscreen,
    onToggleFullscreen
}) => {
    return (
        <div className="absolute top-4 right-4 z-20 flex items-center gap-1.5 bg-background/85 backdrop-blur-md p-1.5 rounded-xl border border-border shadow-lg transition-all duration-200" dir="rtl">
            <Button
                variant="ghost"
                size="icon"
                className="h-8 w-8 hover:bg-primary/10 hover:text-primary rounded-lg transition-colors"
                onClick={() => zoomIn(0.2, 200)}
                title="تكبير"
            >
                <ZoomIn className="w-4 h-4" />
            </Button>

            <span className="text-[11px] font-bold text-muted-foreground px-2 py-1 bg-muted/60 rounded-md min-w-[50px] text-center font-mono select-none">
                {Math.round(scale * 100)}%
            </span>

            <Button
                variant="ghost"
                size="icon"
                className="h-8 w-8 hover:bg-primary/10 hover:text-primary rounded-lg transition-colors"
                onClick={() => zoomOut(0.2, 200)}
                title="تصغير"
            >
                <ZoomOut className="w-4 h-4" />
            </Button>

            <div className="w-[1px] h-5 bg-border mx-0.5" />

            <Button
                variant="ghost"
                size="icon"
                className="h-8 w-8 hover:bg-primary/10 hover:text-primary rounded-lg transition-colors"
                onClick={() => resetTransform(200)}
                title="إعادة تعيين"
            >
                <RotateCcw className="w-4 h-4" />
            </Button>

            <div className="w-[1px] h-5 bg-border mx-0.5" />

            <Button
                variant="ghost"
                size="icon"
                className="h-8 w-8 hover:bg-primary/10 hover:text-primary rounded-lg transition-colors"
                onClick={onToggleFullscreen}
                title={isFullscreen ? "خروج من ملء الشاشة" : "ملء الشاشة"}
            >
                {isFullscreen ? (
                    <Minimize2 className="w-4 h-4" />
                ) : (
                    <Maximize2 className="w-4 h-4" />
                )}
            </Button>
        </div>
    );
};

const TreeSkeleton = ({ className }: { className?: string }) => (
    <div className={cn("w-full bg-background/50 backdrop-blur-sm animate-pulse rounded-xl flex items-center justify-center overflow-hidden z-50", className)}>
        <div className="flex flex-col items-center gap-8 opacity-20">
            <Skeleton className="w-48 h-12 rounded-2xl" />
            <div className="flex gap-12">
                <Skeleton className="w-32 h-24 rounded-2xl" />
                <Skeleton className="w-32 h-24 rounded-2xl" />
                <Skeleton className="w-32 h-24 rounded-2xl" />
            </div>
            <div className="flex gap-12">
                <Skeleton className="w-24 h-16 rounded-2xl" />
                <Skeleton className="w-24 h-16 rounded-2xl" />
                <Skeleton className="w-24 h-16 rounded-2xl" />
                <Skeleton className="w-24 h-16 rounded-2xl" />
            </div>
        </div>
    </div>
);

export const DepartmentMermaidTree: React.FC<DepartmentMermaidTreeProps> = ({
    data,
    highlightId,
    isLoading,
    onNodeClick
}) => {
    const mermaidRef = useRef<HTMLDivElement>(null);
    const containerRef = useRef<HTMLDivElement>(null);
    const [isRendering, setIsRendering] = useState(false);
    const [isFullscreen, setIsFullscreen] = useState(false);
    const [collapsedNodeIds, setCollapsedNodeIds] = useState<Set<string>>(new Set());
    const { theme } = useTheme();
    const isDark = theme === 'dark';

    const onNodeClickRef = useRef(onNodeClick);
    
    useEffect(() => {
        onNodeClickRef.current = onNodeClick;
    }, [onNodeClick]);

    const handleNodeDoubleClick = useCallback((nodeId: string) => {
        setCollapsedNodeIds(prev => {
            const next = new Set(prev);
            if (next.has(nodeId)) {
                next.delete(nodeId);
            } else {
                next.add(nodeId);
            }
            return next;
        });
    }, []);

    const toggleFullscreen = () => {
        if (!containerRef.current) return;
        if (!document.fullscreenElement) {
            containerRef.current.requestFullscreen().catch((err) => {
                console.error("Error enabling fullscreen:", err);
            });
            setIsFullscreen(true);
        } else {
            document.exitFullscreen();
            setIsFullscreen(false);
        }
    };

    useEffect(() => {
        const handleFullscreenChange = () => {
            setIsFullscreen(!!document.fullscreenElement);
        };
        document.addEventListener('fullscreenchange', handleFullscreenChange);
        return () => document.removeEventListener('fullscreenchange', handleFullscreenChange);
    }, []);

    const clickTimeoutRef = useRef<NodeJS.Timeout | null>(null);

    // Setup global callback for Mermaid
    useEffect(() => {
        (window as any).onMermaidNodeClick = (nodeId: string) => {
            const originalId = nodeId.replace(/_/g, '-');
            
            if (clickTimeoutRef.current) {
                // Double click
                clearTimeout(clickTimeoutRef.current);
                clickTimeoutRef.current = null;
                handleNodeDoubleClick(originalId);
            } else {
                // Single click
                clickTimeoutRef.current = setTimeout(() => {
                    clickTimeoutRef.current = null;
                    if (onNodeClickRef.current) {
                        onNodeClickRef.current(originalId);
                    }
                }, 250);
            }
        };

        return () => {
            delete (window as any).onMermaidNodeClick;
            if (clickTimeoutRef.current) clearTimeout(clickTimeoutRef.current);
        };
    }, [handleNodeDoubleClick]);

    useEffect(() => {
        mermaid.initialize({
            startOnLoad: true,
            theme: isDark ? 'dark' : 'default',
            securityLevel: 'loose',
            fontFamily: 'inherit',
            themeVariables: {
                primaryColor: isDark ? '#3b82f6' : '#2563eb',
                edgeColor: isDark ? '#4b5563' : '#9ca3af',
                lineColor: isDark ? '#4b5563' : '#9ca3af',
            }
        });
    }, [isDark]);

    useEffect(() => {
        const renderChart = async () => {
            if (mermaidRef.current && data && data.length > 0) {
                setIsRendering(true);
                const chartConfig = convertToMermaid(data, highlightId, isDark, collapsedNodeIds);

                // Use unique ID for rendering to avoid conflicts
                const id = `mermaid-chart-${Math.random().toString(36).substr(2, 9)}`;

                try {
                    const result = await mermaid.render(id, chartConfig);

                    if (mermaidRef.current) {
                        mermaidRef.current.innerHTML = result.svg;

                        // CRITICAL: Bind functions to the newly inserted SVG to enable click events
                        if (result.bindFunctions) {
                            result.bindFunctions(mermaidRef.current);
                        }
                    }
                } catch (error) {
                    console.error("Mermaid rendering error:", error);
                } finally {
                    setIsRendering(false);
                }
            }
        };

        renderChart();
    }, [data, highlightId, isDark, collapsedNodeIds]);

    if (!data || data.length === 0) {
        if (isLoading) return <TreeSkeleton className="min-h-[500px]" />;
        return (
            <div className="flex items-center justify-center h-64 text-muted-foreground border border-dashed border-border rounded-xl">
                لا توجد بيانات لعرض الشجرة
            </div>
        );
    }

    return (
        <div 
            ref={containerRef}
            className={cn(
                "w-full relative bg-card border border-border shadow-md overflow-hidden group transition-all duration-300",
                isFullscreen ? "fixed inset-0 z-50 h-screen w-screen bg-background" : "rounded-lg min-h-[500px]"
            )}
        >
            {/* Show Skeleton as an absolute overlay to prevent unmounting mermaidRef */}
            {(isLoading || isRendering) && (
                <TreeSkeleton className={cn("absolute inset-0 z-30", isFullscreen ? "h-screen" : "h-[500px]")} />
            )}

            {collapsedNodeIds.size > 0 && (
                <button
                    onClick={() => setCollapsedNodeIds(new Set())}
                    className="absolute top-4 left-4 z-20 bg-background/80 backdrop-blur-md text-[11px] font-bold text-primary px-3 py-1.5 rounded-lg border border-border shadow-sm hover:bg-primary/10 transition-colors"
                >
                    توسيع كافة الفروع ({collapsedNodeIds.size})
                </button>
            )}

            <TransformWrapper
                initialScale={1}
                minScale={0.01}
                maxScale={12}
                centerOnInit={true}
                wheel={{ step: 0.1 }}
                pinch={{ step: 5, disabled: false }}
                doubleClick={{ disabled: true }}
                panning={{ activationKeys: [], disabled: false, velocityDisabled: false }}
            >
                {({ state, zoomIn, zoomOut, resetTransform }) => (
                    <>
                        <Controls
                            zoomIn={zoomIn}
                            zoomOut={zoomOut}
                            resetTransform={resetTransform}
                            scale={state.scale}
                            isFullscreen={isFullscreen}
                            onToggleFullscreen={toggleFullscreen}
                        />
                        <TransformComponent
                            wrapperStyle={{
                                width: '100%',
                                height: isFullscreen ? '100vh' : '500px',
                                cursor: 'grab'
                            }}
                            contentStyle={{
                                width: '100%',
                                height: '100%',
                                display: 'flex',
                                justifyContent: 'center',
                                alignItems: 'center'
                            }}
                        >
                            <div
                                ref={mermaidRef}
                                className={cn(
                                    "p-10 transition-all duration-300 active:cursor-grabbing",
                                    (isLoading || isRendering) && "opacity-0"
                                )}
                            />
                        </TransformComponent>
                    </>
                )}
            </TransformWrapper>

            <div className="absolute bottom-4 right-4 flex flex-col gap-1 text-[10px] text-muted-foreground opacity-60 group-hover:opacity-100 transition-opacity select-none" dir="rtl">
                <div className="flex items-center gap-2">
                    <span className="w-1.5 h-1.5 rounded-full bg-primary" />
                    <span>نقر مفرد: تحديد القسم وعرض تفاصيله</span>
                </div>
                <div className="flex items-center gap-2">
                    <span className="w-1.5 h-1.5 rounded-full bg-amber-500" />
                    <span>نقر مزدوج: طي / توسيع فرع القسم</span>
                </div>
                <div className="flex items-center gap-2">
                    <span>استخدم عجلة الفأرة للتكبير والتصغير، واسحب للتحريك داخل اللوحة</span>
                </div>
            </div>
        </div>
    );
};
