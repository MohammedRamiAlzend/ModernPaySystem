import React from 'react';
import { Input } from '@/shared/ui/input';
import { Label } from '@/shared/ui/label';

interface FilterSectionProps {
  searchDate: string;
  setSearchDate: (date: string) => void;
}

export const FilterSection: React.FC<FilterSectionProps> = ({
  searchDate,
  setSearchDate
}) => {
  return (
    <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8 bg-muted/30 p-4 rounded-xl items-end">
      <div className="space-y-2">
        <Label className="text-xs font-bold text-muted-foreground mr-1">
          تاريخ المعاملات المطلوب إظهارها
        </Label>
        <div className="relative">
          <Input
            type="date"
            value={searchDate}
            onChange={(e) => setSearchDate(e.target.value)}
            className="rounded-xl h-11 bg-background"
          />
        </div>
      </div>
      <div className="md:col-span-2 text-sm text-primary font-medium bg-primary/5 p-3 rounded-lg border border-primary/10">
        يمكنك من خلال هذا النموذج تعريف معاملة جديدة وتعديل وحذف معاملات قديمة. استخدم شريط الأدوات العلوي لإدارة السجلات.
      </div>
    </div>
  );
};