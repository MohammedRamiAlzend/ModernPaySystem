import { Home, FileText, ClipboardList, LayoutDashboard, Settings } from "lucide-react";

export const NAVIGATION_ITEMS = [
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
    },
];

export const FOOTER_NAVIGATION_ITEMS = [
    {
        title: "الإعدادات",
        path: "/settings",
        icon: <Settings className="h-5 w-5" />,
    },
];
