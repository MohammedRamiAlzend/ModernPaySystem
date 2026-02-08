import React from 'react';
import { Save, Trash2, Plus, Edit, Printer, X } from 'lucide-react';
import { Button } from '@/shared/ui/button';

interface HeaderToolbarProps {
  onNew: () => void;
  onSave: () => void;
  onEdit: () => void;
  onDelete: () => void;
  onCancel: () => void;
  onPrint: () => void;
  onExit: () => void;
}

export const HeaderToolbar: React.FC<HeaderToolbarProps> = ({
  onNew,
  onSave,
  onEdit,
  onDelete,
  onCancel,
  onPrint,
  onExit
}) => {
  return (
    <div className="flex flex-wrap gap-2">
      <Button variant="outline" onClick={onNew} className="gap-2 h-10 rounded-xl">
        <Plus className="w-4 h-4" /> جديد
      </Button>
      <Button onClick={onSave} className="gap-2 h-10 rounded-xl px-6 shadow-lg shadow-primary/20">
        <Save className="w-4 h-4" /> تخزين
      </Button>
      <Button variant="secondary" onClick={onEdit} className="gap-2 h-10 rounded-xl">
        <Edit className="w-4 h-4" /> تعديل
      </Button>
      <Button variant="destructive" onClick={onDelete} className="gap-2 h-10 rounded-xl">
        <Trash2 className="w-4 h-4" /> حذف
      </Button>
      <Button variant="ghost" onClick={onCancel} className="gap-2 h-10 rounded-xl">
        إلغاء
      </Button>
      <Button variant="outline" onClick={onPrint} className="gap-2 h-10 rounded-xl">
        <Printer className="w-4 h-4" /> طباعة
      </Button>
      <Button variant="outline" onClick={onExit} className="gap-2 h-10 rounded-xl">
        <X className="w-4 h-4" /> خروج
      </Button>
    </div>
  );
};