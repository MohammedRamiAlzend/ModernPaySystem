import {
    BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer,
} from 'recharts';
import type { ChartDataPoint } from '../../../model/types';

interface DailyActivityChartProps {
    data: ChartDataPoint[];
}

export function DailyActivityChart({ data }: DailyActivityChartProps) {
    return (
        <ResponsiveContainer width="100%" height={300}>
            <BarChart data={data}>
                <CartesianGrid strokeDasharray="3 3" className="stroke-muted" />
                <XAxis dataKey="label" className="text-xs" />
                <YAxis className="text-xs" label={{ value: 'عدد السجلات', angle: -90, position: 'insideLeft' }} />
                <Tooltip
                    contentStyle={{
                        borderRadius: '8px',
                        border: '1px solid hsl(var(--border))',
                        backgroundColor: 'hsl(var(--card))',
                    }}
                />
                <Bar
                    dataKey="value"
                    fill="hsl(var(--primary))"
                    radius={[4, 4, 0, 0]}
                    label={{ position: 'top', fontSize: 11 }}
                />
            </BarChart>
        </ResponsiveContainer>
    );
}
