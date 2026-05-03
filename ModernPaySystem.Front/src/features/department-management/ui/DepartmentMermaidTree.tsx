import React, { useEffect, useRef } from 'react';
import mermaid from 'mermaid';
import { DepartmentTree } from '@/entities/department/model/types';
import { convertToMermaid } from '../model/useDepartmentTree';
import { LoadingSpinner } from '@/shared/ui/common/loading-spinner';
import { useTheme } from '@/app/providers/theme-context';
import { TransformWrapper, TransformComponent, useControls } from "react-zoom-pan-pinch";
import { ZoomIn, ZoomOut, RotateCcw } from 'lucide-react';
import { Button } from '@/shared/ui/button';

interface DepartmentMermaidTreeProps {
    data: DepartmentTree[];
    highlightId?: string;
    isLoading?: boolean;
}

const Controls = () => {
    const { zoomIn, zoomOut, resetTransform } = useControls();
    return (
        <div className="absolute top-4 right-4 z-20 flex flex-col gap-2 bg-background/80 backdrop-blur-sm p-1 rounded-lg border shadow-sm">
            <Button variant="ghost" size="icon" className="h-8 w-8" onClick={() => zoomIn()}>
                <ZoomIn className="w-4 h-4" />
            </Button>
            <Button variant="ghost" size="icon" className="h-8 w-8" onClick={() => zoomOut()}>
                <ZoomOut className="w-4 h-4" />
            </Button>
            <Button variant="ghost" size="icon" className="h-8 w-8" onClick={() => resetTransform()}>
                <RotateCcw className="w-4 h-4" />
            </Button>
        </div>
    );
};

export const DepartmentMermaidTree: React.FC<DepartmentMermaidTreeProps> = ({
    data,
    highlightId,
    isLoading,
    onNodeClick
}) => {
    const mermaidRef = useRef<HTMLDivElement>(null);
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
        if (mermaidRef.current && data && data.length > 0) {
            const chartConfig = convertToMermaid(data, highlightId, isDark);

            // Clear previous content
            mermaidRef.current.innerHTML = `<div class="mermaid">${chartConfig}</div>`;

            // Re-render
            try {
                mermaid.contentLoaded();
            } catch (error) {
                console.error("Mermaid rendering error:", error);
            }
        }
    }, [data, highlightId, isDark]);

    if (isLoading) {
        return (
            <div className="flex items-center justify-center h-64">
                <LoadingSpinner size="lg" />
            </div>
        );
    }

    if (!data || data.length === 0) {
        return (
            <div className="flex items-center justify-center h-64 text-muted-foreground">
                لا توجد بيانات لعرض الشجرة
            </div>
        );
    }

    return (
        <div className="w-full relative bg-card rounded-lg border border-border shadow-sm min-h-[500px] overflow-hidden group">
            <TransformWrapper
                initialScale={1}
                minScale={0.2}
                maxScale={3}
                centerOnInit={true}
                wheel={{ step: 0.1 }}
            >
                {({ zoomIn, zoomOut, resetTransform, ...rest }) => (
                    <>
                        <Controls />
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
                            <div ref={mermaidRef} className="p-10 transition-all duration-300 active:cursor-grabbing" />
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
