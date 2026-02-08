import React, { useState } from 'react';
import { User, Search, Plus, Upload } from 'lucide-react';
import { Button } from '@/shared/ui/button';
import { Input } from '@/shared/ui/input';
import { Label } from '@/shared/ui/label';
import { Textarea } from '@/shared/ui/textarea';
import { DocumentModal } from '@/shared/ui/common/document-modal';
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/shared/ui/select"

import type { ContractFormData } from '@/entities/contracts/types/contract-types';

interface LessorTenantSectionProps {
    formData: ContractFormData;
    onChange: (data: ContractFormData) => void;
}

export const LessorTenantSection: React.FC<LessorTenantSectionProps> = ({ formData, onChange }) => {
    const [showDocumentModal, setShowDocumentModal] = useState(false);
    const [documentType, setDocumentType] = useState<'lessor' | 'tenant'>('lessor');

    const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
        const { name, value } = e.target;
        onChange({ ...formData, [name]: value });
    };

    const handleSelectChange = (name: string, value: string) => {
        onChange({ ...formData, [name]: value });
    };

    const handleAddDocument = (type: 'lessor' | 'tenant') => {
        setDocumentType(type);
        setShowDocumentModal(true);
    };

    return (
        <div className="space-y-8 animate-in fade-in slide-in-from-bottom-4 duration-500">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
                {/* Lessor Card */}
                <div className="bg-card rounded-2xl border shadow-sm p-8 group hover:border-primary/20 transition-all">
                    <div className="flex items-center gap-3 mb-6 text-primary border-b pb-4 justify-end">
                        <h3 className="text-xl font-bold">بيانات المؤجر</h3>
                        <div className="p-2 bg-primary/10 rounded-lg group-hover:scale-110 transition-transform">
                            <User className="w-5 h-5" />
                        </div>
                    </div>

                    <div className="space-y-6">
                        <div className="space-y-2">
                            <Label className="text-sm font-semibold text-muted-foreground text-right block">المؤجر *</Label>
                            <div className="flex gap-2">
                                <div className="flex gap-1">
                                    <Button variant="outline" size="icon" type="button" className="rounded-lg h-11 w-11 hover:text-primary hover:bg-primary/5">
                                        <Plus className="w-4 h-4" />
                                    </Button>
                                    <Button variant="outline" size="icon" type="button" className="rounded-lg h-11 w-11">
                                        <Search className="w-4 h-4" />
                                    </Button>
                                </div>
                                <Select
                                    value={formData.lessorId?.toString() || ''}
                                    onValueChange={(val) => handleSelectChange('lessorId', val)}
                                >
                                    <SelectTrigger className="h-11 rounded-xl text-right flex-row-reverse flex-1">
                                        <SelectValue placeholder="اختر المؤجر..." />
                                    </SelectTrigger>
                                    <SelectContent className="text-right">
                                        <SelectItem value="1">أحمد محمد علي</SelectItem>
                                        <SelectItem value="2">سارة خالد عبدالله</SelectItem>
                                        <SelectItem value="3">محمد حسن إبراهيم</SelectItem>
                                    </SelectContent>
                                </Select>
                            </div>
                        </div>

                        <div className="space-y-2">
                            <Label className="text-sm font-semibold text-muted-foreground text-right block">الصفة القانونية</Label>
                            <Input
                                name="lessorLegality"
                                placeholder="مثال: أصالة، وكالة..."
                                className="text-right h-11 rounded-xl"
                                value={formData.lessorLegality || ''}
                                onChange={handleInputChange}
                            />
                        </div>

                        <Button
                            className="w-full gap-2 h-11 rounded-xl shadow-sm border-dashed"
                            variant="outline"
                            type="button"
                            onClick={() => handleAddDocument('lessor')}
                        >
                            <Upload className="w-4 h-4" /> رفع ثبوتيات المؤجر
                        </Button>
                    </div>
                </div>

                {/* Tenant Card */}
                <div className="bg-card rounded-2xl border shadow-sm p-8 group hover:border-secondary/20 transition-all">
                    <div className="flex items-center gap-3 mb-6 text-secondary border-b pb-4 justify-end">
                        <h3 className="text-xl font-bold">بيانات المستأجر</h3>
                        <div className="p-2 bg-secondary/10 rounded-lg group-hover:scale-110 transition-transform">
                            <User className="w-5 h-5" />
                        </div>
                    </div>

                    <div className="space-y-6">
                        <div className="space-y-2">
                            <Label className="text-sm font-semibold text-muted-foreground text-right block">المستأجر *</Label>
                            <div className="flex gap-2">
                                <div className="flex gap-1">
                                    <Button variant="outline" size="icon" type="button" className="rounded-lg h-11 w-11 hover:text-primary hover:bg-primary/5">
                                        <Plus className="w-4 h-4" />
                                    </Button>
                                    <Button variant="outline" size="icon" type="button" className="rounded-lg h-11 w-11">
                                        <Search className="w-4 h-4" />
                                    </Button>
                                </div>
                                <Select
                                    value={formData.tenantId?.toString() || ''}
                                    onValueChange={(val) => handleSelectChange('tenantId', val)}
                                >
                                    <SelectTrigger className="h-11 rounded-xl text-right flex-row-reverse flex-1">
                                        <SelectValue placeholder="اختر المستأجر..." />
                                    </SelectTrigger>
                                    <SelectContent className="text-right">
                                        <SelectItem value="1">خالد سعيد راشد</SelectItem>
                                        <SelectItem value="2">فاطمة عبد الرحمن</SelectItem>
                                        <SelectItem value="3">علي محمد ناصر</SelectItem>
                                    </SelectContent>
                                </Select>
                            </div>
                        </div>

                        <div className="space-y-2">
                            <Label className="text-sm font-semibold text-muted-foreground text-right block">الصفة القانونية</Label>
                            <Input
                                name="tenantLegality"
                                placeholder="مثال: شخصي، مندوب شركة..."
                                className="text-right h-11 rounded-xl"
                                value={formData.tenantLegality || ''}
                                onChange={handleInputChange}
                            />
                        </div>

                        <Button
                            className="w-full gap-2 h-11 rounded-xl shadow-sm border-dashed"
                            variant="outline"
                            type="button"
                            onClick={() => handleAddDocument('tenant')}
                        >
                            <Upload className="w-4 h-4" /> رفع ثبوتيات المستأجر
                        </Button>
                    </div>
                </div>
            </div>

            {/* Escorts Card */}
            <div className="bg-card rounded-2xl border shadow-sm p-8">
                <div className="flex items-center gap-2 mb-6 justify-end">
                    <h3 className="text-xl font-bold">بيانات المرافقين</h3>
                    <div className="w-1.5 h-6 bg-primary rounded-full ml-1" />
                </div>
                <div className="space-y-2">
                    <Label className="text-sm font-semibold text-muted-foreground text-right block">قائمة بأسماء المرافقين</Label>
                    <Textarea
                        name="escorts"
                        rows={3}
                        className="w-full p-4 rounded-xl text-right"
                        value={formData.escorts || ''}
                        onChange={handleInputChange}
                        placeholder="أدخل أسماء المرافقين (الزوجة، الأبناء، مرافقين آخرين...)"
                    />
                </div>
            </div>

            <DocumentModal
                open={showDocumentModal}
                type={documentType}
                onClose={() => setShowDocumentModal(false)}
            />
        </div>
    );
};

