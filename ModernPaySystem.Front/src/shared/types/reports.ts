export type ReportTabId = 'dashboard' | 'daily' | 'weekly' | 'monthly' | 'user-activity' | 'active-users' | 'charts' | 'daily-work' | 'storage';

export interface ReportTabConfig {
  id: ReportTabId;
  label: string;
}

export interface ReportTabData {
  data: unknown;
  isLoading: boolean;
  refetch: () => void;
}
