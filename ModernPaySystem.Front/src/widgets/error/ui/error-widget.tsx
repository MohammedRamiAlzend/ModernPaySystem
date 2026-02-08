import { Link } from 'react-router-dom';
import { isRouteErrorResponse } from 'react-router-dom';
import { Button } from '@/shared/ui/button';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/shared/ui/card';
import { AlertCircle } from 'lucide-react';

interface ErrorWidgetProps {
  error?: unknown;
}

export const ErrorWidget = ({ error }: ErrorWidgetProps) => {
  const isDynamicImportError = error instanceof Error && 
    error.message.includes('error loading dynamically imported module');

  let errorCode = '500';
  let errorTitle = 'خطأ غير متوقع!';
  let errorMessage = 'حدث خطأ ما. يرجى محاولة مرة أخرى لاحقاً.';
  let errorDetails = '';

  if (isRouteErrorResponse(error)) {
    errorCode = String(error.status);
    errorTitle = error.statusText || 'خطأ';
    errorMessage = error.data?.message || 'حدث خطأ في تحميل الصفحة';
    errorDetails = JSON.stringify(error.data);
  } else if (error instanceof Error) {
    errorTitle = 'خطأ في التطبيق';
    errorMessage = error.message;
    errorDetails = error.stack || '';
  }

  return (
    <div className="flex items-center justify-center min-h-screen px-4 py-12">
      <Card className="w-full max-w-md">
        <CardHeader className="text-center">
          <div className="flex justify-center mb-4">
            <AlertCircle className="w-12 h-12 text-destructive" />
          </div>
          <CardTitle className="text-5xl text-destructive mb-2">
            {errorCode}
          </CardTitle>
          <CardDescription className="text-lg font-semibold text-foreground mb-2">
            {errorTitle}
          </CardDescription>
        </CardHeader>

        <CardContent className="space-y-4">
          <p className="text-center text-muted-foreground">
            {errorMessage}
          </p>

          {/* Special message for dynamic import errors */}
          {isDynamicImportError && (
            <div className="bg-muted border border-border rounded-md p-3 space-y-2">
              <p className="text-sm font-medium text-foreground">
                قد يكون السبب:
              </p>
              <ul className="text-sm text-muted-foreground list-disc list-inside space-y-1">
                <li>الصفحة لم تُحمّل بشكل صحيح</li>
                <li>مشكلة في الاتصال بالخادم</li>
                <li>الملف المطلوب غير موجود</li>
              </ul>
            </div>
          )}

          {/* Error Details (development only) */}
          {import.meta.env.VITE_DEV === 'true' && errorDetails && (
            <details className="text-xs">
              <summary className="cursor-pointer text-sm text-muted-foreground hover:text-foreground">
                📋 تفاصيل الخطأ (للمطورين فقط)
              </summary>
              <pre className="bg-muted p-2 rounded mt-2 overflow-auto max-h-40 text-foreground">
                {errorDetails}
              </pre>
            </details>
          )}

          {/* Action Buttons */}
          <div className="flex flex-col gap-2 pt-4">
            <Link to="/" className="w-full">
              <Button className="w-full">
                العودة للرئيسية
              </Button>
            </Link>
            <Button
              onClick={() => window.location.reload()}
              variant="outline"
              className="w-full"
            >
              إعادة تحميل الصفحة
            </Button>
          </div>
        </CardContent>
      </Card>
    </div>
  );
};
