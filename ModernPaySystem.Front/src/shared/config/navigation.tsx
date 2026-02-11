
import { Home, FileText, ClipboardList, LayoutDashboard, Settings, Reply, Send, PlusCircle } from "lucide-react";

export interface NavigationItem {
    title: string;
    path: string;
    icon: React.ReactNode;
    children?: NavigationItem[];
}

export const NAVIGATION_ITEMS: NavigationItem[] = [
    {
        title: "الرئيسية",
        path: "/",
        icon: <Home className="h-5 w-5" />,
    },
    {
        title: "عقود الإيجار",
        path: "/contracts",
        icon: <FileText className="h-5 w-5" />,
    },
    {
        title: "معاملات سريعة",
        path: "/processes",
        icon: <ClipboardList className="h-5 w-5" />,
    },
    {
        title: "بناء النماذج",
        path: "/form-builder",
        icon: <LayoutDashboard className="h-5 w-5" />,
        children: [
            {
                title: "النماذج",
                path: "/form-builder/templates",
                icon: <PlusCircle className="h-4 w-4" />,
            },
            {
                title: "تقديم طلب",
                path: "/form-builder/requests/new",
                icon: <Send className="h-4 w-4" />,
            },
            {
                title: "الرد على الطلبات",
                path: "/form-builder/responses",
                icon: <Reply className="h-4 w-4" />,
            }
        ]
    },
];

export const FOOTER_NAVIGATION_ITEMS = [
    {
        title: "الإعدادات",
        path: "/settings",
        icon: <Settings className="h-5 w-5" />,
    },
];
