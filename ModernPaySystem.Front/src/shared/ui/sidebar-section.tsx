import * as React from "react"
import { Card } from "./card"
import { cn } from "@/shared/lib/utils"

interface SidebarSectionProps extends React.HTMLAttributes<HTMLDivElement> {
    title: string;
    icon: React.ElementType;
    children: React.ReactNode;
}

export const SidebarSection = ({ title, icon: Icon, children, className, ...props }: SidebarSectionProps) => (
    <Card className={cn("p-6 shadow-lg bg-card/50 backdrop-blur-sm border-primary/5 overflow-hidden relative group transition-all hover:bg-card/70", className)} {...props}>
        <h3 className="font-bold mb-4 flex items-center gap-2 text-primary">
            <div className="p-1.5 bg-primary/10 rounded-lg group-hover:scale-110 transition-transform">
                <Icon className="w-4 h-4" />
            </div>
            {title}
        </h3>
        <div className="relative z-10">
            {children}
        </div>
    </Card>
);
