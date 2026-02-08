import { cn } from "@/shared/lib/utils";

interface SkeletonProps extends React.HTMLAttributes<HTMLDivElement> {
    cards?: number;
}

export const Skeleton: React.FC<SkeletonProps> = ({ cards, className, ...props }) => {
    if (cards) {
        const items = Array.from({ length: cards });
        return (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                {items.map((_, i) => (
                    <div
                        key={i}
                        className={cn("p-6 rounded-md border border-transparent shadow-sm bg-gray-100 dark:bg-gray-800 animate-pulse", className)}
                        {...props}
                    >
                        <div className='flex justify-between items-start mb-5'>
                            <div className="h-4 bg-gray-200 dark:bg-gray-700 rounded w-2/4 " />
                            <div className="h-6 bg-gray-200 dark:bg-gray-700 rounded w-1/6 " />
                        </div>

                        <div className="space-y-3">
                            <div className="h-3 bg-gray-200 dark:bg-gray-700 rounded w-1/6" />
                            <div className="h-3 bg-gray-200 dark:bg-gray-700 rounded w-4/6" />
                        </div>
                    </div>
                ))}
            </div>
        );
    }

    return (
        <div
            className={cn("animate-pulse rounded-md bg-muted bg-gray-100 dark:bg-gray-800", className)}
            {...props}
        />
    );
};

export default Skeleton;
