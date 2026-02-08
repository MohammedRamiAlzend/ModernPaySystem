import { cn } from "@/shared/lib/utils"

interface AnimatedContainerProps extends React.HTMLAttributes<HTMLDivElement> {
    children: React.ReactNode
    show?: boolean
    duration?: number
    delay?: number
}

export function AnimatedContainer({
    children,
    className,
    show = true,
    duration = 500,
    delay = 0,
    ...props
}: AnimatedContainerProps) {
    if (!show) return null

    return (
        <div
            className={cn(
                "animate-in fade-in fill-mode-both",
                className
            )}
            style={{
                animationDuration: `${duration}ms`,
                animationDelay: `${delay}ms`,
            }}
            {...props}
        >
            {children}
        </div>
    )
}

