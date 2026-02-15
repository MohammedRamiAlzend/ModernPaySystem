
import { useForms } from '@/features/form-builder/model/useForms';
import { Button } from '@/shared/ui/button';
import { Card } from '@/shared/ui/card';
import { Skeleton } from '@/shared/ui/common/skeleton';
import { useNavigate } from 'react-router-dom';
import { PlusCircle, FileText } from 'lucide-react';
import { AnimatedContainer } from '@/shared/ui/common/animated-container';

export const TemplatesList = () => {
    const { data: forms = [], isLoading } = useForms();
    const navigate = useNavigate();

    if (isLoading) {
        return <Skeleton cards={3} />;
    }

    return (
        <AnimatedContainer className="space-y-6">
            <div className="flex justify-between items-center bg-card/50 backdrop-blur-sm p-6 rounded-2xl border-none shadow-sm">
                <div className="space-y-1">
                    <h2 className="text-2xl font-bold">نماذج الطلبات</h2>
                    <p className="text-muted-foreground text-sm">قم بإدارة وإنشاء قوالب النماذج الخاصة بك</p>
                </div>
                <Button onClick={() => navigate('/form-builder/templates/new')} className="gap-2 rounded-xl h-11">
                    <PlusCircle className="h-4 w-4" />
                    إنشاء نموذج جديد
                </Button>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                {forms.length === 0 ? (
                    <div className="col-span-full flex flex-col items-center justify-center py-20 text-gray-500 bg-card/30 backdrop-blur-sm rounded-3xl border-2 border-dashed border-muted-foreground/20">
                        <FileText className="h-16 w-16 mb-4 text-primary/20" />
                        <h3 className="text-xl font-bold text-foreground">لا توجد نماذج حالياً</h3>
                        <p className="text-sm mb-8 text-muted-foreground">ابدأ بإنشاء نموذجك الأول للعمليات</p>
                        <Button onClick={() => navigate('/form-builder/templates/new')} className="rounded-xl px-8">إنشاء نموذج جديد</Button>
                    </div>
                ) : (
                    forms.map(form => (
                        <Card
                            key={form.id}
                            className="p-6 hover:shadow-xl transition-all cursor-pointer border-none shadow-md bg-card/50 backdrop-blur-sm group rounded-3xl relative overflow-hidden"
                            onClick={() => navigate(`/form-builder/templates/${form.id}`)}
                        >
                            <div className="absolute top-0 right-0 w-24 h-24 bg-primary/5 rounded-bl-full -mr-10 -mt-10 transition-transform group-hover:scale-110" />

                            <div className="flex justify-between items-start mb-4 relative z-10">
                                <h3 className="text-xl font-black group-hover:text-primary transition-colors line-clamp-1">{form.title}</h3>
                            </div>

                            <div className="flex items-center justify-between text-xs font-bold text-muted-foreground pt-4 border-t border-muted relative z-10">
                                <span className="flex items-center gap-1.5">
                                    <div className="w-1.5 h-1.5 rounded-full bg-primary" />
                                    {form.fields.length} حقول
                                </span>
                                <span className="opacity-0 group-hover:opacity-100 transition-opacity text-primary">تعديل النموذج ←</span>
                            </div>
                        </Card>
                    ))
                )}
            </div>
        </AnimatedContainer>
    );
};
