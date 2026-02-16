import { lazyWithPreload } from '@/shared/utils/lazy-with-preload';
import { AppearanceSettings } from '../ui/AppearanceSettings';
import { ReactNode } from 'react';

// Lazy load feature components to enable preloading
const LookUpManagement = lazyWithPreload(() => import('@/features/lookup-management/ui/LookUpManagement').then(m => ({ default: m.LookUpManagement })));
const UserManagement = lazyWithPreload(() => import('@/features/users/ui/UserManagement').then(m => ({ default: m.UserManagement })));
const TemplatesList = lazyWithPreload(() => import('@/features/form-builder/ui/TemplatesList').then(m => ({ default: m.TemplatesList })));

export interface SettingsTab {
    id: string;
    label: string;
    description: string;
    component: ReactNode;
    preload?: () => void;
}

export const SETTINGS_CONFIG: SettingsTab[] = [
    {
        id: 'lookup',
        label: 'إدارة الحقول (LookUp)',
        description: 'إدارة المسميات الرئيسية للنظام والحقول المساعدة',
        component: <LookUpManagement />,
        preload: () => LookUpManagement.preload()
    },
    {
        id: 'users',
        label: 'إدارة المستخدمين',
        description: 'إدارة حسابات المستخدمين وصلاحيات الوصول للنظام',
        component: <UserManagement />,
        preload: () => UserManagement.preload()
    },
    {
        id: 'templates',
        label: 'نماذج الطلبات',
        description: 'إدارة وتخصيص نماذج الطلبات والمعاملات',
        component: <TemplatesList />,
        preload: () => TemplatesList.preload()
    },
    {
        id: 'appearance',
        label: 'المظهر والتفضيلات',
        description: 'تحكم في كيفية ظهور التطبيق والخيارات الشخصية',
        component: <AppearanceSettings />
    }
];
