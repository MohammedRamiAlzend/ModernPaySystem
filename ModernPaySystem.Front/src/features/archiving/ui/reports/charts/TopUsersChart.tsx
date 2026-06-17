import {
    BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer,
} from 'recharts';
import type { ChartDataPoint } from '../../../model/types';

interface TopUsersChartProps {
    data: ChartDataPoint[];
}

export function TopUsersChart({ data }: TopUsersChartProps) {
    const sorted = [...data].sort((a, b) => b.value - a.value).slice(0, 10);

    return (
        <ResponsiveContainer width="100%" height={300}>
            <BarChart data={sorted} layout="vertical">
                <CartesianGrid strokeDasharray="3 3" className="stroke-muted" />
                <XAxis type="number" className="text-xs" />
                <YAxis type="category" dataKey="label" className="text-xs" width={120} />
                <Tooltip
                    contentStyle={{
                        borderRadius: '8px',
                        border: '1px solid hsl(var(--border))',
                        backgroundColor: 'hsl(var(--card))',
                    }}
                />
                <Bar dataKey="value" fill="hsl(var(--primary))" radius={[0, 4, 4, 0]} />
            </BarChart>
        </ResponsiveContainer>
    );
}
