import {
    LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer,
} from 'recharts';
import type { ChartDataPoint } from '../../../model/types';

interface HourlyDistributionChartProps {
    data: ChartDataPoint[];
}

export function HourlyDistributionChart({ data }: HourlyDistributionChartProps) {
    const sorted = [...data].sort((a, b) => {
        const aNum = parseInt(a.label);
        const bNum = parseInt(b.label);
        return aNum - bNum;
    });

    return (
        <ResponsiveContainer width="100%" height={300}>
            <LineChart data={sorted}>
                <CartesianGrid strokeDasharray="3 3" className="stroke-muted" />
                <XAxis dataKey="label" className="text-xs" />
                <YAxis className="text-xs" label={{ value: 'عدد الإجراءات', angle: -90, position: 'insideLeft' }} />
                <Tooltip
                    contentStyle={{
                        borderRadius: '8px',
                        border: '1px solid hsl(var(--border))',
                        backgroundColor: 'hsl(var(--card))',
                    }}
                />
                <Line
                    type="monotone"
                    dataKey="value"
                    stroke="hsl(var(--primary))"
                    strokeWidth={2}
                    dot={{ r: 4, fill: 'hsl(var(--primary))' }}
                    activeDot={{ r: 6 }}
                />
            </LineChart>
        </ResponsiveContainer>
    );
}
