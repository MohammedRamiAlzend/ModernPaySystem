import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './styles/index.css'
import App from './App.tsx'
import { ThemeProvider } from './app/providers/theme-provider.tsx'
import { ContractProvider } from './app/providers/contract-provider.tsx'
import { QueryClientProvider } from '@tanstack/react-query'
import { queryClient } from './shared/lib/query-client'
import { GlobalDialogContainer } from './shared/ui/modals/status-dialog-container'
import { NuqsAdapter } from 'nuqs/adapters/react-router';
import { ReactQueryDevtools } from '@tanstack/react-query-devtools'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <NuqsAdapter>
      <QueryClientProvider client={queryClient}>
        <ThemeProvider defaultTheme="dark" storageKey="paysystem-theme">
          <ContractProvider>
            <App />

            <GlobalDialogContainer />
          </ContractProvider>
        </ThemeProvider>
        {import.meta.env.VITE_DEV === 'true' && <ReactQueryDevtools initialIsOpen={false} />}
      </QueryClientProvider>
    </NuqsAdapter>
  </StrictMode>,
)
