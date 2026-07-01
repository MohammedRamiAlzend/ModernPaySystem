import { useAuthStore } from '@/app/store/authStore';
import { Navigate } from 'react-router-dom';
// import { useAuthStore } from '@/app/store/authStore';

export const RootRedirect = () => {
  const user = useAuthStore((state) => state.user);
  // const targetPath = "form-builder/reports"
  const targetPath = user?.isDepartmentHead
    ? "/form-builder/reports"
    : "/form-builder/all-pending";

  return <Navigate to={targetPath} replace />;
};
