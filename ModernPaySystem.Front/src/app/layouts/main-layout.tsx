import { Outlet, useNavigate } from "react-router-dom"
// import { Outlet, useNavigate, useLocation } from "react-router-dom"
import { ModeToggle } from "@/shared/ui/common/mode-toggle"
import { Settings, Menu } from "lucide-react"
// import { Settings, Bell, Menu } from "lucide-react"
// import { Settings, Bell, Menu, Home } from "lucide-react"
import { Button } from "@/shared/ui/button"
import { Sidebar } from "@/widgets/sidebar/ui/sidebar"
// import { NAVIGATION_ITEMS } from "@/shared/config/navigation"
import { Sheet, SheetContent, SheetTrigger } from "@/shared/ui/sheet"
import { useState } from "react"
// import { PrefetchNavLink } from "@/shared/navigation/prefetch-nav-link"
import { useAuthStore } from '@/app/store/authStore';

interface MainLayoutProps {
    children?: React.ReactNode
}

export function MainLayout({ children }: MainLayoutProps) {
    const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);
    const currentUser = useAuthStore((state) => state.user);
    const navigate = useNavigate();
    // const location = useLocation();

    return (
        <div className="flex min-h-screen bg-background text-foreground transition-colors duration-300">
            {/* Desktop Sidebar - Hidden on mobile */}
            <Sidebar className="hidden md:flex sticky top-0" />

            <div className="flex-1 flex flex-col min-w-0">
                {/* Navbar / Topbar */}
                <header className="sticky top-0 z-30 w-full border-b bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60">
                    <div className="flex h-16 items-center justify-between px-4 sm:px-8">
                        {/* Left Side (Desktop: Search, Mobile: Burger Menu + Logo) */}
                        <div className="flex items-center gap-4 flex-1">
                            {/* Burger Menu for Mobile */}
                            <div className="md:hidden">
                                <Sheet open={isMobileMenuOpen} onOpenChange={setIsMobileMenuOpen}>
                                    <SheetTrigger asChild>
                                        <Button variant="ghost" size="icon" className="h-10 w-10 rounded-xl bg-accent/50">
                                            <Menu className="h-6 w-6" />
                                        </Button>
                                    </SheetTrigger>
                                    <SheetContent side="right" className="p-0 w-72 border-none">
                                        <Sidebar
                                            className="w-full border-none"
                                            onItemClick={() => setIsMobileMenuOpen(false)}
                                        />
                                    </SheetContent>
                                </Sheet>
                            </div>
                            <div className="hidden lg:flex flex-col items-end ml-2">
                                <span className="text-sm font-bold text-foreground leading-none">
                                    {currentUser?.username || 'مستخدم'}
                                </span>
                                {/* <span className="text-[10px] text-muted-foreground mt-1">
                                    {currentUser?.roles?.[0] || 'عضو'}
                                </span> */}
                            </div>


                            {/* Logo for Mobile (centered or next to burger) */}
                            <div className="md:hidden flex items-center gap-2 mr-2">
                                <div className="w-8 h-8 bg-primary rounded-lg flex items-center justify-center">
                                    <span className="text-primary-foreground font-bold">P</span>
                                </div>
                                <span className="font-bold text-primary tracking-tight">منصة خدمات ريف دمشق</span>
                            </div>
                        </div>

                        {/* Right Side Actions */}
                        <div className="flex items-center gap-2 sm:gap-4">

                            {/* <Button variant="ghost" size="icon" className="relative hidden sm:flex">
                                <Bell className="h-5 w-5" />
                                <span className="absolute top-2 left-2 w-2 h-2 bg-red-500 rounded-full border-2 border-background"></span>
                            </Button> */}
                            <Button
                                variant="ghost"
                                size="icon"
                                className="hidden sm:flex"
                                onClick={() => navigate('/settings')}
                            >
                                <Settings className="h-5 w-5" />
                            </Button>
                            <ModeToggle />
                        </div>
                    </div>
                </header>

                {/* Main Content Area */}
                <main className="flex-1 overflow-x-hidden p-4 sm:p-8 pb-32 md:pb-8">
                    <div className="mx-auto h-full">
                        {children || <Outlet />}
                    </div>
                </main>

                {/* Mobile Tab Bar Nav */}
                {/* <nav className="md:hidden fixed bottom-6 left-6 right-6 h-16 border bg-background/80 backdrop-blur-xl rounded-2xl flex justify-around items-center z-40 shadow-2xl border-white/10 px-2">
                    {(() => {
                        const activeParent = NAVIGATION_ITEMS.find(item =>
                            item.children?.some(child => location.pathname.startsWith(child.path)) ||
                            location.pathname === item.path
                        );

                        if (activeParent && activeParent.children) {
                            return (
                                <>
                                    <Button
                                        variant="ghost"
                                        size="icon"
                                        className="text-muted-foreground h-10 w-10"
                                        onClick={() => navigate('/')}
                                    >
                                        <div className="flex flex-col items-center gap-1">
                                            <Home className="h-4 w-4" />
                                            <span className="text-[8px]">الرئيسية</span>
                                        </div>
                                    </Button>
                                    <div className="w-[1px] h-8 bg-border mx-1" />
                                    {activeParent.children.map((child) => (
                                        <PrefetchNavLink
                                            key={child.path}
                                            to={child.path}
                                            className={({ isActive }) =>
                                                `flex-1 flex flex-col items-center gap-1 p-2 rounded-xl transition-all ${isActive ? 'text-primary scale-110' : 'text-muted-foreground'}`
                                            }
                                        >
                                            {child.icon}
                                            <span className="text-[9px] font-bold text-center leading-none">{child.title}</span>
                                        </PrefetchNavLink>
                                    ))}
                                </>
                            );
                        }

                        return NAVIGATION_ITEMS.map((item) => (
                            <PrefetchNavLink
                                key={item.path}
                                to={item.path}
                                className={({ isActive }) =>
                                    `flex-1 flex flex-col items-center gap-1 p-2 rounded-xl transition-all ${isActive ? 'text-primary scale-110' : 'text-muted-foreground hover:text-foreground'}`
                                }
                            >
                                {item.icon}
                                <span className="text-[9px] font-bold text-center leading-none">{item.title}</span>
                            </PrefetchNavLink>
                        ));
                    })()}
                </nav> */}
            </div>
        </div>
    )
}
