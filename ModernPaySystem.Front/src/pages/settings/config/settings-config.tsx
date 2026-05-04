import { lazyWithPreload } from '@/shared/utils/lazy-with-preload';
import { AppearanceSettings } from '../ui/AppearanceSettings';
import { ToolsSettings } from '../ui/ToolsSettings';
import { ReactNode } from 'react';
import {
    Database,
    Users,
    FileStack,
    Palette,
    Wrench,
    GitBranch,
    type LucideIcon,
} from 'lucide-react';

// Lazy load feature components to enable preloading
const LookUpManagement = lazyWithPreload(() => import('@/features/lookup-management/ui/LookUpManagement').then(m => ({ default: m.LookUpManagement })));
const UserManagement = lazyWithPreload(() => import('@/features/users/ui/UserManagement').then(m => ({ default: m.UserManagement })));
const TemplatesList = lazyWithPreload(() => import('@/features/form-builder/ui/TemplatesList').then(m => ({ default: m.TemplatesList })));
const DepartmentDashboardWidget = lazyWithPreload(() => import('@/widgets/department-dashboard').then(m => ({ default: m.DepartmentDashboardWidget })));

export interface SettingsTab {
    id: string;
    label: string;
    description: string;
    icon: LucideIcon;
    component: ReactNode;
    preload?: () => void;
}

export const SETTINGS_CONFIG: SettingsTab[] = [
    {
        id: 'departments',
        label: 'الأقسام والهيكل التنظيمي',
        description: 'إدارة شجرة الأقسام والهيكلية الإدارية للمؤسسة',
        icon: GitBranch,
        component: <DepartmentDashboardWidget />,
        preload: () => DepartmentDashboardWidget.preload()
    },
    {
        id: 'lookup',
        label: 'إدارة الحقول العامة',
        description: 'إدارة المسميات الرئيسية للنظام والحقول المساعدة',
        icon: Database,
        component: <LookUpManagement />,
        preload: () => LookUpManagement.preload()
    },
    {
        id: 'users',
        label: 'إدارة المستخدمين',
        description: 'إدارة حسابات المستخدمين وصلاحيات الوصول للنظام',
        icon: Users,
        component: <UserManagement />,
        preload: () => UserManagement.preload()
    },
    {
        id: 'templates',
        label: 'نماذج الخدمات',
        description: 'إدارة وتخصيص نماذج الطلبات والمعاملات',
        icon: FileStack,
        component: <TemplatesList />,
        preload: () => TemplatesList.preload()
    },
    {
        id: 'appearance',
        label: 'المظهر والتفضيلات',
        description: 'تحكم في كيفية ظهور التطبيق والخيارات الشخصية',
        icon: Palette,
        component: <AppearanceSettings />
    },
    {
        id: 'tools',
        label: 'أدوات الدعم و التشغيل',
        description: 'تحميل الأدوات المساعدة وبرامج التشغيل للنظام',
        icon: Wrench,
        component: <ToolsSettings />
    }
];
