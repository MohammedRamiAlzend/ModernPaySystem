import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './styles/index.css'
import App from './App.tsx'
import { ThemeProvider } from './app/providers/theme-provider.tsx'
import { ContractProvider } from './app/providers/contract-provider.tsx'
import { QueryClientProvider } from '@tanstack/react-query'
import { queryClient } from './shared/lib/query-client'
import { Provider } from 'react-redux'
import { store } from './app/store'
import { StatusDialogContainer } from './shared/ui/modals/status-dialog-container'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <Provider store={store}>
      <QueryClientProvider client={queryClient}>
        <ThemeProvider defaultTheme="dark" storageKey="paysystem-theme">
          <ContractProvider>
            <App />
            <StatusDialogContainer />
          </ContractProvider>
        </ThemeProvider>
      </QueryClientProvider>
    </Provider>
  </StrictMode>,
)
