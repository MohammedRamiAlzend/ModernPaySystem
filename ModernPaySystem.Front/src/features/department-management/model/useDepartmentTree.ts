import { useQuery } from '@tanstack/react-query';
import { departmentApi } from '@/entities/department/api/departmentApi';
import { DepartmentTree } from '@/entities/department/model/types';
import { queryKeys } from '@/shared/lib/query-keys';

export const useDepartmentTree = (rootId?: string, mode: 'full' | 'subtree' | 'children' = 'full') => {
    return useQuery({
        queryKey: queryKeys.department.tree(rootId, mode),
        queryFn: async () => {
            if (mode === 'full' || !rootId) {
                return departmentApi.getTree();
            } else if (mode === 'subtree') {
                return departmentApi.getSubTree(rootId);
            } else {
                // For 'children' only, we might need a different approach or just filter the tree
                // But the requirement says "children only"
                const children = await departmentApi.getChildren(rootId);
                // Convert simple children list to a pseudo-tree for Mermaid logic consistency
                const root = await departmentApi.getById(rootId);
                return [{
                    id: root.id,
                    name: root.name,
                    level: root.level,
                    type: root.type,
                    children: children.map(c => ({
                        id: c.id,
                        name: c.name,
                        level: c.level,
                        type: c.type,
                        children: []
                    }))
                }] as DepartmentTree[];
            }
        }
    });
};

export const convertToMermaid = (trees: DepartmentTree[], highlightId?: string, isDark: boolean = false) => {
    let mermaidText = 'graph TD\n';
    
    // Define styles based on theme
    if (isDark) {
        mermaidText += '    classDef default fill:#1f2937,stroke:#4b5563,stroke-width:1px,color:#f3f4f6;\n';
        mermaidText += '    classDef highlight fill:#3b82f6,stroke:#60a5fa,stroke-width:2px,color:#ffffff;\n';
        mermaidText += '    classDef root fill:#059669,stroke:#10b981,stroke-width:2px,color:#ffffff;\n';
    } else {
        mermaidText += '    classDef default fill:#ffffff,stroke:#e5e7eb,stroke-width:1px,color:#111827;\n';
        mermaidText += '    classDef highlight fill:#2563eb,stroke:#3b82f6,stroke-width:2px,color:#ffffff;\n';
        mermaidText += '    classDef root fill:#10b981,stroke:#34d399,stroke-width:2px,color:#ffffff;\n';
    }


    const visited = new Set<string>();

    const traverse = (node: DepartmentTree, parentId?: string) => {
        const nodeId = node.id.replace(/-/g, '_');
        const safeName = node.name.replace(/[\[\]\(\)\{\}]/g, '');
        
        mermaidText += `    ${nodeId}["${safeName}"]\n`;
        
        if (parentId) {
            const safeParentId = parentId.replace(/-/g, '_');
            mermaidText += `    ${safeParentId} --> ${nodeId}\n`;
        }

        if (node.id === highlightId) {
            mermaidText += `    class ${nodeId} highlight\n`;
        } else if (node.level === 1) {
            mermaidText += `    class ${nodeId} root\n`;
        }

        if (node.children && node.children.length > 0) {
            node.children.forEach(child => traverse(child, node.id));
        }
    };

    trees.forEach(tree => traverse(tree));

    return mermaidText;
};
