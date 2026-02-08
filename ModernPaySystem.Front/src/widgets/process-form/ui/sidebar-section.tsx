import React from 'react';
import { Plus, ChevronRight } from 'lucide-react';
import { Button } from '@/shared/ui/button';

interface SidebarSectionProps {
  services: any[];
  selectedServices: any[];
  totalRecords: number;
  addService: (service: any) => void;
}

export const SidebarSection: React.FC<SidebarSectionProps> = ({
  services,
  selectedServices,
  totalRecords,
  addService
}) => {
  return (
    <div className="space-y-6">
      <div className="bg-card rounded-2xl border p-6 shadow-sm">
        <div className="flex items-center gap-2 mb-6">
          <div className="w-1.5 h-6 bg-primary rounded-full" />
          <h3 className="font-bold text-lg">الخدمات المتاحة</h3>
        </div>

        <div className="space-y-3 max-h-[400px] overflow-y-auto pr-2 custom-scrollbar">
          {services.map(service => (
            <div key={service.code} className="group flex items-center justify-between p-3 rounded-xl border border-transparent hover:border-primary/20 hover:bg-primary/5 transition-all cursor-pointer">
              <span className="text-sm font-medium">{service.descr}</span>
              <Button
                size="icon"
                variant="ghost"
                className="h-8 w-8 rounded-lg group-hover:bg-primary group-hover:text-primary-foreground transition-colors"
                onClick={() => addService(service)}
              >
                <Plus className="w-4 h-4" />
              </Button>
            </div>
          ))}
        </div>

        <div className="mt-8 grid grid-cols-2 gap-3">
          <Button variant="outline" className="h-12 gap-2 rounded-xl">
            <ChevronRight className="w-4 h-4 shrink-0" />
            انتقال
          </Button>
          <Button variant="outline" className="h-12 gap-2 rounded-xl">
            📧
            توجيه
          </Button>
        </div>
      </div>

      {/* Statistics Card */}
      <div className="bg-primary rounded-2xl p-6 text-primary-foreground shadow-lg shadow-primary/20">
        <h4 className="font-bold opacity-80 text-sm mb-4">ملخص السجلات</h4>
        <div className="flex items-end justify-between">
          <div>
            <div className="text-3xl font-bold">{totalRecords}</div>
            <div className="text-xs opacity-70">إجمالي المعاملات</div>
          </div>
          <div className="text-right">
            <div className="text-3xl font-bold">{selectedServices.length}</div>
            <div className="text-xs opacity-70">خدمات حالية</div>
          </div>
        </div>
      </div>
    </div>
  );
};