import { Navigate } from 'react-router-dom';
// import { useAuthStore } from '@/app/store/authStore';

export const RootRedirect = () => {
  // const user = useAuthStore((state) => state.user);
  const targetPath = "form-builder/reports"
  // const targetPath = user?.isDepartmentHead 
  //   ? "/form-builder/responses" 
  //   : "/form-builder/referrals/pending";

  return <Navigate to={targetPath} replace />;
};
