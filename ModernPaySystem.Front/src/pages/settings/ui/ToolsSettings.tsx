import { Card } from '@/shared/ui/card';
import { Button } from '@/shared/ui/button';
import { Download, Monitor, ShieldCheck, Cpu } from 'lucide-react';

export const ToolsSettings = () => {
    return (
        <div className="space-y-6 animate-in fade-in slide-in-from-bottom-4 duration-500">
            <div className="flex flex-col gap-1">
                <h2 className="text-xl font-bold text-primary">أدوات الدعم والتشغيل</h2>
                <p className="text-sm text-muted-foreground">تحميل البرمجيات والأدوات اللازمة لتشغيل كافة ميزات النظام</p>
            </div>

            <Card className="p-8 border-2 border-primary/10 overflow-hidden relative group">
                <div className="absolute top-0 right-0 p-12 bg-primary/5 rounded-full -mr-16 -mt-16 group-hover:bg-primary/10 transition-colors" />

                <div className="flex flex-col md:flex-row items-center gap-8 relative z-10">
                    <div className="p-6 bg-primary/10 rounded-3xl text-primary shadow-inner">
                        <Monitor className="w-12 h-12" />
                    </div>

                    <div className="flex-1 text-center md:text-right space-y-2">
                        <h3 className="text-xl font-black text-primary flex items-center justify-center md:justify-start gap-2">
                            تعريف الماسح الضوئي (Scanner Setup)
                        </h3>
                        <p className="text-muted-foreground text-sm max-w-xl leading-relaxed">
                            قم بتحميل وتثبيت هذا البرنامج لتمكين ميزة المسح الضوئي المباشر من المتصفح.
                            يدعم هذا المشغل كافة أجهزة الـ Scanner المتصلة بجهاز الكمبيوتر الخاص بك.
                        </p>

                        <div className="flex flex-wrap items-center justify-center md:justify-start gap-4 pt-4 text-[11px] text-muted-foreground/60 font-medium">
                            <span className="flex items-center gap-1.5"><ShieldCheck className="w-3.5 h-3.5" /> آمن وموثوق</span>
                            <span className="flex items-center gap-1.5"><Cpu className="w-3.5 h-3.5" /> إصدار Windows v1.2</span>
                            <span className="flex items-center gap-1.5 underline">ملف تنفيذي (.exe)</span>
                        </div>
                    </div>

                    <div className="shrink-0 pt-4 md:pt-0">
                        <Button
                            asChild
                            size="lg"
                            className="rounded-2xl px-8 h-14 font-black shadow-lg shadow-primary/20 hover:shadow-xl transition-all gap-3"
                        >
                            <a href="/scan-setup.exe" download="scan-setup.exe">
                                <Download className="w-5 h-5" />
                                تحميل الآن
                            </a>
                        </Button>
                    </div>
                </div>
            </Card>


        </div>
    );
};
