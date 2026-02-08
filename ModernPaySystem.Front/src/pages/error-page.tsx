import { useRouteError } from 'react-router-dom';
import { ErrorWidget } from '@/widgets/error';

export const ErrorPage = () => {
  const error = useRouteError();
  
  return <ErrorWidget error={error} />;
};

export default ErrorPage;
