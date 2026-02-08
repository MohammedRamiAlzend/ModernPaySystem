import { lazy, type LazyExoticComponent, type ComponentType } from 'react';

type ImportFn = () => Promise<{ default: ComponentType<any> }>;

export interface PreloadableComponent extends LazyExoticComponent<ComponentType<any>> {
    preload: ImportFn;
}

export function lazyWithPreload(importFn: ImportFn): PreloadableComponent {
    const Component = lazy(importFn) as PreloadableComponent;
    Component.preload = importFn;
    return Component;
}
