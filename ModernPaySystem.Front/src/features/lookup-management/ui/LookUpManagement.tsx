import { useState } from 'react';
import {
    useLookUpFields,
    useLookUpFieldValues,
    useLookUpMutations,
    LookUpField,
    fetchLookUpFieldValues
} from '../api/lookupApi';
import { Button } from '@/shared/ui/button';
import { Input } from '@/shared/ui/input';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/shared/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/shared/ui/table';
import { Plus, Trash2, Edit2, Loader2, ChevronLeft, ChevronRight, Settings2, List } from 'lucide-react';
import {
    Dialog,
    DialogContent,
    DialogHeader,
    DialogTitle,
    DialogFooter,
    DialogDescription
} from '@/shared/ui/dialog';
import { Label } from '@/shared/ui/label';
import { useUIStore } from '@/app/store/uiStore';

export const LookUpManagement = () => {
    const { showStatus, showConfirm } = useUIStore();
    const [selectedField, setSelectedField] = useState<LookUpField | null>(null);
    const [isFieldDialogOpen, setIsFieldDialogOpen] = useState(false);
    const [isValueDialogOpen, setIsValueDialogOpen] = useState(false);
    const [editingField, setEditingField] = useState<LookUpField | null>(null);
    const [editingValue, setEditingValue] = useState<{ id: string, desc: string } | null>(null);
    const [fieldName, setFieldName] = useState('');
    const [valueDesc, setValueDesc] = useState('');

    const { data: fields = [], isLoading: isLoadingFields } = useLookUpFields();
    const { data: values = [], isLoading: isLoadingValues } = useLookUpFieldValues(selectedField?.id || null);
    const { createField, updateField, deleteField, createValue, updateValue, deleteValue } = useLookUpMutations();

    const handleAddField = () => {
        setEditingField(null);
        setFieldName('');
        setIsFieldDialogOpen(true);
    };

    const handleEditField = (field: LookUpField) => {
        setEditingField(field);
        setFieldName(field.filedName);
        setIsFieldDialogOpen(true);
    };

    const handleSaveField = async () => {
        if (!fieldName.trim()) return;

        if (editingField) {
            await updateField.mutateAsync({ id: editingField.id, filedName: fieldName });
        } else {
            await createField.mutateAsync(fieldName);
        }
        setIsFieldDialogOpen(false);
    };

    const handleDeleteFieldClick = async (field: LookUpField) => {
        // Check if field has values
        const fieldValues = await fetchLookUpFieldValues(field.id);
        if (fieldValues && fieldValues.length > 0) {
            showStatus({
                type: 'warning',
                title: 'لا يمكن الحذف',
                message: 'لا يمكن حذف هذا الحقل لأنه يحتوي على قيم مرتبطة به. يرجى حذف القيم أولاً.'
            });
            return;
        }

        showConfirm({
            title: 'حذف الحقل',
            message: `هل أنت متأكد من حذف الحقل "${field.filedName}"؟ لا يمكن التراجع عن هذا الإجراء.`,
            variant: 'destructive',
            confirmLabel: 'حذف',
            onConfirm: async () => {
                await deleteField.mutateAsync(field.id);
                if (selectedField?.id === field.id) setSelectedField(null);
            }
        });
    };

    const handleAddValue = () => {
        if (!selectedField) return;
        setEditingValue(null);
        setValueDesc('');
        setIsValueDialogOpen(true);
    };

    const handleEditValue = (value: { id: string, desc: string }) => {
        setEditingValue(value);
        setValueDesc(value.desc);
        setIsValueDialogOpen(true);
    };

    const handleSaveValue = async () => {
        if (!valueDesc.trim() || !selectedField) return;

        if (editingValue) {
            await updateValue.mutateAsync({
                id: editingValue.id,
                lookUpFiledId: selectedField.id,
                desc: valueDesc
            });
        } else {
            await createValue.mutateAsync({
                lookUpFiledId: selectedField.id,
                desc: valueDesc
            });
        }
        setIsValueDialogOpen(false);
    };

    const handleDeleteValueClick = (id: string, desc: string) => {
        showConfirm({
            title: 'حذف القيمة',
            message: `هل أنت متأكد من حذف القيمة "${desc}"؟`,
            variant: 'destructive',
            confirmLabel: 'حذف',
            onConfirm: () => {
                deleteValue.mutate(id);
            }
        });
    };


    return (
        <div className="grid grid-cols-1 md:grid-cols-12 gap-6" style={{ direction: 'rtl' }}>
            {/* Fields List */}
            <Card className="md:col-span-5 border-none shadow-lg bg-card/50 backdrop-blur-sm">
                <CardHeader className="flex flex-row items-center justify-between pb-2">
                    <div className="flex flex-col gap-1">
                        <CardTitle className="text-xl font-bold flex items-center gap-2">
                            <Settings2 className="w-5 h-5 text-primary" />
                            حقول الإعدادات
                        </CardTitle>
                        <CardDescription className="text-xs">إدارة المسميات الرئيسية للنظام</CardDescription>
                    </div>
                    <Button onClick={handleAddField} size="sm" className="rounded-xl h-9">
                        <Plus className="w-4 h-4 ml-1" />
                        إضافة
                    </Button>
                </CardHeader>
                <CardContent>
                    <div className="space-y-2 max-h-[600px] overflow-y-auto pr-1 custom-scrollbar">
                        {isLoadingFields ? (
                            <div className="flex justify-center p-8"><Loader2 className="animate-spin text-primary" /></div>
                        ) : fields.length > 0 ? (
                            fields.map((field) => (
                                <div
                                    key={field.id}
                                    onClick={() => setSelectedField(field)}
                                    className={`group p-3 rounded-xl border-2 transition-all cursor-pointer flex items-center justify-between ${selectedField?.id === field.id
                                        ? 'border-primary bg-primary/5 shadow-md'
                                        : 'border-transparent hover:border-primary/30 hover:bg-accent'
                                        }`}
                                >
                                    <div className="flex items-center gap-3">
                                        <div className={`p-2 rounded-lg transition-colors ${selectedField?.id === field.id ? 'bg-primary text-white' : 'bg-muted text-muted-foreground group-hover:bg-primary/20 group-hover:text-primary'}`}>
                                            <List className="w-4 h-4" />
                                        </div>
                                        <span className={`font-bold text-sm ${selectedField?.id === field.id ? 'text-primary' : 'text-foreground'}`}>
                                            {field.filedName}
                                        </span>
                                    </div>
                                    <div className="flex items-center gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
                                        <Button
                                            variant="ghost"
                                            size="icon"
                                            className="h-8 w-8 rounded-full text-muted-foreground hover:text-primary"
                                            onClick={(e) => { e.stopPropagation(); handleEditField(field); }}
                                        >
                                            <Edit2 className="w-3.5 h-3.5" />
                                        </Button>
                                        <Button
                                            variant="ghost"
                                            size="icon"
                                            className="h-8 w-8 rounded-full text-muted-foreground hover:text-destructive"
                                            onClick={(e) => { e.stopPropagation(); handleDeleteFieldClick(field); }}
                                        >
                                            <Trash2 className="w-3.5 h-3.5" />
                                        </Button>
                                        <ChevronLeft className={`w-4 h-4 mr-1 transition-transform ${selectedField?.id === field.id ? '-translate-x-1 text-primary' : 'text-muted-foreground'}`} />
                                    </div>
                                </div>
                            ))
                        ) : (
                            <div className="text-center p-8 text-muted-foreground border-2 border-dashed rounded-2xl">
                                لا توجد حقول مضافة بعد
                            </div>
                        )}
                    </div>
                </CardContent>
            </Card>

            {/* Field Values List */}
            <Card className="md:col-span-7 border-none shadow-lg bg-card/50 backdrop-blur-sm overflow-hidden flex flex-col">
                {selectedField ? (
                    <>
                        <CardHeader className="flex flex-row items-center justify-between pb-2 bg-primary/5 border-b border-primary/10">
                            <div className="flex flex-col gap-1">
                                <CardTitle className="text-xl font-bold text-primary flex items-center gap-2">
                                    <div className="p-1.5 bg-primary rounded-lg text-white">
                                        <ChevronRight className="w-4 h-4" />
                                    </div>
                                    قيم {selectedField.filedName}
                                </CardTitle>
                                <CardDescription className="text-xs">تعديل وإضافة القيم لهذا الحقل</CardDescription>
                            </div>
                            <Button onClick={handleAddValue} size="sm" className="rounded-xl h-9">
                                <Plus className="w-4 h-4 ml-1" />
                                إضافة قيمة
                            </Button>
                        </CardHeader>
                        <CardContent className="p-0 flex-1 overflow-hidden">
                            {isLoadingValues ? (
                                <div className="flex justify-center p-8"><Loader2 className="animate-spin text-primary" /></div>
                            ) : (
                                <div className="overflow-auto max-h-[600px] custom-scrollbar">
                                    <Table>
                                        <TableHeader className="bg-muted/50 sticky top-0 z-10">
                                            <TableRow>
                                                <TableHead className="text-right font-bold w-16">#</TableHead>
                                                <TableHead className="text-right font-bold">القيمة / الوصف</TableHead>
                                                <TableHead className="text-left font-bold w-24">الإجراءات</TableHead>
                                            </TableRow>
                                        </TableHeader>
                                        <TableBody>
                                            {values.length > 0 ? (
                                                values.map((value, index) => (
                                                    <TableRow key={value.id} className="hover:bg-muted/30 transition-colors">
                                                        <TableCell className="text-right font-mono text-muted-foreground">{index + 1}</TableCell>
                                                        <TableCell className="text-right font-medium">{value.desc}</TableCell>
                                                        <TableCell className="text-left">
                                                            <div className="flex items-center gap-2">
                                                                <Button
                                                                    variant="ghost"
                                                                    size="icon"
                                                                    className="h-8 w-8 rounded-lg hover:bg-primary/10 hover:text-primary"
                                                                    onClick={() => handleEditValue(value)}
                                                                >
                                                                    <Edit2 className="w-4 h-4" />
                                                                </Button>
                                                                <Button
                                                                    variant="ghost"
                                                                    size="icon"
                                                                    className="h-8 w-8 rounded-lg hover:bg-destructive/10 hover:text-destructive"
                                                                    onClick={() => handleDeleteValueClick(value.id, value.desc)}
                                                                >
                                                                    <Trash2 className="w-4 h-4" />
                                                                </Button>
                                                            </div>
                                                        </TableCell>
                                                    </TableRow>
                                                ))
                                            ) : (
                                                <TableRow>
                                                    <TableCell colSpan={3} className="text-center h-48 text-muted-foreground italic">
                                                        لا توجد قيم مضافة لهذا الحقل
                                                    </TableCell>
                                                </TableRow>
                                            )}
                                        </TableBody>
                                    </Table>
                                </div>
                            )}
                        </CardContent>
                    </>
                ) : (
                    <div className="flex-1 flex flex-col items-center justify-center p-12 text-muted-foreground opacity-60">
                        <div className="w-20 h-20 bg-muted rounded-full flex items-center justify-center mb-4">
                            <ChevronRight className="w-10 h-10" />
                        </div>
                        <p className="text-lg font-bold">يرجى اختيار حقل من القائمة اليمنى</p>
                        <p className="text-sm">لعرض القيم المرتبطة به أو إدارتها</p>
                    </div>
                )}
            </Card>

            {/* Add/Edit Field Dialog */}
            <Dialog open={isFieldDialogOpen} onOpenChange={setIsFieldDialogOpen}>
                <DialogContent className="rounded-2xl max-w-sm" style={{ direction: 'rtl' }}>
                    <DialogHeader>
                        <DialogTitle>{editingField ? 'تعديل حقل' : 'إضافة حقل جديد'}</DialogTitle>
                        <DialogDescription>أدخل اسم الحقل الرئيسي للنظام</DialogDescription>
                    </DialogHeader>
                    <div className="py-4 space-y-4">
                        <div className="space-y-2">
                            <Label htmlFor="fieldName">اسم الحقل</Label>
                            <Input
                                id="fieldName"
                                placeholder="مثلاً: البلدان، الرتب، إلخ"
                                value={fieldName}
                                onChange={(e) => setFieldName(e.target.value)}
                                className="rounded-xl h-11"
                            />
                        </div>
                    </div>
                    <DialogFooter className="gap-2 sm:gap-0">
                        <Button onClick={handleSaveField} className="rounded-xl px-8 flex-1 sm:flex-none">
                            {updateField.isPending || createField.isPending ? <Loader2 className="w-4 h-4 animate-spin" /> : 'حفظ'}
                        </Button>
                        <Button variant="outline" onClick={() => setIsFieldDialogOpen(false)} className="rounded-xl border-none bg-muted hover:bg-muted/80 flex-1 sm:flex-none">
                            إلغاء
                        </Button>
                    </DialogFooter>
                </DialogContent>
            </Dialog>

            {/* Add/Edit Value Dialog */}
            <Dialog open={isValueDialogOpen} onOpenChange={setIsValueDialogOpen}>
                <DialogContent className="rounded-2xl max-w-sm" style={{ direction: 'rtl' }}>
                    <DialogHeader>
                        <DialogTitle>{editingValue ? 'تعديل قيمة' : 'إضافة قيمة جديدة'}</DialogTitle>
                        <DialogDescription>أدخل القيمة التي ستظهر في القوائم المنسدلة</DialogDescription>
                    </DialogHeader>
                    <div className="py-4 space-y-4">
                        <div className="space-y-2">
                            <Label htmlFor="valueDesc">القيمة / الوصف</Label>
                            <Input
                                id="valueDesc"
                                placeholder="مثلاً: لبنان، رائد، إلخ"
                                value={valueDesc}
                                onChange={(e) => setValueDesc(e.target.value)}
                                className="rounded-xl h-11"
                            />
                        </div>
                    </div>
                    <DialogFooter className="gap-2 sm:gap-0">
                        <Button onClick={handleSaveValue} className="rounded-xl px-8 flex-1 sm:flex-none">
                            {updateValue.isPending || createValue.isPending ? <Loader2 className="w-4 h-4 animate-spin" /> : 'حفظ'}
                        </Button>
                        <Button variant="outline" onClick={() => setIsValueDialogOpen(false)} className="rounded-xl border-none bg-muted hover:bg-muted/80 flex-1 sm:flex-none">
                            إلغاء
                        </Button>
                    </DialogFooter>
                </DialogContent>
            </Dialog>

        </div>
    );
};
