import { Component, type ErrorInfo, type ReactNode } from 'react';
import { AlertTriangle, RefreshCw, Home } from 'lucide-react';

interface ErrorBoundaryProps {
    children: ReactNode;
    /** Fallback UI to render instead of the default error display */
    fallback?: ReactNode;
    /** Custom error handler callback */
    onError?: (error: Error, errorInfo: ErrorInfo) => void;
    /** Context label to show in the error message (e.g., "نظام المراسلات") */
    context?: string;
}

interface ErrorBoundaryState {
    hasError: boolean;
    error: Error | null;
}

/**
 * Error Boundary component that catches JavaScript errors in its child component tree.
 * Displays a user-friendly error message in Arabic with recovery options.
 *
 * @example
 * ```tsx
 * <ErrorBoundary context="نظام المراسلات">
 *     <FormBuilder />
 * </ErrorBoundary>
 * ```
 */
export class ErrorBoundary extends Component<ErrorBoundaryProps, ErrorBoundaryState> {
    constructor(props: ErrorBoundaryProps) {
        super(props);
        this.state = {
            hasError: false,
            error: null,
        };
    }

    static getDerivedStateFromError(error: Error): ErrorBoundaryState {
        return { hasError: true, error };
    }

    componentDidCatch(error: Error, errorInfo: ErrorInfo): void {
        console.error(`[ErrorBoundary${this.props.context ? ` - ${this.props.context}` : ''}]:`, error, errorInfo);
        this.props.onError?.(error, errorInfo);
    }

    private handleRetry = (): void => {
        this.setState({ hasError: false, error: null });
    };

    private handleGoHome = (): void => {
        window.location.href = '/';
    };

    render(): ReactNode {
        if (this.state.hasError) {
            if (this.props.fallback) {
                return this.props.fallback;
            }

            return (
                <div className="flex items-center justify-center min-h-[300px] p-8" dir="rtl">
                    <div className="max-w-md w-full text-center space-y-6">
                        {/* Error Icon */}
                        <div className="mx-auto w-16 h-16 bg-destructive/10 rounded-2xl flex items-center justify-center">
                            <AlertTriangle className="w-8 h-8 text-destructive" />
                        </div>

                        {/* Error Message */}
                        <div className="space-y-2">
                            <h3 className="text-xl font-bold text-foreground">
                                حدث خطأ غير متوقع
                            </h3>
                            {this.props.context && (
                                <p className="text-sm text-muted-foreground">
                                    في قسم: <span className="font-semibold">{this.props.context}</span>
                                </p>
                            )}
                            <p className="text-sm text-muted-foreground">
                                نعتذر عن هذا الخطأ. يمكنك المحاولة مرة أخرى أو العودة للصفحة الرئيسية.
                            </p>
                        </div>

                        {/* Error Details (collapsed) */}
                        {this.state.error && (
                            <details className="text-right bg-muted/30 rounded-xl p-3 text-xs">
                                <summary className="cursor-pointer text-muted-foreground font-medium">
                                    تفاصيل الخطأ (للمطور)
                                </summary>
                                <pre className="mt-2 p-2 bg-background rounded-lg overflow-auto max-h-32 text-[10px] font-mono text-destructive whitespace-pre-wrap break-words">
                                    {this.state.error.message}
                                </pre>
                            </details>
                        )}

                        {/* Action Buttons */}
                        <div className="flex items-center justify-center gap-3">
                            <button
                                onClick={this.handleRetry}
                                className="inline-flex items-center gap-2 px-5 py-2.5 bg-primary text-primary-foreground rounded-xl font-bold text-sm shadow-lg shadow-primary/20 hover:opacity-90 transition-opacity"
                            >
                                <RefreshCw className="w-4 h-4" />
                                إعادة المحاولة
                            </button>
                            <button
                                onClick={this.handleGoHome}
                                className="inline-flex items-center gap-2 px-5 py-2.5 bg-muted text-foreground rounded-xl font-bold text-sm hover:bg-muted/80 transition-colors"
                            >
                                <Home className="w-4 h-4" />
                                الرئيسية
                            </button>
                        </div>
                    </div>
                </div>
            );
        }

        return this.props.children;
    }
}
