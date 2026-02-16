import { useRequests } from '../api/formEndpoints';
import { useForms } from './useForms';
import { useRequestDetails } from './useRequestDetails';

export const useActionedRequestsLogic = () => {
    const { data: requests = [], isLoading } = useRequests(true); // Fetch responded requests
    const { data: templates = [] } = useForms();

    const { isModalOpen, setIsModalOpen, viewingResponse, handleViewRequest } = useRequestDetails(templates);

    return {
        requests,
        isLoading,
        templates,
        isModalOpen,
        setIsModalOpen,
        viewingResponse,
        handleViewRequest
    };
};
