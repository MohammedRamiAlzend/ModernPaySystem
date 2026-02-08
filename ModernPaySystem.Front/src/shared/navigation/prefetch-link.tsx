import { forwardRef } from 'react';
import { prefetchRoute } from '@/app/router';
import { Link as RouterLink, type LinkProps } from 'react-router-dom';

export const PrefetchLink = forwardRef<HTMLAnchorElement, LinkProps>((props, ref) => {
    const handleMouseEnter = () => {
        if (typeof props.to === 'string') {
            prefetchRoute(props.to);
        }
    };

    return (
        <RouterLink
            ref={ref}
            {...props}
            onMouseEnter={(e) => {
                handleMouseEnter();
                props.onMouseEnter?.(e);
            }}
        />
    );
});
PrefetchLink.displayName = 'PrefetchLink';
