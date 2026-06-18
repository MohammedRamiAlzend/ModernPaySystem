import { useState } from 'react';
import { useFolderIcons } from '@/features/archiving/model/queries';
import { useCreateFolderIcon, useDeleteFolderIcon } from '@/features/archiving/model/mutations';
import { Button } from '@/shared/ui/button';
import { Input } from '@/shared/ui/input';
import { Label } from '@/shared/ui/label';
import { Textarea } from '@/shared/ui/textarea';
import {
    Dialog,
    DialogContent,
    DialogHeader,
    DialogTitle,
    DialogDescription,
} from '@/shared/ui/dialog';
import {
    AlertDialog,
    AlertDialogAction,
    AlertDialogCancel,
    AlertDialogContent,
    AlertDialogDescription,
    AlertDialogFooter,
    AlertDialogHeader,
    AlertDialogTitle,
} from '@/shared/ui/alert-dialog';
import { Loader2, Plus, Trash2, Image, Check } from 'lucide-react';

export default function FolderIconsPage() {
    const { data: icons = [], isLoading } = useFolderIcons();
    const createIcon = useCreateFolderIcon();
    const deleteIcon = useDeleteFolderIcon();
    const [showCreateDialog, setShowCreateDialog] = useState(false);
    const [iconName, setIconName] = useState('');
    const [svgContent, setSvgContent] = useState('');
    const [deleteConfirmId, setDeleteConfirmId] = useState<string | null>(null);
    const [previewSvg, setPreviewSvg] = useState<string | null>(null);

    const handleSvgChange = (value: string) => {
        setSvgContent(value);
        setPreviewSvg(value.trim() ? value : null);
    };

    const handleCreate = async () => {
        if (!iconName.trim() || !svgContent.trim()) return;
        await createIcon.mutateAsync({
            name: iconName.trim(),
            svgContent: svgContent.trim(),
            isDefault: icons.length === 0
        });
        setShowCreateDialog(false);
        setIconName('');
        setSvgContent('');
        setPreviewSvg(null);
    };

    const handleDelete = async () => {
        if (!deleteConfirmId) return;
        await deleteIcon.mutateAsync(deleteConfirmId);
        setDeleteConfirmId(null);
    };

    return (
        <div className="flex flex-col gap-6 p-6 w-full max-w-full overflow-x-hidden min-w-0" dir="rtl">
            <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
                <div className="flex flex-col gap-1 text-right">
                    <h1 className="text-xl font-bold text-primary">إدارة أيقونات المجلدات</h1>
                    <p className="text-xs text-muted-foreground font-medium">إضافة وحذف الأيقونات المخصصة للمجلدات في مستكشف الأرشيف</p>
                </div>
                <Button onClick={() => setShowCreateDialog(true)} className="rounded-xl px-5 font-bold shadow-lg shadow-primary/20 flex items-center gap-2">
                    <Plus className="h-4 w-4" />
                    <span>إضافة أيقونة جديدة</span>
                </Button>
            </div>

            <div className="bg-card border border-border rounded-3xl p-6 shadow-sm min-h-[400px]">
                {isLoading ? (
                    <div className="flex flex-col items-center justify-center py-24 gap-3 text-muted-foreground">
                        <Loader2 className="h-8 w-8 animate-spin text-primary" />
                        <span className="text-sm font-medium">جاري تحميل الأيقونات...</span>
                    </div>
                ) : icons.length === 0 ? (
                    <div className="py-24 flex flex-col items-center justify-center text-muted-foreground gap-3">
                        <Image className="w-16 h-16 stroke-[1.2] text-muted-foreground/45" />
                        <span className="text-sm font-semibold">لا توجد أيقونات مخصصة</span>
                        <span className="text-xs text-muted-foreground/60">قم بإضافة أيقونات SVG جديدة لتخصيص مظهر المجلدات.</span>
                    </div>
                ) : (
                    <div className="grid grid-cols-2 sm:grid-cols-4 md:grid-cols-6 lg:grid-cols-8 gap-4">
                        {icons.map((icon) => (
                            <IconCard
                                key={icon.id}
                                icon={icon}
                                onDelete={setDeleteConfirmId}
                            />
                        ))}
                    </div>
                )}
            </div>

            <Dialog open={showCreateDialog} onOpenChange={setShowCreateDialog}>
                <DialogContent className="sm:max-w-lg max-h-[80vh] overflow-y-auto">
                    <DialogHeader>
                        <DialogTitle>إضافة أيقونة SVG جديدة</DialogTitle>
                        <DialogDescription>قم بإدخال كود SVG للأيقونة التي تريد استخدامها للمجلدات.</DialogDescription>
                    </DialogHeader>
                    <div className="flex flex-col gap-4 py-4">
                        <div className="flex flex-col gap-2">
                            <Label htmlFor="icon-name">اسم الأيقونة</Label>
                            <Input
                                id="icon-name"
                                value={iconName}
                                onChange={(e) => setIconName(e.target.value)}
                                placeholder="أدخل اسم الأيقونة..."
                            />
                        </div>
                        <div className="flex flex-col gap-2">
                            <Label htmlFor="svg-content">كود SVG</Label>
                            <Textarea
                                id="svg-content"
                                value={svgContent}
                                onChange={(e) => handleSvgChange(e.target.value)}
                                placeholder="<svg>...</svg>"
                                className="min-h-[200px] font-mono text-xs"
                            />
                        </div>
                        {previewSvg && (
                            <div className="flex flex-col gap-2">
                                <Label>معاينة</Label>
                                <div className="border border-border rounded-2xl p-6 flex items-center justify-center bg-muted/20 min-h-[100px]">
                                    <div className="w-16 h-16 flex items-center justify-center overflow-hidden [&_svg]:w-full [&_svg]:h-full [&_svg]:max-w-full [&_svg]:max-h-full" dangerouslySetInnerHTML={{ __html: previewSvg }} />
                                </div>
                            </div>
                        )}
                        <Button
                            onClick={handleCreate}
                            disabled={!iconName.trim() || !svgContent.trim() || createIcon.isPending}
                            className="w-full rounded-xl font-bold"
                        >
                            {createIcon.isPending ? <Loader2 className="h-4 w-4 animate-spin ml-2" /> : <Plus className="h-4 w-4 ml-2" />}
                            إضافة الأيقونة
                        </Button>
                    </div>
                </DialogContent>
            </Dialog>

            <AlertDialog open={!!deleteConfirmId} onOpenChange={() => setDeleteConfirmId(null)}>
                <AlertDialogContent>
                    <AlertDialogHeader>
                        <AlertDialogTitle>حذف الأيقونة</AlertDialogTitle>
                        <AlertDialogDescription>هل أنت متأكد من حذف هذه الأيقونة؟ سيتم إزالة الأيقونة من جميع المجلدات التي تستخدمها.</AlertDialogDescription>
                    </AlertDialogHeader>
                    <AlertDialogFooter>
                        <AlertDialogCancel>إلغاء</AlertDialogCancel>
                        <AlertDialogAction onClick={handleDelete} className="bg-destructive text-destructive-foreground">
                            {deleteIcon.isPending ? <Loader2 className="h-4 w-4 animate-spin ml-2" /> : null}
                            حذف
                        </AlertDialogAction>
                    </AlertDialogFooter>
                </AlertDialogContent>
            </AlertDialog>
        </div>
    );
}

function IconCard({ icon, onDelete }: { icon: { id: string; name: string; isDefault: boolean; svgContent: string }; onDelete: (id: string) => void }) {
    return (
        <div className="group relative bg-muted/20 hover:bg-amber-500/5 border border-border/80 hover:border-amber-500/30 rounded-2xl p-4 flex flex-col items-center justify-center transition-all duration-300 text-center">
            {icon.isDefault && (
                <div className="absolute top-2 right-2 bg-primary/10 text-primary rounded-full p-1">
                    <Check className="h-3 w-3" />
                </div>
            )}
            <div className="absolute top-2 left-2 opacity-0 group-hover:opacity-100 transition-opacity">
                <button
                    onClick={(e) => { e.stopPropagation(); onDelete(icon.id); }}
                    className="p-1.5 rounded-lg text-destructive hover:bg-destructive/10 transition-colors"
                >
                    <Trash2 className="h-3.5 w-3.5" />
                </button>
            </div>
            <div className="w-14 h-14 rounded-xl bg-amber-500/10 flex items-center justify-center mb-2">
                <div className="w-10 h-10 flex items-center justify-center overflow-hidden [&_svg]:w-full [&_svg]:h-full [&_svg]:max-w-full [&_svg]:max-h-full" dangerouslySetInnerHTML={{ __html: icon.svgContent }} />
            </div>
            <span className="text-xs font-bold text-foreground line-clamp-2 break-all w-full px-1">
                {icon.name}
            </span>
        </div>
    );
}
