import React, { useState } from 'react';
import { Search, Eraser } from 'lucide-react';
import { Button } from '@/shared/ui/button';
import { Input } from '@/shared/ui/input';
import { Label } from '@/shared/ui/label';
import {
    Dialog,
    DialogContent,
    DialogHeader,
    DialogTitle,
    DialogFooter,
} from "@/shared/ui/dialog"
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/shared/ui/select"

interface SearchModalProps {
    open: boolean;
    onClose: () => void;
}

export const SearchModal: React.FC<SearchModalProps> = ({ open, onClose }) => {
    const [searchCriteria, setSearchCriteria] = useState({
        contractNo: '',
        tenantName: '',
        lessorName: '',
        propertyNo: '',
        dateFrom: '',
        dateTo: '',
        city: '',
        district: '',
    });

    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const { name, value } = e.target;
        setSearchCriteria(prev => ({ ...prev, [name]: value }));
    };

    const handleSelectChange = (name: string, value: string) => {
        setSearchCriteria(prev => ({ ...prev, [name]: value }));
    };

    const handleClear = () => {
        setSearchCriteria({
            contractNo: '',
            tenantName: '',
            lessorName: '',
            propertyNo: '',
            dateFrom: '',
            dateTo: '',
            city: '',
            district: '',
        });
    };

    const handleSearch = () => {
        console.log('Searching with criteria:', searchCriteria);
        onClose();
    };

    return (
        <Dialog open={open} onOpenChange={onClose}>
            <DialogContent className="max-w-2xl rounded-2xl border-none shadow-2xl p-0 overflow-hidden">
                <DialogHeader className="p-6 border-b bg-muted/20">
                    <DialogTitle className="flex items-center gap-3 text-xl font-bold text-primary ">
                        بحث متقدم عن العقود
                        <div className="p-2 bg-primary/10 rounded-lg">
                            <Search className="w-5 h-5" />
                        </div>
                    </DialogTitle>
                </DialogHeader>

                <div className="p-8 grid grid-cols-1 md:grid-cols-2 gap-6">
                    <div className="space-y-2">
                        <Label className="text-sm font-semibold text-muted-foreground text-right block">رقم العقد</Label>
                        <Input
                            name="contractNo"
                            placeholder="مثال: CON-2023-001"
                            value={searchCriteria.contractNo}
                            onChange={handleChange}
                            className="text-right h-11 rounded-xl"
                        />
                    </div>

                    <div className="space-y-2">
                        <Label className="text-sm font-semibold text-muted-foreground text-right block">اسم المستأجر</Label>
                        <Input
                            name="tenantName"
                            placeholder="ابحث بالاسم..."
                            value={searchCriteria.tenantName}
                            onChange={handleChange}
                            className="text-right h-11 rounded-xl"
                        />
                    </div>

                    <div className="space-y-2">
                        <Label className="text-sm font-semibold text-muted-foreground text-right block">اسم المؤجر</Label>
                        <Input
                            name="lessorName"
                            placeholder="ابحث بالاسم..."
                            value={searchCriteria.lessorName}
                            onChange={handleChange}
                            className="text-right h-11 rounded-xl"
                        />
                    </div>

                    <div className="space-y-2">
                        <Label className="text-sm font-semibold text-muted-foreground text-right block">رقم العقار</Label>
                        <Input
                            name="propertyNo"
                            placeholder="رقم العقار الموحد"
                            value={searchCriteria.propertyNo}
                            onChange={handleChange}
                            className="text-right h-11 rounded-xl"
                        />
                    </div>

                    <div className="space-y-2">
                        <Label className="text-sm font-semibold text-muted-foreground text-right block">تاريخ البداية</Label>
                        <Input
                            type="date"
                            name="dateFrom"
                            value={searchCriteria.dateFrom}
                            onChange={handleChange}
                            className="text-right h-11 rounded-xl"
                        />
                    </div>

                    <div className="space-y-2">
                        <Label className="text-sm font-semibold text-muted-foreground text-right block">تاريخ النهاية</Label>
                        <Input
                            type="date"
                            name="dateTo"
                            value={searchCriteria.dateTo}
                            onChange={handleChange}
                            className="text-right h-11 rounded-xl"
                        />
                    </div>

                    <div className="space-y-2">
                        <Label className="text-sm font-semibold text-muted-foreground text-right block">المدينة</Label>
                        <Select
                            value={searchCriteria.city}
                            onValueChange={(val) => handleSelectChange('city', val)}
                        >
                            <SelectTrigger className="h-11 rounded-xl text-right flex-row-reverse">
                                <SelectValue placeholder="جميع المدن والمناطق" />
                            </SelectTrigger>
                            <SelectContent className="text-right">
                                <SelectItem value="all">جميع المدن والمناطق</SelectItem>
                                <SelectItem value="1">الرياض</SelectItem>
                                <SelectItem value="2">جدة</SelectItem>
                                <SelectItem value="3">مكة المكرمة</SelectItem>
                                <SelectItem value="4">المدينة المنورة</SelectItem>
                            </SelectContent>
                        </Select>
                    </div>

                    <div className="space-y-2">
                        <Label className="text-sm font-semibold text-muted-foreground text-right block">الحي</Label>
                        <Input
                            name="district"
                            placeholder="اسم الحي..."
                            value={searchCriteria.district}
                            onChange={handleChange}
                            className="text-right h-11 rounded-xl"
                        />
                    </div>
                </div>

                <DialogFooter className="p-6 border-t bg-muted/40 flex flex-row-reverse justify-start gap-4">
                    <Button onClick={handleSearch} className="gap-2 px-10 h-11 rounded-xl shadow-lg shadow-primary/20">
                        تنفيذ البحث
                        <Search className="w-4 h-4" />
                    </Button>
                    <Button variant="outline" onClick={handleClear} className="gap-2 h-11 rounded-xl hover:bg-background">
                        مسح الفلاتر
                        <Eraser className="w-4 h-4" />
                    </Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
};

