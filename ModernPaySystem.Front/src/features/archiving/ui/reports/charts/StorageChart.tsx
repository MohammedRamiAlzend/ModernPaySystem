import {
    PieChart, Pie, Cell, Tooltip, ResponsiveContainer, Legend,
    type PieLabelRenderProps,
} from 'recharts';
import type { StoragePerType } from '../../../model/types';

interface StorageChartProps {
    fileTypeBreakdown: StoragePerType[];
}

const COLORS = [
    'hsl(var(--primary))',
    'hsl(var(--chart-2))',
    'hsl(var(--chart-3))',
    'hsl(var(--chart-4))',
    'hsl(var(--chart-5))',
    '#f59e0b',
    '#ef4444',
    '#8b5cf6',
    '#14b8a6',
    '#f97316',
];

export function StorageChart({ fileTypeBreakdown }: StorageChartProps) {
    const chartData = fileTypeBreakdown.slice(0, 10).map((t) => ({
        name: `.${t.extension}`,
        value: t.count,
    }));

    if (chartData.length === 0) return null;

    const renderLabel = ({ name, percent }: PieLabelRenderProps) =>
        `${name ?? ''} ${((percent ?? 0) * 100).toFixed(0)}%`;

    return (
        <ResponsiveContainer width="100%" height={350}>
            <PieChart>
                <Pie
                    data={chartData}
                    cx="50%"
                    cy="50%"
                    outerRadius={90}
                    paddingAngle={2}
                    dataKey="value"
                    label={renderLabel}
                    labelLine
                >
                    {chartData.map((_, index) => (
                        <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                    ))}
                </Pie>
                <Tooltip
                    contentStyle={{
                        borderRadius: '8px',
                        border: '1px solid hsl(var(--border))',
                        backgroundColor: 'hsl(var(--card))',
                    }}
                />
                <Legend />
            </PieChart>
        </ResponsiveContainer>
    );
}
