
import { LayoutDashboard, Settings, Reply, Archive, FileCheck, Forward, Send, Clock } from "lucide-react";
// import { Home, FileText, ClipboardList, LayoutDashboard, Settings, Reply, Archive,List } from "lucide-react";

export interface NavigationItem {
    title: string;
    path: string;
    icon: React.ReactNode;
    isOpen?: boolean;
    children?: NavigationItem[];
}

export const NAVIGATION_ITEMS: NavigationItem[] = [
    // {
    //     title: "الرئيسية",
    //     path: "/",
    //     icon: <Home className="h-5 w-5" />,
    // },
    // {
    //     title: "عقود الإيجار",
    //     path: "/contracts",
    //     icon: <FileText className="h-5 w-5" />,
    // },
    // {
    //     title: "معاملات سريعة",
    //     path: "/processes",
    //     icon: <ClipboardList className="h-5 w-5" />,
    // },
    {
        title: "منصة خدمات ريف دمشق",
        path: "/form-builder/actioned",
        icon: <LayoutDashboard className="h-5 w-5" />,
        isOpen: true,
        children: [
            // {
            //     title: "إدارة النماذج",
            //     path: "/settings?tab=templates",
            //     icon: <Settings className="h-4 w-4" />,
            // },
            // {
            //     title: "تقديم طلب",
            //     path: "/form-builder/requests/new",
            //     icon: <Send className="h-4 w-4" />,
            // },
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
                icon: <Forward className="h-4 w-4 text-amber-500" />,
            },
            {
                title: "الإحالات الصادرة",
                path: "/form-builder/referrals/sent",
                icon: <Send className="h-4 w-4 text-sky-500" />,
            },
            {
                title: "الطلبات المعلقة",
                path: "/form-builder/all-pending",
                icon: <Clock className="h-4 w-4" />,
            }

        ]
    },
    // {
    //     title: "إعدادات النظام",
    //     path: "/settings",
    //     icon: <Settings className="h-5 w-5" />,
    // },
];

export const FOOTER_NAVIGATION_ITEMS = [
    {
        title: "الإعدادات",
        path: "/settings",
        icon: <Settings className="h-5 w-5" />,
    },
];
