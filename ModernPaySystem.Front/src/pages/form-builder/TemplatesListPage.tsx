
import { useForms } from '@/features/form-builder/model/useForms';
import { Button } from '@/shared/ui/button';
import { Card } from '@/shared/ui/card';
import { Skeleton } from '@/shared/ui/common/skeleton';
import { useNavigate } from 'react-router-dom';
import { PlusCircle, FileText } from 'lucide-react';
import { AnimatedContainer } from '@/shared/ui/common/animated-container';

export const TemplatesListPage = () => {
    const { data: forms = [], isLoading } = useForms();
    const navigate = useNavigate();

    if (isLoading) {
        return <Skeleton cards={3} />;
    }

    return (
        <AnimatedContainer className="container mx-auto p-6 space-y-6">
            <div className="flex justify-between items-center">
                <div>
                    <h1 className="text-3xl font-bold">نماذج الطلبات</h1>
                    <p className="text-muted-foreground">قم بإدارة وإنشاء قوالب النماذج الخاصة بك</p>
                </div>
                <Button onClick={() => navigate('/form-builder/templates/new')} className="gap-2">
                    <PlusCircle className="h-4 w-4" />
                    إنشاء نموذج جديد
                </Button>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                {forms.length === 0 ? (
                    <div className="col-span-12 flex flex-col items-center justify-center py-20 text-gray-500  rounded-xl border border-dashed">
                        <FileText className="h-16 w-16 mb-4 text-gray-300" />
                        <h3 className="text-lg font-medium">لا توجد نماذج حالياً</h3>
                        <p className="text-sm mb-6">ابدأ بإنشاء نموذجك الأول للعمليات</p>
                        <Button onClick={() => navigate('/form-builder/templates/new')}>إنشاء نموذج جديد</Button>
                    </div>
                ) : (
                    forms.map(form => (
                        <Card
                            key={form.id}
                            className="p-6 hover:shadow-lg transition-all cursor-pointer border-t-4 border-t-primary group"
                            onClick={() => navigate(`/form-builder/templates/${form.id}`)} // Edit or View
                        >
                            <div className="flex justify-between items-start mb-4">
                                <h3 className="text-xl font-semibold group-hover:text-primary transition-colors">{form.title}</h3>
                            </div>

                            <div className="text-sm text-gray-500 mb-4 line-clamp-2 min-h-[40px]">
                                {form.description || 'لا يوجد وصف'}
                            </div>

                            <div className="flex items-center justify-between text-xs text-gray-400 pt-4 border-t">
                                <span>{form.fields.length} حقول</span>
                                <span>ID: {form.id.substring(0, 8)}...</span>
                            </div>
                        </Card>
                    ))
                )}
            </div>
        </AnimatedContainer>
    );
};
