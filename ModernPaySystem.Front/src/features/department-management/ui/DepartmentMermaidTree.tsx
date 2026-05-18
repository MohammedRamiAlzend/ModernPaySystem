import React, { useEffect, useRef } from 'react';
import mermaid from 'mermaid';
import { DepartmentTree } from '@/entities/department/model/types';
import { convertToMermaid } from '../model/useDepartmentTree';
import { useTheme } from '@/app/providers/theme-context';
import { TransformWrapper, TransformComponent } from "react-zoom-pan-pinch";
import { ZoomIn, ZoomOut, RotateCcw } from 'lucide-react';
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
}

const Controls: React.FC<ControlsProps> = ({ zoomIn, zoomOut, resetTransform }) => {
    return (
        <div className="absolute top-4 right-4 z-20 flex items-center gap-1.5 bg-background/90 backdrop-blur-md p-1.5 rounded-xl border border-border shadow-md transition-all duration-200" dir="rtl">
            <Button
                variant="ghost"
                size="icon"
                className="h-8 w-8 hover:bg-primary/10 hover:text-primary rounded-lg transition-colors"
                onClick={() => zoomIn(0.2, 200)}
                title="تكبير"
            >
                <ZoomIn className="w-4 h-4" />
            </Button>

            {/* <span className="text-[10px] font-bold text-muted-foreground px-2 py-1 bg-muted/50 rounded-md min-w-[45px] text-center font-mono select-none">
                {Math.round(scale * 100)}%
            </span> */}

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
        </div>
    );
};

const TreeSkeleton = ({ className }: { className?: string }) => (
    <div className={cn("w-full h-[500px] bg-background/50 backdrop-blur-sm animate-pulse rounded-xl flex items-center justify-center overflow-hidden z-50", className)}>
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
    const [isRendering, setIsRendering] = React.useState(false);
    const { theme } = useTheme();
    const isDark = theme === 'dark';

    // Setup global callback for Mermaid
    useEffect(() => {
        (window as any).onMermaidNodeClick = (nodeId: string) => {
            if (onNodeClick) {
                // Convert nodeId back to GUID (underscore to hyphen)
                const originalId = nodeId.replace(/_/g, '-');
                onNodeClick(originalId);
            }
        };

        return () => {
            delete (window as any).onMermaidNodeClick;
        };
    }, [onNodeClick]);

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
                const chartConfig = convertToMermaid(data, highlightId, isDark);

                // Use unique ID for rendering to avoid conflicts
                const id = `mermaid-chart-${Math.random().toString(36).substr(2, 9)}`;

                try {
                    // mermaid.render returns { svg, bindFunctions } in newer versions
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
    }, [data, highlightId, isDark]);

    if (!data || data.length === 0) {
        if (isLoading) return <TreeSkeleton />;
        return (
            <div className="flex items-center justify-center h-64 text-muted-foreground">
                لا توجد بيانات لعرض الشجرة
            </div>
        );
    }

    return (
        <div className="w-full relative bg-card rounded-lg border border-border shadow-sm min-h-[500px] overflow-hidden group">
            {/* Show Skeleton as an absolute overlay to prevent unmounting mermaidRef */}
            {(isLoading || isRendering) && <TreeSkeleton className="absolute inset-0" />}

            <TransformWrapper
                initialScale={1}
                minScale={2}
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
                        />
                        <TransformComponent
                            wrapperStyle={{
                                width: '100%',
                                height: '500px',
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

            <div className="absolute bottom-4 right-4 text-[10px] text-muted-foreground opacity-50 group-hover:opacity-100 transition-opacity">
                استخدم العجلة للتكبير، واسحب للتحريك
            </div>
        </div>
    );
};
