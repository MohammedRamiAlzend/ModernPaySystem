import React from 'react';
import { Search, PlusCircle } from 'lucide-react';
import { Button } from '@/shared/ui/button';
import { Input } from '@/shared/ui/input';
import { Label } from '@/shared/ui/label';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/ui/select";

interface MainFormSectionProps {
  formData: any;
  username: string;
  clients: any[];
  kinshipTypes: any[];
  clientInputRef: React.RefObject<HTMLInputElement | null>;
  client2InputRef: React.RefObject<HTMLInputElement | null>;
  handleInputChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
  handleSelectChange: (name: string, value: string) => void;
  handleAddClient: () => void;
  handleSearchClient: (type: 'client1' | 'client2') => void;
}

export const MainFormSection: React.FC<MainFormSectionProps> = ({
  formData,
  username,
  clients,
  kinshipTypes,
  clientInputRef,
  client2InputRef,
  handleInputChange,
  handleSelectChange,
  handleAddClient,
  handleSearchClient
}) => {
  return (
    <div className="bg-card rounded-2xl border p-6 shadow-sm space-y-6">
      <div className="flex items-center gap-2 mb-2">
        <div className="w-1.5 h-6 bg-primary rounded-full" />
        <h3 className="font-bold text-lg">بيانات المعاملة</h3>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <div className="space-y-2">
          <Label className="text-sm font-semibold text-muted-foreground">الرقم</Label>
          <Input value={formData.CODE || ''} readOnly className="h-11 rounded-xl bg-muted/50 border-dashed" />
        </div>

        <div className="space-y-2">
          <Label className="text-sm font-semibold text-muted-foreground">تاريخ العمل</Label>
          <Input
            type="date"
            name="Date_Of_work"
            value={formData.Date_Of_work}
            onChange={handleInputChange}
            className="h-11 rounded-xl"
          />
        </div>

        <div className="space-y-2">
          <Label className="text-sm font-semibold text-muted-foreground">المستخدم</Label>
          <Input value={username} readOnly className="h-11 rounded-xl bg-muted/50 border-dashed" />
        </div>

        <div className="space-y-2">
          <Label className="text-sm font-bold text-primary">مقدم الطلب *</Label>
          <div className="flex gap-2">
            <Input
              ref={clientInputRef}
              placeholder="ابحث هنا..."
              list="clients-list"
              className="h-11 rounded-xl"
            />
            <datalist id="clients-list">
              {clients.map(c => <option key={c.Client_No} value={`${c.FIRST_NAME} ${c.FATHER_NAME} ${c.LAST_NAME}`} />)}
            </datalist>
            <Button size="icon" variant="secondary" className="h-11 w-11 shrink-0 rounded-xl" onClick={() => handleSearchClient('client1')}>
              <Search className="w-4 h-4" />
            </Button>
            <Button size="icon" variant="outline" className="h-11 w-11 shrink-0 rounded-xl" onClick={handleAddClient}>
              <PlusCircle className="w-4 h-4 text-primary" />
            </Button>
          </div>
        </div>

        <div className="space-y-2">
          <Label className="text-sm font-semibold text-muted-foreground">المستفيد</Label>
          <div className="flex gap-2">
            <Input
              ref={client2InputRef}
              placeholder="ابحث هنا..."
              className="h-11 rounded-xl"
            />
            <Button size="icon" variant="secondary" className="h-11 w-11 shrink-0 rounded-xl" onClick={() => handleSearchClient('client2')}>
              <Search className="w-4 h-4" />
            </Button>
            <Button size="icon" variant="outline" className="h-11 w-11 shrink-0 rounded-xl" onClick={handleAddClient}>
              <PlusCircle className="w-4 h-4 text-primary" />
            </Button>
          </div>
        </div>

        <div className="space-y-2">
          <Label className="text-sm font-semibold text-muted-foreground">تاريخ العملية</Label>
          <Input
            type="date"
            name="DATE_ACTION"
            value={formData.DATE_ACTION}
            onChange={handleInputChange}
            className="h-11 rounded-xl"
          />
        </div>

        <div className="space-y-2">
          <Label className="text-sm font-semibold text-muted-foreground">رقم الدور</Label>
          <Input
            name="NOTES"
            value={formData.NOTES}
            onChange={handleInputChange}
            placeholder="أدخل رقم الدور"
            className="h-11 rounded-xl"
          />
        </div>

        <div className="space-y-2">
          <Label className="text-sm font-semibold text-muted-foreground">درجة القربى</Label>
          <Select
            value={formData.CODE_KINSHIP.toString()}
            onValueChange={(val) => handleSelectChange('CODE_KINSHIP', val)}
          >
            <SelectTrigger className="h-11 rounded-xl text-right flex-row-reverse w-full">
              <SelectValue placeholder="اختر درجة القربى..." />
            </SelectTrigger>
            <SelectContent className="text-right">
              {kinshipTypes.map(k => (
                <SelectItem key={k.CODE} value={k.CODE.toString()}>{k.DESCR}</SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>

        <div className="space-y-2">
          <Label className="text-sm font-semibold text-muted-foreground">المبلغ الإجمالي</Label>
          <div className="relative">
            <Input
              type="number"
              value={formData.SUM_AMOUNT}
              readOnly
              className="h-11 rounded-xl bg-primary/5 border-primary/20 text-primary font-bold pr-4 pl-12"
            />
            <span className="absolute left-4 top-1/2 -translate-y-1/2 text-[10px] font-bold text-primary italic">L.L</span>
          </div>
        </div>
      </div>
    </div>
  );
};