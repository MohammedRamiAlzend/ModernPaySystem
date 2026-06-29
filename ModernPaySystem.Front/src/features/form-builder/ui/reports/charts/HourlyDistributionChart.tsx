import {
    BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer,
} from 'recharts';
import type { ChartDataPoint } from '../../../model/transaction-report-types';

interface HourlyDistributionChartProps {
    data: ChartDataPoint[];
}

export function HourlyDistributionChart({ data }: HourlyDistributionChartProps) {
    return (
        <div className="w-full h-full" dir="ltr">
            <ResponsiveContainer width="100%" height={300}>
                <BarChart data={data} margin={{ top: 10, right: 20, left: 20, bottom: 5 }}>
                    <CartesianGrid strokeDasharray="3 3" className="stroke-muted" />
                    <XAxis dataKey="label" className="text-xs" />
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
                    <Bar
                        dataKey="value"
                        name="النشاط"
                        fill="hsl(var(--chart-2))"
                        radius={[4, 4, 0, 0]}
                    />
                </BarChart>
            </ResponsiveContainer>
        </div>
    );
}
