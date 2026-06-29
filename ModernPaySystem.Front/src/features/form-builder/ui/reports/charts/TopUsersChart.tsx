import {
    BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer,
} from 'recharts';
import type { ChartDataPoint } from '../../../model/transaction-report-types';

interface TopUsersChartProps {
    data: ChartDataPoint[];
    tooltipLabel?: string;
}

export function TopUsersChart({ data, tooltipLabel = 'القيمة' }: TopUsersChartProps) {
    return (
        <div className="w-full h-full" dir="ltr">
            <ResponsiveContainer width="100%" height={300}>
                <BarChart data={data} layout="vertical" margin={{ top: 10, right: 20, left: 20, bottom: 5 }}>
                    <CartesianGrid strokeDasharray="3 3" className="stroke-muted" />
                    <XAxis type="number" className="text-xs" />
                    <YAxis type="category" dataKey="label" className="text-xs" width={100} />
                    <Tooltip
                        contentStyle={{
                            borderRadius: '8px',
                            border: '1px solid hsl(var(--border))',
                            backgroundColor: 'hsl(var(--card))',
                            textAlign: 'right',
                            direction: 'rtl'
                        }}
                        formatter={(value) => [Number(value).toLocaleString(), tooltipLabel]}
                    />
                    <Bar
                        dataKey="value"
                        name={tooltipLabel}
                        fill="hsl(var(--primary))"
                        radius={[0, 4, 4, 0]}
                    />
                </BarChart>
            </ResponsiveContainer>
        </div>
    );
}
