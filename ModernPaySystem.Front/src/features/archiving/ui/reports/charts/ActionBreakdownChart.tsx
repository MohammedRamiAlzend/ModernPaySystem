import {
    PieChart, Pie, Cell, Tooltip, ResponsiveContainer, Legend
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

const ACTION_TRANSLATIONS: Record<string, string> = {
    'View': 'عرض',
    'Print': 'طباعة',
    'Download': 'تحميل',
    'Create': 'إنشاء',
    'Update': 'تحديث',
    'Delete': 'حذف',
    'Export': 'تصدير',
    'Upload': 'رفع'
};

export function ActionBreakdownChart({ data }: ActionBreakdownChartProps) {
    const chartData = data.map((d) => ({
        name: ACTION_TRANSLATIONS[d.label] || d.label,
        value: d.value,
        color: d.color || undefined,
    }));

    return (
        <div className="w-full h-full" dir="ltr">
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
                        nameKey="name"
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
                            textAlign: 'right',
                            direction: 'rtl'
                        }}
                    />
                    <Legend />
                </PieChart>
            </ResponsiveContainer>
        </div>
    );
}
