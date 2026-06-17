import {
    PieChart, Pie, Cell, Tooltip, ResponsiveContainer, Legend,
    type PieLabelRenderProps,
} from 'recharts';
import type { ChartDataPoint } from '../../../model/types';

interface ActionBreakdownChartProps {
    data: ChartDataPoint[];
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
];

export function ActionBreakdownChart({ data }: ActionBreakdownChartProps) {
    const chartData = data.map((d) => ({
        name: d.label,
        value: d.value,
        color: d.color || undefined,
    }));

    const renderLabel = (props: PieLabelRenderProps) => {
        const { name, percent } = props;
        return `${name ?? ''} ${((percent ?? 0) * 100).toFixed(0)}%`;
    };

    return (
        <ResponsiveContainer width="100%" height={300}>
            <PieChart>
                <Pie
                    data={chartData}
                    cx="50%"
                    cy="50%"
                    innerRadius={60}
                    outerRadius={100}
                    paddingAngle={2}
                    dataKey="value"
                    label={renderLabel}
                >
                    {chartData.map((entry, index) => (
                        <Cell
                            key={`cell-${index}`}
                            fill={entry.color || COLORS[index % COLORS.length]}
                        />
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
