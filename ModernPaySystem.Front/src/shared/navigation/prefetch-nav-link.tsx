import { NavLink as RouterNavLink, type NavLinkProps } from 'react-router-dom';
import { prefetchRoute } from '@/app/router';

export function PrefetchNavLink(props: NavLinkProps) {
    const handleMouseEnter = () => {
        if (typeof props.to === 'string') {
            prefetchRoute(props.to);
        }
    };

    return (
        <RouterNavLink
            {...props}
            onMouseEnter={(e) => {
                handleMouseEnter();
                props.onMouseEnter?.(e);
            }}
        />
    );
}
