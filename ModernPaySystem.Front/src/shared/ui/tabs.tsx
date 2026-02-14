import * as React from "react"
import { cn } from "@/shared/lib/utils"

interface TabsProps {
    defaultValue: string;
    value?: string;
    onValueChange?: (value: string) => void;
    children: React.ReactNode;
    className?: string;
}

const TabsContext = React.createContext<{
    value?: string;
    onValueChange?: (value: string) => void;
}>({});

const Tabs: React.FC<TabsProps> = ({ defaultValue, value: controlledValue, onValueChange, children, className }) => {
    const [internalValue, setInternalValue] = React.useState(defaultValue);
    const value = controlledValue !== undefined ? controlledValue : internalValue;

    const handleValueChange = (newValue: string) => {
        setInternalValue(newValue);
        onValueChange?.(newValue);
    };

    return (
        <TabsContext.Provider value={{ value, onValueChange: handleValueChange }}>
            <div className={cn("w-full", className)}>
                {children}
            </div>
        </TabsContext.Provider>
    );
};

const TabsList = React.forwardRef<
    HTMLDivElement,
    React.HTMLAttributes<HTMLDivElement>
>(({ className, ...props }, ref) => (
    <div
        ref={ref}
        className={cn(
            "inline-flex h-10 items-center justify-center rounded-lg bg-muted p-1 text-muted-foreground",
            className
        )}
        {...props}
    />
))
TabsList.displayName = "TabsList"

interface TabsTriggerProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
    value: string;
}

const TabsTrigger = React.forwardRef<
    HTMLButtonElement,
    TabsTriggerProps
>(({ className, value, ...props }, ref) => {
    const { value: activeValue, onValueChange } = React.useContext(TabsContext);
    const isActive = activeValue === value;

    return (
        <button
            type="button"
            ref={ref}
            onClick={() => onValueChange?.(value)}
            className={cn(
                "inline-flex items-center justify-center whitespace-nowrap rounded-md px-3 py-1.5 text-sm font-medium ring-offset-background transition-all focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:pointer-events-none disabled:opacity-50",
                isActive ? "bg-background text-foreground shadow-sm" : "hover:bg-background/50",
                className
            )}
            {...props}
        />
    );
})
TabsTrigger.displayName = "TabsTrigger"

interface TabsContentProps extends React.HTMLAttributes<HTMLDivElement> {
    value: string;
}

const TabsContent = React.forwardRef<
    HTMLDivElement,
    TabsContentProps
>(({ className, value, ...props }, ref) => {
    const { value: activeValue } = React.useContext(TabsContext);
    if (activeValue !== value) return null;

    return (
        <div
            ref={ref}
            className={cn(
                "mt-2 ring-offset-background focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 animate-in fade-in-50 duration-200",
                className
            )}
            {...props}
        />
    );
})
TabsContent.displayName = "TabsContent"

export { Tabs, TabsList, TabsTrigger, TabsContent }
