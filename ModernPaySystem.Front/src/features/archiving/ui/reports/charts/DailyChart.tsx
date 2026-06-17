import {
    BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Legend,
} from 'recharts';
import type { HourlyBreakdown } from '../../../model/types';

interface DailyChartProps {
    hourlyBreakdown: HourlyBreakdown[];
}

export function DailyChart({ hourlyBreakdown }: DailyChartProps) {
    const data = hourlyBreakdown.map((h) => ({
        name: `${h.hour}:00`,
        'سجلات منشأة': h.recordsCreated,
        'إجراءات': h.actions,
    }));

    return (
        <ResponsiveContainer width="100%" height={300}>
            <BarChart data={data}>
                <CartesianGrid strokeDasharray="3 3" className="stroke-muted" />
                <XAxis dataKey="name" className="text-xs" />
                <YAxis className="text-xs" label={{ value: 'العدد', angle: -90, position: 'insideLeft' }} />
                <Tooltip
                    contentStyle={{
                        borderRadius: '8px',
                        border: '1px solid hsl(var(--border))',
                        backgroundColor: 'hsl(var(--card))',
                    }}
                />
                <Legend />
                <Bar dataKey="سجلات منشأة" fill="hsl(var(--primary))" radius={[4, 4, 0, 0]} />
                <Bar dataKey="إجراءات" fill="hsl(var(--chart-2))" radius={[4, 4, 0, 0]} />
            </BarChart>
        </ResponsiveContainer>
    );
}
