import {
    BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Legend,
} from 'recharts';
import type { HourlyBreakdown } from '../../../model/transaction-report-types';

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
        <div className="w-full h-full" dir="ltr">
            <ResponsiveContainer width="100%" height={300}>
                <BarChart data={data} margin={{ top: 10, right: 20, left: 20, bottom: 5 }}>
                    <CartesianGrid strokeDasharray="3 3" className="stroke-muted" />
                    <XAxis dataKey="name" className="text-xs" />
                    <YAxis className="text-xs" width={40} tickMargin={8} />
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
                    <Bar dataKey="سجلات منشأة" fill="hsl(var(--primary))" radius={[4, 4, 0, 0]} />
                    <Bar dataKey="إجراءات" fill="hsl(var(--chart-2))" radius={[4, 4, 0, 0]} />
                </BarChart>
            </ResponsiveContainer>
        </div>
    );
}
