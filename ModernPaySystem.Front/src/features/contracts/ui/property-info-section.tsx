import React from 'react';
import { Home } from 'lucide-react';
import { Input } from '@/shared/ui/input';
import { Label } from '@/shared/ui/label';
import { Textarea } from '@/shared/ui/textarea';
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/shared/ui/select"

import type { ContractFormData } from '@/entities/contracts/types/contract-types';

interface PropertyInfoSectionProps {
    formData: ContractFormData;
    onChange: (data: ContractFormData) => void;
}

export const PropertyInfoSection: React.FC<PropertyInfoSectionProps> = ({ formData, onChange }) => {
    const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
        const { name, value } = e.target;
        onChange({ ...formData, [name]: value });
    };

    const handleSelectChange = (name: string, value: string) => {
        onChange({ ...formData, [name]: value });
    };

    return (
        <div className="bg-card rounded-2xl border shadow-sm p-8 overflow-hidden">
            <div className="flex items-center gap-3 mb-8 text-primary border-b pb-4 justify-end">
                <h3 className="text-xl font-bold">معلومات عن العقار المؤجر</h3>
                <div className="p-2 bg-primary/10 rounded-lg">
                    <Home className="w-5 h-5" />
                </div>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
                <div className="space-y-2">
                    <Label className="text-sm font-semibold text-muted-foreground text-right block">رقم العقار *</Label>
                    <Input
                        name="propertyNo"
                        placeholder="أدخل رقم العقار..."
                        value={formData.propertyNo || ''}
                        onChange={handleInputChange}
                        className="text-right h-11 rounded-xl"
                        required
                    />
                </div>

                <div className="space-y-2">
                    <Label className="text-sm font-semibold text-muted-foreground text-right block">المدينة *</Label>
                    <Select
                        value={formData.city || ''}
                        onValueChange={(val) => handleSelectChange('city', val)}
                    >
                        <SelectTrigger className="h-11 rounded-xl text-right flex-row-reverse">
                            <SelectValue placeholder="اختر مدينة..." />
                        </SelectTrigger>
                        <SelectContent className="text-right">
                            <SelectItem value="1">الرياض</SelectItem>
                            <SelectItem value="2">جدة</SelectItem>
                            <SelectItem value="3">مكة المكرمة</SelectItem>
                            <SelectItem value="4">المدينة المنورة</SelectItem>
                            <SelectItem value="5">الدمام</SelectItem>
                        </SelectContent>
                    </Select>
                </div>

                <div className="space-y-2">
                    <Label className="text-sm font-semibold text-muted-foreground text-right block">الحي *</Label>
                    <Input
                        name="district"
                        placeholder="اسم الحي..."
                        value={formData.district || ''}
                        onChange={handleInputChange}
                        className="text-right h-11 rounded-xl"
                        required
                    />
                </div>

                <div className="space-y-2">
                    <Label className="text-sm font-semibold text-muted-foreground text-right block">الشارع *</Label>
                    <Input
                        name="street"
                        placeholder="اسم الشارع..."
                        value={formData.street || ''}
                        onChange={handleInputChange}
                        className="text-right h-11 rounded-xl"
                        required
                    />
                </div>

                <div className="space-y-2 lg:col-span-1">
                    <Label className="text-sm font-semibold text-muted-foreground text-right block">الطابق *</Label>
                    <Input
                        name="floor"
                        placeholder="رقم الطابق..."
                        value={formData.floor || ''}
                        onChange={handleInputChange}
                        className="text-right h-11 rounded-xl"
                        required
                    />
                </div>

                <div className="space-y-2">
                    <Label className="text-sm font-semibold text-muted-foreground text-right block">الشقة *</Label>
                    <Input
                        name="apartment"
                        placeholder="رقم الشقة..."
                        value={formData.apartment || ''}
                        onChange={handleInputChange}
                        className="text-right h-11 rounded-xl"
                        required
                    />
                </div>

                <div className="space-y-2">
                    <Label className="text-sm font-semibold text-muted-foreground text-right block">صفة الإشغال *</Label>
                    <Input
                        name="occupancyType"
                        placeholder="مثال: سكني، تجاري..."
                        value={formData.occupancyType || ''}
                        onChange={handleInputChange}
                        className="text-right h-11 rounded-xl"
                        required
                    />
                </div>

                <div className="space-y-2">
                    <Label className="text-sm font-semibold text-muted-foreground text-right block">رقم الهاتف *</Label>
                    <Input
                        name="phone"
                        placeholder="رقم الهاتف المرتبط بالعقار..."
                        value={formData.phone || ''}
                        onChange={handleInputChange}
                        className="text-right h-11 rounded-xl"
                        required
                    />
                </div>

                <div className="space-y-2">
                    <Label className="text-sm font-semibold text-muted-foreground text-right block">المساحة (م²) *</Label>
                    <Input
                        name="area"
                        type="number"
                        placeholder="0.00"
                        value={formData.area || ''}
                        onChange={handleInputChange}
                        className="text-right h-11 rounded-xl"
                        required
                    />
                </div>

                <div className="space-y-2">
                    <Label className="text-sm font-semibold text-muted-foreground text-right block">المنطقة العقارية</Label>
                    <Select
                        value={formData.realEstateRegion || ''}
                        onValueChange={(val) => handleSelectChange('realEstateRegion', val)}
                    >
                        <SelectTrigger className="h-11 rounded-xl text-right flex-row-reverse">
                            <SelectValue placeholder="اختر منطقة..." />
                        </SelectTrigger>
                        <SelectContent className="text-right">
                            <SelectItem value="1">شمال</SelectItem>
                            <SelectItem value="2">جنوب</SelectItem>
                            <SelectItem value="3">شرق</SelectItem>
                            <SelectItem value="4">غرب</SelectItem>
                            <SelectItem value="5">وسط</SelectItem>
                        </SelectContent>
                    </Select>
                </div>

                <div className="space-y-2">
                    <Label className="text-sm font-semibold text-muted-foreground text-right block">رقم الإيصال الضريبي</Label>
                    <Input
                        name="taxReceiptNo"
                        placeholder="أدخل الرقم..."
                        value={formData.taxReceiptNo || ''}
                        onChange={handleInputChange}
                        className="text-right h-11 rounded-xl"
                    />
                </div>

                <div className="space-y-2">
                    <Label className="text-sm font-semibold text-muted-foreground text-right block">مبلغ الإيصال</Label>
                    <Input
                        name="taxReceiptAmount"
                        type="number"
                        placeholder="0.00"
                        value={formData.taxReceiptAmount || ''}
                        onChange={handleInputChange}
                        className="text-right h-11 rounded-xl"
                    />
                </div>
            </div>

            <div className="mt-10 space-y-8 text-right">
                <div className="space-y-2">
                    <Label className="text-sm font-semibold text-muted-foreground text-right block">الأشياء الثابتة</Label>
                    <Textarea
                        name="assets"
                        rows={2}
                        className="w-full p-4 rounded-xl text-right"
                        value={formData.assets || ''}
                        onChange={handleInputChange}
                        placeholder="الأشياء الثابتة في العقار (الأثاث، المكيفات، ...)"
                    />
                </div>

                <div className="space-y-2">
                    <Label className="text-sm font-semibold text-muted-foreground text-right block">أوصاف المستأجر</Label>
                    <Textarea
                        name="tenantDescription"
                        rows={2}
                        className="w-full p-4 rounded-xl text-right"
                        value={formData.tenantDescription || ''}
                        onChange={handleInputChange}
                        placeholder="أية أوصاف إضافية للمستأجر..."
                    />
                </div>

                <div className="space-y-2">
                    <Label className="text-sm font-semibold text-muted-foreground text-right block">مالك وموقع السكن السابق</Label>
                    <Input
                        name="previousResidence"
                        placeholder="اسم المالك والموقع السابق..."
                        value={formData.previousResidence || ''}
                        onChange={handleInputChange}
                        className="text-right h-11 rounded-xl"
                    />
                </div>
            </div>
        </div>
    );
};

