import React from 'react';
import { ServicesTable } from '@/shared/ui/common/process-data-table';

interface ServicesGridSectionProps {
  services: any[];
  selectedServices: any[];
  onRemoveService: (code: number) => void;
}

export const ServicesGridSection: React.FC<ServicesGridSectionProps> = ({
  services,
  selectedServices,
  onRemoveService
}) => {
  return (
    <div className="grid grid-cols-1 md:grid-cols-1 gap-6">
      <div className="bg-card rounded-2xl border shadow-sm p-6">
        <div className="flex items-center justify-between mb-6">
          <div className="flex items-center gap-2">
            <div className="w-1.5 h-6 bg-primary rounded-full" />
            <h3 className="font-bold text-lg">الخدمات المحددة</h3>
          </div>
          <div className="bg-primary/10 text-primary text-xs font-bold px-3 py-1 rounded-full">
            {selectedServices.length} خدمات
          </div>
        </div>

        <div className="rounded-xl border overflow-hidden">
          <ServicesTable 
            services={services} 
            selectedServices={selectedServices} 
            onRemoveService={onRemoveService}
          />
        </div>
      </div>
    </div>
  );
};