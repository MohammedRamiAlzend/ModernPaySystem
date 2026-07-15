import { useState, useRef } from 'react';
import type { ReportTabId } from '@/shared/types/reports';

export const useReportsPage = (initialTab: ReportTabId = 'dashboard') => {
  const [activeTab, setActiveTab] = useState<ReportTabId>(initialTab);
  const [fromDate, setFromDate] = useState('');
  const [toDate, setToDate] = useState('');
  const [selectedDate, setSelectedDate] = useState('');
  const [workDate, setWorkDate] = useState('');
  const [weekStart, setWeekStart] = useState('');
  const [reportYear, setReportYear] = useState<number>(new Date().getFullYear());
  const [reportMonth, setReportMonth] = useState<number>(new Date().getMonth() + 1);

  const dailyChartRef = useRef<HTMLDivElement>(null);
  const chartsSectionRef = useRef<HTMLDivElement>(null);

  return {
    activeTab,
    setActiveTab,
    fromDate, setFromDate,
    toDate, setToDate,
    selectedDate, setSelectedDate,
    workDate, setWorkDate,
    weekStart, setWeekStart,
    reportYear, setReportYear,
    reportMonth, setReportMonth,
    dailyChartRef,
    chartsSectionRef,
  };
};
