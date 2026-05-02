import { useEffect } from 'react';
import { RouterProvider } from 'react-router-dom';
import router from './app/router';
import { useAuthStore } from '@/app/store/authStore';

function App() {
  const { checkTokenExpiration, isAuthenticated } = useAuthStore();

  useEffect(() => {
    // Initial check on app load
    if (isAuthenticated) {
      checkTokenExpiration();
    }

    // Proactive background check every minute
    const intervalId = setInterval(() => {
      if (isAuthenticated) {
        checkTokenExpiration();
      }
    }, 60000);

    return () => clearInterval(intervalId);
  }, [isAuthenticated, checkTokenExpiration]);

  return <RouterProvider router={router} />;
}

export default App
