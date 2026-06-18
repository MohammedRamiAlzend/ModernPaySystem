import {
    BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Legend,
} from 'recharts';
import type { DailyBreakdownItem } from '../../../model/types';

interface DailyBreakdownChartProps {
    dailyBreakdown: DailyBreakdownItem[];
}

function formatDate(dateStr: string): string {
    return new Date(dateStr).toLocaleDateString('ar-SY', { month: 'short', day: 'numeric' });
}

export function DailyBreakdownChart({ dailyBreakdown }: DailyBreakdownChartProps) {
    const data = dailyBreakdown.map((d) => ({
        name: formatDate(d.date),
        'سجلات منشأة': d.recordsCreated,
        'إجراءات': d.actions,
        'مستخدمون نشطون': d.activeUsers,
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
                    <Bar dataKey="مستخدمون نشطون" fill="hsl(var(--chart-3))" radius={[4, 4, 0, 0]} />
                </BarChart>
            </ResponsiveContainer>
        </div>
    );
}
