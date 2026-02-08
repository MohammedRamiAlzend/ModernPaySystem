import { Link } from 'react-router-dom';
import { Button } from '@/shared/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/ui/card';

export const NotFoundWidget = () => {
  return (
    <div className="flex items-center justify-center min-h-[60vh] p-4">
      <Card className="w-full max-w-md text-center">
        <CardHeader>
          <CardTitle className="text-2xl">404</CardTitle>
        </CardHeader>
        <CardContent>
          <p className="mb-4 text-muted-foreground">
            الصفحة المطلوبة غير موجودة
          </p>
          <Link to="/">
            <Button>العودة لصفحة لرئيسية</Button>
          </Link>
        </CardContent>
      </Card>
    </div>
  );
};