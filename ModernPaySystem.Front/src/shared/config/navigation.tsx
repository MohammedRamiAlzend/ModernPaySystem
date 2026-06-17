
import { LayoutDashboard, Settings, Reply, Archive, FileCheck, Forward, Send, Clock, BarChart3, Search, Brain, History } from "lucide-react";

export interface NavigationItem {
    title: string;
    path: string;
    icon: React.ReactNode;
    isOpen?: boolean;
    children?: NavigationItem[];
}

export const NAVIGATION_ITEMS: NavigationItem[] = [
    {
        title: "منصة خدمات ريف دمشق",
        path: "/form-builder/actioned",
        icon: <LayoutDashboard className="h-5 w-5" />,
        isOpen: true,
        children: [
            {
                title: "الرد على الطلبات",
                path: "/form-builder/responses",
                icon: <Reply className="h-4 w-4" />,
            },

            {
                title: "الطلبات التي تم الرد عليها",
                path: "/form-builder/my-responses",
                icon: <FileCheck className="h-4 w-4" />,
            },

            // {
            //     title: "طلباتي",
            //     path: "/form-builder/my-requests",
            //     icon: <List className="h-4 w-4" />,
            // },

            {
                title: "الردود الصادرة",
                path: "/form-builder/actioned",
                icon: <Archive className="h-4 w-4" />,
            },
            {
                title: "الرد على الإحالات",
                path: "/form-builder/referrals/pending",
                icon: <Forward className="h-4 w-4" />,
            },
            {
                title: "الإحالات الصادرة",
                path: "/form-builder/referrals/sent",
                icon: <Send className="h-4 w-4" />,
            },
            {
                title: "الطلبات المعلقة",
                path: "/form-builder/all-pending",
                icon: <Clock className="h-4 w-4" />,
            },
            {
                title: "التقارير والإحصائيات",
                path: "/form-builder/reports",
                icon: <BarChart3 className="h-4 w-4" />,
            }

        ]
    },

    {
        title: "نظام الأرشفة",
        path: "/archiving",
        icon: <Archive className="h-5 w-5 " />,
        isOpen: true,
        children: [
            {
                title: "مستكشف الأرشيف",
                path: "/archiving",
                icon: <LayoutDashboard className="h-4 w-4" />,
            },
            {
                title: "البحث المتقدم",
                path: "/archiving/search",
                icon: <Search className="h-4 w-4" />,
            },
            {
                title: "البحث الدلالي",
                path: "/archiving/semantic-search",
                icon: <Brain className="h-4 w-4" />,
            },
            {
                title: "طلبات تعديل الأرشيف",
                path: "/archiving/edit-requests",
                icon: <Clock className="h-4 w-4" />,
            },
            {
                title: "سجلات النشاط (Audit Logs)",
                path: "/archiving/audit-logs",
                icon: <History className="h-4 w-4" />,
            },
            {
                title: "التقارير والإحصائيات",
                path: "/archiving/reports",
                icon: <BarChart3 className="h-4 w-4" />,
            }
        ]
    }
];

export const FOOTER_NAVIGATION_ITEMS = [
    {
        title: "الإعدادات",
        path: "/settings",
        icon: <Settings className="h-5 w-5" />,
    },
];
