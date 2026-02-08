import { useState } from "react";
import { Button } from "@/shared/ui/button";
import { PrefetchLink } from "@/shared/navigation/prefetch-link";
import { Alert, AlertDescription, AlertTitle } from "@/shared/ui/alert";
import { AnimatedContainer } from "@/shared/ui/common/animated-container";
import {
    AlertDialog,
    AlertDialogAction,
    AlertDialogCancel,
    AlertDialogContent,
    AlertDialogDescription,
    AlertDialogFooter,
    AlertDialogHeader,
    AlertDialogTitle,
    AlertDialogTrigger,
} from "@/shared/ui/alert-dialog";

export function HomePageWidget() {
    const [showAlert, setShowAlert] = useState(false);

    const handleConfirm = () => {
        console.log("النتيجة: تم التأكيد (Confirmed)");
        alert("تم تنفيذ العملية بنجاح!");
    };

    const handleCancel = () => {
        console.log("النتيجة: تم الإلغاء (Cancelled)");
    };

    return (
        <AnimatedContainer className="flex flex-col items-center justify-center gap-6 py-12">
            <div className="text-center space-y-2">
                <h1 className="text-4xl font-extrabold tracking-tight lg:text-5xl text-primary">
                    مرحباً بك في نظام الدفع
                </h1>
                <p className="text-muted-foreground text-lg">
                    لوحة تحكم ذكية، سريعة، وآمنة.
                </p>
            </div>

            <div className="flex flex-wrap items-center justify-center gap-6 mt-8">
                <Button asChild size="lg" className="rounded-xl px-8 h-12 shadow-lg shadow-primary/20">
                    <PrefetchLink to="/contracts">
                        إدارة العقود
                    </PrefetchLink>
                </Button>
                <Button asChild size="lg" className="rounded-xl px-8 h-12 shadow-lg shadow-primary/20">
                    <PrefetchLink to="/processes">
                        معاملات سريعة
                    </PrefetchLink>
                </Button>
            </div>

            <div className="flex gap-4 mt-6">
                <Button variant="ghost" size="sm" onClick={() => setShowAlert(!showAlert)}>
                    {showAlert ? 'إخفاء التنبيه' : 'إظهار التنبيه'}
                </Button>

                {/* مثال الـ Alert Dialog */}
                <AlertDialog>
                    <AlertDialogTrigger asChild>
                        <Button variant="ghost" size="sm">اختبار الـ Dialog</Button>
                    </AlertDialogTrigger>
                    <AlertDialogContent>
                        <AlertDialogHeader>
                            <AlertDialogTitle>هل أنت متأكد من تنفيذ هذه العملية؟</AlertDialogTitle>
                            <AlertDialogDescription>
                                هذا الإجراء لا يمكن التراجع عنه. سيتم تنفيذ العملية فور نقرك على "تأكيد".
                            </AlertDialogDescription>
                        </AlertDialogHeader>
                        <AlertDialogFooter className="flex-row-reverse gap-3 pt-3">
                            <AlertDialogCancel onClick={handleCancel}>إلغاء</AlertDialogCancel>
                            <AlertDialogAction onClick={handleConfirm}>تأكيد</AlertDialogAction>
                        </AlertDialogFooter>
                    </AlertDialogContent>
                </AlertDialog>
            </div>

            {showAlert && (
                <AnimatedContainer duration={300} className="max-w-md w-full">
                    <Alert className="border-none shadow-lg">
                        <AlertTitle className=" font-bold text-lg">نجاح العملية!</AlertTitle>
                        <AlertDescription className="">
                            لقد تم تفعيل نظام التنبيهات الجديد بنجاح في هيكلة المشروع الجديدة.
                        </AlertDescription>
                    </Alert>
                </AnimatedContainer>
            )}
        </AnimatedContainer>
    );
}