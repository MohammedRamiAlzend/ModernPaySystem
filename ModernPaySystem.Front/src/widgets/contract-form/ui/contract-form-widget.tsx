/* eslint-disable react-hooks/set-state-in-effect */
import React, { useState, useEffect, useRef } from 'react';
import { Save, X, ArrowRight, Printer, Check } from 'lucide-react';
import { useNavigate, useParams } from 'react-router-dom';
import { useContractManager } from '@/features/contracts/model/contract-manager';
import { Button } from '@/shared/ui/button';
import { Input } from '@/shared/ui/input';
import { Label } from '@/shared/ui/label';
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/shared/ui/select"
import { AnimatedContainer } from '@/shared/ui/common/animated-container';
import { cn } from '@/shared/lib/utils';
import { LessorTenantSection } from '@/features/contracts/ui/lessor-tenant-section';
import { PropertyInfoSection } from '@/features/contracts/ui/property-info-section';

export const ContractFormWidget = () => {
    const navigate = useNavigate();
    const { id } = useParams<{ id: string }>();
    const { addContract, updateContract, getContract } = useContractManager();

    const [activeStep, setActiveStep] = useState(0);
    const [editing, setEditing] = useState(!id);
    const hasLoadedRef = useRef(false);
    const [formData, setFormData] = useState({
        contractNo: '',
        contractDate: new Date().toISOString().split('T')[0],
        contractType: '',
        contractPeriod: '',
        rentAmount: '',
        rentStart: new Date().toISOString().split('T')[0],
        rentEnd: new Date(new Date().setFullYear(new Date().getFullYear() + 1)).toISOString().split('T')[0],
        paymentMethod: '',
        contractOrganizer: '',
        ownershipProof: '',
        specialConditions: '',

        // Property Info
        propertyNo: '',
        city: '',
        district: '',
        street: '',
        floor: '',
        apartment: '',
        occupancyType: '',
        phone: '',
        area: '',
        realEstateRegion: '',
        taxReceiptNo: '',
        taxReceiptAmount: '',
        taxReceiptDate: new Date().toISOString().split('T')[0],
        assets: '',
        tenantDescription: '',
        previousResidence: '',

        // Lessor/Tenant
        lessorId: '',
        lessorLegality: '',
        tenantId: '',
        tenantLegality: '',
        escorts: '',
    });

    useEffect(() => {
        if (id && !hasLoadedRef.current) {
            const contract = getContract(parseInt(id));
            if (contract) {
                setFormData(prev => ({
                    ...prev,
                    ...contract,
                    rentAmount: contract.rentAmount.toString(),
                    lessorId: contract.lessorId.toString(),
                    tenantId: contract.tenantId.toString(),
                }));
                hasLoadedRef.current = true;
            }
        }
    }, [id, getContract]);

    const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
        const { name, value } = e.target;
        setFormData(prev => ({ ...prev, [name]: value }));
    };

    const handleSelectChange = (name: string, value: string) => {
        setFormData(prev => ({ ...prev, [name]: value }));
    };

    const handleSave = () => {
        if (id) {
            updateContract(parseInt(id), formData);
        } else {
            addContract(formData);
        }

        navigate('/contracts');
    };

    const steps = ['بيانات العقد', 'المؤجر والمستأجر', 'معلومات العقار'];

    const renderStepContent = (step: number) => {
        switch (step) {
            case 0:
                return (
                    <div className="bg-card rounded-2xl border shadow-sm p-8 grid grid-cols-1 md:grid-cols-2 gap-8 animate-in fade-in slide-in-from-bottom-4 duration-500">
                        <div className="space-y-2">
                            <Label className="text-sm font-semibold text-muted-foreground text-right block">رقم العقد *</Label>
                            <Input
                                name="contractNo"
                                placeholder="CON-0000"
                                className="text-right h-11 rounded-xl"
                                value={formData.contractNo}
                                onChange={handleChange}
                                required
                            />
                        </div>
                        <div className="space-y-2">
                            <Label className="text-sm font-semibold text-muted-foreground text-right block">تاريخ العقد *</Label>
                            <Input
                                name="contractDate"
                                type="date"
                                className="text-right h-11 rounded-xl"
                                value={formData.contractDate}
                                onChange={handleChange}
                                required
                            />
                        </div>

                        <div className="space-y-2">
                            <Label className="text-sm font-semibold text-muted-foreground text-right block">نوع العقد *</Label>
                            <Select
                                value={formData.contractType}
                                onValueChange={(val) => handleSelectChange('contractType', val)}
                            >
                                <SelectTrigger className="h-11 rounded-xl text-right flex-row-reverse w-full">
                                    <SelectValue placeholder="اختر نوع العقد..." />
                                </SelectTrigger>
                                <SelectContent className="text-right">
                                    <SelectItem value="1">إيجار سكني</SelectItem>
                                    <SelectItem value="2">إيجار تجاري</SelectItem>
                                    <SelectItem value="3">إيجار صناعي</SelectItem>
                                    <SelectItem value="4">إيجار زراعي</SelectItem>
                                </SelectContent>
                            </Select>
                        </div>

                        <div className="space-y-2">
                            <Label className="text-sm font-semibold text-muted-foreground text-right block">المدة *</Label>
                            <Select
                                value={formData.contractPeriod}
                                onValueChange={(val) => handleSelectChange('contractPeriod', val)}
                            >
                                <SelectTrigger className="h-11 rounded-xl text-right flex-row-reverse w-full">
                                    <SelectValue placeholder="اختر المدة..." />
                                </SelectTrigger>
                                <SelectContent className="text-right">
                                    <SelectItem value="1">سنة واحدة</SelectItem>
                                    <SelectItem value="2">سنتين</SelectItem>
                                    <SelectItem value="3">3 سنوات</SelectItem>
                                    <SelectItem value="4">5 سنوات</SelectItem>
                                </SelectContent>
                            </Select>
                        </div>

                        <div className="space-y-2">
                            <Label className="text-sm font-semibold text-muted-foreground text-right block">بدل الإيجار *</Label>
                            <div className="relative">
                                <Input
                                    name="rentAmount"
                                    type="number"
                                    placeholder="0.00"
                                    className="text-right h-11 rounded-xl pr-4 pl-12"
                                    value={formData.rentAmount}
                                    onChange={handleChange}
                                    required
                                />
                                {/* <span className="absolute left-4 top-1/2 -translate-y-1/2 text-xs font-bold text-muted-foreground">ريال</span> */}
                            </div>
                        </div>

                        <div className="space-y-2">
                            <Label className="text-sm font-semibold text-muted-foreground text-right block">كيفية الدفع *</Label>
                            <Select
                                value={formData.paymentMethod}
                                onValueChange={(val) => handleSelectChange('paymentMethod', val)}
                            >
                                <SelectTrigger className="h-11 rounded-xl text-right flex-row-reverse w-full">
                                    <SelectValue placeholder="اختر طريقة الدفع..." />
                                </SelectTrigger>
                                <SelectContent className="text-right">
                                    <SelectItem value="1">شهري</SelectItem>
                                    <SelectItem value="2">ربع سنوي</SelectItem>
                                    <SelectItem value="3">نصف سنوي</SelectItem>
                                    <SelectItem value="4">سنوي</SelectItem>
                                </SelectContent>
                            </Select>
                        </div>

                        <div className="space-y-2">
                            <Label className="text-sm font-semibold text-muted-foreground text-right block">من تاريخ *</Label>
                            <Input
                                name="rentStart"
                                type="date"
                                className="text-right h-11 rounded-xl"
                                value={formData.rentStart}
                                onChange={handleChange}
                                required
                            />
                        </div>

                        <div className="space-y-2">
                            <Label className="text-sm font-semibold text-muted-foreground text-right block">إلى تاريخ *</Label>
                            <Input
                                name="rentEnd"
                                type="date"
                                className="text-right h-11 rounded-xl"
                                value={formData.rentEnd}
                                onChange={handleChange}
                                required
                            />
                        </div>

                        <div className="space-y-2 md:col-span-2">
                            <Label className="text-sm font-semibold text-muted-foreground text-right block">الشروط الخاصة</Label>
                            <textarea
                                name="specialConditions"
                                className="w-full p-4 rounded-xl border border-input bg-background focus:ring-2 focus:ring-primary/20 focus:border-primary outline-none transition-all resize-none text-right"
                                rows={3}
                                value={formData.specialConditions}
                                onChange={handleChange}
                                placeholder="اكتب أية شروط إضافية هنا..."
                            />
                        </div>
                    </div>
                );

            case 1:
                return <LessorTenantSection formData={formData} onChange={setFormData} />;

            case 2:
                return <PropertyInfoSection formData={formData} onChange={setFormData} />;

            default:
                return null;
        }
    };

    return (
        <AnimatedContainer className="max-w-6xl mx-auto space-y-8 pb-12 text-right" style={{ direction: 'rtl' }}>
            <div className="bg-card rounded-2xl border shadow-lg p-8 md:p-12">
                {/* Header */}
                <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-6 mb-12">
                    <div className="flex items-center gap-4">
                        <div className="p-3 bg-primary/10 rounded-2xl text-primary">
                            <ArrowRight className="w-6 h-6 rotate-180" onClick={() => navigate('/contracts')} style={{ cursor: 'pointer' }} />
                        </div>
                        <div>
                            <h2 className="text-3xl font-bold tracking-tight text-right">
                                {id ? 'تعديل بيانات العقد' : 'إنشاء عقد إيجار جديد'}
                            </h2>
                            <p className="text-muted-foreground text-sm mt-1">يرجى اتباع الخطوات لإكمال تسجيل كافة بيانات العقد.</p>
                        </div>
                    </div>

                    <div className="flex flex-wrap gap-3">
                        <Button variant="outline" onClick={() => navigate('/contracts')} className="gap-2 h-11 rounded-xl px-6">
                            خروج
                            <X className="w-4 h-4" />
                        </Button>

                        {editing ? (
                            <Button onClick={handleSave} className="gap-2 px-8 h-11 rounded-xl shadow-lg shadow-primary/20">
                                حفظ التغييرات
                                <Save className="w-4 h-4" />
                            </Button>
                        ) : (
                            <Button onClick={() => setEditing(true)} variant="secondary" className="gap-2 px-8 h-11 rounded-xl">
                                تعديل البيانات
                            </Button>
                        )}

                        <Button variant="ghost" size="icon" className="w-11 h-11 border rounded-xl hover:bg-primary/10 hover:text-primary transition-all">
                            <Printer className="w-5 h-5" />
                        </Button>
                    </div>
                </div>

                {/* Custom Stepper */}
                <div className="relative mb-16 max-w-3xl mx-auto">
                    <div className="absolute top-5 left-8 right-8 h-0.5 bg-muted z-0" />
                    <div className="relative z-10 flex justify-between">
                        {steps.map((label, index) => (
                            <div key={label} className="flex flex-col items-center gap-3">
                                <div
                                    className={cn(
                                        "w-11 h-11 rounded-2xl flex items-center justify-center border-2 transition-all duration-500 cursor-pointer",
                                        activeStep >= index
                                            ? "bg-primary border-primary text-primary-foreground shadow-xl shadow-primary/30 rotate-0"
                                            : "bg-card border-muted text-muted-foreground hover:border-primary/50"
                                    )}
                                    onClick={() => setActiveStep(index)}
                                >
                                    {activeStep > index ? <Check className="w-6 h-6" /> : <span className="font-bold">{index + 1}</span>}
                                </div>
                                <span className={cn(
                                    "text-sm font-bold transition-all duration-300",
                                    activeStep >= index ? "text-primary scale-110" : "text-muted-foreground"
                                )}>
                                    {label}
                                </span>
                            </div>
                        ))}
                    </div>
                </div>

                {/* Content Area */}
                <div className="min-h-[400px]">
                    {renderStepContent(activeStep)}
                </div>

                {/* Footer Navigation */}
                <div className="mt-12 pt-8 border-t flex flex-row-reverse justify-between">
                    <Button
                        onClick={() => {
                            if (activeStep === steps.length - 1) {
                                handleSave();
                            } else {
                                setActiveStep(prev => prev + 1);
                            }
                        }}
                        className="min-w-[160px] h-12 rounded-xl text-base font-bold shadow-lg shadow-primary/20"
                    >
                        {activeStep === steps.length - 1 ? 'حفظ وإرسال العقد' : 'الخطوة التالية'}
                    </Button>

                    <Button
                        variant="ghost"
                        disabled={activeStep === 0}
                        onClick={() => setActiveStep(prev => prev - 1)}
                        className="min-w-[140px] h-12 rounded-xl text-base font-medium hover:bg-muted"
                    >
                        الرجوع للسابق
                    </Button>
                </div>
            </div>
        </AnimatedContainer>
    );
};